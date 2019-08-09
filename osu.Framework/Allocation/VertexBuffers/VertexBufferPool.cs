// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics.OpenGL.Buffers;
using osu.Framework.Graphics.OpenGL.Vertices;
using osuTK.Graphics.ES30;

namespace osu.Framework.Allocation.VertexBuffers
{
    public class VertexBufferPool<T>
        where T : struct, IVertex, IEquatable<T>
    {
        public readonly VertexBufferPoolType Type;
        private readonly int allocationQuantum;

        private readonly BufferUsageHint bufferUsageHint;
        private readonly List<VertexBuffer<T>> buffers = new List<VertexBuffer<T>>();

        public VertexBufferPool(VertexBufferPoolType type, int allocationQuantum)
        {
            this.allocationQuantum = allocationQuantum;
            Type = type;

            switch (type)
            {
                case VertexBufferPoolType.Streaming:
                    bufferUsageHint = BufferUsageHint.StreamDraw;
                    break;

                case VertexBufferPoolType.Dynamic:
                    bufferUsageHint = BufferUsageHint.DynamicDraw;
                    break;

                case VertexBufferPoolType.Static:
                    bufferUsageHint = BufferUsageHint.StaticDraw;
                    break;

                default:
                    throw new ArgumentException($"Unexpected type: {type}", nameof(type));
            }
        }

        public void AddVertex(VertexAllocationInfo vertexAllocationInfo, ref T vertex)
        {
        }

        public void Reset()
        {
        }

        public void Flush()
        {
        }
    }
}
