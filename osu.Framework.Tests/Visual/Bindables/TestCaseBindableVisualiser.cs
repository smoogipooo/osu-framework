// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Visualisation;
using osu.Framework.Platform;
using osu.Framework.Testing;

namespace osu.Framework.Tests.Visual.Bindables
{
    public class TestCaseBindableVisualiser : TestCase
    {
        [BackgroundDependencyLoader]
        private void load(Game game)
        {
            Children = new Drawable[]
            {
                new BindableVisualiser(game)
                {
                    RelativeSizeAxes = Axes.Both,
                    State = Visibility.Visible
                },
            };
        }

        private class BindableClass : Drawable
        {
            public readonly Bindable<int> PublicBindable = new Bindable<int>();
            private readonly Bindable<int> privateBindable = new Bindable<int>();
        }
    }
}
