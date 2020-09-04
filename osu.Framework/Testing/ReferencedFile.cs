// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Framework.Testing
{
    public readonly struct ReferencedFile
    {
        public readonly uint Priority;
        public readonly string[] Paths;

        public ReferencedFile(uint priority, string[] paths)
        {
            Priority = priority;
            Paths = paths;
        }
    }
}
