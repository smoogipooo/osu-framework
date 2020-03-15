// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Framework.Graphics.Rendering.Intents
{
    public readonly struct SetBlendIntent : IIntent
    {
        public readonly BlendingParameters Parameters;

        public SetBlendIntent(BlendingParameters parameters)
        {
            Parameters = parameters;
        }
    }
}
