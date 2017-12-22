using System;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using OpenTK;

namespace osu.Framework.Graphics.Visualisation.Tree.Nodes
{
    public class TreeSpriteNode : TreeDrawableNode
    {
        private readonly Sprite previewBox;

        private readonly Sprite target;

        public TreeSpriteNode(Sprite target)
            : base(target)
        {
            this.target = target;

            AddInternal(previewBox = new Sprite
            {
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fit,
                Position = new Vector2(9, 0)
            });
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
