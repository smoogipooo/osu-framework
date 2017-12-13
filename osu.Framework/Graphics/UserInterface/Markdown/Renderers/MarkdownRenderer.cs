// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using Markdig.Renderers;
using Markdig.Syntax;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface.Markdown.Renderers.Inlines;

namespace osu.Framework.Graphics.UserInterface.Markdown.Renderers
{
    public class MarkdownRenderer : RendererBase
    {
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
        }

        private Container<FillFlowContainer> documentContainer;
        private FillFlowContainer currentLine;

        public override object Render(MarkdownObject markdownObject)
        {
            documentContainer = CreateDocumentContainer();

            Write(markdownObject);
            return documentContainer;
        }

        public void EnsureNewLine()
        {
            if (currentLine == null || currentLine.Count > 0)
                documentContainer.Add(currentLine = CreateLine());
        }

        public void EnsureNewParagraph()
        {
            EnsureNewLine();

            // The first line is its own paragraph
            if (documentContainer.Count == 1)
                return;

            var lineIndex = documentContainer.IndexOf(currentLine) - 1;
            if (documentContainer[lineIndex].Count > 0)
                addNewLine();
        }

        private void addNewLine() => documentContainer.Add(currentLine = CreateLine());

        public FillFlowContainer GetLine()
        {
            if (currentLine == null)
                EnsureNewLine();
            return currentLine;
        }

        public TextFlowContainer GetTextFlow()
        {
            if (currentLine == null)
                EnsureNewLine();

            var textFlow = currentLine.Children.LastOrDefault();
            if (!(textFlow is TextFlowContainer))
                currentLine.Add(textFlow = CreateTextFlow());
            return (TextFlowContainer)textFlow;
        }

        private readonly List<Action<SpriteText>> textFormatters = new List<Action<SpriteText>>();
        public void PushFormatting(Action<SpriteText> formatter) => textFormatters.Add(formatter);
        public void PopFormatting() => textFormatters.RemoveAt(textFormatters.Count - 1);
        private void applyTextFormats(SpriteText text) => textFormatters.ForEach(f => f.Invoke(text));

        public FillFlowContainer<FillFlowContainer> CreateDocumentContainer() => new FillFlowContainer<FillFlowContainer>
        {
            RelativeSizeAxes = Axes.Both,
            Direction = FillDirection.Vertical
        };

        public FillFlowContainer CreateLine() => new FillFlowContainer { AutoSizeAxes = Axes.Both };
        public TextFlowContainer CreateTextFlow() => new TextFlowContainer(applyTextFormats) { AutoSizeAxes = Axes.Both };
    }
}
