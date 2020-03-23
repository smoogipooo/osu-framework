// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osuTK;

namespace osu.Framework.Tests.Visual
{
    public class TestSceneScratch : FrameworkTestScene
    {
        public TestSceneScratch()
        {
            Add(new TriangleContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(50),
                Masking = true,
                Child = new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(200),
                }
            });
        }

        private class TriangleContainer : Container
        {
            protected override Quad MaskingQuad
            {
                get
                {
                    var baseQuad = base.MaskingQuad;
                    return new Quad((baseQuad.TopLeft + baseQuad.TopRight) / 2, (baseQuad.TopLeft + baseQuad.TopRight) / 2, baseQuad.BottomLeft, baseQuad.BottomRight);
                }
            }
        }
    }
}
