// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using Microsoft.VisualStudio.TestPlatform.Common;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.MathUtils;
using osu.Framework.Testing;
using osuTK;
using osuTK.Graphics;

namespace osu.Framework.Tests.Visual.Drawables
{
    public class TestSceneModelBackedDrawable : TestScene
    {
        private TestModelBackedDrawable backedDrawable;

        private void createModelBackedDrawable(bool withPlaceholder, bool fadeOutImmediately) =>
            Child = backedDrawable = new TestModelBackedDrawable
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(200),
                InternalTransformImmediately = fadeOutImmediately,
                HasPlaceholder = withPlaceholder
            };

        [Test]
        public void TestEmptyDefaultState()
        {
            AddStep("setup", () => createModelBackedDrawable(false, false));
            AddAssert("nothing shown", () => backedDrawable.DisplayedDrawable == null);
        }

        [Test]
        public void TestModelDefaultState()
        {
            TestDrawableModel drawableModel = null;

            AddStep("setup", () =>
            {
                createModelBackedDrawable(false, false);
                backedDrawable.Model = new TestModel(drawableModel = new TestDrawableModel(1).With(d => d.AllowLoad.Set()));
            });

            assertDrawableVisibility(1, () => drawableModel);
        }

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void TestChangeModel(bool intermediatePlaceholder, bool fadeOutImmediately)
        {
            TestDrawableModel firstModel = null;
            TestDrawableModel secondModel = null;

            AddStep("setup", () =>
            {
                createModelBackedDrawable(intermediatePlaceholder, intermediatePlaceholder);
                backedDrawable.Model = new TestModel(firstModel = new TestDrawableModel(1).With(d => d.AllowLoad.Set()));
            });

            AddStep("set second model", () => backedDrawable.Model = new TestModel(secondModel = new TestDrawableModel(2)));
            assertIntermediateVisibility(() => firstModel);

            AddStep("allow second model to load", () => secondModel.AllowLoad.Set());
            assertDrawableVisibility(2, () => secondModel);
        }

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void TestChangeModelDuringLoad(bool intermediatePlaceholder, bool fadeOutImmediately)
        {
            TestDrawableModel firstModel = null;
            TestDrawableModel secondModel = null;
            TestDrawableModel thirdModel = null;

            AddStep("setup", () =>
            {
                createModelBackedDrawable(intermediatePlaceholder, intermediatePlaceholder);
                backedDrawable.Model = new TestModel(firstModel = new TestDrawableModel(1).With(d => d.AllowLoad.Set()));
            });

            AddStep("set second model", () => backedDrawable.Model = new TestModel(secondModel = new TestDrawableModel(2)));
            assertIntermediateVisibility(() => firstModel);

            Drawable firstIntermediate = null;
            AddStep("capture intermediate drawable", () => firstIntermediate = backedDrawable.DisplayedDrawable);

            AddStep("set third model", () => backedDrawable.Model = new TestModel(thirdModel = new TestDrawableModel(3)));
            assertIntermediateVisibility(() => firstModel);

            if (fadeOutImmediately && intermediatePlaceholder)
                AddAssert("intermediate drawable hasn't changed", () => backedDrawable.DisplayedDrawable == firstIntermediate);

            AddStep("allow second model to load", () => secondModel.AllowLoad.Set());
            assertIntermediateVisibility(() => firstModel);

            AddStep("allow third model to load", () => thirdModel.AllowLoad.Set());
            assertDrawableVisibility(3, () => thirdModel);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestOutOfOrderLoad(bool intermediatePlaceholder)
        {
            TestDrawableModel firstModel = null;
            TestDrawableModel secondModel = null;

            AddStep("setup", () =>
            {
                createModelBackedDrawable(intermediatePlaceholder, intermediatePlaceholder);
                backedDrawable.Model = new TestModel(firstModel = new TestDrawableModel(1));
            });

            AddStep("set second model", () => backedDrawable.Model = new TestModel(secondModel = new TestDrawableModel(2)));
            assertIntermediateVisibility(() => null);

            AddStep("allow second model to load", () => secondModel.AllowLoad.Set());
            assertDrawableVisibility(2, () => secondModel);

            AddStep("allow first model to load", () => firstModel.AllowLoad.Set());
            assertDrawableVisibility(2, () => secondModel);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestSetNullModel(bool withPlaceholder)
        {
            TestDrawableModel drawableModel = null;

            AddStep("setup", () =>
            {
                createModelBackedDrawable(withPlaceholder, false);
                backedDrawable.Model = new TestModel(drawableModel = new TestDrawableModel(1).With(d => d.AllowLoad.Set()));
            });

            assertDrawableVisibility(1, () => drawableModel);

            AddStep("set null model", () => backedDrawable.Model = null);
        }

        private void assertIntermediateVisibility(Func<Drawable> getLastFunc)
        {
            if (backedDrawable.InternalTransformImmediately)
            {
                if (backedDrawable.HasPlaceholder)
                    AddAssert("intermediate drawable visible", () => backedDrawable.DisplayedDrawable is TestIntermediateDrawable);
                else
                    AddAssert("no drawable visible", () => backedDrawable.DisplayedDrawable == null);
            }
            else
                AddAssert("last drawable visible", () => backedDrawable.DisplayedDrawable == getLastFunc());
        }

        private void assertDrawableVisibility(int id, Func<Drawable> getFunc)
        {
            AddAssert($"model {id} visible", () => backedDrawable.DisplayedDrawable == getFunc());
        }

        private class TestModel
        {
            public readonly TestDrawableModel DrawableModel;

            public TestModel(TestDrawableModel drawableModel)
            {
                DrawableModel = drawableModel;
            }
        }

        private class TestDrawableModel : CompositeDrawable
        {
            public readonly ManualResetEventSlim AllowLoad = new ManualResetEventSlim(false);

            protected virtual Color4 BackgroundColour => Color4.SkyBlue;

            public TestDrawableModel(int id)
                : this($"Model {id}")
            {
            }

            protected TestDrawableModel(string text)
            {
                RelativeSizeAxes = Axes.Both;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = BackgroundColour
                    },
                    new SpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = text
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                if (!AllowLoad.Wait(TimeSpan.FromSeconds(10)))
                {
                }
            }
        }

        private class TestIntermediateDrawable : TestDrawableModel
        {
            public TestIntermediateDrawable()
                : base("Intermediate")
            {
                AllowLoad.Set();
            }
        }

        private class TestModelBackedDrawable : ModelBackedDrawable<TestModel>
        {
            public bool HasPlaceholder;

            protected override Drawable CreateDrawable(TestModel model)
            {
                if (model == null)
                    return HasPlaceholder ? new TestIntermediateDrawable() : null;

                return model.DrawableModel;
            }

            public new Drawable DisplayedDrawable => base.DisplayedDrawable?.Parent;

            public new TestModel Model
            {
                get => base.Model;
                set => base.Model = value;
            }

            public bool InternalTransformImmediately;

            protected override bool TransformImmediately => InternalTransformImmediately;
        }
    }
}
