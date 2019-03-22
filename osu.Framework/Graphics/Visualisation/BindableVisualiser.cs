// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using osu.Framework.Bindables;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace osu.Framework.Graphics.Visualisation
{
    public class BindableVisualiser : OverlayContainer
    {
        private readonly Drawable target;

        public BindableVisualiser(Drawable target)
        {
            this.target = target;
        }

        private bool first = true;

        protected override void Update()
        {
            base.Update();

            if (!first)
                return;
            first = false;

            var map = getBindables(target).ToDictionary(d => d.Target);

            int index = 0;
            int groupIndex = 0;
            Drawable currentParent = null;
            foreach (var v in map)
            {
                if (v.Value.Parent == currentParent)
                    v.Value.TopologicalIndex = index;
                else
                {
                    v.Value.TopologicalIndex = index++;
                    currentParent = v.Value.Parent;
                }

                if (visitRecursively(v.Value))
                    groupIndex++;
            }

            var groupCounts = new int[groupIndex + 1];
            foreach (var v in map.Values)
                groupCounts[v.GroupIndex]++;

            var pruned = map.Values.Where(v => groupCounts[v.GroupIndex] > 1).ToArray();

            FillFlowContainer flow;
            Child = flow = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(20, 0)
            };

            foreach (var p in pruned.GroupBy(d => d.Parent))
                flow.Add(new ElementVisualiser(p.Key, p));

            bool visitRecursively(BindableDescriptor d)
            {
                if (d.GroupIndex != -1)
                    return false;
                d.GroupIndex = groupIndex;

                foreach (var b in d.Target.GetBindings())
                {
                    if (map.TryGetValue(b, out var next))
                        visitRecursively(next);
                }

                return true;
            }
        }

        private IEnumerable<BindableDescriptor> getBindables(Drawable target)
        {
            foreach (var t in target.GetType().EnumerateBaseTypes())
            {
                foreach (var b in getBindables(target, t))
                    yield return new BindableDescriptor { Target = b.Item1, Description = b.Item2, Parent = target };
            }

            if (!(target is CompositeDrawable composite))
                yield break;

            foreach (var c in composite.InternalChildren)
            {
                foreach (var b in getBindables(c))
                    yield return b;
            }
        }

        private IEnumerable<(IBindable, string)> getBindables(Drawable targetObject, Type targetType)
        {
            return targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                             .Where(f => typeof(IBindable).IsAssignableFrom(f.FieldType))
                             .Select(m => ((IBindable)m.GetValue(targetObject), m.Name)).Where(value => value.Item1 != null);
        }

        protected override void PopIn() => this.FadeIn();

        protected override void PopOut() => this.FadeOut();
    }

    public class ElementVisualiser : CompositeDrawable
    {
        public ElementVisualiser(Drawable drawable, IEnumerable<BindableDescriptor> bindables)
        {
            AutoSizeAxes = Axes.Y;
            Width = 200;
            Masking = true;

            FillFlowContainer bindableContainer;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.2f
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding(10),
                    Spacing = new Vector2(0, 10),
                    Children = new Drawable[]
                    {
                        new SpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = drawable.ToString()
                        },
                        bindableContainer = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, 2)
                        }
                    }
                }
            };

            foreach (var b in bindables)
            {
                bindableContainer.Add(new SpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Text = b.Description
                });
            }
        }
    }

    public class BindableDescriptor
    {
        public IBindable Target;
        public Drawable Parent;
        public string Description;
        public int GroupIndex = - 1;
        public int TopologicalIndex = -1;
    }
}
