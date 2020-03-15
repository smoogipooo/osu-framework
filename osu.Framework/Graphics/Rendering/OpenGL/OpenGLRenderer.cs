// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.OpenGL.Buffers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering.Intents;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Platform;
using osuTK;
using osuTK.Graphics;
using osuTK.Graphics.ES30;

namespace osu.Framework.Graphics.Rendering.OpenGL
{
    public abstract class OpenGLRenderer : IRenderer
    {
        public ref readonly MaskingInfo CurrentMaskingInfo => ref currentMaskingInfo;
        private MaskingInfo currentMaskingInfo;

        public RectangleI Viewport { get; private set; }
        public RectangleF Ortho { get; private set; }
        public RectangleI Scissor { get; private set; }
        public Vector2I ScissorOffset { get; private set; }
        public Matrix4 ProjectionMatrix { get; set; }
        public DepthInfo CurrentDepthInfo { get; private set; }
        public float BackbufferDrawDepth { get; private set; }

        private readonly Stack<MaskingInfo> maskingStack = new Stack<MaskingInfo>();
        private readonly Stack<RectangleI> scissorRectStack = new Stack<RectangleI>();
        private readonly Stack<DepthInfo> depthStack = new Stack<DepthInfo>();
        private readonly Stack<bool> scissorStateStack = new Stack<bool>();
        private readonly Stack<RectangleI> viewportStack = new Stack<RectangleI>();
        private readonly Stack<Vector2I> scissorOffsetStack = new Stack<Vector2I>();
        private readonly Stack<RectangleF> orthoStack = new Stack<RectangleF>();

        private readonly List<IIntent> intents = new List<IIntent>();

        private readonly IWindow window;

        protected OpenGLRenderer(IWindow window)
        {
            this.window = window;
        }

        public void BeginFrame(Vector2 size)
        {
            lastBlendingParameters = new BlendingParameters();
            lastBlendingEnabledState = null;

            viewportStack.Clear();
            orthoStack.Clear();
            maskingStack.Clear();
            scissorRectStack.Clear();
            depthStack.Clear();
            scissorStateStack.Clear();
            scissorOffsetStack.Clear();

            Scissor = RectangleI.Empty;
            ScissorOffset = Vector2I.Zero;
            Viewport = RectangleI.Empty;
            Ortho = RectangleF.Empty;

            handlePushScissorState(true);
            handlePushViewport(new RectangleI(0, 0, (int)size.X, (int)size.Y));
            handlePushScissor(new RectangleI(0, 0, (int)size.X, (int)size.Y));
            handlePushScissorOffset(Vector2I.Zero);
            handlePushMaskingInfo(new MaskingInfo
            {
                ScreenSpaceAABB = new RectangleI(0, 0, (int)size.X, (int)size.Y),
                MaskingRect = new RectangleF(0, 0, size.X, size.Y),
                ToMaskingSpace = Matrix3.Identity,
                BlendRange = 1,
                AlphaExponent = 1,
                CornerExponent = 2.5f,
            }, true);

            handlePushDepthInfo(DepthInfo.Default);
            handleClear(new ClearInfo(Color4.Black));
        }

        public void FinishFrame()
        {
            Flush();
            Swap();
        }

        public virtual void Add<TIntent>(in TIntent intent)
            where TIntent : IIntent
            => intents.Add(intent);

        public void Flush()
        {
            foreach (var intent in intents)
                handle(intent);
            intents.Clear();
        }

        /// <summary>
        /// Swap the buffers.
        /// </summary>
        protected virtual void Swap()
        {
            window.SwapBuffers();

            if (window.VSync == VSyncMode.On)
                // without glFinish, vsync is basically unplayable due to the extra latency introduced.
                // we will likely want to give the user control over this in the future as an advanced setting.
                GL.Finish();
        }

        private void handle(in IIntent intent)
        {
            switch (intent)
            {
                case ClearIntent i:
                    handleClear(i.ClearInfo);
                    break;

                case PushScissorStateIntent i:
                    handlePushScissorState(i.ShouldScissor);
                    break;

                case PopScissorStateIntent _:
                    handlePopScissorState();
                    break;

                case PushScissorIntent i:
                    handlePushScissor(i.Scissor);
                    break;

                case PopScissorIntent _:
                    handlePopScissor();
                    break;

                case PushScissorOffsetIntent i:
                    handlePushScissorOffset(i.Offset);
                    break;

                case PopScissorOffsetIntent _:
                    handlePopScissorOffset();
                    break;

                case SetBlendIntent i:
                    handleSetBlend(i.Parameters);
                    break;

                case PushViewportIntent i:
                    handlePushViewport(i.Viewport);
                    break;

                case PopViewportIntent _:
                    handlePopViewport();
                    break;

                case PushOrthoIntent i:
                    handlePushOrtho(i.Ortho);
                    break;

                case PopOrthoIntent _:
                    handlePopOrtho();
                    break;

                case PushMaskingIntent i:
                    handlePushMaskingInfo(i.MaskingInfo);
                    break;

                case PopMaskingIntent _:
                    handlePopMaskingInfo();
                    break;

                case PushDepthIntent i:
                    handlePushDepthInfo(i.DepthInfo);
                    break;

                case PopDepthIntent _:
                    handlePopDepthInfo();
                    break;

                case DrawDepthIntent i:
                    handleSetDrawDepth(i.DrawDepth);
                    break;
            }
        }

