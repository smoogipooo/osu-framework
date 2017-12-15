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
using osu.Framework.Graphics.Sprites;

namespace osu.Framework.Tests.Visual
{
    public class TestCaseMarkdownRenderer : TestCase
    {
        private readonly DisplayBox markdownBox;
        private readonly DisplayBox userInputBox;

        public TestCaseMarkdownRenderer()
        {
            Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(10),
                Children = new Drawable[]
                {
                    userInputBox = new UserTextBox
                    {
                        RelativeSizeAxes = Axes.Both,
                        Width = 0.45f
                    },
                    markdownBox = new MarkdownBox
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        RelativeSizeAxes = Axes.Both,
                        Width = 0.45f
                    }
                }
            };
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
                var str = Encoding.UTF8.GetString(bytes);

                userInputBox.Text = str;
                markdownBox.Text = str;
            });
        }

        private abstract class DisplayBox : Container
        {
            public abstract string Text { set; }
            protected abstract string Description { get; }

            private ScrollContainer content;
            protected override Container<Drawable> Content => content;

            public DisplayBox()
            {
                InternalChildren = new[]
                {
                    new Container
                    {
                        Name = "Header",
                        RelativeSizeAxes = Axes.X,
                        Height = 20,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Colour = Color4.Gold,
                                RelativeSizeAxes = Axes.Both,
                            },
                            new SpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = Description,
                                Colour = Color4.Black
                            }
                        }
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Top = 20 },
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Name = "Background",
                                RelativeSizeAxes = Axes.Both,
                                Colour = new Color4(50, 50, 50, 255)
                            },
                            content = new ScrollContainer { RelativeSizeAxes = Axes.Both }
                        }
                    }
                };
            }
        }

        private class MarkdownBox : DisplayBox
        {
            protected override string Description => "Markdown";

            private readonly MarkdownTextContainer markdownContainer;

            public MarkdownBox()
            {
                Add(markdownContainer = new MarkdownTextContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                });
            }

            public override string Text { set => markdownContainer.Text = value; }
        }

        private class UserTextBox : DisplayBox
        {
            protected override string Description => "Raw";

            private readonly TextFlowContainer textContainer;

            public UserTextBox()
            {
                Add(textContainer = new TextFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                });
            }

            public override string Text { set => textContainer.Text = value; }
        }
    }
}
