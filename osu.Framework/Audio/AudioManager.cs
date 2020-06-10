﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ManagedBass;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Framework.Threading;

namespace osu.Framework.Audio
{
    public class AudioManager : AudioCollectionManager<AdjustableAudioComponent>
    {
        /// <summary>
        /// The manager component responsible for audio tracks (e.g. songs).
        /// </summary>
        public ITrackStore Tracks => globalTrackStore.Value;

        /// <summary>
        /// The manager component responsible for audio samples (e.g. sound effects).
        /// </summary>
        public ISampleStore Samples => globalSampleStore.Value;

        /// <summary>
        /// The thread audio operations (mainly Bass calls) are ran on.
        /// </summary>
        private readonly AudioThread thread;

        /// <summary>
        /// The names of all available audio devices.
        /// </summary>
        /// <remarks>
        /// This property does not contain the names of disabled audio devices.
        /// </remarks>
        public IEnumerable<string> AudioDeviceNames => audioDeviceNames;

        /// <summary>
        /// Is fired whenever a new audio device is discovered and provides its name.
        /// </summary>
        public event Action<string> OnNewDevice;

        /// <summary>
        /// Is fired whenever an audio device is lost and provides its name.
        /// </summary>
        public event Action<string> OnLostDevice;

        /// <summary>
        /// The preferred audio device we should use. A value of
        /// <see cref="string.Empty"/> denotes the OS default.
        /// </summary>
        public readonly Bindable<string> AudioDevice = new Bindable<string>();

        /// <summary>
        /// Volume of all samples played game-wide.
        /// </summary>
        public readonly BindableDouble VolumeSample = new BindableDouble(1)
        {
            MinValue = 0,
            MaxValue = 1
        };

        /// <summary>
        /// Volume of all tracks played game-wide.
        /// </summary>
        public readonly BindableDouble VolumeTrack = new BindableDouble(1)
        {
            MinValue = 0,
            MaxValue = 1
        };

        public override bool IsLoaded => base.IsLoaded &&
                                         // bass default device is a null device (-1), not the actual system default.
                                         Bass.CurrentDevice != Bass.DefaultDevice;

        // Mutated by multiple threads, must be thread safe.
        private ImmutableList<DeviceInfo> audioDevices = ImmutableList<DeviceInfo>.Empty;
        private ImmutableList<string> audioDeviceNames = ImmutableList<string>.Empty;

        private Scheduler scheduler => thread.Scheduler;

        private Scheduler eventScheduler => EventScheduler ?? scheduler;

        private readonly CancellationTokenSource cancelSource = new CancellationTokenSource();
        private readonly DeviceInfoUpdateComparer updateComparer = new DeviceInfoUpdateComparer();

        /// <summary>
        /// The scheduler used for invoking publicly exposed delegate events.
        /// </summary>
        public Scheduler EventScheduler;

        private readonly Lazy<TrackStore> globalTrackStore;
        private readonly Lazy<SampleStore> globalSampleStore;

        /// <summary>
        /// Constructs an AudioStore given a track resource store, and a sample resource store.
        /// </summary>
        /// <param name="audioThread">The host's audio thread.</param>
        /// <param name="trackStore">The resource store containing all audio tracks to be used in the future.</param>
        /// <param name="sampleStore">The sample store containing all audio samples to be used in the future.</param>
        public AudioManager(AudioThread audioThread, ResourceStore<byte[]> trackStore, ResourceStore<byte[]> sampleStore)
        {
            thread = audioThread;

            thread.RegisterManager(this);

            AudioDevice.ValueChanged += onDeviceChanged;

            globalTrackStore = new Lazy<TrackStore>(() =>
            {
                var store = new TrackStore(trackStore);
                AddItem(store);
                store.AddAdjustment(AdjustableProperty.Volume, VolumeTrack);
                return store;
            });

            globalSampleStore = new Lazy<SampleStore>(() =>
            {
                var store = new SampleStore(sampleStore);
                AddItem(store);
                store.AddAdjustment(AdjustableProperty.Volume, VolumeSample);
                return store;
            });

            CancellationToken token = cancelSource.Token;

            scheduler.Add(() =>
            {
                // sync audioDevices every 1000ms
                new Thread(() =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        try
                        {
                            syncAudioDevices();
                            Thread.Sleep(1000);
                        }
                        catch
                        {
                        }
                    }
                })
                {
                    IsBackground = true
                }.Start();
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            OnNewDevice = null;
            OnLostDevice = null;

            cancelSource.Cancel();
            thread.UnregisterManager(this);

            thread.FreeDevice(Bass.CurrentDevice);
        }

