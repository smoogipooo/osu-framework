// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Development;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Threading;
using osuTK;
using osuTK.Graphics;
using osuTK.Graphics.ES30;
using osu.Framework.Statistics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Rendering.OpenGL;
using osu.Framework.Platform;
using osu.Framework.Timing;
using GameWindow = osu.Framework.Platform.GameWindow;

namespace osu.Framework.Graphics.OpenGL
{
    public static class GLWrapper
    {
        /// <summary>
        /// Maximum number of <see cref="DrawNode"/>s a <see cref="Drawable"/> can draw with.
        /// This is a carefully-chosen number to enable the update and draw threads to work concurrently without causing unnecessary load.
        /// </summary>
        public const int MAX_DRAW_NODES = 3;

        public static bool UsingBackbuffer => frame_buffer_stack.Peek() == DefaultFrameBuffer;

        private static readonly Stack<int> frame_buffer_stack = new Stack<int>();

        public static bool IsMaskingActive => ((ImmediateOpenGLRenderer)Renderer).IsMaskingActive;

        public static ref readonly MaskingInfo CurrentMaskingInfo => ref ((ImmediateOpenGLRenderer)Renderer).CurrentMaskingInfo;

        public static float BackbufferDrawDepth => ((ImmediateOpenGLRenderer)Renderer).BackbufferDrawDepth;

        public static int DefaultFrameBuffer { get; internal set; }

        internal static IRenderer Renderer;

        public static bool IsEmbedded { get; internal set; }

        /// <summary>
        /// Check whether we have an initialised and non-disposed GL context.
        /// </summary>
        public static bool HasContext => GraphicsContext.CurrentContext != null;

        public static int MaxTextureSize { get; private set; } = 4096; // default value is to allow roughly normal flow in cases we don't have a GL context, like headless CI.
        public static int MaxRenderBufferSize { get; private set; } = 4096; // default value is to allow roughly normal flow in cases we don't have a GL context, like headless CI.

        /// <summary>
        /// The maximum number of texture uploads to dequeue and upload per frame.
        /// Defaults to 32.
        /// </summary>
        public static int MaxTexturesUploadedPerFrame { get; set; } = 32;

        /// <summary>
        /// The maximum number of pixels to upload per frame.
        /// Defaults to 2 megapixels (8mb alloc).
        /// </summary>
        public static int MaxPixelsUploadedPerFrame { get; set; } = 1024 * 1024 * 2;

        private static readonly Scheduler reset_scheduler = new Scheduler(() => ThreadSafety.IsDrawThread, new StopwatchClock(true)); // force no thread set until we are actually on the draw thread.

        /// <summary>
        /// A queue from which a maximum of one operation is invoked per draw frame.
        /// </summary>
        private static readonly ConcurrentQueue<Action> expensive_operation_queue = new ConcurrentQueue<Action>();

        private static readonly ConcurrentQueue<TextureGL> texture_upload_queue = new ConcurrentQueue<TextureGL>();

        private static readonly List<IVertexBatch> batch_reset_list = new List<IVertexBatch>();

        public static bool IsInitialized { get; private set; }

        private static WeakReference<GameHost> host;

        internal static void Initialize(GameHost host)
        {
            if (IsInitialized) return;

            if (host.Window is GameWindow win)
                IsEmbedded = win.IsEmbedded;

            GLWrapper.host = new WeakReference<GameHost>(host);

            MaxTextureSize = GL.GetInteger(GetPName.MaxTextureSize);
            MaxRenderBufferSize = GL.GetInteger(GetPName.MaxRenderbufferSize);

            GL.Disable(EnableCap.StencilTest);
            GL.Enable(EnableCap.Blend);

            IsInitialized = true;
        }

        internal static void ScheduleDisposal(Action disposalAction)
        {
            int frameCount = 0;

            if (host != null && host.TryGetTarget(out GameHost h))
                h.UpdateThread.Scheduler.Add(scheduleNextDisposal);
            else
                disposalAction.Invoke();

            void scheduleNextDisposal() => reset_scheduler.Add(() =>
            {
                // There may be a number of DrawNodes queued to be drawn
                // Disposal should only take place after
                if (frameCount++ >= MAX_DRAW_NODES)
                    disposalAction.Invoke();
                else
                    scheduleNextDisposal();
            });
        }

