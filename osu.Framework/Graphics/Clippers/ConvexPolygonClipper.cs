// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Primitives;
using osuTK;

namespace osu.Framework.Graphics.Clippers
{
    public readonly struct ConvexPolygonClipper
    {
        private readonly IConvexPolygon clipPolygon;
        private readonly IConvexPolygon subjectPolygon;

        public ConvexPolygonClipper(IConvexPolygon clipPolygon, IConvexPolygon subjectPolygon)
        {
            this.clipPolygon = clipPolygon;
            this.subjectPolygon = subjectPolygon;
        }

        /// <summary>
        /// Determines the minimum buffer size required to clip the two polygons.
        /// </summary>
        /// <returns>The minimum buffer size required for <see cref="clipPolygon"/> to clip <see cref="subjectPolygon"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetClipBufferSize()
            => subjectPolygon.GetVertices().Length * 2; // There can only be at most two intersections for each of the subject's vertices

        /// <summary>
        /// Clips <see cref="subjectPolygon"/> by <see cref="clipPolygon"/>.
        /// </summary>
        /// <returns>A clockwise-ordered set of vertices representing the result of clipping <see cref="subjectPolygon"/> by <see cref="clipPolygon"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<Vector2> Clip() => Clip(new Vector2[GetClipBufferSize()]);

        /// <summary>
        /// Clips <see cref="subjectPolygon"/> by <see cref="clipPolygon"/> using an intermediate buffer.
        /// </summary>
        /// <param name="buffer">The buffer to contain the clipped vertices. Must have a length of <see cref="GetClipBufferSize"/>.</param>
        /// <returns>A clockwise-ordered set of vertices representing the result of clipping <see cref="subjectPolygon"/> by <see cref="clipPolygon"/>.</returns>
        public Span<Vector2> Clip(Span<Vector2> buffer)
        {
            if (buffer.Length < GetClipBufferSize())
            {
                throw new ArgumentException($"Clip buffer must have a length of {GetClipBufferSize()}, but was {buffer.Length}."
                                            + $"Use {nameof(GetClipBufferSize)} to calculate the size of the buffer.", nameof(buffer));
            }

            ReadOnlySpan<Vector2> subjectVertices = subjectPolygon.GetVertices();
            ReadOnlySpan<Vector2> clipVertices = clipPolygon.GetVertices();

            Span<Line> subjectEdges = stackalloc Line[subjectVertices.Length];
            Span<Line> clipEdges = stackalloc Line[subjectVertices.Length];

            getEdges(subjectVertices, subjectEdges);
            getEdges(clipVertices, clipEdges);

            int bufferIndex = 0;

            int sEdgeIndex = 0;
            int cEdgeIndex = 0;

            // 0 -> Unknown
            // 1 -> Inside
            // 2 -> Outside
            VertexLocation location = VertexLocation.Unknown;

            while (sEdgeIndex < subjectEdges.Length || cEdgeIndex < clipEdges.Length)
            {
                Line sEdge = subjectEdges[sEdgeIndex % subjectEdges.Length];
                Line cEdge = clipEdges[cEdgeIndex % clipEdges.Length];

                // t represents a parameter along cEdge of the intersection point
                // u represents a parameter along sEdge of the intersection point

                (bool intersects, float t) = cEdge.IntersectWith(sEdge);
                (_, float u) = sEdge.IntersectWith(cEdge);

                // Check whether the line segments intersect. If so:
                // 1. Update inside with whether the subject is inside the clipping region.
                // 2. Save the intersection point.

                if (intersects && t >= 0 && t <= 1 && u >= 0 && u <= 1)
                {
                    location = cEdge.RightHalfPlaneContains(sEdge.EndPoint) ? VertexLocation.Inside : VertexLocation.Outside;
                    saveVertex(cEdge.At(t), buffer, ref bufferIndex);
                }

                // Advance sEdge or cEdge based on the following rules:
                // 1. If cEdge points towards sEdge (t > 1), advance cEdge.
                // 2. If sEdge points towards cEdge (u > 1), advance sEdge.
                // 3. Advance the outer-most edge (t < 1 && u < 1).

                bool advanceClip = false;
                bool advanceSubject = false;

                if (!intersects || t >= 0 && u >= 0 || t <= 0 && u <= 0)
                {
                    switch (location)
                    {
                        case VertexLocation.Inside:
                            advanceClip = true;
                            break;
                        case VertexLocation.Outside:
                            advanceSubject = true;
                            break;
                        default:
                        {
                            if (cEdge.RightHalfPlaneContains(sEdge.EndPoint))
                                advanceClip = true;
                            else
                                advanceSubject = true;
                            break;
                        }
                    }
                }
                else if (t >= 0)
                    advanceSubject = true;
                else if (u >= 0)
                    advanceClip = true;

                if (advanceSubject)
                    advanceEdge(ref sEdgeIndex, subjectEdges, buffer, ref bufferIndex, location == VertexLocation.Inside);
                else if (advanceClip)
                    advanceEdge(ref cEdgeIndex, clipEdges, buffer, ref bufferIndex, location == VertexLocation.Outside);
            }

            return buffer.Slice(0, bufferIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void advanceEdge(ref int edgeIndex, Span<Line> edges, Span<Vector2> buffer, ref int bufferIndex, bool save)
        {
            if (save)
                saveVertex(edges[edgeIndex].EndPoint, buffer, ref bufferIndex);
            edgeIndex++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void saveVertex(Vector2 vertex, Span<Vector2> buffer, ref int bufferIndex)
            => buffer[bufferIndex++] = vertex;

        private void getEdges(ReadOnlySpan<Vector2> vertices, Span<Line> edges)
        {
            Debug.Assert(edges.Length == vertices.Length);

            if (Vector2Extensions.GetRotation(vertices) < 0)
            {
                for (int i = vertices.Length - 1, c = 0; i > 0; i--, c++)
                    edges[c] = new Line(vertices[i], vertices[i - 1]);
                edges[edges.Length - 1] = new Line(vertices[0], vertices[vertices.Length - 1]);
            }
            else
            {
                for (int i = 0; i < vertices.Length - 1; i++)
                    edges[i] = new Line(vertices[i], vertices[i + 1]);
                edges[edges.Length - 1] = new Line(vertices[vertices.Length - 1], vertices[0]);
            }
        }

        private enum VertexLocation
        {
            Unknown,
            Inside,
            Outside
        }
    }
}
