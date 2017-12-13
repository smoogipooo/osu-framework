// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using Markdig.Syntax;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace osu.Framework.Graphics.UserInterface.Markdown.Renderers.Blocks
{
    public class ListRenderer : ObjectRenderer<ListBlock>
    {
        protected override void Write(MarkdownRenderer renderer, ListBlock obj)
        {
            renderer.EnsureNewParagraph();
            renderer.PushIndentation();

            for (int i = 0; i < obj.Count; i++)
            {
                renderer.EnsureNewLine();
                renderer.Write(obj.IsOrdered ? CreateNumber(i + 1) : CreateBullet(obj.BulletType));
                renderer.Write(" ");
                renderer.Write(renderItem((ListItemBlock)obj[i]));
            }

            renderer.EnsureNewParagraph();
            renderer.PopIndentation();
        }

        private Drawable renderItem(ListItemBlock item)
        {
            var renderer = new MarkdownRenderer();
            var document = (FillFlowContainer)renderer.Render(null);
            document.AutoSizeAxes = Axes.Both;

            renderer.WriteChildren(item);

            return document;
        }

        protected Drawable CreateBullet(char bulletChar) => new SpriteText { Text = bulletChar.ToString() };
        protected Drawable CreateNumber(int value) => new SpriteText { Text = value.ToString() };
    }
}
