// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Rendering.Intents;
using osuTK;

namespace osu.Framework.Graphics.Rendering
{
    public interface IRenderer
    {
        void BeginFrame(Vector2 size);

        void FinishFrame();

        void Add<TIntent>(in TIntent intent) where TIntent : IIntent;

        void Flush();
    }
}
