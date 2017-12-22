using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Framework.Graphics.Visualisation.Tree.Nodes
{
    public abstract class TreeLeafNode : CompositeDrawable
    {
        private const float line_height = 12;

        protected readonly SpriteText Text;
        private readonly Box background;
        private readonly Box highlightBackground;

        public TreeLeafNode()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.X,
                Height = line_height,
                Padding = new MarginPadding { Left = 24 },
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Height = 0.8f,
                        Size = new Vector2(1, 0.8f),
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Colour = Color4.Transparent
                    },
                    highlightBackground = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Height = 0.8f,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Colour = Color4.Khaki.Opacity(0.4f),
                        Alpha = 0
                    },
                    Text = new SpriteText { TextSize = line_height }
                }
            };
        }

        private bool isHighlighted;
        public bool IsHighlighted
        {
            get { return isHighlighted; }
            set
            {
                if (isHighlighted)
                    return;
                isHighlighted = value;

                if (!IsLoaded)
                    return;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            UpdateHighlight();
        }

        protected override bool OnHover(InputState state)
        {
            background.Colour = Color4.PaleVioletRed.Opacity(0.7f);
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            background.Colour = Color4.Transparent;
            base.OnHoverLost(state);
        }

        protected override bool OnClick(InputState state) => true;

        protected override void Update()
        {
            base.Update();

            if (IsMaskedAway)
            {
                Text.Text = string.Empty;
                return;
            }

            UpdateDetails();
        }

        protected virtual void UpdateHighlight()
        {
            if (IsHighlighted)
                highlightBackground.FadeIn();
            else
                highlightBackground.FadeOut();
        }

        protected virtual void UpdateDetails()
        {
        }

        public static TreeLeafNode CreateNodeFor(Drawable drawable)
        {
            switch (drawable)
            {
                case SpriteText text:
                    return new TreeSpriteTextNode(text);
                case Sprite sprite:
                    return new TreeSpriteNode(sprite);
                case IFlowContainer flow:
                    return new TreeFlowContainerNode(flow);
                case CompositeDrawable composite:
                    return new TreeCompositeDrawableNode(composite);
                default:
                    return new TreeDrawableNode(drawable);
            }
        }
    }
}
