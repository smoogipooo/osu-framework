// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.MathUtils;
using osu.Framework.Testing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES20;
using osu.Framework.Graphics.OpenGL.Buffers;
using osu.Framework.Graphics.Primitives;

namespace osu.Framework.Desktop.Tests.Visual
{
    internal class TestCaseFrontToBack : TestCase
    {
        private FrontToBackContainer backgroundContainer;

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(backgroundContainer = new FrontToBackContainer { RelativeSizeAxes = Axes.Both });

            var buttonFlow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
            };

            for (int i = 25; i <= 500; i += 25)
            {
                var c = i;
                buttonFlow.Add(new Button
                {
                    Size = new Vector2(100, 50),
                    Text = $"{c}x fill",
                    BackgroundColour = new Color4(0.1f, 0.1f, 0.1f, 1),
                    Action = () =>
                    {
                        backgroundContainer.Clear();
                        for (int j = 0; j < c; j++)
                        {
                            backgroundContainer.Add(new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.White.Multiply((j + 1f) / c),
                                Height = (j + 1f) / c,
                                Depth = j
                            });
                        }
                    }
                });
            }

            Button enableButton = new Button
            {
                Size = new Vector2(200, 50),
                Text = "Enable ftb",
                BackgroundColour = new Color4(0.1f, 0.1f, 0.1f, 1),
            };

            var clickAction = new Action(() =>
            {
                backgroundContainer.Enabled = !backgroundContainer.Enabled;
                backgroundContainer.Invalidate(Invalidation.DrawNode);

                enableButton.Text = backgroundContainer.Enabled ? "Disable ftb" : "Enable ftb";
            });

            enableButton.Action = clickAction;

            buttonFlow.Add(enableButton);
            Add(buttonFlow);
        }

        private class FrontToBackContainer : Container
        {
            public bool Enabled;
            protected override bool CanBeFlattened => false;

            protected override DrawNode CreateDrawNode() => new FrontToBackContainerDrawNode();
            private readonly FrontToBackContainerSharedData shared = new FrontToBackContainerSharedData();

            protected override void ApplyDrawNode(DrawNode node)
            {
                var n = (FrontToBackContainerDrawNode)node;
                n.Shared = shared;
                n.Enabled = Enabled;
                n.ScreenSpaceDrawRectangle = ScreenSpaceDrawQuad.AABBFloat;

                base.ApplyDrawNode(n);
            }

            private class FrontToBackContainerSharedData
            {
                public readonly QuadBatch<TexturedVertex2D> QuadBatch = new QuadBatch<TexturedVertex2D>(200, 10);
            }

            private class FrontToBackContainerDrawNode : CompositeDrawNode
            {
                public RectangleF ScreenSpaceDrawRectangle;

                public bool Enabled;
                public new FrontToBackContainerSharedData Shared;

                public override void Draw(Action<TexturedVertex2D> vertexAction)
                {
                    if (!Enabled)
                    {
                        foreach (DrawNode c in Children)
                            c.Draw(vertexAction);
                        return;
                    }

                    if (Children == null)
                        return;

                    var forStencilUniform = Shader.GetUniform<bool>("g_ForStencil");

                    GLWrapper.SetStencilTest(true);

                    GL.StencilFunc(StencilFunction.Equal, 0xFF, 0xFF);
                    GL.StencilOp(StencilOp.Zero, StencilOp.Zero, StencilOp.Zero);

                    for (int i = Children.Count - 1; i >= 0; i--)
                    {
                        // Colour pass
                        GL.ColorMask(true, true, true, true);
                        GL.StencilMask(0);

                        forStencilUniform.Value = false;
                        Children[i].Draw(a => Shared.QuadBatch.Add(a));
                        Shared.QuadBatch.Draw();

                        // Stencil pass
                        GL.ColorMask(false, false, false, false);
                        GL.StencilMask(0xFF);

                        forStencilUniform.Value = true;
                        Children[i].Draw(a => Shared.QuadBatch.Add(a));
                        Shared.QuadBatch.Draw();
                    }

                    forStencilUniform.Value = false;
                    GL.ColorMask(true, true, true, true);

                    GLWrapper.SetStencilTest(false);
                }
            }
        }
    }
}
