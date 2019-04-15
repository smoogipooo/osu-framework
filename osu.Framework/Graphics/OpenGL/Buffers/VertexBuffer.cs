// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.OpenGL.Vertices;
using osuTK.Graphics.ES30;
using osu.Framework.Statistics;
using osu.Framework.Development;

namespace osu.Framework.Graphics.OpenGL.Buffers
{
    public abstract class VertexBuffer<T> : IDisposable
        where T : struct, IEquatable<T>, IVertex
    {
        protected static readonly int STRIDE = VertexUtils<T>.STRIDE;

        public readonly T[] Vertices;

        private readonly BufferUsageHint usage;

        private bool isInitialised;

        private int vaoID;
        private int vboId;

        protected VertexBuffer(int amountVertices, BufferUsageHint usage)
        {
            this.usage = usage;

            Vertices = new T[amountVertices];
        }

        /// <summary>
        /// Initialises this <see cref="VertexBuffer{T}"/>. Guaranteed to be run on the draw thread.
        /// </summary>
        protected virtual void Initialise()
        {
        }

        ~VertexBuffer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected bool IsDisposed;

        protected virtual void Dispose(bool disposing) => GLWrapper.ScheduleDisposal(() =>
        {
            if (IsDisposed)
                return;

            if (isInitialised)
            {
                GL.DeleteBuffer(vboId);
                GL.DeleteVertexArray(vaoID);
            }

            IsDisposed = true;
        });

        public void Bind(bool forRendering)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(ToString(), "Can not bind disposed vertex buffers.");

            ThreadSafety.EnsureDrawThread();

            if (!isInitialised)
            {
                vaoID = GL.GenVertexArray();
                GLWrapper.BindVertexArray(vaoID);

                vboId = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, vboId);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vertices.Length * STRIDE), IntPtr.Zero, usage);

                VertexUtils<T>.SetAttributes();

                Initialise();

                GLWrapper.BindVertexArray(0);

                isInitialised = true;
            }

            GLWrapper.BindVertexArray(vaoID);
        }

        protected virtual int ToElements(int vertices) => vertices;

        protected virtual int ToElementIndex(int vertexIndex) => vertexIndex;

        protected abstract PrimitiveType Type { get; }

        public void Draw()
        {
            DrawRange(0, Vertices.Length);
        }

        public void DrawRange(int startIndex, int endIndex)
        {
            Bind(true);

            int amountVertices = endIndex - startIndex;
            GL.DrawElements(Type, ToElements(amountVertices), DrawElementsType.UnsignedShort, (IntPtr)(ToElementIndex(startIndex) * sizeof(ushort)));
        }

        public void Update()
        {
            UpdateRange(0, Vertices.Length);
        }

        public void UpdateRange(int startIndex, int endIndex)
        {
            Bind(false);

            int amountVertices = endIndex - startIndex;

            GL.BindBuffer(BufferTarget.ArrayBuffer, vboId);
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)(startIndex * STRIDE), (IntPtr)(amountVertices * STRIDE), ref Vertices[startIndex]);

            FrameStatistics.Add(StatisticsCounterType.VerticesUpl, amountVertices);
        }
    }
}
