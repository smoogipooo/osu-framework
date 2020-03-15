// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Primitives;

namespace osu.Framework.Graphics.Rendering.Intents
{
    public readonly struct PushOrthoIntent : IIntent
    {
        public readonly RectangleF Ortho;

        public PushOrthoIntent(RectangleF ortho)
        {
            Ortho = ortho;
        }
    }
}
