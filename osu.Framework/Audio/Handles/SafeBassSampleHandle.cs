// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using ManagedBass;

namespace osu.Framework.Audio.Handles
{
    public sealed class SafeBassSampleHandle : SafeBassHandle
    {
        public SafeBassSampleHandle(int handle, bool ownsHandle)
            : base(handle, ownsHandle)
        {
        }

        protected override bool ReleaseHandle()
        {
            return Bass.SampleFree(handle.ToInt32());
        }
    }
}
