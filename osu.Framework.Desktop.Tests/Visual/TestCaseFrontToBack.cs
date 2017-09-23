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
using osu.Framework.Graphics.Shaders;

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
                                EdgeSmoothness = new Vector2(2),
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

            protected override void ApplyDrawNode(DrawNode node)
            {
                var n = (FrontToBackContainerDrawNode)node;
                n.Enabled = Enabled;
                n.ScreenSpaceDrawRectangle = ScreenSpaceDrawQuad.AABBFloat;

                base.ApplyDrawNode(n);
            }

            private class FrontToBackContainerDrawNode : CompositeDrawNode
            {
                public RectangleF ScreenSpaceDrawRectangle;

                public bool Enabled;

                public override void Draw(Action<TexturedVertex2D> vertexAction)
                {
                    if (!Enabled)
                    {
                        base.Draw(vertexAction);
                        return;
                    }

                    if (Children == null)
                        return;

                    GLWrapper.SetDepthTest(true);

                    Shader.SetGlobalProperty("g_ForStencil", true);

                    GL.ColorMask(false, false, false, false);
                    GL.DepthMask(true);

                    GL.DepthFunc(DepthFunction.Less);
                    GL.ClearDepth(1);
                    GL.Clear(ClearBufferMask.DepthBufferBit);

                    const float depthIncrement = 1f / ushort.MaxValue;

                    for (int i = Children.Count - 1; i >= 0; i--)
                    {
                        float d = 1f - depthIncrement * (i + 1);
                        Children[i].Draw(v =>
                        {
                            v.Depth = d;
                            vertexAction(v);
                        });
                    }

                    GLWrapper.FlushCurrentBatch(); // Todo: This shouldn't be needed
                    Shader.SetGlobalProperty("g_ForStencil", false);

                    GL.ColorMask(true, true, true, true);
                    GL.DepthMask(false);

                    GL.DepthFunc(DepthFunction.Lequal);

                    for (int i = 0; i < Children.Count; i++)
                    {
                        float d = 1f - depthIncrement * (i + 1);
                        Children[i].Draw(v =>
                        {
                            v.Depth = d;
                            vertexAction(v);
                        });
                    }

                    GLWrapper.SetDepthTest(false);
                }
            }
        }
    }
}
