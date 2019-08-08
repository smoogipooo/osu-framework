// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.OpenGL.Vertices;

namespace osu.Framework.Allocation.VertexBuffers
{
    public static class VertexBufferAllocator<T>
        where T : struct, IVertex, IEquatable<T>
    {
        private static readonly VertexBatch<T> batch = new QuadBatch<T>(512, 10);

        static VertexBufferAllocator()
        {
            GLWrapper.OnReset += onReset;
            GLWrapper.OnFinish += onFinish;
            GLWrapper.OnBatchBroken += onBatchBroken;
        }

        public static void AddVertex(T vertex)
        {
            batch.Add(vertex);
        }

        private static void onReset()
        {
            batch.ResetCounters();
        }

        private static void onFinish()
        {
            batch.Draw();
        }

        private static void onBatchBroken()
        {
            batch.Draw();
        }
    }
}
