// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Framework.Graphics.Rendering.Intents
{
    public readonly struct PushScissorStateIntent : IIntent
    {
        public readonly bool ShouldScissor;

        public PushScissorStateIntent(bool shouldScissor)
        {
            ShouldScissor = shouldScissor;
        }
    }
}
