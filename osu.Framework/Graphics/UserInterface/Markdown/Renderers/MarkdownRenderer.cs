// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using Markdig.Renderers;
using Markdig.Syntax;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface.Markdown.Renderers.Inlines;

namespace osu.Framework.Graphics.UserInterface.Markdown.Renderers
{
    public class MarkdownRenderer : RendererBase
    {
        protected virtual float IndentationAmount { get; } = 25;

        public MarkdownRenderer()
        {
            ObjectRenderers.Add(new CodeRenderer());
            ObjectRenderers.Add(new DelimiterRenderer());
            ObjectRenderers.Add(new Inlines.HtmlRenderer());
            ObjectRenderers.Add(new LineBreakRenderer());
            ObjectRenderers.Add(new LinkRenderer());
            ObjectRenderers.Add(new LiteralRenderer());

            ObjectRenderers.Add(new Blocks.ParagraphRenderer());
            ObjectRenderers.Add(new Blocks.CodeBlockRenderer());
            ObjectRenderers.Add(new Blocks.HeadingRenderer());
            ObjectRenderers.Add(new Blocks.ListRenderer());
            ObjectRenderers.Add(new Tables.TableRenderer());
        }

        private CustomizableTextContainer document;

        public override object Render(MarkdownObject markdownObject)
        {
            document = CreateDocument();

            Write(markdownObject);
            return document;
        }

        public void EnsureNewLine()
        {
            if (document.CurrentLineLength > 0)
                document.NewLine();
        }

        public void EnsureNewParagraph()
        {
            if (document.CurrentLineLength > 0)
                AddParagraph();
        }

        public void AddParagraph() => document.AddText("\n\n");

        private int currentIndentationLevel = 0;
        public void PushIndentation()
        {
            currentIndentationLevel++;
            document.SetCurrentIndentation(currentIndentationLevel * IndentationAmount);
        }

        public void PopIndentation()
        {
            currentIndentationLevel--;
            document.SetCurrentIndentation(currentIndentationLevel * IndentationAmount);
        }

        public void Write(string text) => document.AddText(text);

        private int currentPlaceholderIndex;
        public void Write(Drawable drawable)
        {
            document.AddPlaceholder(drawable);
            Write($"[{currentPlaceholderIndex}]");
            currentPlaceholderIndex++;
        }

        private readonly List<Action<SpriteText>> textFormatters = new List<Action<SpriteText>>();

        public void PushFormatting(Action<SpriteText> formatter) => textFormatters.Add(formatter);

        public void PopFormatting() => textFormatters.RemoveAt(textFormatters.Count - 1);

        private void applyTextFormats(SpriteText text) => textFormatters.ForEach(f => f.Invoke(text));

        public CustomizableTextContainer CreateDocument() => new CustomizableTextContainer(applyTextFormats);
    }
}
