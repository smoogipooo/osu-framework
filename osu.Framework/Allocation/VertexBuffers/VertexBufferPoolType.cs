// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Framework.Allocation.VertexBuffers
{
    public enum VertexBufferPoolType
    {
        /// <summary>
        /// A pool has not been selected yet.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// A pool of vertex buffers that are updated almost every frame.
        /// </summary>
        Streaming = 0,

        /// <summary>
        /// A pool of vertex buffers that are updated frequently but not every frame.
        /// </summary>
        Dynamic = 1,

        /// <summary>
        /// A pool of vertex buffers that are rarely updated.
        /// </summary>
        Static = 2,
    }
}
