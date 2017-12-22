// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Visualisation;
using osu.Framework.Input;
using osu.Framework.Testing;
using OpenTK;
using System.Collections.Generic;
using System;
using osu.Framework.Threading;

namespace osu.Framework.Tests.Visual
{
    public class TestCaseDrawVisualiser : TestCase
    {
        private readonly Random rng;
        private readonly DrawVisualiser vis;

        public TestCaseDrawVisualiser()
        {
            rng = new Random(1337);

            FillFlowContainer contentContainer;

            AddRange(new Drawable[]
            {
                new ScrollContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Child = contentContainer = new FillFlowContainer { RelativeSizeAxes = Axes.X, AutoSizeAxes = Axes.Y, Spacing = new Vector2(4) }
                },
                vis = new DrawVisualiser { State = Visibility.Visible }
            });

            for (int i = 0; i < 256; i++)
                contentContainer.Add(createHierarchy());
        }

        private Drawable createHierarchy()
        {
            switch (rng.Next(2))
            {
                // Leaf
                default:
                case 0:
                    return new InvalidatingBox { Size = new Vector2(50) };
                // Relative-size container
                case 1:
                    return new Container { AutoSizeAxes = Axes.Both, Child = createHierarchy() };
            }
        }

        private class InvalidatingBox : Box
        {
            protected override void Update()
            {
                base.Update();
                Invalidate();
            }
        }
    }
}
