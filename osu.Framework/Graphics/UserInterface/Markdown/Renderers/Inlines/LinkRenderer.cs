// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using Markdig.Syntax.Inlines;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Framework.Graphics.UserInterface.Markdown.Renderers.Inlines
{
    public class LinkRenderer : ObjectRenderer<LinkInline>
    {
        protected MarkdownRenderer Renderer { get; private set; }

        protected override void Write(MarkdownRenderer renderer, LinkInline obj)
        {
            Renderer = renderer;

            if (obj.IsImage)
                renderer.GetLine().Add(new DelayedLoadWrapper(new AsyncSprite(obj.GetDynamicUrl?.Invoke() ?? obj.Url)));
            else
            {
                // Todo: This shouldn't be a spritetext
                renderer.GetLine().Add(CreateLink(obj.GetDynamicUrl?.Invoke() ?? obj.Url, obj.Title));
            }
        }

        protected virtual Drawable CreateLink(string url, string title) => new SpriteText { Text = title };

        private class AsyncSprite : Sprite
        {
            private readonly string url;

            public AsyncSprite(string url)
            {
                this.url = url;
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                Texture = textures.Get(url);
            }
        }
    }
}
