// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.Utilities;
using nkast.Wasm.Canvas;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteGraphicsDevice : GraphicsDeviceStrategy
    {
        private readonly Dictionary<int, ShaderProgram> _programCache = new Dictionary<int, ShaderProgram>();

        internal Dictionary<int, ShaderProgram> ProgramCache { get { return _programCache; } }

        internal WebGLFramebuffer _glDefaultFramebuffer = null;


        internal ConcreteGraphicsDevice(GraphicsDevice device, GraphicsAdapter adapter, GraphicsProfile graphicsProfile, bool preferHalfPixelOffset, PresentationParameters presentationParameters)
            : base(device, adapter, graphicsProfile, preferHalfPixelOffset, presentationParameters)
        {
        }


        public override void Reset()
        {
            PlatformReset(this.PresentationParameters);
        }

        public override void Reset(PresentationParameters presentationParameters)
        {
            PlatformReset(presentationParameters);
        }

        private void PlatformReset(PresentationParameters presentationParameters)
        {
            this.PresentationParameters = presentationParameters;

            ((IPlatformGraphicsContext)_mainContext).Strategy.ApplyRenderTargets(null);
        }

        public override void Present(Rectangle? sourceRectangle, Rectangle? destinationRectangle, IntPtr overrideWindowHandle)
        {
            throw new NotImplementedException();
        }

        public override void Present()
        {
            base.Present();
        }

        public unsafe override void GetBackBufferData<T>(Rectangle? rect, T[] data, int startIndex, int elementCount)
        {
            if (PresentationParameters.RenderTargetUsage != RenderTargetUsage.PreserveContents)
                throw new InvalidOperationException(
                    "GetBackBufferData() on BlazorGL requires PresentationParameters.RenderTargetUsage to be set to PreserveContents.");

            ConcreteGraphicsContext context = ((IPlatformGraphicsContext)_mainContext).Strategy.ToConcrete<ConcreteGraphicsContext>();
            var GL = context.GL;

            ((IWebGL2RenderingContext)GL).BindFramebuffer(WebGL2FramebufferType.READ_FRAMEBUFFER, _glDefaultFramebuffer);
            GL.CheckGLError();
            ((IWebGL2RenderingContext)GL).ReadBuffer(WebGL2DrawBufferAttachmentPoint.BACK);
            GL.CheckGLError();

            Rectangle srcRect = rect ?? new Rectangle(0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);
            int fSize = PresentationParameters.BackBufferFormat.GetSize();
            int tSize = sizeof(T);
            int flippedY = PresentationParameters.BackBufferHeight - srcRect.Y - srcRect.Height;

            ConcreteTexture.ToGLSurfaceFormat(PresentationParameters.BackBufferFormat, context,
                        out WebGLInternalFormat glInternalFormat,
                        out WebGLFormat glFormat,
                        out WebGLTexelType glType,
                        out bool glIsCompressedFormat);

            GL.PixelStore(WebGLPixelParameter.PACK_ALIGNMENT, Math.Min(tSize, 8));
            GL.CheckGLError();

            GL.ReadPixels(srcRect.X, flippedY, srcRect.Width, srcRect.Height, glFormat, glType, data, startIndex, elementCount);
            GL.CheckGLError();

            // buffer is returned upside down, so we swap the rows around when copying over
            int rowSize = srcRect.Width * fSize / tSize;

            T[] row = new T[rowSize];

            for (int dy = 0; dy < srcRect.Height / 2; dy++)
            {
                int topRow = startIndex + dy * rowSize;
                int bottomRow = startIndex + (srcRect.Height - dy - 1) * rowSize;

                // copy the bottom row to buffer
                Array.Copy(data, bottomRow, row, 0, rowSize);

                // copy top row to bottom row
                Array.Copy(data, topRow, data, bottomRow, rowSize);

                // copy buffer to top row
                Array.Copy(row, 0, data, topRow, rowSize);
            }
        }

        protected override void PlatformInitialize()
        {
            // set actual backbuffer size
            PresentationParameters.BackBufferWidth = ((IPlatformGraphicsContext)_mainContext).Strategy.ToConcrete<ConcreteGraphicsContext>().GlContext.Canvas.Width;
            PresentationParameters.BackBufferHeight = ((IPlatformGraphicsContext)_mainContext).Strategy.ToConcrete<ConcreteGraphicsContext>().GlContext.Canvas.Height;
        }


        private void ClearProgramCache()
        {
            foreach (ShaderProgram shaderProgram in ProgramCache.Values)
            {
                shaderProgram.Program.Dispose();
            }
            ProgramCache.Clear();
        }

        internal int GetMaxMultiSampleCount(SurfaceFormat surfaceFormat)
        {
            switch (surfaceFormat)
            {
                case SurfaceFormat.Color:
                case SurfaceFormat.Bgr565:
                case SurfaceFormat.Bgra4444:
                case SurfaceFormat.Bgra5551:
                case SurfaceFormat.Single:
                case SurfaceFormat.HalfSingle:
                case SurfaceFormat.Vector2:
                case SurfaceFormat.HalfVector2:
                case SurfaceFormat.Vector4:
                case SurfaceFormat.HalfVector4:
                    // See: PlatformCreateRenderTarget(...)
                    // for the supported surface types.
                    break;

                default:
                    return 0;
            }

            var GL = ((IPlatformGraphicsContext)CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            int maxMultiSampleCount = 0;
            if (GL is IWebGL2RenderingContext)
                maxMultiSampleCount = ((IWebGL2RenderingContext)GL).GetParameter(WebGL2PNameInteger.MAX_SAMPLES);

            return maxMultiSampleCount;
        }


        public override GraphicsContextStrategy CreateGraphicsContextStrategy(GraphicsContext context)
        {
            IntPtr handle = PresentationParameters.DeviceWindowHandle;
            GameWindow gameWindow = BlazorGameWindow.FromHandle(handle);
            Canvas canvas = ((BlazorGameWindow)gameWindow)._canvas;

            ContextAttributes contextAttributes = new ContextAttributes();
            contextAttributes.PowerPreference = ContextAttributes.PowerPreferenceType.HighPerformance;

            contextAttributes.Antialias = (PresentationParameters.MultiSampleCount > 0);

            switch (PresentationParameters.DepthStencilFormat)
            {
                case DepthFormat.None:
                    contextAttributes.Depth = false;
                    contextAttributes.Stencil = false;
                    break;

                case DepthFormat.Depth16:
                case DepthFormat.Depth24:
                    contextAttributes.Depth = true;
                    contextAttributes.Stencil = false;
                    break;

                case DepthFormat.Depth24Stencil8:
                    contextAttributes.Depth = true;
                    contextAttributes.Stencil = true;
                    break;
            }

            switch (PresentationParameters.RenderTargetUsage)
            {
                case RenderTargetUsage.PreserveContents:
                    contextAttributes.PreserveDrawingBuffer = true;
                    break;

                case RenderTargetUsage.DiscardContents:
                    contextAttributes.PreserveDrawingBuffer = false;
                    break;

                case RenderTargetUsage.PlatformContents:
                    break;
            }

            IWebGLRenderingContext glContext = null;
            switch (this.GraphicsProfile)
            {
                case GraphicsProfile.Reach:
                    glContext = canvas.GetContext<IWebGLRenderingContext>(contextAttributes);
                    break;
                case GraphicsProfile.HiDef:
                    glContext = canvas.GetContext<IWebGL2RenderingContext>(contextAttributes);
                    break;
                case GraphicsProfile.FL10_0:
                    glContext = canvas.GetContext<IWebGL2RenderingContext>(contextAttributes);
                    break;

                default:
                    throw new NotSupportedException("GraphicsProfile "+ this.GraphicsProfile);
            }

            return new ConcreteGraphicsContext(context, glContext);
        }

        public override System.Reflection.Assembly ConcreteAssembly
        {
            get { return ReflectionHelpers.GetAssembly(typeof(ConcreteGraphicsDevice)); }
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            base.Dispose(disposing);
        }

    }
}
