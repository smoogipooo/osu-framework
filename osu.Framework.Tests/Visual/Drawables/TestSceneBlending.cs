// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;

namespace osu.Framework.Tests.Visual.Drawables
{
    public class TestSceneBlending : FrameworkTestScene
    {
        [Test]
        public void TestAdditiveSelf()
        {
            Drawable blended = null;

            AddStep("create test", () =>
            {
                Child = new Container
                {
                    Size = new Vector2(200),
                    Children = new[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Orange
                        },
                        blended = new Box
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(50),
                            Alpha = 0.5f,
                            Blending = BlendingParameters.Additive
                        }
                    }
                };
            });

            AddAssert("blended additively", () => blended.DrawColourInfo.Blending == BlendingParameters.Additive);
        }

        [Test]
        public void TestAdditiveThroughMultipleParents()
        {
            Drawable blended = null;

            AddStep("create test", () =>
            {
                Child = new Container
                {
                    Size = new Vector2(200),
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Orange
                        },
                        new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(50),
                            Blending = BlendingParameters.Additive,
                            Child = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Child = blended = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0.5f,
                                }
                            }
                        }
                    }
                };
            });

            AddAssert("blended additively", () => blended.DrawColourInfo.Blending == BlendingParameters.Additive);
        }

        [TestCase(TestBlendMode.Mixture)]
        [TestCase(TestBlendMode.Additive)]
        public void TestBufferedContainerEffect(TestBlendMode mode)
        {
            BlendingParameters parameters = mode == TestBlendMode.Mixture ? BlendingParameters.Mixture : BlendingParameters.Additive;

            BufferedContainer buffered = null;
            Drawable blended = null;

            AddStep("create test", () =>
            {
                Child = new Container
                {
                    Size = new Vector2(200),
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Yellow
                        },
                        new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(100),
                            Blending = parameters,
                            Child = buffered = new BufferedContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Children = new[]
                                {
                                    blended = new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Alpha = 0.5f,
                                        Colour = Color4.Cyan
                                    },
                                    new Box
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        RelativeSizeAxes = Axes.Both,
                                        Size = new Vector2(0.5f),
                                        Alpha = 0.5f,
                                        Colour = Color4.Magenta
                                    },
                                }
                            }
                        }
                    }
                };
            });

            AddAssert($"contents blended using {mode.ToString()}", () => blended.DrawColourInfo.Blending == parameters);
            AddAssert($"effect blended using: {mode.ToString()}", () => buffered.DrawEffectBlending == parameters);
        }

        [TestCase(TestBlendMode.Mixture)]
        [TestCase(TestBlendMode.Additive)]
        public void TestBufferedContainerOriginal(TestBlendMode mode)
        {
            BlendingParameters parameters = mode == TestBlendMode.Mixture ? BlendingParameters.Mixture : BlendingParameters.Additive;

            Drawable blended = null;

            AddStep("create test", () =>
            {
                Child = new Container
                {
                    Size = new Vector2(200),
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Yellow
                        },
                        new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(100),
                            Blending = parameters,
                            Child = new BufferedContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                EffectColour = Color4.Transparent, // The effect is always drawn, but we only want to see the drawn original's colour
                                DrawOriginal = true,
                                Children = new[]
                                {
                                    blended = new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Alpha = 0.5f,
                                        Colour = Color4.Cyan
                                    },
                                    new Box
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        RelativeSizeAxes = Axes.Both,
                                        Size = new Vector2(0.5f),
                                        Alpha = 0.5f,
                                        Colour = Color4.Magenta
                                    },
                                }
                            }
                        }
                    }
                };
            });

            AddAssert($"contents blended using {mode.ToString()}", () => blended.DrawColourInfo.Blending == parameters);
        }

        [Test]
        public void TestBufferedContainerAdditiveEffectMixedOriginal()
        {
            AddStep("create test", () =>
            {
                Child = new Container
                {
                    Size = new Vector2(200),
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Red
                        },
                        new BufferedContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            EffectBlending = BlendingParameters.Additive,
                            BlurSigma = new Vector2(10),
                            DrawOriginal = true,
                            Children = new[]
                            {
                                new Box
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Size = new Vector2(0.5f),
                                    Alpha = 0.4f,
                                    Colour = Color4.Lime
                                },
                            }
                        }
                    }
                };
            });
        }

        public enum TestBlendMode
        {
            Mixture,
            Additive
        }
    }
}
