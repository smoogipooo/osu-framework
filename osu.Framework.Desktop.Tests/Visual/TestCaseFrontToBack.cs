// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Framework.Desktop.Tests.Visual
{
    internal class TestCaseFrontToBack : TestCase
    {
        private FrontToBackContainer backgroundContainer;

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(backgroundContainer = new FrontToBackContainer { RelativeSizeAxes = Axes.Both });

            var buttonFlow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
            };

            for (int i = 25; i <= 500; i += 25)
            {
                var c = i;
                buttonFlow.Add(new Button
                {
                    Size = new Vector2(100, 50),
                    Text = $"{c}x fill",
                    BackgroundColour = new Color4(0.1f, 0.1f, 0.1f, 1),
                    Action = () =>
                    {
                        backgroundContainer.Clear();
                        for (int j = 0; j < c; j++)
                        {
                            backgroundContainer.Add(new Box
                            {
                                EdgeSmoothness = new Vector2(2),
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.White.Multiply((j + 1f) / c),
                                Height = (j + 1f) / c,
                                Depth = j
                            });
                        }
                    }
                });
            }

            Add(buttonFlow);
        }

        private class FrontToBackContainer : Container
        {
            protected override bool CanBeFlattened => false;
        }
    }
}
