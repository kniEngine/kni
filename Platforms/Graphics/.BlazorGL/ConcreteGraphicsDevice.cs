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

        internal bool _supportsInvalidateFramebuffer;
        internal bool _supportsBlitFramebuffer;

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

        public override void GetBackBufferData<T>(Rectangle? rect, T[] data, int startIndex, int elementCount)
        {
            throw new NotImplementedException();
        }


        protected override void PlatformSetup(PresentationParameters presentationParameters)
        {
            _mainContext = base.CreateGraphicsContext();

            ((IPlatformGraphicsContext)_mainContext).Strategy.PlatformSetup();
       }

        protected override void PlatformInitialize()
        {
            // set actual backbuffer size
            PresentationParameters.BackBufferWidth = ((IPlatformGraphicsContext)_mainContext).Strategy.ToConcrete<ConcreteGraphicsContext>().GlContext.Canvas.Width;
            PresentationParameters.BackBufferHeight = ((IPlatformGraphicsContext)_mainContext).Strategy.ToConcrete<ConcreteGraphicsContext>().GlContext.Canvas.Height;

            ((IPlatformGraphicsContext)_mainContext).Strategy._viewport = new Viewport(0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);

            // TODO: check for FramebufferObjectARB
            //if (this.Capabilities.SupportsFramebufferObjectARB
            //||  this.Capabilities.SupportsFramebufferObjectEXT)
            if (true)
            {
                _supportsBlitFramebuffer = false; // GL.BlitFramebuffer != null;
                _supportsInvalidateFramebuffer = false; // GL.InvalidateFramebuffer != null;
            }
            else
            {
                throw new PlatformNotSupportedException(
                    "GraphicsDevice requires either ARB_framebuffer_object or EXT_framebuffer_object." +
                    "Try updating your graphics drivers.");
            }

            // Force resetting states
            ((IPlatformBlendState) ((IPlatformGraphicsContext)_mainContext).Strategy._actualBlendState).GetStrategy<ConcreteBlendState>().PlatformApplyState(((IPlatformGraphicsContext)_mainContext).Strategy.ToConcrete<ConcreteGraphicsContext>(), true);
            ((IPlatformDepthStencilState) ((IPlatformGraphicsContext)_mainContext).Strategy._actualDepthStencilState).GetStrategy<ConcreteDepthStencilState>().PlatformApplyState(((IPlatformGraphicsContext)_mainContext).Strategy.ToConcrete<ConcreteGraphicsContext>(), true);
            ((IPlatformRasterizerState) ((IPlatformGraphicsContext)_mainContext).Strategy._actualRasterizerState).GetStrategy<ConcreteRasterizerState>().PlatformApplyState(((IPlatformGraphicsContext)_mainContext).Strategy.ToConcrete<ConcreteGraphicsContext>(), true);

            ((IPlatformGraphicsContext)_mainContext).Strategy.ToConcrete<ConcreteGraphicsContext>()._bufferBindingInfos = new BufferBindingInfo[((IPlatformGraphicsContext)_mainContext).Strategy.Capabilities.MaxVertexBufferSlots];
            for (int i = 0; i < ((IPlatformGraphicsContext)_mainContext).Strategy.ToConcrete<ConcreteGraphicsContext>()._bufferBindingInfos.Length; i++)
                ((IPlatformGraphicsContext)_mainContext).Strategy.ToConcrete<ConcreteGraphicsContext>()._bufferBindingInfos[i] = new BufferBindingInfo(null, null, IntPtr.Zero, 0);
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
            var GL = ((IPlatformGraphicsContext)CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            int maxMultiSampleCount = 0;
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
