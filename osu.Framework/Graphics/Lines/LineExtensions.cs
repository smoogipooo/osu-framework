// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Primitives;
using osuTK;

namespace osu.Framework.Graphics.Lines
{
    public static class LineExtensions
    {
        /// <summary>
        /// Determines whether a point is within the right half-plane of a line.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="point">The point.</param>
        /// <returns>Whether <paramref name="point"/> is in the right half-plane of <paramref name="line"/>.</returns>
        public static bool IsInRightHalfPlane(this Line line, Vector2 point)
        {
            var diff1 = line.Direction;
            var diff2 = point - line.StartPoint;

            return diff1.X * diff2.Y - diff1.Y * diff2.X <= 0;
        }
    }
}
