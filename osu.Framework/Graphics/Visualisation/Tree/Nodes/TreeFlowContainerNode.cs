using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Framework.Graphics.Visualisation.Tree.Nodes
{
    public class TreeFlowContainerNode : TreeCompositeDrawableNode
    {
        private readonly Drawable layoutMarker;

        private readonly IFlowContainer target;

        public TreeFlowContainerNode(IFlowContainer target)
            : base((CompositeDrawable)target)
        {
            this.target = target;

            AddInternal(layoutMarker = new Box
            {
                RelativeSizeAxes = Axes.Y,
                Width = 2,
                Colour = Color4.Orange,
                Position = new Vector2(3, 0),
                Alpha = 0
            });
        }

        protected override void AttachEvents()
        {
            base.AttachEvents();

            target.OnLayout += onLayout;
        }

        protected override void DetachEvents()
        {
            base.DetachEvents();

            target.OnLayout -= onLayout;
        }

        private void onLayout()
        {
            Scheduler.Add(() => layoutMarker.FadeOutFromOne(1));
        }
    }
}
