// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Platform.Graphics;
using MonoGame.Framework.Utilities;
using MonoGame.OpenGL;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class GraphicsDevice
    {
        internal int _glMajorVersion = 0;
        internal int _glMinorVersion = 0;


        private void PlatformSetup()
        {
            ((ConcreteGraphicsDevice)_strategy)._programCache = new ShaderProgramCache(this);

#if DESKTOPGL
            System.Diagnostics.Debug.Assert(_strategy._mainContext == null);

#if DEBUG
            // create debug context, so we get better error messages (glDebugMessageCallback)
            Sdl.Current.OpenGL.SetAttribute(Sdl.GL.Attribute.ContextFlags, 1); // 1 = SDL_GL_CONTEXT_DEBUG_FLAG
#endif
#endif

            var contextStrategy = new ConcreteGraphicsContext(this);
            _strategy._mainContext = new GraphicsContext(this, contextStrategy);

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

            Strategy._capabilities = new GraphicsCapabilities();
            Strategy._capabilities.PlatformInitialize(this, _glMajorVersion, _glMinorVersion);


#if DESKTOPGL
            // Initialize draw buffer attachment array
            ((ConcreteGraphicsContext)_strategy._mainContext.Strategy)._drawBuffers = new DrawBuffersEnum[Strategy.Capabilities.MaxDrawBuffers];
			for (int i = 0; i < ((ConcreteGraphicsContext)_strategy._mainContext.Strategy)._drawBuffers.Length; i++)
                ((ConcreteGraphicsContext)_strategy._mainContext.Strategy)._drawBuffers[i] = (DrawBuffersEnum)(DrawBuffersEnum.ColorAttachment0 + i);
#endif

            ((ConcreteGraphicsContext)_strategy._mainContext.Strategy)._newEnabledVertexAttributes = new bool[Strategy.Capabilities.MaxVertexBufferSlots];
        }

        private void PlatformInitialize()
        {
            _strategy._mainContext.Strategy._viewport = new Viewport(0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);

            if (Strategy.Capabilities.SupportsFramebufferObjectARB
            ||  Strategy.Capabilities.SupportsFramebufferObjectEXT)
            {
                ((ConcreteGraphicsDevice)_strategy)._supportsBlitFramebuffer = GL.BlitFramebuffer != null;
                ((ConcreteGraphicsDevice)_strategy)._supportsInvalidateFramebuffer = GL.InvalidateFramebuffer != null;
            }
            else
            {
                throw new PlatformNotSupportedException(
                    "MonoGame requires either ARB_framebuffer_object or EXT_framebuffer_object." +
                    "Try updating your graphics drivers.");
            }

            // Force resetting states
            _strategy._mainContext.Strategy._actualBlendState.PlatformApplyState((ConcreteGraphicsContextGL)_strategy._mainContext.Strategy, true);
            _strategy._mainContext.Strategy._actualDepthStencilState.PlatformApplyState((ConcreteGraphicsContextGL)_strategy._mainContext.Strategy, true);
            _strategy._mainContext.Strategy._actualRasterizerState.PlatformApplyState((ConcreteGraphicsContextGL)_strategy._mainContext.Strategy, true);

            ((ConcreteGraphicsContext)_strategy._mainContext.Strategy)._bufferBindingInfos = new ConcreteGraphicsContext.BufferBindingInfo[Strategy.Capabilities.MaxVertexBufferSlots];
            for (int i = 0; i < ((ConcreteGraphicsContext)_strategy._mainContext.Strategy)._bufferBindingInfos.Length; i++)
                ((ConcreteGraphicsContext)_strategy._mainContext.Strategy)._bufferBindingInfos[i] = new ConcreteGraphicsContext.BufferBindingInfo(null, IntPtr.Zero, 0, -1);
        }

        private void PlatformDispose()
        {
            // Free all the cached shader programs.
            ((ConcreteGraphicsDevice)_strategy)._programCache.Dispose();

#if DESKTOPGL
            _strategy._mainContext.Dispose();
            _strategy._mainContext = null;
#endif
        }

        internal void OnPresentationChanged()
        {
#if DESKTOPGL
            ((ConcreteGraphicsContext)_strategy._mainContext.Strategy).MakeCurrent(this.PresentationParameters.DeviceWindowHandle);
            int swapInterval = ConcreteGraphicsContext.ToGLSwapInterval(PresentationParameters.PresentationInterval);
            Sdl.Current.OpenGL.SetSwapInterval(swapInterval);
#endif

            _strategy._mainContext.ApplyRenderTargets(null);
        }

        internal void Android_OnDeviceResetting()
        {
            var handler = DeviceResetting;
            if (handler != null)
                handler(this, EventArgs.Empty);

            lock (_strategy.ResourcesLock)
            {
                foreach (WeakReference resource in _strategy.Resources)
                {
                    GraphicsResource target = resource.Target as GraphicsResource;
                    if (target != null)
                        target.GraphicsDeviceResetting();
                }

                // Remove references to resources that have been garbage collected.
                for (int i = _strategy.Resources.Count - 1; i >= 0; i--)
                {
                    WeakReference resource = _strategy.Resources[i];

                    if (!resource.IsAlive)
                        _strategy.Resources.RemoveAt(i);
                }
            }
        }

        internal void Android_OnDeviceReset()
        {
            var handler = DeviceReset;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        internal void Android_ReInitializeContext()
        {
            _strategy._mainContext.Strategy._viewport = new Viewport(0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);

            // Ensure the vertex attributes are reset
            ((ConcreteGraphicsContext)_strategy._mainContext.Strategy)._enabledVertexAttributes.Clear();

            // Free all the cached shader programs.
            ((ConcreteGraphicsDevice)_strategy)._programCache.Clear();
            ((ConcreteGraphicsContext)_strategy._mainContext.Strategy)._shaderProgram = null;

            if (Strategy.Capabilities.SupportsFramebufferObjectARB
            ||  Strategy.Capabilities.SupportsFramebufferObjectEXT)
            {
                ((ConcreteGraphicsDevice)_strategy)._supportsBlitFramebuffer = GL.BlitFramebuffer != null;
                ((ConcreteGraphicsDevice)_strategy)._supportsInvalidateFramebuffer = GL.InvalidateFramebuffer != null;
            }
            else
            {
                throw new PlatformNotSupportedException(
                    "MonoGame requires either ARB_framebuffer_object or EXT_framebuffer_object." +
                    "Try updating your graphics drivers.");
            }

            // Force resetting states
            _strategy._mainContext.Strategy._actualBlendState.PlatformApplyState((ConcreteGraphicsContextGL)_strategy._mainContext.Strategy, true);
            _strategy._mainContext.Strategy._actualDepthStencilState.PlatformApplyState((ConcreteGraphicsContextGL)_strategy._mainContext.Strategy, true);
            _strategy._mainContext.Strategy._actualRasterizerState.PlatformApplyState((ConcreteGraphicsContextGL)_strategy._mainContext.Strategy, true);

            ((ConcreteGraphicsContext)_strategy._mainContext.Strategy)._bufferBindingInfos = new ConcreteGraphicsContext.BufferBindingInfo[Strategy.Capabilities.MaxVertexBufferSlots];
            for (int i = 0; i < ((ConcreteGraphicsContext)_strategy._mainContext.Strategy)._bufferBindingInfos.Length; i++)
                ((ConcreteGraphicsContext)_strategy._mainContext.Strategy)._bufferBindingInfos[i] = new ConcreteGraphicsContext.BufferBindingInfo(null, IntPtr.Zero, 0, -1);


            // Force set the default render states.
            _strategy._mainContext.Strategy._blendStateDirty = true;
            _strategy._mainContext.Strategy._blendFactorDirty = true;
            _strategy._mainContext.Strategy._depthStencilStateDirty = true;
            _strategy._mainContext.Strategy._rasterizerStateDirty = true;
            BlendState = BlendState.Opaque;
            DepthStencilState = DepthStencilState.Default;
            RasterizerState = RasterizerState.CullCounterClockwise;

            // Clear the texture and sampler collections forcing
            // the state to be reapplied.
            _strategy._mainContext.Strategy.VertexTextures.Clear();
            _strategy._mainContext.Strategy.VertexSamplerStates.Clear();
            _strategy._mainContext.Strategy.Textures.Clear();
            _strategy._mainContext.Strategy.SamplerStates.Clear();

            // Clear constant buffers
            _strategy._mainContext.Strategy._vertexConstantBuffers.Clear();
            _strategy._mainContext.Strategy._pixelConstantBuffers.Clear();

            // Force set the buffers and shaders on next ApplyState() call
            _strategy._mainContext.Strategy._vertexBuffers = new VertexBufferBindings(Strategy.Capabilities.MaxVertexBufferSlots);
            _strategy._mainContext.Strategy._vertexBuffersDirty = true;
            _strategy._mainContext.Strategy._indexBufferDirty = true;
            _strategy._mainContext.Strategy._vertexShaderDirty = true;
            _strategy._mainContext.Strategy._pixelShaderDirty = true;

            // Set the default scissor rect.
            _strategy._mainContext.Strategy._scissorRectangleDirty = true;
            ScissorRectangle = _strategy._mainContext.Strategy._viewport.Bounds;

            // Set the default render target.
            _strategy._mainContext.ApplyRenderTargets(null);
        }

    }
}
