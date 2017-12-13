// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using Markdig.Syntax;
using OpenTK;

namespace osu.Framework.Graphics.UserInterface.Markdown.Renderers.Blocks
{
    public class HeadingRenderer : ObjectRenderer<HeadingBlock>
    {
        protected virtual float[] HeadingSizes => new float[]
        {
            34,
            30,
            26,
            22,
            18,
            14
        };

        protected override void Write(MarkdownRenderer renderer, HeadingBlock obj)
        {
            renderer.EnsureNewParagraph();

            int index = MathHelper.Clamp(obj.Level - 1, 0, HeadingSizes.Length - 1);

            renderer.PushFormatting(s => s.TextSize = HeadingSizes[index]);
            renderer.Write(obj.Inline);
            renderer.PopFormatting();
        }
    }
}
