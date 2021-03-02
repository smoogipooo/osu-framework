// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Runtime.InteropServices;
using ManagedBass;

namespace osu.Framework.Audio.Sample
{
    internal sealed class SafeSampleBassHandle : SafeHandle
    {
        public SafeSampleBassHandle(IntPtr handle, bool ownsHandle)
            : base(handle, ownsHandle)
        {
        }

        protected override bool ReleaseHandle()
        {
            Bass.SampleFree(handle.ToInt32());
            return true;
        }

        public override bool IsInvalid => handle == IntPtr.Zero;
    }
}
