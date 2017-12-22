// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System.Linq;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input;

namespace osu.Framework.Graphics.Visualisation
{
    public class DrawVisualiser : OverlayContainer
    {
        private readonly TreeContainer treeContainer;

        private readonly PropertyDisplay propertyDisplay;

        private readonly InfoOverlay overlay;

        private InputManager inputManager;

        public DrawVisualiser()
        {
            RelativeSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                overlay = new InfoOverlay(),
                treeContainer = new TreeContainer
                {
                    ChooseTarget = chooseTarget,
                    GoUpOneParent = delegate
                    {
                    },
                    ToggleProperties = delegate
                    {
                        if (targetDrawable == null)
                            return;
                    },
                },
                new CursorContainer()
            };

            propertyDisplay = treeContainer.PropertyDisplay;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            inputManager = GetContainingInputManager();
        }

        protected override bool BlockPassThroughMouse => false;

        protected override void PopIn()
        {
            this.FadeIn(100);
            if (Target == null)
                chooseTarget();
            else
                createRootVisualisedDrawable();
        }

        protected override void PopOut()
        {
            this.FadeOut(100);

            // Don't keep resources for visualizing the target
            // allocated; unbind callback events.
            removeRootVisualisedDrawable();
        }

        private bool targetSearching;

        private void chooseTarget()
        {
            Target = null;
            targetSearching = true;
        }

        private Drawable findTargetIn(Drawable d, InputState state)
        {
            if (d is DrawVisualiser) return null;
            if (d is CursorContainer) return null;
            if (d is PropertyDisplay) return null;

            if (!d.IsPresent) return null;

            bool containsCursor = d.ScreenSpaceDrawQuad.Contains(state.Mouse.NativeState.Position);
            // This is an optimization: We don't need to consider drawables which we don't hover, and which do not
            // forward input further to children (via d.ReceiveMouseInputAt). If they do forward input to children, then there
            // is a good chance they have children poking out of their bounds, which we need to catch.
            if (!containsCursor && !d.ReceiveMouseInputAt(state.Mouse.NativeState.Position))
                return null;

            var dAsContainer = d as CompositeDrawable;

            Drawable containedTarget = null;

            if (dAsContainer != null)
            {
                if (!dAsContainer.InternalChildren.Any())
                    return null;

                foreach (var c in dAsContainer.AliveInternalChildren)
                {
                    var contained = findTargetIn(c, state);
                    if (contained != null)
                    {
                        if (containedTarget == null ||
                            containedTarget.DrawWidth * containedTarget.DrawHeight > contained.DrawWidth * contained.DrawHeight)
                        {
                            containedTarget = contained;
                        }
                    }
                }
            }

            return containedTarget ?? (containsCursor ? d : null);
        }

        private TreeLeafNode targetDrawable;

        private void removeRootVisualisedDrawable(bool hideProperties = true)
        {
            if (hideProperties)
                propertyDisplay.State = Visibility.Hidden;

            if (targetDrawable != null)
            {
                if (targetDrawable.Parent != null)
                {
                    // targetDrawable may have gotten purged from the TreeContainer
                    treeContainer.Remove(targetDrawable);
                    targetDrawable.Dispose();
                }
                targetDrawable = null;
            }
        }

        private void createRootVisualisedDrawable()
        {
            removeRootVisualisedDrawable(target == null);

            if (target == null)
                return;

            targetDrawable = TreeLeafNode.CreateNodeFor(target);
            treeContainer.Add(targetDrawable);
        }

        private Drawable target;
        public Drawable Target
        {
            get { return target; }
            set
            {
                target = value;
                createRootVisualisedDrawable();
            }
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            return targetSearching;
        }

        private Drawable findTarget(InputState state)
        {
            return findTargetIn(Parent?.Parent, state);
        }

        protected override bool OnClick(InputState state)
        {
            if (targetSearching)
            {
                Target = findTarget(state)?.Parent;

                if (Target != null)
                {
                    targetSearching = false;
                    overlay.Target = null;
                    return true;
                }
            }

            return base.OnClick(state);
        }

        protected override bool OnMouseMove(InputState state)
        {
            overlay.Target = targetSearching ? findTarget(state) : inputManager.HoveredDrawables.OfType<TreeDrawableNode>().FirstOrDefault()?.Target;
            return base.OnMouseMove(state);
        }
    }
}
