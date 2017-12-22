using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;

namespace osu.Framework.Graphics.Visualisation
{
    public class TreeDrawableNode : TreeLeafNode
    {
        private readonly Drawable invalidationMarker;

        public readonly Drawable Target;

        public TreeDrawableNode(Drawable target)
        {
            Target = target;

            AddInternal(invalidationMarker = new Box
            {
                RelativeSizeAxes = Axes.Y,
                Width = 2,
                Colour = Color4.Yellow,
                Position = new Vector2(6, 0),
                Alpha = 0
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AttachEvents();
        }

        protected virtual void AttachEvents()
        {
            Target.OnInvalidate += onInvalidate;
        }

        protected virtual void DetachEvents()
        {
            Target.OnInvalidate -= onInvalidate;
        }

        private void onInvalidate(Drawable invalidated)
        {
            Scheduler.Add(() => invalidationMarker.FadeOutFromOne(1));
        }

        protected override void UpdateDetails()
        {
            base.UpdateDetails();

            Text.Text = Target.ToString();
            Alpha = Target.IsPresent ? 1 : 0.3f;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            DetachEvents();
        }
    }
}