        private static readonly GlobalStatistic<int> stat_expensive_operations_queued = GlobalStatistics.Get<int>(nameof(GLWrapper), "Expensive operation queue length");
        private static readonly GlobalStatistic<int> stat_texture_uploads_queued = GlobalStatistics.Get<int>(nameof(GLWrapper), "Texture upload queue length");
        private static readonly GlobalStatistic<int> stat_texture_uploads_dequeued = GlobalStatistics.Get<int>(nameof(GLWrapper), "Texture uploads dequeued");
        private static readonly GlobalStatistic<int> stat_texture_uploads_performed = GlobalStatistics.Get<int>(nameof(GLWrapper), "Texture uploads performed");

        internal static void Reset(Vector2 size)
        {
            Trace.Assert(shader_stack.Count == 0);

            reset_scheduler.Update();

            stat_expensive_operations_queued.Value = expensive_operation_queue.Count;
            if (expensive_operation_queue.TryDequeue(out Action action))
                action.Invoke();

            stat_texture_uploads_queued.Value = texture_upload_queue.Count;
            stat_texture_uploads_dequeued.Value = 0;
            stat_texture_uploads_performed.Value = 0;

            // increase the number of items processed with the queue length to ensure it doesn't get out of hand.
            int targetUploads = Math.Clamp(texture_upload_queue.Count / 2, 1, MaxTexturesUploadedPerFrame);
            int uploads = 0;
            int uploadedPixels = 0;

            // continue attempting to upload textures until enough uploads have been performed.
            while (texture_upload_queue.TryDequeue(out TextureGL texture))
            {
                stat_texture_uploads_dequeued.Value++;

                texture.IsQueuedForUpload = false;

                if (!texture.Upload())
                    continue;

                stat_texture_uploads_performed.Value++;

                if (++uploads >= targetUploads)
                    break;

                if ((uploadedPixels += texture.Width * texture.Height) > MaxPixelsUploadedPerFrame)
                    break;
            }

            Array.Clear(last_bound_texture, 0, last_bound_texture.Length);
            Array.Clear(last_bound_texture_is_atlas, 0, last_bound_texture_is_atlas.Length);

            lastActiveBatch = null;

            foreach (var b in batch_reset_list)
                b.ResetCounters();
            batch_reset_list.Clear();

            frame_buffer_stack.Clear();
            BindFrameBuffer(DefaultFrameBuffer);
        }

        /// <summary>
        /// Enqueues a texture to be uploaded in the next frame.
        /// </summary>
        /// <param name="texture">The texture to be uploaded.</param>
        public static void EnqueueTextureUpload(TextureGL texture)
        {
            if (texture.IsQueuedForUpload)
                return;

            if (host != null)
            {
                texture.IsQueuedForUpload = true;
                texture_upload_queue.Enqueue(texture);
            }
        }

        /// <summary>
        /// Enqueues the compile of a shader.
        /// </summary>
        /// <param name="shader">The shader to compile.</param>
        public static void EnqueueShaderCompile(Shader shader)
        {
            if (host != null)
                expensive_operation_queue.Enqueue(shader.EnsureLoaded);
        }

        private static IVertexBatch lastActiveBatch;

        /// <summary>
        /// Sets the last vertex batch used for drawing.
        /// <para>
        /// This is done so that various methods that change GL state can force-draw the batch
        /// before continuing with the state change.
        /// </para>
        /// </summary>
        /// <param name="batch">The batch.</param>
        internal static void SetActiveBatch(IVertexBatch batch)
        {
            if (lastActiveBatch == batch)
                return;

            batch_reset_list.Add(batch);

            FlushCurrentBatch();

            lastActiveBatch = batch;
        }

        internal static void FlushCurrentBatch()
        {
            lastActiveBatch?.Draw();
        }

        private static readonly int[] last_bound_buffers = new int[2];