        private ClearInfo currentClearInfo;

        private void handleClear(ClearInfo clearInfo)
        {
            handlePushDepthInfo(new DepthInfo(writeDepth: true));
            handlePushScissorState(false);
            if (clearInfo.Colour != currentClearInfo.Colour)
                GL.ClearColor(clearInfo.Colour);

            if (clearInfo.Depth != currentClearInfo.Depth)
            {
                if (GLWrapper.IsEmbedded)
                {
                    // GL ES only supports glClearDepthf
                    // See: https://www.khronos.org/registry/OpenGL-Refpages/es3.0/html/glClearDepthf.xhtml
                    GL.ClearDepth((float)clearInfo.Depth);
                }
                else
                {
                    // Older desktop platforms don't support glClearDepthf, so standard GL's double version is used instead
                    // See: https://www.khronos.org/registry/OpenGL-Refpages/gl4/html/glClearDepth.xhtml
                    osuTK.Graphics.OpenGL.GL.ClearDepth(clearInfo.Depth);
                }
            }

            if (clearInfo.Stencil != currentClearInfo.Stencil)
                GL.ClearStencil(clearInfo.Stencil);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            currentClearInfo = clearInfo;

            handlePopScissorState();
            handlePopDepthInfo();
        }

        private bool currentScissorState;

        private void handlePushScissorState(bool enabled)
        {
            scissorStateStack.Push(enabled);
            setScissorState(enabled);
        }

        private void handlePopScissorState()
        {
            Trace.Assert(scissorStateStack.Count > 1);

            scissorStateStack.Pop();

            setScissorState(scissorStateStack.Peek());
        }

        private void setScissorState(bool enabled)
        {
            if (enabled == currentScissorState)
                return;

            currentScissorState = enabled;

            if (enabled)
                GL.Enable(EnableCap.ScissorTest);
            else
                GL.Disable(EnableCap.ScissorTest);
        }

        private BlendingParameters lastBlendingParameters;
        private bool? lastBlendingEnabledState;

        /// <summary>
        /// Sets the blending function to draw with.
        /// </summary>
        /// <param name="blendingParameters">The info we should use to update the active state.</param>
        private void handleSetBlend(BlendingParameters blendingParameters)
        {
            if (lastBlendingParameters == blendingParameters)
                return;

            GLWrapper.FlushCurrentBatch();

            if (blendingParameters.IsDisabled)
            {
                if (!lastBlendingEnabledState.HasValue || lastBlendingEnabledState.Value)
                    GL.Disable(EnableCap.Blend);

                lastBlendingEnabledState = false;
            }
            else
            {
                if (!lastBlendingEnabledState.HasValue || !lastBlendingEnabledState.Value)
                    GL.Enable(EnableCap.Blend);

                lastBlendingEnabledState = true;

                GL.BlendEquationSeparate(blendingParameters.RGBEquationMode, blendingParameters.AlphaEquationMode);
                GL.BlendFuncSeparate(blendingParameters.SourceBlendingFactor, blendingParameters.DestinationBlendingFactor,
                    blendingParameters.SourceAlphaBlendingFactor, blendingParameters.DestinationAlphaBlendingFactor);
            }

            lastBlendingParameters = blendingParameters;
        }

        /// <summary>
        /// Applies a new viewport rectangle.
        /// </summary>
        /// <param name="viewport">The viewport rectangle.</param>
        private void handlePushViewport(RectangleI viewport)
        {
            var actualRect = viewport;

            if (actualRect.Width < 0)
            {
                actualRect.X += viewport.Width;
                actualRect.Width = -viewport.Width;
            }

            if (actualRect.Height < 0)
            {
                actualRect.Y += viewport.Height;
                actualRect.Height = -viewport.Height;
            }

            handlePushOrtho(viewport);

            viewportStack.Push(actualRect);

            if (Viewport == actualRect)
                return;

            Viewport = actualRect;

            GL.Viewport(Viewport.Left, Viewport.Top, Viewport.Width, Viewport.Height);
        }

