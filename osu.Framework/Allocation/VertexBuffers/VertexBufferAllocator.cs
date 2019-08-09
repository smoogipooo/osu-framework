// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.OpenGL.Vertices;

namespace osu.Framework.Allocation.VertexBuffers
{
    public static class VertexBufferAllocator<T>
        where T : struct, IVertex, IEquatable<T>
    {
        private static readonly VertexBufferPool<T>[] buffer_pools =
        {
            new VertexBufferPool<T>(VertexBufferPoolType.Static, 8192),
            new VertexBufferPool<T>(VertexBufferPoolType.Dynamic, 2048),
            new VertexBufferPool<T>(VertexBufferPoolType.Streaming, 1024),
        };

        static VertexBufferAllocator()
        {
            GLWrapper.OnReset += onReset;
            GLWrapper.OnFinish += onFinish;
            GLWrapper.OnBatchBroken += onBatchBroken;
        }

        /// <summary>
        /// Adds vertex.
        /// </summary>
        /// <param name="vertex"></param>
        public static void AddVertex(T vertex)
        {
            VertexAllocationInfo vai = GLWrapper.CurrentDrawNode.VertexAllocationInfo;
            buffer_pools[(int)vai.Type].AddVertex(vai, ref vertex);
        }

        /// <summary>
        /// Resets all the vertex buffer pools in preparation to draw a new frame.
        /// </summary>
        private static void onReset()
        {
            foreach (var pool in buffer_pools)
                pool.Reset();
        }

        private static void onFinish() => flush();

        private static void onBatchBroken() => flush();

        private static void flush()
        {
            foreach (var buffer in buffer_pools)
                buffer.Flush();
        }
    }
}