        private void onDeviceChanged(ValueChangedEvent<string> args)
        {
            scheduler.Add(() => setAudioDevice(args.NewValue));
        }

        private void onDevicesChanged()
        {
            scheduler.Add(() =>
            {
                if (!IsCurrentDeviceValid())
                    setAudioDevice();
            });
        }

        /// <summary>
        /// Obtains the <see cref="TrackStore"/> corresponding to a given resource store.
        /// Returns the global <see cref="TrackStore"/> if no resource store is passed.
        /// </summary>
        /// <param name="store">The <see cref="IResourceStore{T}"/> of which to retrieve the <see cref="TrackStore"/>.</param>
        public ITrackStore GetTrackStore(IResourceStore<byte[]> store = null)
        {
            if (store == null) return globalTrackStore.Value;

            TrackStore tm = new TrackStore(store);
            globalTrackStore.Value.AddItem(tm);
            return tm;
        }

        /// <summary>
        /// Obtains the <see cref="SampleStore"/> corresponding to a given resource store.
        /// Returns the global <see cref="SampleStore"/> if no resource store is passed.
        /// </summary>
        /// <param name="store">The <see cref="IResourceStore{T}"/> of which to retrieve the <see cref="SampleStore"/>.</param>
        public ISampleStore GetSampleStore(IResourceStore<byte[]> store = null)
        {
            if (store == null) return globalSampleStore.Value;

            SampleStore sm = new SampleStore(store);
            globalSampleStore.Value.AddItem(sm);
            return sm;
        }

        /// <summary>
        /// Sets the output audio device by its name.
        /// This will automatically fall back to the system default device on failure.
        /// </summary>
        /// <param name="deviceName">Name of the audio device, or null to use the configured device preference <see cref="AudioDevice"/>.</param>
        private bool setAudioDevice(string deviceName = null)
        {
            deviceName ??= AudioDevice.Value;

            // try using the specified device
            if (setAudioDevice(audioDevices.FindIndex(d => d.Name == deviceName)))
                return true;

            // try using the system default device
            if (setAudioDevice(audioDevices.FindIndex(d => d.Name != deviceName && d.IsDefault)))
                return true;

            // no audio devices can be used, so try using Bass-provided "No sound" device as last resort
            if (setAudioDevice(Bass.NoSoundDevice))
                return true;

            //we're fucked. even "No sound" device won't initialise.
            return false;
        }

