// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using Markdig.Syntax.Inlines;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using OpenTK.Graphics;

namespace osu.Framework.Graphics.UserInterface.Markdown.Renderers.Inlines
{
    public class CodeRenderer : ObjectRenderer<CodeInline>
    {
        protected override void Write(MarkdownRenderer renderer, CodeInline obj)
        {
            var container = CreateCodeContainer();
            container.Add(new TextFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Text = obj.Content
            });

            renderer.Write(container);
        }

        protected virtual Container CreateCodeContainer() => new CodeContainer();

        private class CodeContainer : Container
        {
            private readonly Container content;
            protected override Container<Drawable> Content => content;

            public CodeContainer()
            {
                AutoSizeAxes = Axes.Both;

                Masking = true;
                BorderColour = Color4.Black;
                BorderThickness = 2;
                MaskingSmoothness = 1;
                CornerRadius = 5;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = new Color4(25, 25, 25, 255)
                    },
                    content = new Container
                    {
                        AutoSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Left = 4, Right = 4 }
                    }
                };
            }
        }
    }
}
