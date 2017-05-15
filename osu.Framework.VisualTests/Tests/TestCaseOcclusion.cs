// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;

namespace osu.Framework.VisualTests.Tests
{
    internal class TestCaseOcclusion : TestCase
    {
        public override void Reset()
        {
            base.Reset();

            Box movingBox;
            Children = new []
            {
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        movingBox = new Box
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(100),
                            X = 500,
                            Colour = Color4.Green,
                        },
                        new OccludingContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        }
                    }
                }
            };

            movingBox.MoveToX(-500, 4000);
        }

        private class OccludingContainer : Container, IOccluder
        {
            public OccludingContainer()
            {
                Size = new Vector2(400);

                Children = new[]
                {
                    new Box
                    {
                        Name = "Should never be occluded",
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0.5f
                    },
                    new Box
                    {
                        Name = "Should never be occluded",
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(50),
                        Colour = Color4.Red,
                        Alpha = 0.5f
                    }
                };
            }
        }
    }
}
