// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace osu.Framework.Graphics.UserInterface.Markdown.Renderers.Blocks
{
    public class ParagraphRenderer : ObjectRenderer<ParagraphBlock>
    {
        protected override void Write(MarkdownRenderer renderer, ParagraphBlock obj)
        {
            renderer.AddParagraph();

            var inline = (Inline)obj.Inline;
            while (inline != null)
            {
                renderer.Write(inline);
                inline = inline.NextSibling;
            }
        }
    }
}
