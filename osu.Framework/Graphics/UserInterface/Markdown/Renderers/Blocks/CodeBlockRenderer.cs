// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using Markdig.Syntax;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using OpenTK.Graphics;

namespace osu.Framework.Graphics.UserInterface.Markdown.Renderers.Blocks
{
    public class CodeBlockRenderer : ObjectRenderer<CodeBlock>
    {
        protected override void Write(MarkdownRenderer renderer, CodeBlock obj)
        {
            renderer.EnsureNewLine();

            var container = CreateCodeContainer();
            var textFlow = renderer.CreateTextFlow();

            textFlow.Text = obj.Lines.ToString();

            container.Add(textFlow);
            renderer.GetLine().Add(container);
        }

        protected virtual Container CreateCodeContainer() => new CodeContainer();

        private class CodeContainer : Container
        {
            private readonly Container content;
            protected override Container<Drawable> Content => content;

            public CodeContainer()
            {
                AutoSizeAxes = Axes.Both;
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
