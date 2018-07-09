// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Platform;
using OpenTK.Graphics.ES30;

namespace osu.Framework.Graphics
{
    public class Occluder : CompositeDrawable
    {
        private readonly IBindable<bool> depthTesting = new Bindable<bool>();

        private Shader shader;

        [BackgroundDependencyLoader]
        private void load(GameHost host, ShaderManager shaders)
        {
            shader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.COLOUR);

            InternalChild = new Box { RelativeSizeAxes = Axes.Both };

            depthTesting.BindTo(host.DepthTesting);
            depthTesting.BindValueChanged(_ => Invalidate(Invalidation.DrawNode));
        }

        protected override bool CanBeFlattened => false;

        protected override DrawNode CreateDrawNode() => new OccluderDrawNode();

        protected override void ApplyDrawNode(DrawNode node)
        {
            var n = (OccluderDrawNode)node;
            n.Bypass = !depthTesting.Value;
            n.Shader = shader;
            base.ApplyDrawNode(node);
        }

        internal override DrawNode GenerateDrawNodeSubtree(ulong frame, int treeIndex)
        {
            DepthIndex = DepthIndex++;
            return base.GenerateDrawNodeSubtree(frame, treeIndex);
        }

        private class OccluderDrawNode : CompositeDrawNode
        {
            public bool Bypass;

            public override void Draw(Action<TexturedVertex2D> vertexAction)
            {
                if (Bypass || DrawInfo.Blending.Destination == BlendingFactorDest.One || DrawInfo.Colour.MinAlpha < 1)
                    return;

                Shader.Bind();

                GLWrapper.PushDepthInfo(new DepthInfo
                {
                    DepthTest = true,
                    WriteDepth = true,
                    DepthTestFunction = DepthFunction.Less
                });

                base.Draw(vertexAction);

                GLWrapper.PopDepthInfo();

                Shader.Unbind();
            }
        }
    }
}
