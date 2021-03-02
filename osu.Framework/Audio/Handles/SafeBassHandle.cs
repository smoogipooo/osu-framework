// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Runtime.InteropServices;

namespace osu.Framework.Audio.Handles
{
    public abstract class SafeBassHandle : SafeHandle
    {
        protected SafeBassHandle(int handle, bool ownsHandle)
            : base(new IntPtr(handle), ownsHandle)
        {
        }

        public sealed override bool IsInvalid => handle == IntPtr.Zero;
    }
}