        /// <summary>
        /// Applies the last viewport rectangle.
        /// </summary>
        private void handlePopViewport()
        {
            Trace.Assert(viewportStack.Count > 1);

            handlePopOrtho();

            viewportStack.Pop();
            RectangleI actualRect = viewportStack.Peek();

            if (Viewport == actualRect)
                return;

            Viewport = actualRect;

            GL.Viewport(Viewport.Left, Viewport.Top, Viewport.Width, Viewport.Height);
        }

        /// <summary>
        /// Applies a new scissor rectangle.
        /// </summary>
        /// <param name="scissor">The scissor rectangle.</param>
        private void handlePushScissor(RectangleI scissor)
        {
            GLWrapper.FlushCurrentBatch();

            scissorRectStack.Push(scissor);
            if (Scissor == scissor)
                return;

            Scissor = scissor;
            setScissor(scissor);
        }

        /// <summary>
        /// Applies the last scissor rectangle.
        /// </summary>
        private void handlePopScissor()
        {
            Trace.Assert(scissorRectStack.Count > 1);

            GLWrapper.FlushCurrentBatch();

            scissorRectStack.Pop();
            RectangleI scissor = scissorRectStack.Peek();

            if (Scissor == scissor)
                return;

            Scissor = scissor;
            setScissor(scissor);
        }

        private void setScissor(RectangleI scissor)
        {
            if (scissor.Width < 0)
            {
                scissor.X += scissor.Width;
                scissor.Width = -scissor.Width;
            }

            if (scissor.Height < 0)
            {
                scissor.Y += scissor.Height;
                scissor.Height = -scissor.Height;
            }

            GL.Scissor(scissor.X, Viewport.Height - scissor.Bottom, scissor.Width, scissor.Height);
        }

        /// <summary>
        /// Applies an offset to the scissor rectangle.
        /// </summary>
        /// <param name="offset">The offset.</param>
        private void handlePushScissorOffset(Vector2I offset)
        {
            GLWrapper.FlushCurrentBatch();

            scissorOffsetStack.Push(offset);
            if (ScissorOffset == offset)
                return;

            ScissorOffset = offset;
        }

        /// <summary>
        /// Applies the last scissor rectangle offset.
        /// </summary>
        private void handlePopScissorOffset()
        {
            Trace.Assert(scissorOffsetStack.Count > 1);

            GLWrapper.FlushCurrentBatch();

            scissorOffsetStack.Pop();
            Vector2I offset = scissorOffsetStack.Peek();

            if (ScissorOffset == offset)
                return;

            ScissorOffset = offset;
        }

        /// <summary>
        /// Applies a new orthographic projection rectangle.
        /// </summary>
        /// <param name="ortho">The orthographic projection rectangle.</param>
        private void handlePushOrtho(RectangleF ortho)
        {
            GLWrapper.FlushCurrentBatch();

            orthoStack.Push(ortho);
            if (Ortho == ortho)
                return;

            Ortho = ortho;

            ProjectionMatrix = Matrix4.CreateOrthographicOffCenter(Ortho.Left, Ortho.Right, Ortho.Bottom, Ortho.Top, -1, 1);
            GlobalPropertyManager.Set(GlobalProperty.ProjMatrix, ProjectionMatrix);
        }

        /// <summary>
        /// Applies the last orthographic projection rectangle.
        /// </summary>
        private void handlePopOrtho()
        {
            Trace.Assert(orthoStack.Count > 1);

            GLWrapper.FlushCurrentBatch();

            orthoStack.Pop();
            RectangleF actualRect = orthoStack.Peek();

            if (Ortho == actualRect)
                return;

            Ortho = actualRect;

            ProjectionMatrix = Matrix4.CreateOrthographicOffCenter(Ortho.Left, Ortho.Right, Ortho.Bottom, Ortho.Top, -1, 1);
            GlobalPropertyManager.Set(GlobalProperty.ProjMatrix, ProjectionMatrix);
        }

        public bool IsMaskingActive => maskingStack.Count > 1;

        /// <summary>
        /// Applies a new scissor rectangle.
        /// </summary>
        /// <param name="maskingInfo">The masking info.</param>
        /// <param name="overwritePreviousScissor">Whether or not to shrink an existing scissor rectangle.</param>
        private void handlePushMaskingInfo(in MaskingInfo maskingInfo, bool overwritePreviousScissor = false)
        {
            maskingStack.Push(maskingInfo);
            if (CurrentMaskingInfo == maskingInfo)
                return;

            currentMaskingInfo = maskingInfo;
            setMaskingInfo(CurrentMaskingInfo, true, overwritePreviousScissor);
        }

