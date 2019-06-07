// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Framework.Graphics.Primitives
{
    public static class RectangleIExtensions
    {
        /// <summary>
        /// Intersects a <see cref="RectangleI"/> with another.
        /// </summary>
        /// <param name="subject">The <see cref="RectangleI"/> to intersect. This rectangle will be modified.</param>
        /// <param name="other">The other <see cref="RectangleI"/> to use in the intersection.</param>
        public static void IntersectWith(ref this RectangleI subject, RectangleI other)
        {
            int leftMax = Math.Max(subject.X, other.X);
            int rightMin = Math.Min(subject.X + subject.Width, other.X + other.Width);

            int topMax = Math.Max(subject.Y, other.Y);
            int bottomMin = Math.Min(subject.Y + subject.Height, other.Y + other.Height);

            subject.X = leftMax;
            subject.Y = topMax;
            subject.Width = rightMin - leftMax;
            subject.Height = bottomMin - topMax;
        }
    }
}
