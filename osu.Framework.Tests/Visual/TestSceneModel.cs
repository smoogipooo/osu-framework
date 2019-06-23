// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Modelling;
using osu.Framework.Graphics.Modelling.Shapes;
using osuTK;

namespace osu.Framework.Tests.Visual
{
    public class TestSceneModel : FrameworkTestScene
    {
        private readonly Cube cube;

        public TestSceneModel()
        {
            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(500),
                Child = new ModelContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new Cube()
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            cube.Spin(5000, ModelAxes.All, RotationDirection.Clockwise);
        }
    }
}