        /// <summary>
        /// Bind an OpenGL buffer object.
        /// </summary>
        /// <param name="target">The buffer type to bind.</param>
        /// <param name="buffer">The buffer ID to bind.</param>
        /// <returns>Whether an actual bind call was necessary. This value is false when repeatedly binding the same buffer.</returns>
        public static bool BindBuffer(BufferTarget target, int buffer)
        {
            int bufferIndex = target - BufferTarget.ArrayBuffer;
            if (last_bound_buffers[bufferIndex] == buffer)
                return false;

            last_bound_buffers[bufferIndex] = buffer;
            GL.BindBuffer(target, buffer);

            FrameStatistics.Increment(StatisticsCounterType.VBufBinds);

            return true;
        }

        private static readonly int[] last_bound_texture = new int[16];
        private static readonly bool[] last_bound_texture_is_atlas = new bool[16];

        public static int GetTextureUnitId(TextureUnit unit) => (int)unit - (int)TextureUnit.Texture0;
        public static bool AtlasTextureIsBound(TextureUnit unit) => last_bound_texture_is_atlas[GetTextureUnitId(unit)];

        /// <summary>
        /// Binds a texture to draw with.
        /// </summary>
        /// <param name="texture">The texture to bind.</param>
        /// <param name="unit">The texture unit to bind it to.</param>
        /// <returns>true if the provided texture was not already bound (causing a binding change).</returns>
        public static bool BindTexture(TextureGL texture, TextureUnit unit = TextureUnit.Texture0)
        {
            bool didBind = BindTexture(texture?.TextureId ?? 0, unit);
            last_bound_texture_is_atlas[GetTextureUnitId(unit)] = texture is TextureGLAtlas;

            return didBind;
        }

        /// <summary>
        /// Binds a texture to draw with.
        /// </summary>
        /// <param name="textureId">The texture to bind.</param>
        /// <param name="unit">The texture unit to bind it to.</param>
        /// <returns>true if the provided texture was not already bound (causing a binding change).</returns>
        public static bool BindTexture(int textureId, TextureUnit unit = TextureUnit.Texture0)
        {
            var index = GetTextureUnitId(unit);

            if (last_bound_texture[index] == textureId)
                return false;

            FlushCurrentBatch();

            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            last_bound_texture[index] = textureId;
            last_bound_texture_is_atlas[GetTextureUnitId(unit)] = false;

            FrameStatistics.Increment(StatisticsCounterType.TextureBinds);
            return true;
        }

        /// <summary>
        /// Binds a framebuffer.
        /// </summary>
        /// <param name="frameBuffer">The framebuffer to bind.</param>
        public static void BindFrameBuffer(int frameBuffer)
        {
            if (frameBuffer == -1) return;

            bool alreadyBound = frame_buffer_stack.Count > 0 && frame_buffer_stack.Peek() == frameBuffer;

            frame_buffer_stack.Push(frameBuffer);

            if (!alreadyBound)
            {
                FlushCurrentBatch();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);
                GlobalPropertyManager.Set(GlobalProperty.BackbufferDraw, UsingBackbuffer);
            }

            GlobalPropertyManager.Set(GlobalProperty.GammaCorrection, UsingBackbuffer);
        }

        /// <summary>
        /// Binds a framebuffer.
        /// </summary>
        /// <param name="frameBuffer">The framebuffer to bind.</param>
        public static void UnbindFrameBuffer(int frameBuffer)
        {
            if (frameBuffer == -1) return;

            if (frame_buffer_stack.Peek() != frameBuffer)
                return;

            frame_buffer_stack.Pop();

            FlushCurrentBatch();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, frame_buffer_stack.Peek());

