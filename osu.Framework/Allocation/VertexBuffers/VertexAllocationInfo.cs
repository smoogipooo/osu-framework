// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Framework.Allocation.VertexBuffers
{
    public class VertexAllocationInfo
    {
        public VertexBufferPoolType Type = VertexBufferPoolType.Unknown;

        public int PoolIndex = -1;

        public int DrawCount;
        public int UpdateCount;
    }
}
