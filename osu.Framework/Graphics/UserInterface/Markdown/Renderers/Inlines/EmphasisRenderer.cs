// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using Markdig.Syntax.Inlines;
using osu.Framework.Graphics.Sprites;

namespace osu.Framework.Graphics.UserInterface.Markdown.Renderers.Inlines
{
    public class EmphasisRenderer : ObjectRenderer<EmphasisInline>
    {
        protected char Delimiter { get; private set; }

        protected override void Write(MarkdownRenderer renderer, EmphasisInline obj)
        {
            Delimiter = obj.DelimiterChar;

            renderer.PushFormatting(Format);
            renderer.WriteChildren(obj);
            renderer.PopFormatting();
        }

        protected virtual void Format(SpriteText text) { }
    }
}
