// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Statistics;

namespace osu.Framework.Graphics.Layout
{
    /// <summary>
    /// A dependency required for the layout of a <see cref="Drawable"/> to become valid.
    /// </summary>
    public abstract class LayoutItem
    {
        /// <summary>
        /// The <see cref="Invalidation"/> type which this <see cref="LayoutItem"/> responds to.
        /// </summary>
        public readonly Invalidation Type;

        /// <summary>
        /// The dependent that is satisfied by this <see cref="LayoutItem"/>. It will be validated when this <see cref="LayoutItem"/> is validated.
        /// </summary>
        internal LayoutItem Dependent { get; set; }

        /// <summary>
        /// The current invalidation state of the layout subtree starting at this <see cref="LayoutItem"/>.
        /// </summary>
        private Invalidation subTreeInvalidationState;

        /// <summary>
        /// Creates a new <see cref="LayoutItem"/>.
        /// </summary>
        /// <param name="type">The <see cref="Invalidation"/> type which this <see cref="LayoutItem"/> responds to.</param>
        protected LayoutItem(Invalidation type)
        {
            Type = type;
        }

        /// <summary>
        /// Whether this <see cref="LayoutItem"/> is valid.
        /// </summary>
        public bool IsValid => subTreeInvalidationState == Invalidation.None;

        /// <summary>
        /// Validates this <see cref="LayoutItem"/> and its dependent with a given <see cref="Invalidation"/> type.
        /// </summary>
        /// <param name="type">The <see cref="Invalidation"/> type to validate with.</param>
        protected void Validate(Invalidation type)
        {
            if ((subTreeInvalidationState & type) == 0)
            {
                // If the subtree invalidation state doesn't change, there's nothing to do.
                // This is an optimisation that blocks multiple validations on the same type from propagating upwards and resulting in an O(N^2) computational complexity.
                return;
            }

            subTreeInvalidationState &= ~type;

            Dependent?.Validate(type);

            FrameStatistics.Increment(StatisticsCounterType.Refreshes);
        }

        /// <summary>
        /// Invalidates this <see cref="LayoutItem"/> if it matches a given <see cref="Invalidation"/> type.
        /// </summary>
        /// <param name="type">The <see cref="Invalidation"/> type to invalidate with.</param>
        public void Invalidate(Invalidation type = Invalidation.All)
        {
            if ((subTreeInvalidationState & type) == type)
            {
                // If the subtree invalidation state doesn't change, there's nothing to.
                // This is an optimisation that blocks multiple invalidations on the same type from propagating downwards and resulting in an O(N^2) computational complexity.
                return;
            }

            subTreeInvalidationState |= type;

            InvalidateInternal(type);

            FrameStatistics.Increment(StatisticsCounterType.Invalidations);
        }

        /// <summary>
        /// Performs the invalidation of this <see cref="LayoutItem"/> with a given <see cref="Invalidation"/> type.
        /// </summary>
        /// <param name="type">The <see cref="Invalidation"/> type to invalidate with.</param>
        protected virtual void InvalidateInternal(Invalidation type)
        {
        }
    }
}
