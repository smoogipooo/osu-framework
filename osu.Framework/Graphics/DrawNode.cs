// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Graphics.OpenGL;
using System;
using osu.Framework.Graphics.OpenGL.Vertices;
using OpenTK.Graphics.ES20;
using System.Diagnostics;

namespace osu.Framework.Graphics
{
    /// <summary>
    /// Contains all the information required to draw a single <see cref="Drawable"/>.
    /// A hierarchy of DrawNodes is passed to the draw thread for rendering every frame.
    /// </summary>
    public class DrawNode
    {
        /// <summary>
        /// Contains a linear transformation, colour information, and blending information
        /// of this draw node.
        /// </summary>
        public DrawInfo DrawInfo;

        /// <summary>
        /// Identifies the state of this draw node with an invalidation state of its corresponding
        /// <see cref="Drawable"/>. Whenever the invalidation state of this draw node disagrees
        /// with the state of its <see cref="Drawable"/> it has to be updated.
        /// </summary>
        public long InvalidationID;

        internal bool Occluder;

        private bool performOcclusion = true;

        /// <summary>
        /// Draws this draw node to the screen.
        /// </summary>
        /// <param name="vertexAction">The action to be performed on each vertex of
        /// the draw node in order to draw it if required. This is primarily used by
        /// textured sprites.</param>
        public virtual void Draw(Action<TexturedVertex2D> vertexAction, ref byte currentOccluder)
        {
            if (performOcclusion && Occluder)
            {
                GLWrapper.FlushCurrentBatch();

                currentOccluder++;
                GL.StencilFunc(StencilFunction.Gequal, currentOccluder, 0xFF);
            }

            GLWrapper.SetBlend(DrawInfo.Blending);
        }

        public virtual void DrawOcclusion(Action<TexturedVertex2D> vertexAction, ref byte currentOccluder)
        {
            if (Occluder)
            {
                if (currentOccluder == 0)
                {
                    Debug.Fail("Scene graph contains more than 256 occluders. Any further occluders will not act as occluders.");
                    return;
                }

                GL.StencilFunc(StencilFunction.Gequal, currentOccluder, 0xFF);

                performOcclusion = false;
                Draw(vertexAction, ref currentOccluder);
                performOcclusion = true;

                GLWrapper.FlushCurrentBatch();

                if (currentOccluder > 0)
                    currentOccluder--;
            }
        }
    }
}
