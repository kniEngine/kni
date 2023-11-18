// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;
using Microsoft.Xna.Platform.Graphics.OpenGL;


namespace Microsoft.Xna.Platform.Graphics
{
    internal abstract class ConcreteGraphicsDeviceGL : GraphicsDeviceStrategy
    {
        private readonly Dictionary<int, ShaderProgram> _programCache = new Dictionary<int, ShaderProgram>();

        internal Dictionary<int, ShaderProgram> ProgramCache { get { return _programCache; } }

        internal bool _supportsInvalidateFramebuffer;
        internal bool _supportsBlitFramebuffer;

        internal int _glDefaultFramebuffer = 0;

        internal int _glMajorVersion = 0;
        internal int _glMinorVersion = 0;


        internal ConcreteGraphicsDeviceGL(GraphicsDevice device, GraphicsAdapter adapter, GraphicsProfile graphicsProfile, bool preferHalfPixelOffset, PresentationParameters presentationParameters)
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

        public override void GetBackBufferData<T>(Rectangle? rect, T[] data, int startIndex, int elementCount)
        {
            var GL = _mainContext.Strategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

            Rectangle srcRect = rect ?? new Rectangle(0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);
            int tSize = ReflectionHelpers.SizeOf<T>();
            int flippedY = PresentationParameters.BackBufferHeight - srcRect.Y - srcRect.Height;
            GL.ReadPixels(srcRect.X, flippedY, srcRect.Width, srcRect.Height, PixelFormat.Rgba, PixelType.UnsignedByte, data);

            // buffer is returned upside down, so we swap the rows around when copying over
            int rowSize = srcRect.Width * PresentationParameters.BackBufferFormat.GetSize() / tSize;
            T[] row = new T[rowSize];
            for (int dy = 0; dy < srcRect.Height/2; dy++)
            {
                int topRow = startIndex + dy*rowSize;
                int bottomRow = startIndex + (srcRect.Height - dy - 1)*rowSize;
                // copy the bottom row to buffer
                Array.Copy(data, bottomRow, row, 0, rowSize);
                // copy top row to bottom row
                Array.Copy(data, topRow, data, bottomRow, rowSize);
                // copy buffer to top row
                Array.Copy(row, 0, data, topRow, rowSize);
                elementCount -= rowSize;
            }
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


        internal void PlatformInitialize()
        {
            var GL = _mainContext.Strategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

            _mainContext.Strategy._viewport = new Viewport(0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);

            if (((ConcreteGraphicsCapabilities)this.Capabilities).SupportsFramebufferObjectARB
            ||  ((ConcreteGraphicsCapabilities)this.Capabilities).SupportsFramebufferObjectEXT)
            {
                _supportsBlitFramebuffer = GL.BlitFramebuffer != null;
                _supportsInvalidateFramebuffer = GL.InvalidateFramebuffer != null;
            }
            else
            {
                throw new PlatformNotSupportedException(
                    "GraphicsDevice requires either ARB_framebuffer_object or EXT_framebuffer_object." +
                    "Try updating your graphics drivers.");
            }

            // Force resetting states
            _mainContext.Strategy._actualBlendState.PlatformApplyState((ConcreteGraphicsContextGL)_mainContext.Strategy, true);
            _mainContext.Strategy._actualDepthStencilState.PlatformApplyState((ConcreteGraphicsContextGL)_mainContext.Strategy, true);
            _mainContext.Strategy._actualRasterizerState.PlatformApplyState((ConcreteGraphicsContextGL)_mainContext.Strategy, true);

            _mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>()._bufferBindingInfos = new BufferBindingInfo[Capabilities.MaxVertexBufferSlots];
            for (int i = 0; i < _mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>()._bufferBindingInfos.Length; i++)
                _mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>()._bufferBindingInfos[i] = new BufferBindingInfo(null, IntPtr.Zero, 0, null);
        }


        internal void Android_ReInitializeContext()
        {
            var GL = _mainContext.Strategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

            _mainContext.Strategy._viewport = new Viewport(0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);

            // Ensure the vertex attributes are reset
            _mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>()._enabledVertexAttributes.Clear();

            // Free all the cached shader programs.
            ClearProgramCache();
            _mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>()._shaderProgram = null;

            if (((ConcreteGraphicsCapabilities)this.Capabilities).SupportsFramebufferObjectARB
            ||  ((ConcreteGraphicsCapabilities)this.Capabilities).SupportsFramebufferObjectEXT)
            {
                _supportsBlitFramebuffer = GL.BlitFramebuffer != null;
                _supportsInvalidateFramebuffer = GL.InvalidateFramebuffer != null;
            }
            else
            {
                throw new PlatformNotSupportedException(
                    "GraphicsDevice requires either ARB_framebuffer_object or EXT_framebuffer_object." +
                    "Try updating your graphics drivers.");
            }

            // Force resetting states
            _mainContext.Strategy._actualBlendState.PlatformApplyState((ConcreteGraphicsContextGL)_mainContext.Strategy, true);
            _mainContext.Strategy._actualDepthStencilState.PlatformApplyState((ConcreteGraphicsContextGL)_mainContext.Strategy, true);
            _mainContext.Strategy._actualRasterizerState.PlatformApplyState((ConcreteGraphicsContextGL)_mainContext.Strategy, true);

            _mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>()._bufferBindingInfos = new BufferBindingInfo[this.Capabilities.MaxVertexBufferSlots];
            for (int i = 0; i < _mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>()._bufferBindingInfos.Length; i++)
                _mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>()._bufferBindingInfos[i] = new BufferBindingInfo(null, IntPtr.Zero, 0, null);


            // Force set the default render states.
            _mainContext.Strategy._blendStateDirty = true;
            _mainContext.Strategy._blendFactorDirty = true;
            _mainContext.Strategy._depthStencilStateDirty = true;
            _mainContext.Strategy._rasterizerStateDirty = true;
            _mainContext.BlendState = BlendState.Opaque;
            _mainContext.DepthStencilState = DepthStencilState.Default;
            _mainContext.RasterizerState = RasterizerState.CullCounterClockwise;

            // Clear the texture and sampler collections forcing
            // the state to be reapplied.
            _mainContext.Strategy.VertexTextures.Clear();
            _mainContext.Strategy.VertexSamplerStates.Clear();
            _mainContext.Strategy.Textures.Clear();
            _mainContext.Strategy.SamplerStates.Clear();

            // Clear constant buffers
            _mainContext.Strategy._vertexConstantBuffers.Clear();
            _mainContext.Strategy._pixelConstantBuffers.Clear();

            // Force set the buffers and shaders on next ApplyState() call
            _mainContext.Strategy._vertexBuffers = new VertexBufferBindings(this.Capabilities.MaxVertexBufferSlots);
            _mainContext.Strategy._vertexBuffersDirty = true;
            _mainContext.Strategy._indexBufferDirty = true;
            _mainContext.Strategy._vertexShaderDirty = true;
            _mainContext.Strategy._pixelShaderDirty = true;

            // Set the default scissor rect.
            _mainContext.Strategy._scissorRectangleDirty = true;
            _mainContext.ScissorRectangle = _mainContext.Strategy._viewport.Bounds;

            // Set the default render target.
            _mainContext.ApplyRenderTargets(null);
        }


        private void ClearProgramCache()
        {
            var GL = _mainContext.Strategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

            foreach (ShaderProgram shaderProgram in ProgramCache.Values)
            {
                if (GL.IsProgram(shaderProgram.Program))
                {
                    GL.DeleteProgram(shaderProgram.Program);
                    GL.CheckGLError();
                }
            }
            ProgramCache.Clear();
        }

        internal int GetMaxMultiSampleCount(SurfaceFormat surfaceFormat)
        {
            var GL = this.MainContext.Strategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

            int maxMultiSampleCount = 0;
            GL.GetInteger(GetPName.MaxSamples, out maxMultiSampleCount);
            return maxMultiSampleCount;
        }

        internal void OnPresentationChanged()
        {
#if DESKTOPGL
            _mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>().MakeCurrent(this.PresentationParameters.DeviceWindowHandle);
            int swapInterval = ConcreteGraphicsContext.ToGLSwapInterval(this.PresentationParameters.PresentationInterval);
            Sdl.Current.OpenGL.SetSwapInterval(swapInterval);
#endif

            _mainContext.ApplyRenderTargets(null);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            // Free all the cached shader programs.
            ClearProgramCache();

            base.Dispose(disposing);
        }

    }
}