        private bool setAudioDevice(int deviceIndex)
        {
            var device = audioDevices.ElementAtOrDefault(deviceIndex);

            // device is invalid.
            if (!device.IsEnabled)
                return false;

            // same device.
            if (device.IsInitialized && deviceIndex == Bass.CurrentDevice)
                return true;

            // initialize new device.
            int lastDevice = Bass.CurrentDevice;

            if (!InitBass(deviceIndex))
                return false;

            Logger.Log($@"BASS Initialized
                          BASS Version:               {Bass.Version}
                          BASS FX Version:            {ManagedBass.Fx.BassFx.Version}
                          Device:                     {device.Name}
                          Drive:                      {device.Driver}");

            // we have successfully initialised a new device.
            UpdateDevice(deviceIndex);

            // free the old device.
            thread.FreeDevice(lastDevice);

            Bass.PlaybackBufferLength = 100;
            Bass.UpdatePeriod = 5;

            return true;
        }

        /// <summary>
        /// This method calls <see cref="Bass.Init(int, int, DeviceInitFlags, IntPtr, IntPtr)"/>.
        /// It can be overridden for unit testing.
        /// </summary>
        protected virtual bool InitBass(int device)
        {
            // reduce latency to a known sane minimum.
            Bass.Configure(ManagedBass.Configuration.DeviceBufferLength, 10);

            // this likely doesn't help us but also doesn't seem to cause any issues or any cpu increase.
            Bass.Configure(ManagedBass.Configuration.UpdatePeriod, 5);

            // without this, if bass falls back to directsound legacy mode the audio playback offset will be way off.
            Bass.Configure(ManagedBass.Configuration.TruePlayPosition, 0);

            // Enable custom BASS_CONFIG_MP3_OLDGAPS flag for backwards compatibility.
            Bass.Configure((ManagedBass.Configuration)68, 1);

            // For iOS devices, set the default audio policy to one that obeys the mute switch.
            Bass.Configure(ManagedBass.Configuration.IOSMixAudio, 5);

            // ensure there are no brief delays on audio operations (causing stream STALLs etc.) after periods of silence.
            Bass.Configure(ManagedBass.Configuration.DevNonStop, true);

            return thread.InitDevice(device);
        }

        private void syncAudioDevices()
        {
            // audioDevices are updated if:
            // - A new device is added
            // - An existing device is Enabled/Disabled or set as Default
            var updatedAudioDevices = EnumerateAllDevices().ToImmutableList();
            if (audioDevices.SequenceEqual(updatedAudioDevices, updateComparer))
                return;

            audioDevices = updatedAudioDevices;

            // Bass should always be providing "No sound" device
            Trace.Assert(audioDevices.Count > 0, "Bass did not provide any audio devices.");

            onDevicesChanged();

            var oldDeviceNames = audioDeviceNames;
            var newDeviceNames = audioDeviceNames = audioDevices.Skip(1).Where(d => d.IsEnabled).Select(d => d.Name).ToImmutableList();

            var newDevices = newDeviceNames.Except(oldDeviceNames).ToList();
            var lostDevices = oldDeviceNames.Except(newDeviceNames).ToList();

            if (newDevices.Count > 0 || lostDevices.Count > 0)
            {
                eventScheduler.Add(delegate
                {
                    foreach (var d in newDevices)
                        OnNewDevice?.Invoke(d);
                    foreach (var d in lostDevices)
                        OnLostDevice?.Invoke(d);
                });
            }
        }

        protected virtual IEnumerable<DeviceInfo> EnumerateAllDevices()
        {
            int deviceCount = Bass.DeviceCount;
            for (int i = 0; i < deviceCount; i++)
                yield return Bass.GetDeviceInfo(i);
        }

        // The current device is considered valid if it is enabled, initialized, and not a fallback device.
        protected virtual bool IsCurrentDeviceValid()
        {
            var device = audioDevices.ElementAtOrDefault(Bass.CurrentDevice);
            bool isFallback = AudioDevice.Value == null ? !device.IsDefault : device.Name != AudioDevice.Value;
            return device.IsEnabled && device.IsInitialized && !isFallback;
        }

        public override string ToString()
        {
            var deviceName = audioDevices.ElementAtOrDefault(Bass.CurrentDevice).Name;
            return $@"{GetType().ReadableName()} ({deviceName ?? "Unknown"})";
        }

        private class DeviceInfoUpdateComparer : IEqualityComparer<DeviceInfo>
        {
            public bool Equals(DeviceInfo x, DeviceInfo y) => x.IsEnabled == y.IsEnabled && x.IsDefault == y.IsDefault;

            public int GetHashCode(DeviceInfo obj) => obj.Name.GetHashCode();
        }
    }
}
