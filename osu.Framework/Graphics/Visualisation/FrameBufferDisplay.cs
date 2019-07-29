// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.OpenGL.Buffers;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
using osuTK.Graphics.ES30;

namespace osu.Framework.Graphics.Visualisation
{
    public class FrameBufferDisplay : VisibilityContainer
    {
        private const float width = 600;

        private readonly FillFlowContainer<DrawNodeVisualiser> drawNodeVisualisers;

        public FrameBufferDisplay()
        {
            Width = width;
            RelativeSizeAxes = Axes.Y;

            Child = new BasicScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = drawNodeVisualisers = new FillFlowContainer<DrawNodeVisualiser>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical
                }
            };

            for (int i = 0; i < GLWrapper.MAX_DRAW_NODES; i++)
                drawNodeVisualisers.Add(new DrawNodeVisualiser(i));
        }

        protected override void PopIn()
        {
        }

        protected override void PopOut()
        {
        }

        public void UpdateFrom(IBufferedDrawable source)
        {
            foreach (var vis in drawNodeVisualisers)
                vis.UpdateFrom(source);
        }

        private class DrawNodeVisualiser : CompositeDrawable
        {
            private readonly int treeIndex;

            private readonly FillFlowContainer<BufferSet> bufferSets;
            private readonly ScrollContainer<Drawable> scroll;

            private Drawable source;
            private BufferedDrawNode sourceDrawNode;

            public DrawNodeVisualiser(int treeIndex)
            {
                this.treeIndex = treeIndex;

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                InternalChild = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(0, 5),
                    Children = new Drawable[]
                    {
                        new SpriteText
                        {
                            Text = $"Draw Node {treeIndex + 1}",
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 2,
                        },
                        scroll = new BasicScrollContainer(Direction.Horizontal)
                        {
                            RelativeSizeAxes = Axes.X,
                            Child = bufferSets = new FillFlowContainer<BufferSet>
                            {
                                AutoSizeAxes = Axes.Both,
                            }
                        },
                    }
                };
            }

            protected override void Update()
            {
                base.Update();

                scroll.Height = bufferSets.DrawHeight;
            }

            public void UpdateFrom(IBufferedDrawable source)
            {
                this.source = (Drawable)source;

                sourceDrawNode = null;
                bufferSets.Clear();
            }

            protected override bool CanBeFlattened => false;

            internal override DrawNode GenerateDrawNodeSubtree(ulong frame, int treeIndex, bool forceNewDrawNode)
            {
                var result = base.GenerateDrawNodeSubtree(frame, treeIndex, forceNewDrawNode);

                if (sourceDrawNode == null && source != null && this.treeIndex == treeIndex)
                {
                    sourceDrawNode = (BufferedDrawNode)source.GenerateDrawNodeSubtree(frame, treeIndex, false);
                    setDrawNode(sourceDrawNode);
                }

                return result;
            }

            private void setDrawNode(BufferedDrawNode drawNode)
            {
                bufferSets.Add(new BufferSet("main", drawNode.SharedData.MainBuffer));
                for (int i = 0; i < drawNode.SharedData.EffectBuffers.Length; i++)
                    bufferSets.Add(new BufferSet($"effect {i}", drawNode.SharedData.EffectBuffers[i]));
            }

            private class BufferSet : CompositeDrawable
            {
                public BufferSet(string name, FrameBuffer frameBuffer)
                {
                    AutoSizeAxes = Axes.Both;

                    FillFlowContainer<MaskVisualiser> visFlow;

                    InternalChild = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(2),
                        Children = new Drawable[]
                        {
                            new SpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Text = name
                            },
                            visFlow = new FillFlowContainer<MaskVisualiser>
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Direction = FillDirection.Vertical,
                                AutoSizeAxes = Axes.Both,
                                Spacing = new Vector2(2),
                            }
                        }
                    };

                    visFlow.Add(new MaskVisualiser(frameBuffer, ClearBufferMask.ColorBufferBit));

                    for (int i = 0; i < frameBuffer.AttachedRenderBuffers.Count; i++)
                    {
                        switch (frameBuffer.AttachedRenderBuffers[i].Format)
                        {
                            case RenderbufferInternalFormat.DepthComponent16:
                                visFlow.Add(new MaskVisualiser(frameBuffer, ClearBufferMask.DepthBufferBit));
                                break;

                            case RenderbufferInternalFormat.StencilIndex8:
                                visFlow.Add(new MaskVisualiser(frameBuffer, ClearBufferMask.StencilBufferBit));
                                break;
                        }
                    }
                }

                private class MaskVisualiser : Drawable, ITexturedShaderDrawable
                {
                    private readonly FrameBuffer frameBuffer;
                    private readonly ClearBufferMask mask;

                    public MaskVisualiser(FrameBuffer frameBuffer, ClearBufferMask mask)
                    {
                        this.frameBuffer = frameBuffer;
                        this.mask = mask;

                        Size = new Vector2(100);
                    }

                    public IShader TextureShader { get; private set; }
                    public IShader RoundedTextureShader { get; private set; }

                    [BackgroundDependencyLoader]
                    private void load(ShaderManager shaders)
                    {
                        TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE);
                        RoundedTextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE_ROUNDED);
                    }

                    protected override DrawNode CreateDrawNode() => new MaskVisualiserDrawNode(this);

                    private class MaskVisualiserDrawNode : TexturedShaderDrawNode
                    {
                        protected new MaskVisualiser Source => (MaskVisualiser)base.Source;

                        private Quad screenSpaceDrawQuad;
                        private ClearBufferMask mask;
                        private FrameBuffer sourceFrameBuffer;

                        private FramebufferAttachment currentAttachment;
                        private FrameBuffer drawFrameBuffer;
                        private bool refreshFrameBuffer = true;

                        public MaskVisualiserDrawNode(ITexturedShaderDrawable source)
                            : base(source)
                        {
                        }

                        public override void ApplyState()
                        {
                            base.ApplyState();

                            screenSpaceDrawQuad = Source.ScreenSpaceDrawQuad;
                            mask = Source.mask;
                            sourceFrameBuffer = Source.frameBuffer;

                            FramebufferAttachment requiredAttachment = 0;

                            switch (mask)
                            {
                                case ClearBufferMask.ColorBufferBit:
                                    requiredAttachment = FramebufferAttachment.ColorAttachment0;
                                    break;

                                case ClearBufferMask.DepthBufferBit:
                                    requiredAttachment = FramebufferAttachment.DepthAttachment;
                                    break;

                                case ClearBufferMask.StencilBufferBit:
                                    requiredAttachment = FramebufferAttachment.StencilAttachment;
                                    break;
                            }

                            if (currentAttachment != requiredAttachment)
                            {
                                currentAttachment = requiredAttachment;
                                refreshFrameBuffer = true;
                            }
                        }

                        public override void Draw(Action<TexturedVertex2D> vertexAction)
                        {
                            if (refreshFrameBuffer)
                            {
                                drawFrameBuffer?.Dispose();
                                drawFrameBuffer = new FrameBuffer();
                                drawFrameBuffer.Initialise(textureAttachment: currentAttachment);

                                refreshFrameBuffer = false;
                            }

                            drawFrameBuffer.Size = sourceFrameBuffer.Size;

                            GLWrapper.FlushCurrentBatch();

                            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, sourceFrameBuffer.FrameBufferId);
                            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, drawFrameBuffer.FrameBufferId);
                            GL.BlitFramebuffer(0, 0, (int)sourceFrameBuffer.Size.X, (int)sourceFrameBuffer.Size.Y, 0, 0, (int)drawFrameBuffer.Size.X, (int)drawFrameBuffer.Size.Y, mask,
                                BlitFramebufferFilter.Nearest);

                            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

                            base.Draw(vertexAction);

                            Shader.Bind();
                            DrawFrameBuffer(drawFrameBuffer, screenSpaceDrawQuad, Color4.White);
                            Shader.Unbind();
                        }

                        protected override void Dispose(bool isDisposing)
                        {
                            base.Dispose(isDisposing);

                            drawFrameBuffer?.Dispose();
                        }
                    }
                }
            }
        }
    }
}