        /// <summary>
        /// Applies the last scissor rectangle.
        /// </summary>
        private void handlePopMaskingInfo()
        {
            Trace.Assert(maskingStack.Count > 1);

            maskingStack.Pop();
            MaskingInfo maskingInfo = maskingStack.Peek();

            if (CurrentMaskingInfo == maskingInfo)
                return;

            currentMaskingInfo = maskingInfo;
            setMaskingInfo(CurrentMaskingInfo, false, true);
        }

        private void setMaskingInfo(MaskingInfo maskingInfo, bool isPushing, bool overwritePreviousScissor)
        {
            GLWrapper.FlushCurrentBatch();

            GlobalPropertyManager.Set(GlobalProperty.MaskingRect, new Vector4(
                maskingInfo.MaskingRect.Left,
                maskingInfo.MaskingRect.Top,
                maskingInfo.MaskingRect.Right,
                maskingInfo.MaskingRect.Bottom));

            GlobalPropertyManager.Set(GlobalProperty.ToMaskingSpace, maskingInfo.ToMaskingSpace);

            GlobalPropertyManager.Set(GlobalProperty.CornerRadius, maskingInfo.CornerRadius);
            GlobalPropertyManager.Set(GlobalProperty.CornerExponent, maskingInfo.CornerExponent);

            GlobalPropertyManager.Set(GlobalProperty.BorderThickness, maskingInfo.BorderThickness / maskingInfo.BlendRange);

            if (maskingInfo.BorderThickness > 0)
            {
                GlobalPropertyManager.Set(GlobalProperty.BorderColour, new Vector4(
                    maskingInfo.BorderColour.Linear.R,
                    maskingInfo.BorderColour.Linear.G,
                    maskingInfo.BorderColour.Linear.B,
                    maskingInfo.BorderColour.Linear.A));
            }

            GlobalPropertyManager.Set(GlobalProperty.MaskingBlendRange, maskingInfo.BlendRange);
            GlobalPropertyManager.Set(GlobalProperty.AlphaExponent, maskingInfo.AlphaExponent);

            GlobalPropertyManager.Set(GlobalProperty.EdgeOffset, maskingInfo.EdgeOffset);

            GlobalPropertyManager.Set(GlobalProperty.DiscardInner, maskingInfo.Hollow);
            if (maskingInfo.Hollow)
                GlobalPropertyManager.Set(GlobalProperty.InnerCornerRadius, maskingInfo.HollowCornerRadius);

            if (isPushing)
            {
                // When drawing to a viewport that doesn't match the projection size (e.g. via framebuffers), the resultant image will be scaled
                Vector2 viewportScale = Vector2.Divide(Viewport.Size, Ortho.Size);

                Vector2 location = (maskingInfo.ScreenSpaceAABB.Location - ScissorOffset) * viewportScale;
                Vector2 size = maskingInfo.ScreenSpaceAABB.Size * viewportScale;

                RectangleI actualRect = new RectangleI(
                    (int)Math.Floor(location.X),
                    (int)Math.Floor(location.Y),
                    (int)Math.Ceiling(size.X),
                    (int)Math.Ceiling(size.Y));

                handlePushScissor(overwritePreviousScissor ? actualRect : RectangleI.Intersect(scissorRectStack.Peek(), actualRect));
            }
            else
                handlePopScissor();
        }

        /// <summary>
        /// Applies a new depth information.
        /// </summary>
        /// <param name="depthInfo">The depth information.</param>
        private void handlePushDepthInfo(DepthInfo depthInfo)
        {
            depthStack.Push(depthInfo);

            if (CurrentDepthInfo.Equals(depthInfo))
                return;

            CurrentDepthInfo = depthInfo;
            setDepthInfo(CurrentDepthInfo);
        }

        /// <summary>
        /// Applies the last depth information.
        /// </summary>
        private void handlePopDepthInfo()
        {
            Trace.Assert(depthStack.Count > 1);

            depthStack.Pop();
            DepthInfo depthInfo = depthStack.Peek();

            if (CurrentDepthInfo.Equals(depthInfo))
                return;

            CurrentDepthInfo = depthInfo;
            setDepthInfo(CurrentDepthInfo);
        }

        private void setDepthInfo(DepthInfo depthInfo)
        {
            GLWrapper.FlushCurrentBatch();

            if (depthInfo.DepthTest)
            {
                GL.Enable(EnableCap.DepthTest);
                GL.DepthFunc(depthInfo.Function);
            }
            else
                GL.Disable(EnableCap.DepthTest);

            GL.DepthMask(depthInfo.WriteDepth);
        }

        /// <summary>
        /// Sets the current draw depth.
        /// The draw depth is written to every vertex added to <see cref="VertexBuffer{T}"/>s.
        /// </summary>
        /// <param name="drawDepth">The draw depth.</param>
        private void handleSetDrawDepth(float drawDepth) => BackbufferDrawDepth = drawDepth;
    }
}