            GlobalPropertyManager.Set(GlobalProperty.BackbufferDraw, UsingBackbuffer);
            GlobalPropertyManager.Set(GlobalProperty.GammaCorrection, UsingBackbuffer);
        }

        /// <summary>
        /// Deletes a frame buffer.
        /// </summary>
        /// <param name="frameBuffer">The frame buffer to delete.</param>
        internal static void DeleteFrameBuffer(int frameBuffer)
        {
            if (frameBuffer == -1) return;

            while (frame_buffer_stack.Peek() == frameBuffer)
                UnbindFrameBuffer(frameBuffer);

            ScheduleDisposal(() => { GL.DeleteFramebuffer(frameBuffer); });
        }

        private static int currentShader;

        private static readonly Stack<int> shader_stack = new Stack<int>();

        public static void UseProgram(int? shader)
        {
            ThreadSafety.EnsureDrawThread();

            if (shader != null)
            {
                shader_stack.Push(shader.Value);
            }
            else
            {
                shader_stack.Pop();

                //check if the stack is empty, and if so don't restore the previous shader.
                if (shader_stack.Count == 0)
                    return;
            }

            int s = shader ?? shader_stack.Peek();

            if (currentShader == s) return;

            FrameStatistics.Increment(StatisticsCounterType.ShaderBinds);

            FlushCurrentBatch();

            GL.UseProgram(s);
            currentShader = s;
        }

        internal static void SetUniform<T>(IUniformWithValue<T> uniform)
            where T : struct, IEquatable<T>
        {
            if (uniform.Owner == currentShader)
                FlushCurrentBatch();

            switch (uniform)
            {
                case IUniformWithValue<bool> b:
                    GL.Uniform1(uniform.Location, b.GetValue() ? 1 : 0);
                    break;

                case IUniformWithValue<int> i:
                    GL.Uniform1(uniform.Location, i.GetValue());
                    break;

                case IUniformWithValue<float> f:
                    GL.Uniform1(uniform.Location, f.GetValue());
                    break;

                case IUniformWithValue<Vector2> v2:
                    GL.Uniform2(uniform.Location, ref v2.GetValueByRef());
                    break;

                case IUniformWithValue<Vector3> v3:
                    GL.Uniform3(uniform.Location, ref v3.GetValueByRef());
                    break;

                case IUniformWithValue<Vector4> v4:
                    GL.Uniform4(uniform.Location, ref v4.GetValueByRef());
                    break;

                case IUniformWithValue<Matrix2> m2:
                    GL.UniformMatrix2(uniform.Location, false, ref m2.GetValueByRef());
                    break;

                case IUniformWithValue<Matrix3> m3:
                    GL.UniformMatrix3(uniform.Location, false, ref m3.GetValueByRef());
                    break;

                case IUniformWithValue<Matrix4> m4:
                    GL.UniformMatrix4(uniform.Location, false, ref m4.GetValueByRef());
                    break;
            }
        }
    }

    public struct MaskingInfo : IEquatable<MaskingInfo>
    {
        public RectangleI ScreenSpaceAABB;
        public RectangleF MaskingRect;

        public Quad ConservativeScreenSpaceQuad;

        /// <summary>
        /// This matrix transforms screen space coordinates to masking space (likely the parent
        /// space of the container doing the masking).
        /// It is used by a shader to determine which pixels to discard.
        /// </summary>
        public Matrix3 ToMaskingSpace;

        public float CornerRadius;
        public float CornerExponent;

        public float BorderThickness;
        public SRGBColour BorderColour;

        public float BlendRange;
        public float AlphaExponent;

        public Vector2 EdgeOffset;

        public bool Hollow;
        public float HollowCornerRadius;

        public readonly bool Equals(MaskingInfo other) => this == other;

        public static bool operator ==(in MaskingInfo left, in MaskingInfo right) =>
            left.ScreenSpaceAABB == right.ScreenSpaceAABB &&
            left.MaskingRect == right.MaskingRect &&
            left.ToMaskingSpace == right.ToMaskingSpace &&
            left.CornerRadius == right.CornerRadius &&
            left.CornerExponent == right.CornerExponent &&
            left.BorderThickness == right.BorderThickness &&
            left.BorderColour.Equals(right.BorderColour) &&
            left.BlendRange == right.BlendRange &&
            left.AlphaExponent == right.AlphaExponent &&
            left.EdgeOffset == right.EdgeOffset &&
            left.Hollow == right.Hollow &&
            left.HollowCornerRadius == right.HollowCornerRadius;

        public static bool operator !=(in MaskingInfo left, in MaskingInfo right) => !(left == right);

        public override readonly bool Equals(object obj) => obj is MaskingInfo other && this == other;

        public override readonly int GetHashCode() => 0; // Shouldn't be used; simplifying implementation here.
    }
}
