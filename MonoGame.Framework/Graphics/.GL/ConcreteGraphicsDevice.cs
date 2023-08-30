// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Platform.Graphics
{
    internal abstract class ConcreteGraphicsDeviceGL : GraphicsDeviceStrategy
    {
        private readonly Dictionary<int, ShaderProgram> _programCache = new Dictionary<int, ShaderProgram>();

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


        internal void PlatformSetup()
        {
            _mainContext = new GraphicsContext(this);

            // try getting the context version
            // GL_MAJOR_VERSION and GL_MINOR_VERSION are GL 3.0+ only, so we need to rely on GL_VERSION string
            try
            {
                string version = GL.GetString(StringName.Version);
                if (string.IsNullOrEmpty(version))
                    throw new NoSuitableGraphicsDeviceException("Unable to retrieve OpenGL version");

                // for OpenGL, the GL_VERSION string always starts with the version number in the "major.minor" format,
                // optionally followed by multiple vendor specific characters
                // for GLES, the GL_VERSION string is formatted as:
                //     OpenGL<space>ES<space><version number><space><vendor-specific information>
#if GLES
                if (version.StartsWith("OpenGL ES "))
                    version =  version.Split(' ')[2];
                else // if it fails, we assume to be on a 1.1 context
                    version = "1.1";
#endif
                _glMajorVersion = Convert.ToInt32(version.Substring(0, 1));
                _glMinorVersion = Convert.ToInt32(version.Substring(2, 1));
            }
            catch (FormatException)
            {
                // if it fails, we assume to be on a 1.1 context
                _glMajorVersion = 1;
                _glMinorVersion = 1;
            }

            _capabilities = new GraphicsCapabilities();
            _capabilities.PlatformInitialize(this, _glMajorVersion, _glMinorVersion);


#if DESKTOPGL
            // Initialize draw buffer attachment array
            _mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>()._drawBuffers = new DrawBuffersEnum[this.Capabilities.MaxDrawBuffers];
			for (int i = 0; i < _mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>()._drawBuffers.Length; i++)
                _mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>()._drawBuffers[i] = (DrawBuffersEnum)(DrawBuffersEnum.ColorAttachment0 + i);
#endif

            _mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>()._newEnabledVertexAttributes = new bool[this.Capabilities.MaxVertexBufferSlots];
        }

        internal void PlatformInitialize()
        {
            _mainContext.Strategy._viewport = new Viewport(0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);

            if (this.Capabilities.SupportsFramebufferObjectARB
            || this.Capabilities.SupportsFramebufferObjectEXT)
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

            _mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>()._bufferBindingInfos = new ConcreteGraphicsContext.BufferBindingInfo[Capabilities.MaxVertexBufferSlots];
            for (int i = 0; i < _mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>()._bufferBindingInfos.Length; i++)
                _mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>()._bufferBindingInfos[i] = new ConcreteGraphicsContext.BufferBindingInfo(null, IntPtr.Zero, 0, -1);
        }


        internal void Android_ReInitializeContext()
        {
            _mainContext.Strategy._viewport = new Viewport(0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);

            // Ensure the vertex attributes are reset
            _mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>()._enabledVertexAttributes.Clear();

            // Free all the cached shader programs.
            ClearProgramCache();
            _mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>()._shaderProgram = null;

            if (this.Capabilities.SupportsFramebufferObjectARB
            ||  this.Capabilities.SupportsFramebufferObjectEXT)
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

            _mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>()._bufferBindingInfos = new ConcreteGraphicsContext.BufferBindingInfo[this.Capabilities.MaxVertexBufferSlots];
            for (int i = 0; i < _mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>()._bufferBindingInfos.Length; i++)
                _mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>()._bufferBindingInfos[i] = new ConcreteGraphicsContext.BufferBindingInfo(null, IntPtr.Zero, 0, -1);


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


        internal ShaderProgram GetProgram(Shader vertexShader, Shader pixelShader, int shaderProgramHash)
        {
            ShaderProgram shaderProgram;
            if (_programCache.TryGetValue(shaderProgramHash, out shaderProgram))
                return shaderProgram;

            // the key does not exist so we need to link the programs
            shaderProgram = CreateProgram(vertexShader, pixelShader);
            _programCache.Add(shaderProgramHash, shaderProgram);
            return shaderProgram;
        }

        private ShaderProgram CreateProgram(Shader vertexShader, Shader pixelShader)
        {
            int program = GL.CreateProgram();
            GraphicsExtensions.CheckGLError();

            GL.AttachShader(program, vertexShader.GetShaderHandle());
            GraphicsExtensions.CheckGLError();

            GL.AttachShader(program, pixelShader.GetShaderHandle());
            GraphicsExtensions.CheckGLError();

            //vertexShader.BindVertexAttributes(program);

            GL.LinkProgram(program);
            GraphicsExtensions.CheckGLError();

            GL.UseProgram(program);
            GraphicsExtensions.CheckGLError();

            vertexShader.GetVertexAttributeLocations(program);

            pixelShader.ApplySamplerTextureUnits(program);

            int linkStatus;
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out linkStatus);
            GraphicsExtensions.LogGLError("VertexShaderCache.Link(), GL.GetProgram");

            if (linkStatus == (int)Bool.True)
            {
                return new ShaderProgram(program);
            }
            else
            { 
                var log = GL.GetProgramInfoLog(program);
                Console.WriteLine(log);
                GL.DetachShader(program, vertexShader.GetShaderHandle());
                GL.DetachShader(program, pixelShader.GetShaderHandle());
                
                if (GL.IsProgram(program))
                {
                    GL.DeleteProgram(program);
                    GraphicsExtensions.CheckGLError();
                }
                throw new InvalidOperationException("Unable to link effect program");
            }
        }

        private void ClearProgramCache()
        {
            foreach (ShaderProgram shaderProgram in _programCache.Values)
            {
                if (GL.IsProgram(shaderProgram.Program))
                {
                    GL.DeleteProgram(shaderProgram.Program);
                    GraphicsExtensions.CheckGLError();
                }
            }
            _programCache.Clear();
        }

        internal override int GetClampedMultiSampleCount(SurfaceFormat surfaceFormat, int multiSampleCount)
        {
            if (multiSampleCount > 1)
            {
                // Round down MultiSampleCount to the nearest power of two
                // hack from http://stackoverflow.com/a/2681094
                // Note: this will return an incorrect, but large value
                // for very large numbers. That doesn't matter because
                // the number will get clamped below anyway in this case.
                var msc = multiSampleCount;
                msc = msc | (msc >> 1);
                msc = msc | (msc >> 2);
                msc = msc | (msc >> 4);
                msc -= (msc >> 1);
                // and clamp it to what the device can handle
                if (msc > Capabilities.MaxMultiSampleCount)
                    msc = Capabilities.MaxMultiSampleCount;

                return msc;
            }
            else return 0;
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
            System.Diagnostics.Debug.Assert(!IsDisposed);

            if (disposing)
            {
            }

            // Free all the cached shader programs.
            ClearProgramCache();

            base.Dispose(disposing);
        }

    }
}
