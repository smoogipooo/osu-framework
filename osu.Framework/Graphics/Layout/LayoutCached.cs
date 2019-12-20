// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Caching;

namespace osu.Framework.Graphics.Layout
{
    /// <summary>
    /// A <see cref="LayoutItem"/> which can be manually validated.
    /// </summary>
    public sealed class LayoutCached : LayoutItem
    {
        public LayoutCached(Invalidation type)
            : base(type)
        {
        }

        /// <summary>
        /// Validates this <see cref="LayoutItem"/> and its dependent.
        /// </summary>
        public void Validate() => Validate(Type);
    }

    /// <summary>
    /// A <see cref="LayoutItem"/> which stores a <typeparamref name="T"/> value and is validated when the value is set.
    /// </summary>
    /// <typeparam name="T">The type of value stored.</typeparam>
    public sealed class LayoutCached<T> : LayoutItem
    {
        public LayoutCached(Invalidation type)
            : base(type)
        {
        }

        private T value;

        /// <summary>
        /// The stored value.
        /// </summary>
        /// <exception cref="InvalidOperationException">If this <see cref="LayoutCached{T}"/> is not value.</exception>
        public T Value
        {
            get
            {
                if (!IsValid)
                    throw new InvalidOperationException($"May not query {nameof(Value)} of an invalid {nameof(Cached<T>)}.");

                return value;
            }
            set
            {
                this.value = value;

                Validate(Type);
            }
        }

        public static implicit operator T(LayoutCached<T> cached) => cached.Value;
    }
}
