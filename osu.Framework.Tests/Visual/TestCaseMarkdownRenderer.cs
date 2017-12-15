// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface.Markdown;
using osu.Framework.Testing;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.IO.Stores;
using System.Text;

namespace osu.Framework.Tests.Visual
{
    public class TestCaseMarkdownRenderer : TestCase
    {
        private readonly MarkdownTextContainer markdownContainer;

        public TestCaseMarkdownRenderer()
        {
            Add(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = new Color4(50, 50, 50, 255)
            });

            Add(new ScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = markdownContainer = new MarkdownTextContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                }
            });
        }

        private IResourceStore<byte[]> articleStore;

        [BackgroundDependencyLoader]
        private void load(Game game)
        {
            articleStore = new NamespacedResourceStore<byte[]>(game.Resources, "Articles");

            addFileStep("banchobot");
            addFileStep("bestof2016");
            addFileStep("chatconsole");
            addFileStep("modding");
            addFileStep("news1");
            addFileStep("news2");
            addFileStep("news3");
            addFileStep("news4");
            addFileStep("tourneymultiplayer");
            addFileStep("tourneyskinning");
        }

        private void addFileStep(string name)
        {
            AddStep(name, () =>
            {
                var bytes = articleStore.Get($"{name}.md");
                markdownContainer.Text = Encoding.UTF8.GetString(bytes);
            });
        }
    }
}
