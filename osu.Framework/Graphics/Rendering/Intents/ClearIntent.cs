// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.OpenGL;

namespace osu.Framework.Graphics.Rendering.Intents
{
    public readonly struct ClearIntent : IIntent
    {
        public readonly ClearInfo ClearInfo;

        public ClearIntent(ClearInfo clearInfo)
        {
            ClearInfo = clearInfo;
        }
    }
}
