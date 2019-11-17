// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Runtime.CompilerServices;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osuTK;

namespace osu.Framework.MathUtils.Clipping
{
    public readonly ref struct ConvexPolygonClipper
    {
        private readonly ReadOnlySpan<Vector2> clipVertices;
        private readonly ReadOnlySpan<Vector2> subjectVertices;
        private readonly Span<Vector2> buffer;

        public static ConvexPolygonClipper Create<TClip, TSubject>(ref TClip clipPolygon, ref TSubject subjectPolygon)
            where TClip : IConvexPolygon
            where TSubject : IConvexPolygon
            => Create(ref clipPolygon, ref subjectPolygon, new Vector2[GetClipBufferSize(subjectPolygon.GetVertices())]);

        public static ConvexPolygonClipper Create<TClip, TSubject>(ref TClip clipPolygon, ref TSubject subjectPolygon, in Span<Vector2> buffer)
            where TClip : IConvexPolygon
            where TSubject : IConvexPolygon
            => new ConvexPolygonClipper(clipPolygon.GetVertices(), subjectPolygon.GetVertices(), buffer);

        public static ConvexPolygonClipper Create<TClip>(ref TClip clipPolygon, in ReadOnlySpan<Vector2> subjectVertices)
            where TClip : IConvexPolygon
            => Create(ref clipPolygon, subjectVertices, new Vector2[GetClipBufferSize(subjectVertices)]);

        public static ConvexPolygonClipper Create<TClip>(ref TClip clipPolygon, in ReadOnlySpan<Vector2> subjectVertices, in Span<Vector2> buffer)
            where TClip : IConvexPolygon
            => new ConvexPolygonClipper(clipPolygon.GetVertices(), subjectVertices, buffer);

        public static ConvexPolygonClipper Create<TSubject>(in ReadOnlySpan<Vector2> clipVertices, ref TSubject subjectPolygon)
            where TSubject : IConvexPolygon
            => Create(clipVertices, ref subjectPolygon, new Vector2[GetClipBufferSize(subjectPolygon.GetVertices())]);

        public static ConvexPolygonClipper Create<TSubject>(in ReadOnlySpan<Vector2> clipVertices, ref TSubject subjectPolygon, in Span<Vector2> buffer)
            where TSubject : IConvexPolygon
            => new ConvexPolygonClipper(clipVertices, subjectPolygon.GetVertices(), buffer);

        private ConvexPolygonClipper(in ReadOnlySpan<Vector2> clipVertices, in ReadOnlySpan<Vector2> subjectVertices, in Span<Vector2> buffer)
        {
            this.clipVertices = clipVertices;
            this.subjectVertices = subjectVertices;
            this.buffer = buffer;
        }

        /// <summary>
        /// Determines the minimum buffer size required to clip a set of vertices.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetClipBufferSize(ReadOnlySpan<Vector2> subjectVertices)
        {
            // There can only be at most two intersections for each of the subject's vertices
            return subjectVertices.Length * 2;
        }

        /// <summary>
        /// Clips <see cref="subjectVertices"/> by <see cref="clipVertices"/>.
        /// </summary>
        /// <returns>A clockwise-ordered set of vertices representing the result of clipping <see cref="subjectVertices"/> by <see cref="clipVertices"/>.</returns>
        public Span<Vector2> Clip()
        {
            if (subjectVertices.Length == 0)
                return Span<Vector2>.Empty;

            if (clipVertices.Length == 0)
                return Span<Vector2>.Empty;

            // Add the subject vertices to the buffer and immediately normalise them
            Span<Vector2> normalisedSubject = getNormalised(subjectVertices, buffer.Slice(0, subjectVertices.Length), true);

            // Since the clip vertices aren't modified, we can use them as they are if they are normalised
            // However if they are not normalised, then we must add them to the buffer and normalise them there
            bool clipNormalised = Vector2Extensions.GetOrientation(clipVertices) >= 0;
            Span<Vector2> clipBuffer = clipNormalised ? null : stackalloc Vector2[clipVertices.Length];
            ReadOnlySpan<Vector2> normalisedClip = clipNormalised
                ? clipVertices
                : getNormalised(clipVertices, clipBuffer, false);

            // Number of vertices in the buffer that need to be tested against
            // This becomes the number of vertices in the resulting polygon after each clipping iteration
            int inputCount = normalisedSubject.Length;

            // Process the clip edge connecting the last vertex to the first vertex
            inputCount = processClipEdge(new Line(normalisedClip[normalisedClip.Length - 1], normalisedClip[0]), buffer, inputCount);

            // Process all other edges
            for (int c = 1; c < normalisedClip.Length; c++)
            {
                if (inputCount == 0)
                    break;

                inputCount = processClipEdge(new Line(normalisedClip[c - 1], normalisedClip[c]), buffer, inputCount);
            }

            return buffer.Slice(0, inputCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int processClipEdge(in Line clipEdge, in Span<Vector2> buffer, in int inputCount)
        {
            // Temporary storage for the vertices from the buffer as the buffer gets altered
            Span<Vector2> inputVertices = stackalloc Vector2[buffer.Length];

            // Store the original vertices (buffer will get altered)
            buffer.CopyTo(inputVertices);

            int outputCount = 0;

            // Process the edge connecting the last vertex with the first vertex
            outputVertices(ref inputVertices[inputCount - 1], ref inputVertices[0], clipEdge, buffer, ref outputCount);

            // Process all other vertices
            for (int i = 1; i < inputCount; i++)
                outputVertices(ref inputVertices[i - 1], ref inputVertices[i], clipEdge, buffer, ref outputCount);

            return outputCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void outputVertices(ref Vector2 startVertex, ref Vector2 endVertex, in Line clipEdge, in Span<Vector2> buffer, ref int bufferIndex)
        {
            if (endVertex.InRightHalfPlaneOf(clipEdge))
            {
                if (!startVertex.InRightHalfPlaneOf(clipEdge))
                {
                    clipEdge.TryIntersectWith(ref startVertex, ref endVertex, out var t);
                    buffer[bufferIndex++] = clipEdge.At(t);
                }

                buffer[bufferIndex++] = endVertex;
            }
            else if (startVertex.InRightHalfPlaneOf(clipEdge))
            {
                clipEdge.TryIntersectWith(ref startVertex, ref endVertex, out var t);
                buffer[bufferIndex++] = clipEdge.At(t);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Span<Vector2> getNormalised(in ReadOnlySpan<Vector2> original, in Span<Vector2> bufferSlice, bool verify)
        {
            original.CopyTo(bufferSlice);

            if (!verify || Vector2Extensions.GetOrientation(original) < 0)
                bufferSlice.Reverse();

            return bufferSlice;
        }
    }
}
