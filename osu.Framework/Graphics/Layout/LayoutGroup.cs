// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;

namespace osu.Framework.Graphics.Layout
{
    /// <summary>
    /// A grouping of <see cref="LayoutItem"/>s as dependencies for the layout of a <see cref="Drawable"/> to become valid.
    /// </summary>
    public sealed class LayoutGroup : LayoutItem
    {
        /// <summary>
        /// All the dependencies that must be satisfied in order for this <see cref="LayoutItem"/> to become valid.
        /// </summary>
        private readonly List<LayoutItem> dependencies = new List<LayoutItem>();

        public LayoutGroup()
            : base(Invalidation.All)
        {
        }

        /// <summary>
        /// Adds a dependency which must be satisfied in order for this <see cref="LayoutItem"/> to become valid.
        /// Dependencies are invalidated when this <see cref="LayoutItem"/> is invalidated.
        /// </summary>
        public void AddDependency(LayoutItem item)
        {
            dependencies.Add(item);
            item.Dependent = this;
        }

        protected override void InvalidateInternal(Invalidation type)
        {
            foreach (var dep in dependencies)
            {
                // Match on any of the given type flags.
                if ((dep.Type & type) > 0)
                    dep.Invalidate(type);
            }
        }
    }
}
