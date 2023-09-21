// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;
using nkast.Wasm.Canvas;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteGraphicsDevice : GraphicsDeviceStrategy
    {
        private readonly Dictionary<int, ShaderProgram> _programCache = new Dictionary<int, ShaderProgram>();

        internal bool _supportsInvalidateFramebuffer;
        internal bool _supportsBlitFramebuffer;

        internal WebGLFramebuffer _glDefaultFramebuffer = null;


        internal ConcreteGraphicsDevice(GraphicsDevice device, GraphicsAdapter adapter, GraphicsProfile graphicsProfile, bool preferHalfPixelOffset, PresentationParameters presentationParameters)
            : base(device, adapter, graphicsProfile, preferHalfPixelOffset, presentationParameters)
        {
        }


        public override void Reset(PresentationParameters presentationParameters)
        {
            PresentationParameters = presentationParameters;
            Reset();
        }

        public override void Reset()
        {
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


        internal void PlatformSetup()
        {
            // create context.
            _mainContext = new GraphicsContext(this);
            GraphicsExtensions.GL = _mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>().GlContext; // for GraphicsExtensions.CheckGLError()
            //_glContext = new LogContent(_glContext);

            _capabilities = new ConcreteGraphicsCapabilities();
            ((ConcreteGraphicsCapabilities)_capabilities).PlatformInitialize(this);


            _mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>()._newEnabledVertexAttributes = new bool[this.Capabilities.MaxVertexBufferSlots];
        }

        internal void PlatformInitialize()
        {
            // set actual backbuffer size
            PresentationParameters.BackBufferWidth = _mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>().GlContext.Canvas.Width;
            PresentationParameters.BackBufferHeight = _mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>().GlContext.Canvas.Height;

            _mainContext.Strategy._viewport = new Viewport(0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);

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
            _mainContext.Strategy._actualBlendState.PlatformApplyState(_mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>(), true);
            _mainContext.Strategy._actualDepthStencilState.PlatformApplyState(_mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>(), true);
            _mainContext.Strategy._actualRasterizerState.PlatformApplyState(_mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>(), true);

            _mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>()._bufferBindingInfos = new BufferBindingInfo[this.Capabilities.MaxVertexBufferSlots];
            for (int i = 0; i < _mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>()._bufferBindingInfos.Length; i++)
                _mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>()._bufferBindingInfos[i] = new BufferBindingInfo(null, IntPtr.Zero, 0,  null);
        }

        internal ShaderProgram GetProgram(VertexShader vertexShader, PixelShader pixelShader, int shaderProgramHash)
        {   
            ShaderProgram shaderProgram;
            if(_programCache.TryGetValue(shaderProgramHash, out shaderProgram))
                return shaderProgram;

            // the key does not exist so we need to link the programs
            shaderProgram = CreateProgram(vertexShader, pixelShader);
            _programCache.Add(shaderProgramHash, shaderProgram);
            return shaderProgram;
        }

        private ShaderProgram CreateProgram(VertexShader vertexShader, PixelShader pixelShader)
        {
            var GL = CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            var program = GL.CreateProgram();
            GraphicsExtensions.CheckGLError();

            GL.AttachShader(program, ((ConcreteVertexShader)vertexShader.Strategy).GetVertexShaderHandle());
            GraphicsExtensions.CheckGLError();

            GL.AttachShader(program, ((ConcretePixelShader)pixelShader.Strategy).GetPixelShaderHandle());
            GraphicsExtensions.CheckGLError();

            //vertexShader.BindVertexAttributes(program);

            GL.LinkProgram(program);
            GraphicsExtensions.CheckGLError();

            GL.UseProgram(program);
            GraphicsExtensions.CheckGLError();

            ((ConcreteVertexShader)vertexShader.Strategy).GetVertexAttributeLocations(program);

            ((ConcretePixelShader)pixelShader.Strategy).ApplySamplerTextureUnits(program);

            bool linkStatus;
            linkStatus = GL.GetProgramParameter(program, WebGLProgramStatus.LINK);

            if (linkStatus == true)
            {
                return new ShaderProgram(program);
            }
            else
            {
                var log = GL.GetProgramInfoLog(program);
                vertexShader.Dispose();
                pixelShader.Dispose();
                program.Dispose();
                throw new InvalidOperationException("Unable to link effect program");
            }
        }

        private void ClearProgramCache()
        {
            foreach (ShaderProgram shaderProgram in _programCache.Values)
            {
                shaderProgram.Program.Dispose();
            }
            _programCache.Clear();
        }

        internal int GetMaxMultiSampleCount(SurfaceFormat surfaceFormat)
        {
            var GL = CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            int maxMultiSampleCount = 0;
            return maxMultiSampleCount;
        }


        internal override GraphicsContextStrategy CreateGraphicsContextStrategy(GraphicsContext context)
        {
            IntPtr handle = PresentationParameters.DeviceWindowHandle;
            GameWindow gameWindow = BlazorGameWindow.FromHandle(handle);
            Canvas canvas = ((BlazorGameWindow)gameWindow)._canvas;

            IWebGLRenderingContext glContext = canvas.GetContext<IWebGLRenderingContext>();

            return new ConcreteGraphicsContext(context, glContext);
        }

        public override System.Reflection.Assembly ConcreteAssembly
        {
            get { return ReflectionHelpers.GetAssembly(typeof(ConcreteGraphicsDevice)); }
        }

        public override string ResourceNameAlphaTestEffect { get { return "Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.ogl.fxo"; } }
        public override string ResourceNameBasicEffect { get { return "Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.ogl.fxo"; } }
        public override string ResourceNameDualTextureEffect { get { return "Microsoft.Xna.Framework.Graphics.Effect.Resources.DualTextureEffect.ogl.fxo"; } }
        public override string ResourceNameEnvironmentMapEffect { get { return "Microsoft.Xna.Framework.Graphics.Effect.Resources.EnvironmentMapEffect.ogl.fxo"; } }
        public override string ResourceNameSkinnedEffect { get { return "Microsoft.Xna.Framework.Graphics.Effect.Resources.SkinnedEffect.ogl.fxo"; } }
        public override string ResourceNameSpriteEffect { get { return "Microsoft.Xna.Framework.Graphics.Effect.Resources.SpriteEffect.ogl.fxo"; } }

        internal void OnPresentationChanged()
        {
            _mainContext.ApplyRenderTargets(null);
        }


        protected override void Dispose(bool disposing)
        {
            System.Diagnostics.Debug.Assert(!IsDisposed);

            if (disposing)
            {

            }

            base.Dispose(disposing);
        }

    }
}
