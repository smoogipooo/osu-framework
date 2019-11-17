// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Primitives;
using osu.Framework.MathUtils;
using osu.Framework.MathUtils.Clipping;
using osuTK;

namespace osu.Framework.Graphics.OpenGL
{
    public class OcclusionLayer
    {
        private const int tile_size = 32;

        private readonly bool[,] tiles = new bool[tile_size, tile_size];

        private readonly int fullScreenWidth;
        private readonly int fullScreenHeight;

        public OcclusionLayer(int width, int height)
        {
            fullScreenWidth = width;
            fullScreenHeight = height;
        }

        /// <returns>Whether the input is fully occluded.</returns>
        public bool IsOccluded<T>(T input)
            where T : IConvexPolygon
        {
            RectangleI tileAabb = screenToTile(input.GetAABB());

            // Todo: these may be off-by-one in right / bottom
            for (int x = tileAabb.Left; x < tileAabb.Right; x++)
            {
                for (int y = tileAabb.Top; y < tileAabb.Bottom; y++)
                {
                    if (!tiles[x, y])
                        return false;
                }
            }

            return true;
        }

        public void Add<T>(T input)
            where T : IConvexPolygon
        {
            RectangleI inputAabb = input.GetAABB();
            RectangleI tileAabb = screenToTile(inputAabb);

            // Todo: these may be off-by-one in right / bottom
            for (int x = tileAabb.Left; x < tileAabb.Right; x++)
            {
                for (int y = tileAabb.Top; y < tileAabb.Bottom; y++)
                {
                    // If the tile is already occluding, it does not need to be processed
                    if (tiles[x, y])
                        continue;

                    tiles[x, y] = tileContains(getScreenSpaceTile(x, y), input);
                }
            }
        }

        private bool tileContains<T>(Quad tileQuad, T input)
            where T : IConvexPolygon
        {
            // Clip the input by the tile
            var clipper = new ConvexPolygonClipper<Quad, T>(ref tileQuad, ref input);
            Span<Vector2> clipBuffer = stackalloc Vector2[clipper.GetClipBufferSize()];
            clipBuffer = clipper.Clip(clipBuffer);

            return Precision.AlmostEquals(Vector2Extensions.GetOrientation(clipBuffer), Vector2Extensions.GetOrientation(tileQuad.GetVertices()));
        }

        /// <summary>
        /// Converts a screen-space rectangle to a rectangle in the tile area.
        /// </summary>
        /// <param name="screenSpaceRectangle">The screen-space rectangle.</param>
        /// <returns>A rectangle representing the mapping of <paramref name="screenSpaceRectangle"/> into the tile area.</returns>
        private RectangleI screenToTile(RectangleF screenSpaceRectangle)
            => new RectangleI(
                (int)Math.Floor(screenSpaceRectangle.Left / fullScreenWidth * tile_size),
                (int)Math.Floor(screenSpaceRectangle.Top / fullScreenHeight * tile_size),
                (int)Math.Ceiling(screenSpaceRectangle.Width / fullScreenWidth * tile_size),
                (int)Math.Ceiling(screenSpaceRectangle.Height / fullScreenHeight * tile_size));

        /// <summary>
        /// Retrieves the screen-space rectangle of a tile.
        /// </summary>
        /// <param name="x">The x-coordinate of the tile.</param>
        /// <param name="y">The y-coordinate of the tile.</param>
        /// <returns>A rectangle representing the screen-space dimensions of the tile.</returns>
        private RectangleF getScreenSpaceTile(int x, int y)
            => new RectangleF(
                (float)x / tile_size * fullScreenWidth,
                (float)y / tile_size * fullScreenHeight,
                (float)fullScreenWidth / tile_size,
                (float)fullScreenHeight / tile_size
            );
    }
}
