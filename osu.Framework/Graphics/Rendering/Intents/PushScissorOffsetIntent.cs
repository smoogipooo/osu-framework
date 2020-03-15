// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Primitives;

namespace osu.Framework.Graphics.Rendering.Intents
{
    public readonly struct PushScissorOffsetIntent : IIntent
    {
        public readonly Vector2I Offset;

        public PushScissorOffsetIntent(Vector2I offset)
        {
            Offset = offset;
        }
    }
}
