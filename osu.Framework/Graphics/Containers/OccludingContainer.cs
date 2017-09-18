// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using OpenTK.Graphics.ES20;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.OpenGL.Vertices;
using System;
using osu.Framework.Allocation;
using osu.Framework.Configuration;

namespace osu.Framework.Graphics.Containers
{
    internal class OccludingContainer : Container
    {
        private Bindable<bool> occlusionsEnabled;

        public OccludingContainer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager config)
        {
            occlusionsEnabled = config.GetBindable<bool>(FrameworkSetting.OcclusionOptimisations);
            occlusionsEnabled.ValueChanged += v => Invalidate(Invalidation.DrawNode);
        }

        protected override DrawNode CreateDrawNode() => new OccludingContainerDrawNode();

        protected override void ApplyDrawNode(DrawNode node)
        {
            var n = (OccludingContainerDrawNode)node;
            n.OcclusionsEnabled = occlusionsEnabled;

            base.ApplyDrawNode(node);
        }

        private class OccludingContainerDrawNode : CompositeDrawNode
        {
            internal bool OcclusionsEnabled;

            public override void Draw(Action<TexturedVertex2D> vertexAction, ref byte currentOccluder)
            {
                if (OcclusionsEnabled)
                {
                    GLWrapper.SetStencilTest(true);

                    GL.StencilOp(StencilOp.Keep, StencilOp.Replace, StencilOp.Replace);

                    // Perform a pre-pass to fill the stencil buffer
                    Shader.SetGlobalProperty("g_ForStencil", true);
                    GL.ColorMask(false, false, false, false);
                    GL.StencilMask(255);

                    base.DrawOcclusion(vertexAction, ref currentOccluder);

                    // Perform the colour pass - this can be done back-to-front due to the above stencil generation
                    Shader.SetGlobalProperty("g_ForStencil", false);
                    GL.StencilFunc(StencilFunction.Gequal, currentOccluder, 0xFF);
                    GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
                    GL.ColorMask(true, true, true, true);
                    GL.StencilMask(0);
                }

                base.Draw(vertexAction, ref currentOccluder);

                if (OcclusionsEnabled)
                    GLWrapper.SetStencilTest(false);
            }
        }
    }
}
