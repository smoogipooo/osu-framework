// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections;
using System.Collections.Specialized;
using osu.Framework.Graphics.Primitives;
using osu.Framework.MathUtils;
using osu.Framework.MathUtils.Clipping;
using osuTK;

namespace osu.Framework.Graphics.OpenGL
{
    public class OcclusionLayer
    {
        private const int tile_count = 32;

        private readonly BitArray tiles;
        private readonly int fullScreenWidth;
        private readonly int fullScreenHeight;

        public OcclusionLayer(int width, int height)
        {
            fullScreenWidth = width;
            fullScreenHeight = height;

            tiles = new BitArray(tile_count * tile_count, false);
        }

        /// <returns>Whether the input is fully occluded.</returns>
        public bool IsOccluded<T>(T input)
            where T : IConvexPolygon
            => IsOccluded(input.GetVertices());

        /// <returns>Whether the input is fully occluded.</returns>
        public bool IsOccluded(ReadOnlySpan<Vector2> input)
        {
            RectangleI tileAabb = screenToTile(getAabb(input));

            for (int x = tileAabb.Left; x < tileAabb.Right; x++)
            {
                for (int y = tileAabb.Top; y < tileAabb.Bottom; y++)
                {
                    if (tiles[getTileIndex(x, y)])
                        return false;
                }
            }

            return true;
        }

        internal void Add<T>(T polygon)
            where T : IConvexPolygon
            => Add(polygon.GetVertices());

        public void Add(ReadOnlySpan<Vector2> input)
        {
            RectangleI tileAabb = screenToTile(getAabb(input));

            for (int x = tileAabb.Left; x < tileAabb.Right; x++)
            {
                for (int y = tileAabb.Top; y < tileAabb.Bottom; y++)
                {
                    // If the tile is already occluding, it does not need to be processed
                    if (tiles[getTileIndex(x, y)])
                        continue;

                    tiles[getTileIndex(x, y)] = tileContains(getScreenSpaceTile(x, y), input);
                }
            }
        }

        private int getTileIndex(int x, int y) => y * tile_count + x;

        private bool tileContains(Quad tileQuad, in ReadOnlySpan<Vector2> vertices)
        {
            // Clip the input by the tile
            Span<Vector2> buffer = stackalloc Vector2[ConvexPolygonClipper.GetClipBufferSize(vertices)];
            ReadOnlySpan<Vector2> clipped = ConvexPolygonClipper.Create(ref tileQuad, vertices, buffer).Clip();

            return Precision.AlmostEquals(Vector2Extensions.GetOrientation(clipped), Vector2Extensions.GetOrientation(tileQuad.GetVertices()));
        }

        private RectangleF getAabb(in ReadOnlySpan<Vector2> vertexSpan)
        {
            if (vertexSpan.IsEmpty)
                return RectangleF.Empty;

            float minX = vertexSpan[0].X;
            float minY = vertexSpan[0].Y;
            float maxX = vertexSpan[0].X;
            float maxY = vertexSpan[0].Y;

            for (int i = 1; i < vertexSpan.Length; i++)
            {
                minX = Math.Min(minX, vertexSpan[i].X);
                minY = Math.Min(minY, vertexSpan[i].Y);
                maxX = Math.Max(maxX, vertexSpan[i].X);
                maxY = Math.Max(maxY, vertexSpan[i].Y);
            }

            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }

        /// <summary>
        /// Converts a screen-space rectangle to a rectangle in the tile area.
        /// </summary>
        /// <param name="screenSpaceRectangle">The screen-space rectangle.</param>
        /// <returns>A rectangle representing the mapping of <paramref name="screenSpaceRectangle"/> into the tile area.</returns>
        private RectangleI screenToTile(RectangleF screenSpaceRectangle)
            => RectangleI.FromLTRB(
                (int)MathHelper.Clamp(Math.Floor(screenSpaceRectangle.Left / fullScreenWidth * tile_count), 0, tile_count),
                (int)MathHelper.Clamp(Math.Floor(screenSpaceRectangle.Top / fullScreenHeight * tile_count), 0, tile_count),
                (int)MathHelper.Clamp(Math.Ceiling(screenSpaceRectangle.Right / fullScreenWidth * tile_count), 0, tile_count),
                (int)MathHelper.Clamp(Math.Ceiling(screenSpaceRectangle.Bottom / fullScreenHeight * tile_count), 0, tile_count));

        /// <summary>
        /// Retrieves the screen-space rectangle of a tile.
        /// </summary>
        /// <param name="x">The x-coordinate of the tile.</param>
        /// <param name="y">The y-coordinate of the tile.</param>
        /// <returns>A rectangle representing the screen-space dimensions of the tile.</returns>
        private RectangleF getScreenSpaceTile(int x, int y)
            => new RectangleF(
                (float)x / tile_count * fullScreenWidth,
                (float)y / tile_count * fullScreenHeight,
                (float)fullScreenWidth / tile_count,
                (float)fullScreenHeight / tile_count
            );
    }
}
