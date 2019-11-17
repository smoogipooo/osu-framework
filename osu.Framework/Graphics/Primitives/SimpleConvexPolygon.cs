// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK;

namespace osu.Framework.Graphics.Primitives
{
    public class SimpleConvexPolygon : IConvexPolygon
    {
        private readonly Vector2[] vertices;

        public SimpleConvexPolygon(Vector2[] vertices)
        {
            this.vertices = vertices;
        }

        public ReadOnlySpan<Vector2> GetAxisVertices() => vertices;

        public ReadOnlySpan<Vector2> GetVertices() => vertices;

        public RectangleF GetAABBFloat()
        {
            float minX = vertices[0].X;
            float minY = vertices[0].Y;
            float maxX = vertices[0].X;
            float maxY = vertices[0].Y;

            for (int i = 1; i < vertices.Length; i++)
            {
                minX = Math.Min(minX, vertices[i].X);
                minY = Math.Min(minY, vertices[i].Y);
                maxX = Math.Max(maxX, vertices[i].X);
                maxY = Math.Max(maxY, vertices[i].Y);
            }

            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }

        public RectangleI GetAABB()
        {
            int minX = (int)vertices[0].X;
            int minY = (int)vertices[0].Y;
            int maxX = (int)Math.Ceiling(vertices[0].X);
            int maxY = (int)Math.Ceiling(vertices[0].Y);

            for (int i = 1; i < vertices.Length; i++)
            {
                minX = Math.Min(minX, (int)vertices[i].X);
                minY = Math.Min(minY, (int)vertices[i].Y);
                maxX = Math.Max(maxX, (int)Math.Ceiling(vertices[i].X));
                maxY = Math.Max(maxY, (int)Math.Ceiling(vertices[i].Y));
            }

            return new RectangleI(minX, minY, maxX - minX, maxY - minY);
        }

        public int MaxClipVertices => vertices.Length * 2;
    }
}
