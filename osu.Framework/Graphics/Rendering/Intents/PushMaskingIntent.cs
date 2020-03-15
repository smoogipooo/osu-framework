// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.OpenGL;

namespace osu.Framework.Graphics.Rendering.Intents
{
    public readonly struct PushMaskingIntent : IIntent
    {
        public readonly MaskingInfo MaskingInfo;
        public readonly bool OverwritePreviousScissor;

        public PushMaskingIntent(MaskingInfo maskingInfo, bool overwritePreviousScissor)
        {
            MaskingInfo = maskingInfo;
            OverwritePreviousScissor = overwritePreviousScissor;
        }
    }
}
