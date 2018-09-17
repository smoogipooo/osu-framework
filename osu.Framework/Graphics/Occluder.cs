// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Containers;
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

        [BackgroundDependencyLoader]
        private void load(GameHost host, ShaderManager shaders)
        {
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
            base.ApplyDrawNode(node);
        }

        private class OccluderDrawNode : CompositeDrawNode
        {
            public bool Bypass;

            public override void DrawDepth(Action<TexturedVertex2D> vertexAction, bool fromOccluder, ref int depthIndex)
            {
                if (Bypass || DrawColourInfo.Blending.Destination == BlendingFactorDest.One || DrawColourInfo.Colour.MinAlpha < 1)
                    return;

                ++depthIndex;

                base.DrawDepth(vertexAction, true, ref depthIndex);
            }

            public override void Draw(Action<TexturedVertex2D> vertexAction)
            {
            }
        }
    }
}
