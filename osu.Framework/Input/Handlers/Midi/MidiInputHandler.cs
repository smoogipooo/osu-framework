// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Commons.Music.Midi;
using osu.Framework.Input.StateChanges;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Statistics;
using osu.Framework.Threading;

namespace osu.Framework.Input.Handlers.Midi
{
    public class MidiInputHandler : InputHandler
    {
        public override bool IsActive => true;
        public override int Priority => 0;

        private ScheduledDelegate scheduledRefreshDevices;

        private readonly Dictionary<string, IMidiInput> openedDevices = new Dictionary<string, IMidiInput>();

        /// <summary>
        /// The last event for each midi device. This is required for Running Status (repeat messages sent without
        /// event type).
        /// </summary>
        private readonly Dictionary<string, byte> runningStatus = new Dictionary<string, byte>();

        public override bool Initialize(GameHost host)
        {
            // Try to initialize. This can throw on Linux if asound cannot be found.
            try
            {
                var unused = MidiAccessManager.Default.Inputs.ToList();
            }
            catch (Exception e)
            {
                Logger.Error(e, RuntimeInfo.OS == RuntimeInfo.Platform.Linux
                    ? "Couldn't list input devices, is libasound2-dev installed?"
                    : "Couldn't list input devices.");
                return false;
            }

            Enabled.BindValueChanged(e =>
            {
                if (e.NewValue)
                {
                    host.InputThread.Scheduler.Add(scheduledRefreshDevices = new ScheduledDelegate(refreshDevices, 0, 500));
                }
                else
                {
                    scheduledRefreshDevices?.Cancel();

                    foreach (var value in openedDevices.Values)
                    {
                        value.MessageReceived -= onMidiMessageReceived;
                    }

                    openedDevices.Clear();
                }
            }, true);

            return true;
        }

        private void refreshDevices()
        {
            var inputs = MidiAccessManager.Default.Inputs.ToList();

            // check removed devices
            foreach (string key in openedDevices.Keys.ToArray())
            {
                var value = openedDevices[key];

                if (inputs.All(i => i.Id != key))
                {
                    value.MessageReceived -= onMidiMessageReceived;
                    openedDevices.Remove(key);

                    Logger.Log($"Disconnected MIDI device: {value.Details.Name}");
                }
            }

            // check added devices
            foreach (IMidiPortDetails input in inputs)
            {
                if (openedDevices.All(x => x.Key != input.Id))
                {
                    var newInput = MidiAccessManager.Default.OpenInputAsync(input.Id).Result;
                    newInput.MessageReceived += onMidiMessageReceived;
                    openedDevices[input.Id] = newInput;

                    Logger.Log($"Connected MIDI device: {newInput.Details.Name}");
                }
            }
        }

        private void onMidiMessageReceived(object sender, MidiReceivedEventArgs e)
        {
            Debug.Assert(sender is IMidiInput);
            var senderId = ((IMidiInput)sender).Details.Id;

            try
            {
                for (int i = e.Start; i < e.Length;)
                {
                    readEvent(e.Data, senderId, ref i, out byte eventType, out byte key, out byte velocity);
                    dispatchEvent(eventType, key, velocity);
                }
            }
            catch (Exception exception)
            {
                var dataString = string.Join("-", e.Data.Select(b => b.ToString("X2")));
                Logger.Error(exception, $"An exception occurred while reading MIDI data from sender {senderId}: {dataString}");
            }
        }

        private void readEvent(byte[] data, string senderId, ref int i, out byte eventType, out byte key, out byte velocity)
        {
            byte statusType = data[i++];

            if (statusType <= 0x7F)
            {
                // This is a running status, re-use the event type from the previous message
                if (!runningStatus.ContainsKey(senderId))
                    throw new InvalidDataException($"Received running status of sender {senderId}, but no event type was stored");

                eventType = runningStatus[senderId];
                key = statusType;
                velocity = data[i++];
            }
            else if (statusType >= 0xF8)
            {
                // Events F8 through FF do not reset the last known status byte
                eventType = statusType;
                key = data[i++];
                velocity = data[i++];
            }
            else if (statusType >= 0xF0)
            {
                // Events F0 through F7 reset the running status
                eventType = statusType;
                key = data[i++];
                velocity = data[i++];

                if (runningStatus.ContainsKey(senderId))
                    runningStatus.Remove(senderId);
            }
            else
            {
                // normal event, update running status
                eventType = statusType;
                key = data[i++];
                velocity = data[i++];

                runningStatus[senderId] = eventType;
            }
        }

        private void dispatchEvent(byte eventType, byte key, byte velocity)
        {
            Logger.Log($"Handling MIDI event {eventType:X2}:{key:X2}:{velocity:X2}");

            switch (eventType)
            {
                case MidiEvent.NoteOn when velocity != 0:
                    Logger.Log($"NoteOn: {(MidiKey)key}/{velocity / 128f:P}");
                    PendingInputs.Enqueue(new MidiKeyInput((MidiKey)key, velocity, true));
                    FrameStatistics.Increment(StatisticsCounterType.MidiEvents);
                    break;

                case MidiEvent.NoteOff:
                case MidiEvent.NoteOn when velocity == 0:
                    Logger.Log($"NoteOff: {(MidiKey)key}/{velocity / 128f:P}");
                    PendingInputs.Enqueue(new MidiKeyInput((MidiKey)key, false));
                    FrameStatistics.Increment(StatisticsCounterType.MidiEvents);
                    break;
            }
        }
    }
}
