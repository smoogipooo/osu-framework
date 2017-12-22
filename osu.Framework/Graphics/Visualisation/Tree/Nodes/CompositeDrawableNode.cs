using System.Collections.Generic;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Framework.Graphics.Visualisation.Tree.Nodes
{
    public class CompositeDrawableNode : DrawableNode
    {
        private readonly FillFlowContainer<LeafNode> flow;
        private readonly Drawable autoSizeMarker;

        private readonly CompositeDrawable target;

        public CompositeDrawableNode(CompositeDrawable target)
            : base(target)
        {
            this.target = target;

            AddRangeInternal(new[]
            {
                autoSizeMarker = new Box
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 2,
                    Colour = Color4.Red,
                    Position = new Vector2(0, 0),
                    Alpha = 0
                },
                flow = new FillFlowContainer<LeafNode>
                {
                    Direction = FillDirection.Vertical,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Position = new Vector2(10, 14)
                },
            });

            target.AliveInternalChildren.ForEach(addChild);
        }

        protected override void AttachEvents()
        {
            base.AttachEvents();

            target.OnAutoSize += onAutoSize;
            target.ChildBecameAlive += addChild;
            target.ChildDied += removeChild;
        }

        protected override void DetachEvents()
        {
            base.DetachEvents();

            target.OnAutoSize -= onAutoSize;
            target.ChildBecameAlive -= addChild;
            target.ChildDied -= removeChild;
        }

        private readonly Dictionary<Drawable, LeafNode> visCache = new Dictionary<Drawable, LeafNode>();
        private void addChild(Drawable drawable)
        {
            // Make sure to never add the DrawVisualiser (recursive scenario)
            if (drawable is DrawVisualiser) return;

            LeafNode vis;
            if (!visCache.TryGetValue(drawable, out vis))
                visCache[drawable] = vis = CreateNodeFor(drawable);
            flow.Add(vis);
        }

        private void removeChild(Drawable drawable)
        {
            if (!visCache.ContainsKey(drawable))
                return;
            flow.Remove(visCache[drawable]);
        }

        private void onAutoSize()
        {
            Scheduler.Add(() => autoSizeMarker.FadeOutFromOne(1));
        }

        protected override bool OnClick(InputState state)
        {
            if (isExpanded)
                Collapse();
            else
                Expand();

            return true;
        }

        private bool isExpanded = true;
        public void Expand()
        {
            flow.FadeIn();
            isExpanded = true;
        }

        public void Collapse()
        {
            flow.FadeOut();
            isExpanded = false;
        }

        protected override void UpdateHighlight()
        {
            if (IsHighlighted)
                Expand();
        }

        protected override void UpdateDetails()
        {
            base.UpdateDetails();

            int childCount = target.InternalChildren.Count;

            Text.Text += !isExpanded && childCount > 0 ? $@" ({childCount} children)" : string.Empty;
            Text.Colour = !isExpanded && childCount > 0 ? Color4.LightBlue : Color4.White;
        }
    }
}
