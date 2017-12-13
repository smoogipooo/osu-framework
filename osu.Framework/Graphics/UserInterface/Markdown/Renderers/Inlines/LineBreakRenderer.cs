﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using Markdig.Syntax.Inlines;

namespace osu.Framework.Graphics.UserInterface.Markdown.Renderers.Inlines
{
    public class LineBreakRenderer : ObjectRenderer<LineBreakInline>
    {
        protected override void Write(MarkdownRenderer renderer, LineBreakInline obj)
        {
            if (obj.IsHard)
                renderer.AddParagraph();
            else
                renderer.EnsureNewLine();
        }
    }
}
