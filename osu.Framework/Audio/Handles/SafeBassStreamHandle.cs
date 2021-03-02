// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using ManagedBass;

namespace osu.Framework.Audio.Handles
{
    public class SafeBassStreamHandle : SafeBassHandle
    {
        public SafeBassStreamHandle(int handle, bool ownsHandle)
            : base(handle, ownsHandle)
        {
        }

        protected override bool ReleaseHandle()
        {
            Bass.ChannelStop(handle.ToInt32()); // Todo: Is this required?
            return Bass.StreamFree(handle.ToInt32());
        }
    }
}
