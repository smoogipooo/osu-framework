using System;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using OpenTK;

namespace osu.Framework.Graphics.Visualisation.Tree.Nodes
{
    public class SpriteNode : DrawableNode
    {
        private const float text_padding = 4;

        private readonly Sprite previewBox;

        private readonly Sprite target;

        public SpriteNode(Sprite target)
            : base(target)
        {
            this.target = target;

            AddInternal(previewBox = new Sprite
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Size = new Vector2(CONTENT_HEIGHT),
                X = CONTENT_PADDING,
            });

            Text.X = LINE_HEIGHT + text_padding;
        }

        protected override void UpdateDetails()
        {
            base.UpdateDetails();

            previewBox.Texture = target.Texture ?? Texture.WhitePixel;
            previewBox.Scale = new Vector2(previewBox.Texture.DisplayWidth / previewBox.Texture.DisplayHeight, 1);
            previewBox.Alpha = Math.Max(0.2f, target.Alpha);
            previewBox.Colour = target.Colour;
        }
    }
}
