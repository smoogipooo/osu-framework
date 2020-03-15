// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Platform;

namespace osu.Framework.Graphics.Rendering
{
    /// <summary>
    /// An <see cref="IRenderer"/> that immediately flushes intents.
    /// </summary>
    public class ImmediateOpenGLRenderer : OpenGLRenderer
    {
        public ImmediateOpenGLRenderer(IWindow window)
            : base(window)
        {
        }

        public override void Add<TIntent>(in TIntent intent)
        {
            base.Add(in intent);
            Flush();
        }
    }
}
