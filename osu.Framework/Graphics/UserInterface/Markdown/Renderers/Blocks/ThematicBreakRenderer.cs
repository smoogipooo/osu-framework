// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using Markdig.Syntax;
using osu.Framework.Graphics.Shapes;

namespace osu.Framework.Graphics.UserInterface.Markdown.Renderers.Blocks
{
    public class ThematicBreakRenderer : ObjectRenderer<ThematicBreakBlock>
    {
        protected override void Write(MarkdownRenderer renderer, ThematicBreakBlock obj)
        {
            renderer.EnsureNewParagraph();
            renderer.Write(CreateBreak());
            renderer.EnsureNewParagraph();
        }

        protected virtual Drawable CreateBreak() => new Box { RelativeSizeAxes = Axes.X, Height = 1};
    }
}
