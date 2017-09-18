﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
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
using osu.Framework.Graphics.Sprites;

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

                        backgroundContainer.Add(new OccludingBox { RelativeSizeAxes = Axes.Both });
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

        private class OccludingBox : Box
        {
            protected override DrawNode CreateDrawNode() => new OccludingBoxDrawNode();

            private class OccludingBoxDrawNode : SpriteDrawNode
            {
                public OccludingBoxDrawNode()
                {
                    Occluder = true;
                }
            }
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
                        foreach (DrawNode c in Children)
                            c.Draw(vertexAction);
                        return;
                    }

                    if (Children == null)
                        return;

                    var forStencilUniform = Shader.GetUniform<bool>("g_ForStencil");

                    GLWrapper.SetStencilTest(true);

                    GL.StencilOp(StencilOp.Keep, StencilOp.Replace, StencilOp.Replace);

                    // Perform a pre-pass to fill the stencil buffer
                    forStencilUniform.Value = true;
                    GL.ColorMask(false, false, false, false);
                    GL.StencilMask(255);

                    byte currentOccluder = 255;
                    for (int i = Children.Count - 1; i >= 0; i--)
                    {
                        if (!Children[i].Occluder)
                            continue;

                        GL.StencilFunc(StencilFunction.Gequal, currentOccluder, 0xFF);
                        Children[i].Draw(vertexAction);
                        GLWrapper.FlushCurrentBatch();

                        currentOccluder--;

                        if (currentOccluder < 0)
                            throw new Exception("Can't have more than 256 occluders.");
                    }

                    // Perform the colour pass - this can be done back-to-front due to the above stencil generation
                    forStencilUniform.Value = false;
                    GL.StencilFunc(StencilFunction.Gequal, currentOccluder, 0xFF);
                    GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
                    GL.ColorMask(true, true, true, true);
                    GL.StencilMask(0);

                    foreach (var child in Children)
                    {
                        if (child.Occluder)
                        {
                            GLWrapper.FlushCurrentBatch();

                            currentOccluder++;
                            GL.StencilFunc(StencilFunction.Gequal, currentOccluder, 0xFF);
                        }

                        child.Draw(vertexAction);
                    }

                    GLWrapper.SetStencilTest(false);
                }
            }
        }
    }
}
