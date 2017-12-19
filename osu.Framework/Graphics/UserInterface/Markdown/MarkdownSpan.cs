using System;
using System.Collections.Generic;
using System.Linq;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;

namespace osu.Framework.Graphics.UserInterface.Markdown
{
    public class MarkdownSpan : StyleStackTextFlowContainer
    {
        private readonly List<IStyler> stylers;

        public MarkdownSpan(Inline inline)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            stylers = CreateStylers().ToList();

            write(inline);
        }

        private void write(Inline inline)
        {
            if (inline == null)
                return;

            foreach (var styler in stylers)
                if (inline.GetType() == styler.ExpectedType)
                    styler.BeginDecoration(this, inline);

            switch (inline)
            {
                case ContainerInline container:
                    foreach (var elem in container)
                        write(elem);
                    break;
                case LiteralInline literal:
                    AddText(literal.ToString());
                    break;
                case LineBreakInline lineBreak:
                    NewLine();
                    break;
                case CodeInline code:
                    write(code);
                    break;
                default:
                    // Unstyled
                    AddText($"{{{inline.GetType()}}}");
                    break;
            }

            foreach (var styler in stylers)
                if (inline.GetType() == styler.ExpectedType)
                    styler.EndDecoration(this);
        }

        private void write(CodeInline codeInline)
        {
            // Todo: Stylize
            AddText(codeInline.Content);
        }

        protected virtual IEnumerable<IStyler> CreateStylers() => new IStyler[]
        {
            new LinkStyler(),
            new EmphasisStyler()
        };
    }

    public interface IStyler
    {
        Type ExpectedType { get; }

        void BeginDecoration(StyleStackTextFlowContainer textFlow, Inline spanElement);
        void EndDecoration(StyleStackTextFlowContainer textFlow);
    }

    public class LinkStyler : IStyler
    {
        public Type ExpectedType => typeof(LinkInline);

        private Stack<List<LinkText>> linkTexts = new Stack<List<LinkText>>();

        public void BeginDecoration(StyleStackTextFlowContainer textFlow, Inline spanElement)
        {
            linkTexts.Push(new List<LinkText>());
            var localList = linkTexts.Peek();

            textFlow.PushCreator(() =>
            {
                var text = new LinkText();
                localList.Add(text);
                return text;
            });

            textFlow.PushStyle(s => s.Colour = Color4.LightBlue);
        }

        public void EndDecoration(StyleStackTextFlowContainer textFlow)
        {
            var localList = linkTexts.Pop();

            localList.ForEach(link =>
            {
                link.Hovered = () => localList.ForEach(t => t.Colour = Color4.Pink);
                link.HoverLost = () => localList.ForEach(t => t.Colour = Color4.LightBlue);
            });

            textFlow.PopCreator();
            textFlow.PopStyle();
        }

        private class LinkText : SpriteText
        {
            public Action Hovered;
            public Action HoverLost;

            public override bool HandleInput => true;

            protected override bool OnHover(InputState state)
            {
                Hovered?.Invoke();
                return true;
            }

            protected override void OnHoverLost(InputState state)
            {
                HoverLost?.Invoke();
            }
        }
    }

    public class EmphasisStyler : IStyler
    {
        public Type ExpectedType => typeof(EmphasisInline);

        public void BeginDecoration(StyleStackTextFlowContainer textFlow, Inline spanElement)
        {
            textFlow.PushStyle(s => s.Colour = Color4.Red);
        }

        public void EndDecoration(StyleStackTextFlowContainer textFlow)
        {
            textFlow.PopStyle();
        }
    }
}
