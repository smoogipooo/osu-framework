// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Runtime.InteropServices;
using osuTK.Graphics.ES30;

namespace osu.Framework.Graphics.OpenGL.Vertices
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct VertexWithDepth<TVertex> : IEquatable<VertexWithDepth<TVertex>>, IEquatable<TVertex>, IVertex
        where TVertex : IEquatable<TVertex>, IVertex
    {
        public TVertex Vertex;

        [VertexMember(1, VertexAttribPointerType.Float)]
        public float BackbufferDrawDepth;

        public bool Equals(TVertex other)
            => Vertex.Equals(other);

        public bool Equals(VertexWithDepth<TVertex> other)
            => Vertex.Equals(other.Vertex)
               && BackbufferDrawDepth.Equals(other.BackbufferDrawDepth);
    }
}
