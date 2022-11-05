// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Xna.Platform.Graphics;
using MonoGame.Framework.Utilities;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class GraphicsDevice
    {
#if DESKTOPGL
        private IntPtr _currentWindowHandle;
        internal IntPtr CurrentGlContext
        {
            get { return ((ConcreteGraphicsContext)CurrentContext.Strategy).GlContext; }
        }
        internal ConcreteGraphicsContext CurrentConcreteContext
        {
            get { return ((ConcreteGraphicsContext)CurrentContext.Strategy); }
        }
#endif

#if !GLES
        private DrawBuffersEnum[] _drawBuffers;
#endif

        enum ResourceType
        {
            Texture,
            Buffer,
            Shader,
            Program,
            Query,
            Framebuffer
        }

        struct ResourceHandle
        {
            public ResourceType type;
            public int handle;

            public static ResourceHandle Texture(int handle)
            {
                return new ResourceHandle() { type = ResourceType.Texture, handle = handle };
            }

            public static ResourceHandle Buffer(int handle)
            {
                return new ResourceHandle() { type = ResourceType.Buffer, handle = handle };
            }

            public static ResourceHandle Shader(int handle)
            {
                return new ResourceHandle() { type = ResourceType.Shader, handle = handle };
            }

            public static ResourceHandle Program(int handle)
            {
                return new ResourceHandle() { type = ResourceType.Program, handle = handle };
            }

            public static ResourceHandle Query(int handle)
            {
                return new ResourceHandle() { type = ResourceType.Query, handle = handle };
            }

            public static ResourceHandle Framebuffer(int handle)
            {
                return new ResourceHandle() { type = ResourceType.Framebuffer, handle = handle };
            }

            public void Free()
            {
                switch (type)
                {
                    case ResourceType.Texture:
                        GL.DeleteTextures(1, ref handle);
                        break;
                    case ResourceType.Buffer:
                        GL.DeleteBuffers(1, ref handle);
                        break;
                    case ResourceType.Shader:
                        if (GL.IsShader(handle))
                            GL.DeleteShader(handle);
                        break;
                    case ResourceType.Program:
                        if (GL.IsProgram(handle))
                        {
                            GL.DeleteProgram(handle);
                        }
                        break;
                    case ResourceType.Query:
#if !GLES
                        GL.DeleteQueries(1, ref handle);
#endif
                        break;
                    case ResourceType.Framebuffer:
                        GL.DeleteFramebuffers(1, ref handle);
                        break;
                }
                GraphicsExtensions.CheckGLError();
            }
        }

        List<ResourceHandle> _disposeThisFrame = new List<ResourceHandle>();
        List<ResourceHandle> _disposeNextFrame = new List<ResourceHandle>();
        object _disposeActionsLock = new object();

        static List<IntPtr> _disposeContexts = new List<IntPtr>();
        static object _disposeContextsLock = new object();

        private ShaderProgramCache _programCache;
        private ShaderProgram _shaderProgram = null;

        Vector4 _posFixup;

        private static BufferBindingInfo[] _bufferBindingInfos;
        private static int _activeBufferBindingInfosCount;
        private static bool[] _newEnabledVertexAttributes;
        internal static readonly HashSet<int> _enabledVertexAttributes = new HashSet<int>();
        internal static bool _attribsDirty;

        internal FramebufferHelper framebufferHelper;

        internal int glMajorVersion = 0;
        internal int glMinorVersion = 0;
        internal int glFramebuffer = 0;
        internal int MaxVertexAttributes;
        internal int _maxTextureSize = 0;

        // Keeps track of last applied state to avoid redundant OpenGL calls
        internal bool _lastBlendEnable = false;
        internal BlendState _lastBlendState = new BlendState();
        internal DepthStencilState _lastDepthStencilState = new DepthStencilState();
        internal RasterizerState _lastRasterizerState = new RasterizerState();
        private Vector4 _lastClearColor = Vector4.Zero;
        private float _lastClearDepth = 1.0f;
        private int _lastClearStencil = 0;

        internal ShaderProgram PlatformShaderProgram { get { return _shaderProgram; } }

        // Get a hashed value based on the currently bound shaders
        // throws an exception if no shaders are bound
        private int ShaderProgramHash
        {
            get
            {
                if (_vertexShader == null && _pixelShader == null)
                    throw new InvalidOperationException("There is no shader bound!");
                if (_vertexShader == null)
                    return _pixelShader.HashKey;
                if (_pixelShader == null)
                    return _vertexShader.HashKey;
                return _vertexShader.HashKey ^ _pixelShader.HashKey;
            }
        }

        internal void SetVertexAttributeArray(bool[] attrs)
        {
            for (var x = 0; x < attrs.Length; x++)
            {
                if (attrs[x])
                {
                    if (_enabledVertexAttributes.Add(x))
                    {
                        GL.EnableVertexAttribArray(x);
                        GraphicsExtensions.CheckGLError();
                    }
                }
                else
                {
                    if (_enabledVertexAttributes.Remove(x))
                    {
                        GL.DisableVertexAttribArray(x);
                        GraphicsExtensions.CheckGLError();
                    }
                }
            }
        }

        private void ApplyAttribs(Shader shader, int baseVertex)
        {
            var programHash = ShaderProgramHash;
            var bindingsChanged = false;

            for (var slot = 0; slot < _vertexBuffers.Count; slot++)
            {
                var vertexBufferBinding = _vertexBuffers.Get(slot);
                var vertexDeclaration = vertexBufferBinding.VertexBuffer.VertexDeclaration;
                var attrInfo = vertexDeclaration.GetAttributeInfo(shader, programHash);

                var vertexStride = vertexDeclaration.VertexStride;
                var offset = (IntPtr)(vertexDeclaration.VertexStride * (baseVertex + vertexBufferBinding.VertexOffset));

                if (!_attribsDirty &&
                    slot < _activeBufferBindingInfosCount &&
                    _bufferBindingInfos[slot].VertexOffset == offset &&
                    ReferenceEquals(_bufferBindingInfos[slot].AttributeInfo, attrInfo) &&
                    _bufferBindingInfos[slot].InstanceFrequency == vertexBufferBinding.InstanceFrequency &&
                    _bufferBindingInfos[slot].Vbo == vertexBufferBinding.VertexBuffer.vbo)
                    continue;

                bindingsChanged = true;

                GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferBinding.VertexBuffer.vbo);
                GraphicsExtensions.CheckGLError();

                for (int e = 0; e < attrInfo.Elements.Count; e++)
                {
                    var element = attrInfo.Elements[e];
                    GL.VertexAttribPointer(element.AttributeLocation,
                        element.NumberOfElements,
                        element.VertexAttribPointerType,
                        element.Normalized,
                        vertexStride,
                        (IntPtr)(offset.ToInt64() + element.Offset));
                    GraphicsExtensions.CheckGLError();

                    // only set the divisor if instancing is supported
                    if (GraphicsCapabilities.SupportsInstancing)
                    {
                        GL.VertexAttribDivisor(element.AttributeLocation, vertexBufferBinding.InstanceFrequency);
                        GraphicsExtensions.CheckGLError();
                    }
                    else // If instancing is not supported, but InstanceFrequency of the buffer is not zero, throw an exception
                    {
                        if (vertexBufferBinding.InstanceFrequency > 0)
                            throw new PlatformNotSupportedException("Instanced geometry drawing requires at least OpenGL 3.2 or GLES 3.2. Try upgrading your graphics drivers.");
                    }
                }

                _bufferBindingInfos[slot].VertexOffset = offset;
                _bufferBindingInfos[slot].AttributeInfo = attrInfo;
                _bufferBindingInfos[slot].InstanceFrequency = vertexBufferBinding.InstanceFrequency;
                _bufferBindingInfos[slot].Vbo = vertexBufferBinding.VertexBuffer.vbo;
            }

            _attribsDirty = false;

            if (bindingsChanged)
            {
                for (int eva = 0; eva < _newEnabledVertexAttributes.Length; eva++)
                    _newEnabledVertexAttributes[eva] = false;
                for (var slot = 0; slot < _vertexBuffers.Count; slot++)
                {
                    for (int e = 0; e< _bufferBindingInfos[slot].AttributeInfo.Elements.Count; e++)
                    {
                        var element = _bufferBindingInfos[slot].AttributeInfo.Elements[e];
                        _newEnabledVertexAttributes[element.AttributeLocation] = true;
                    }
                }
                _activeBufferBindingInfosCount = _vertexBuffers.Count;
            }
            SetVertexAttributeArray(_newEnabledVertexAttributes);
        }

        private void PlatformSetup()
        {
            _programCache = new ShaderProgramCache(this);
#if DESKTOPGL
            _currentWindowHandle = SdlGameWindow.Instance.Handle;

            if (_mainContext == null)
            {
#if DEBUG
                // create debug context, so we get better error messages (glDebugMessageCallback)
                Sdl.GL.SetAttribute(Sdl.GL.Attribute.ContextFlags, 1); // 1 = SDL_GL_CONTEXT_DEBUG_FLAG
#endif

                var contextStrategy = new ConcreteGraphicsContext(_currentWindowHandle);
                _mainContext = new GraphicsContext(this, contextStrategy);
            }

            CurrentConcreteContext.MakeCurrent(_currentWindowHandle);
            int swapInterval = ToGLSwapInterval(PresentationParameters.PresentationInterval);
            Sdl.GL.SetSwapInterval(swapInterval);

            CurrentConcreteContext.MakeCurrent(_currentWindowHandle);
#endif
            GL.GetInteger(GetPName.MaxCombinedTextureImageUnits, out MaxTextureSlots);
            GraphicsExtensions.CheckGLError();
            MaxTextureSlots = Math.Min(MaxTextureSlots, 16);

            GL.GetInteger(GetPName.MaxTextureSize, out _maxTextureSize);
            GraphicsExtensions.CheckGLError();

            MaxVertexAttributes = 16;
            if (this.GraphicsProfile >= GraphicsProfile.FL10_1) _maxVertexBufferSlots = 32;

            _maxVertexBufferSlots = MaxVertexAttributes;
            _newEnabledVertexAttributes = new bool[MaxVertexAttributes];


            // try getting the context version
            // GL_MAJOR_VERSION and GL_MINOR_VERSION are GL 3.0+ only, so we need to rely on the GL_VERSION string
            // for non GLES this string always starts with the version number in the "major.minor" format, but can be followed by
            // multiple vendor specific characters
            // For GLES this string is formatted as: OpenGL<space>ES<space><version number><space><vendor-specific information>
#if GLES
            try
            {
                string version = GL.GetString(StringName.Version);

                if (string.IsNullOrEmpty(version))
                    throw new NoSuitableGraphicsDeviceException("Unable to retrieve OpenGL version");

                string[] versionSplit = version.Split(' ');
                if (versionSplit.Length > 2 && versionSplit[0].Equals("OpenGL") && versionSplit[1].Equals("ES"))
                {
                    glMajorVersion = Convert.ToInt32(versionSplit[2].Substring(0, 1));
                    glMinorVersion = Convert.ToInt32(versionSplit[2].Substring(2, 1));
                }
                else
                {
                    glMajorVersion = 1;
                    glMinorVersion = 1;
                }
            }
            catch (FormatException)
            {
                //if it fails we default to 1.1 context
                glMajorVersion = 1;
                glMinorVersion = 1;
            }
#else
                try
            {
                string version = GL.GetString(StringName.Version);

                if (string.IsNullOrEmpty(version))
                    throw new NoSuitableGraphicsDeviceException("Unable to retrieve OpenGL version");

                glMajorVersion = Convert.ToInt32(version.Substring(0, 1));
                glMinorVersion = Convert.ToInt32(version.Substring(2, 1));
            }
            catch (FormatException)
            {
                // if it fails, we assume to be on a 1.1 context
                glMajorVersion = 1;
                glMinorVersion = 1;
            }
#endif

#if !GLES
			// Initialize draw buffer attachment array
			int maxDrawBuffers;
            GL.GetInteger(GetPName.MaxDrawBuffers, out maxDrawBuffers);
            GraphicsExtensions.CheckGLError ();
			_drawBuffers = new DrawBuffersEnum[maxDrawBuffers];
			for (int i = 0; i < maxDrawBuffers; i++)
				_drawBuffers[i] = (DrawBuffersEnum)(FramebufferAttachment.ColorAttachment0Ext + i);
#endif
        }

        private void PlatformInitialize()
        {
            _viewport = new Viewport(0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);

            // Ensure the vertex attributes are reset
            _enabledVertexAttributes.Clear();

            // Free all the cached shader programs. 
            _programCache.Clear();
            _shaderProgram = null;

            framebufferHelper = FramebufferHelper.Create(this);

            // Force resetting states
            this.PlatformApplyBlend(true);
            this.DepthStencilState.PlatformApplyState(this, true);
            this.RasterizerState.PlatformApplyState(this, true);

            _bufferBindingInfos = new BufferBindingInfo[_maxVertexBufferSlots];
            for (int i = 0; i < _bufferBindingInfos.Length; i++)
                _bufferBindingInfos[i] = new BufferBindingInfo(null, IntPtr.Zero, 0, -1);
        }
        
        private DepthStencilState clearDepthStencilState = new DepthStencilState { StencilEnable = true };

        private void PlatformClear(ClearOptions options, Vector4 color, float depth, int stencil)
        {
            // TODO: We need to figure out how to detect if we have a
            // depth stencil buffer or not, and clear options relating
            // to them if not attached.

            // Unlike with XNA and DirectX...  GL.Clear() obeys several
            // different render states:
            //
            //  - The color write flags.
            //  - The scissor rectangle.
            //  - The depth/stencil state.
            //
            // So overwrite these states with what is needed to perform
            // the clear correctly and restore it afterwards.
            //
		    var prevScissorRect = ScissorRectangle;
		    var prevDepthStencilState = DepthStencilState;
            var prevBlendState = BlendState;
            ScissorRectangle = _viewport.Bounds;
            // DepthStencilState.Default has the Stencil Test disabled; 
            // make sure stencil test is enabled before we clear since
            // some drivers won't clear with stencil test disabled
            DepthStencilState = this.clearDepthStencilState;
		    BlendState = BlendState.Opaque;
            PlatformApplyState();

            ClearBufferMask bufferMask = 0;
            if ((options & ClearOptions.Target) == ClearOptions.Target)
            {
                if (color != _lastClearColor)
                {
                    GL.ClearColor(color.X, color.Y, color.Z, color.W);
                    GraphicsExtensions.CheckGLError();
                    _lastClearColor = color;
                }
                bufferMask = bufferMask | ClearBufferMask.ColorBufferBit;
            }
			if ((options & ClearOptions.Stencil) == ClearOptions.Stencil)
            {
                if (stencil != _lastClearStencil)
                {
				    GL.ClearStencil(stencil);
                    GraphicsExtensions.CheckGLError();
                    _lastClearStencil = stencil;
                }
                bufferMask = bufferMask | ClearBufferMask.StencilBufferBit;
			}

			if ((options & ClearOptions.DepthBuffer) == ClearOptions.DepthBuffer) 
            {
                if (depth != _lastClearDepth)
                {
                    GL.ClearDepth(depth);
                    GraphicsExtensions.CheckGLError();
                    _lastClearDepth = depth;
                }
				bufferMask = bufferMask | ClearBufferMask.DepthBufferBit;
			}

#if IOS || TVOS
            if (GL.CheckFramebufferStatus(FramebufferTarget.FramebufferExt) == FramebufferErrorCode.FramebufferComplete)
            {
#endif
                GL.Clear(bufferMask);
                GraphicsExtensions.CheckGLError();
#if IOS || TVOS
            }
#endif
           		
            // Restore the previous render state.
		    ScissorRectangle = prevScissorRect;
		    DepthStencilState = prevDepthStencilState;
		    BlendState = prevBlendState;
        }

        private void PlatformDispose()
        {
            // Free all the cached shader programs.
            _programCache.Dispose();

#if DESKTOPGL
            _mainContext.Dispose();
            _mainContext = null;
            _currentWindowHandle = IntPtr.Zero;
#endif
        }

        internal void DisposeTexture(int handle)
        {
            if (!_isDisposed)
            {
                lock (_disposeActionsLock)
                {
                    _disposeNextFrame.Add(ResourceHandle.Texture(handle));
                }
            }
        }

        internal void DisposeBuffer(int handle)
        {
            if (!_isDisposed)
            {
                lock (_disposeActionsLock)
                {
                    _disposeNextFrame.Add(ResourceHandle.Buffer(handle));
                }
            }
        }

        internal void DisposeShader(int handle)
        {
            if (!_isDisposed)
            {
                lock (_disposeActionsLock)
                {
                    _disposeNextFrame.Add(ResourceHandle.Shader(handle));
                }
            }
        }

        internal void DisposeProgram(int handle)
        {
            if (!_isDisposed)
            {
                lock (_disposeActionsLock)
                {
                    _disposeNextFrame.Add(ResourceHandle.Program(handle));
                }
            }
        }

        internal void DisposeQuery(int handle)
        {
            if (!_isDisposed)
            {
                lock (_disposeActionsLock)
                {
                    _disposeNextFrame.Add(ResourceHandle.Query(handle));
                }
            }
        }

        internal void DisposeFramebuffer(int handle)
        {
            if (!_isDisposed)
            {
                lock (_disposeActionsLock)
                {
                    _disposeNextFrame.Add(ResourceHandle.Framebuffer(handle));
                }
            }
        }

#if DESKTOPGL
        static internal void DisposeContext(IntPtr resource)
        {
            lock (_disposeContextsLock)
            {
                _disposeContexts.Add(resource);
            }
        }

        static internal void DisposeContexts()
        {
            lock (_disposeContextsLock)
            {
                int count = _disposeContexts.Count;
                for (int i = 0; i < count; ++i)
                    Sdl.GL.DeleteContext(_disposeContexts[i]);
                _disposeContexts.Clear();
            }
        }
#endif

        private void PlatformPresent()
        {
#if DESKTOPGL
            Sdl.GL.SwapWindow(_currentWindowHandle);
#endif
            GraphicsExtensions.CheckGLError();

            // Dispose of any GL resources that were disposed in another thread
            int count = _disposeThisFrame.Count;
            for (int i = 0; i < count; ++i)
                _disposeThisFrame[i].Free();
            _disposeThisFrame.Clear();

            lock (_disposeActionsLock)
            {
                // Swap lists so resources added during this draw will be released after the next draw
                var temp = _disposeThisFrame;
                _disposeThisFrame = _disposeNextFrame;
                _disposeNextFrame = temp;
            }
        }

        private void PlatformApplyDefaultRenderTarget()
        {
            this.framebufferHelper.BindFramebuffer(this.glFramebuffer);

            // Reset the raster state because we flip vertices
            // when rendering offscreen and hence the cull direction.
            _rasterizerStateDirty = true;

            // Textures will need to be rebound to render correctly in the new render target.
            Textures.Dirty();
        }

        private class RenderTargetBindingArrayComparer : IEqualityComparer<RenderTargetBinding[]>
        {
            public bool Equals(RenderTargetBinding[] first, RenderTargetBinding[] second)
            {
                if (object.ReferenceEquals(first, second))
                    return true;

                if (first == null || second == null)
                    return false;

                if (first.Length != second.Length)
                    return false;

                for (var i = 0; i < first.Length; ++i)
                {
                    if ((first[i].RenderTarget != second[i].RenderTarget) || (first[i].ArraySlice != second[i].ArraySlice))
                    {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode(RenderTargetBinding[] array)
            {
                if (array != null)
                {
                    unchecked
                    {
                        int hash = 17;
                        foreach (var item in array)
                        {
                            if (item.RenderTarget != null)
                                hash = hash * 23 + item.RenderTarget.GetHashCode();
                            hash = hash * 23 + item.ArraySlice.GetHashCode();
                        }
                        return hash;
                    }
                }
                return 0;
            }
        }

        // FBO cache, we create 1 FBO per RenderTargetBinding combination
        private Dictionary<RenderTargetBinding[], int> glFramebuffers = new Dictionary<RenderTargetBinding[], int>(new RenderTargetBindingArrayComparer());
        // FBO cache used to resolve MSAA rendertargets, we create 1 FBO per RenderTargetBinding combination
        private Dictionary<RenderTargetBinding[], int> glResolveFramebuffers = new Dictionary<RenderTargetBinding[], int>(new RenderTargetBindingArrayComparer());

        internal void PlatformCreateRenderTarget(IRenderTarget renderTarget, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
        {
            var color = 0;
            var depth = 0;
            var stencil = 0;
            
            if (preferredMultiSampleCount > 0 && this.framebufferHelper.SupportsBlitFramebuffer)
            {
                this.framebufferHelper.GenRenderbuffer(out color);
                this.framebufferHelper.BindRenderbuffer(color);
                this.framebufferHelper.RenderbufferStorageMultisample(preferredMultiSampleCount, (int)RenderbufferStorage.Rgba8, width, height);
            }

            if (preferredDepthFormat != DepthFormat.None)
            {
                var depthInternalFormat = RenderbufferStorage.DepthComponent16;
                var stencilInternalFormat = (RenderbufferStorage)0;
                switch (preferredDepthFormat)
                {
                    case DepthFormat.Depth16: 
                        depthInternalFormat = RenderbufferStorage.DepthComponent16;
                        break;
#if GLES
                    case DepthFormat.Depth24:
                        if (GraphicsCapabilities.SupportsDepth24)
                            depthInternalFormat = RenderbufferStorage.DepthComponent24Oes;
                        else if (GraphicsCapabilities.SupportsDepthNonLinear)
                            depthInternalFormat = (RenderbufferStorage)0x8E2C;
                        else
                            depthInternalFormat = RenderbufferStorage.DepthComponent16;
                        break;
                    case DepthFormat.Depth24Stencil8:
                        if (GraphicsCapabilities.SupportsPackedDepthStencil)
                            depthInternalFormat = RenderbufferStorage.Depth24Stencil8Oes;
                        else
                        {
                            if (GraphicsCapabilities.SupportsDepth24)
                                depthInternalFormat = RenderbufferStorage.DepthComponent24Oes;
                            else if (GraphicsCapabilities.SupportsDepthNonLinear)
                                depthInternalFormat = (RenderbufferStorage)0x8E2C;
                            else
                                depthInternalFormat = RenderbufferStorage.DepthComponent16;
                            stencilInternalFormat = RenderbufferStorage.StencilIndex8;
                            break;
                        }
                        break;
#else
                    case DepthFormat.Depth24:
                        depthInternalFormat = RenderbufferStorage.DepthComponent24;
                        break;
                    case DepthFormat.Depth24Stencil8:
                        depthInternalFormat = RenderbufferStorage.Depth24Stencil8;
                        break;
#endif
                }

                if (depthInternalFormat != 0)
                {
                    this.framebufferHelper.GenRenderbuffer(out depth);
                    this.framebufferHelper.BindRenderbuffer(depth);
                    this.framebufferHelper.RenderbufferStorageMultisample(preferredMultiSampleCount, (int)depthInternalFormat, width, height);
                    if (preferredDepthFormat == DepthFormat.Depth24Stencil8)
                    {
                        stencil = depth;
                        if (stencilInternalFormat != 0)
                        {
                            this.framebufferHelper.GenRenderbuffer(out stencil);
                            this.framebufferHelper.BindRenderbuffer(stencil);
                            this.framebufferHelper.RenderbufferStorageMultisample(preferredMultiSampleCount, (int)stencilInternalFormat, width, height);
                        }
                    }
                }
            }

            var renderTargetGL = (IRenderTargetGL)renderTarget;
            if (color != 0)
                renderTargetGL.GLColorBuffer = color;
            else
                renderTargetGL.GLColorBuffer = renderTargetGL.GLTexture;
            renderTargetGL.GLDepthBuffer = depth;
            renderTargetGL.GLStencilBuffer = stencil;
        }

        internal void PlatformDeleteRenderTarget(IRenderTarget renderTarget)
        {
            var color = 0;
            var depth = 0;
            var stencil = 0;
            var colorIsRenderbuffer = false;

            var renderTargetGL = (IRenderTargetGL)renderTarget;
            color = renderTargetGL.GLColorBuffer;
            depth = renderTargetGL.GLDepthBuffer;
            stencil = renderTargetGL.GLStencilBuffer;
            colorIsRenderbuffer = color != renderTargetGL.GLTexture;

            if (color != 0)
            {
                if (colorIsRenderbuffer)
                    this.framebufferHelper.DeleteRenderbuffer(color);
                if (stencil != 0 && stencil != depth)
                    this.framebufferHelper.DeleteRenderbuffer(stencil);
                if (depth != 0)
                    this.framebufferHelper.DeleteRenderbuffer(depth);

                var bindingsToDelete = new List<RenderTargetBinding[]>();
                foreach (var bindings in this.glFramebuffers.Keys)
                {
                    foreach (var binding in bindings)
                    {
                        if (binding.RenderTarget == renderTarget)
                        {
                            bindingsToDelete.Add(bindings);
                            break;
                        }
                    }
                }

                foreach (var bindings in bindingsToDelete)
                {
                    var fbo = 0;
                    if (this.glFramebuffers.TryGetValue(bindings, out fbo))
                    {
                        this.framebufferHelper.DeleteFramebuffer(fbo);
                        this.glFramebuffers.Remove(bindings);
                    }
                    if (this.glResolveFramebuffers.TryGetValue(bindings, out fbo))
                    {
                        this.framebufferHelper.DeleteFramebuffer(fbo);
                        this.glResolveFramebuffers.Remove(bindings);
                    }
                }
            }
        }

        private void PlatformResolveRenderTargets()
        {
            if (this._currentRenderTargetCount == 0)
                return;

            var renderTargetBinding = this._currentRenderTargetBindings[0];
            var renderTarget = renderTargetBinding.RenderTarget as IRenderTarget;
            if (renderTarget.MultiSampleCount > 0 && this.framebufferHelper.SupportsBlitFramebuffer)
            {
                var glResolveFramebuffer = 0;
                if (!this.glResolveFramebuffers.TryGetValue(this._currentRenderTargetBindings, out glResolveFramebuffer))
                {
                    this.framebufferHelper.GenFramebuffer(out glResolveFramebuffer);
                    this.framebufferHelper.BindFramebuffer(glResolveFramebuffer);
                    for (var i = 0; i < this._currentRenderTargetCount; ++i)
                    {
                        var renderTargetGL = (IRenderTargetGL)this._currentRenderTargetBindings[i].RenderTarget;
                        this.framebufferHelper.FramebufferTexture2D((int)(FramebufferAttachment.ColorAttachment0 + i), (int)renderTargetGL.GetFramebufferTarget(renderTargetBinding.ArraySlice), renderTargetGL.GLTexture);
                    }
                    this.glResolveFramebuffers.Add((RenderTargetBinding[])this._currentRenderTargetBindings.Clone(), glResolveFramebuffer);
                }
                else
                {
                    this.framebufferHelper.BindFramebuffer(glResolveFramebuffer);
                }
                // The only fragment operations which affect the resolve are the pixel ownership test, the scissor test, and dithering.
                if (this._lastRasterizerState.ScissorTestEnable)
                {
                    GL.Disable(EnableCap.ScissorTest);
                    GraphicsExtensions.CheckGLError();
                }
                var glFramebuffer = this.glFramebuffers[this._currentRenderTargetBindings];
                this.framebufferHelper.BindReadFramebuffer(glFramebuffer);
                for (var i = 0; i < this._currentRenderTargetCount; ++i)
                {
                    renderTargetBinding = this._currentRenderTargetBindings[i];
                    renderTarget = renderTargetBinding.RenderTarget as IRenderTarget;
                    this.framebufferHelper.BlitFramebuffer(i, renderTarget.Width, renderTarget.Height);
                }
                if (renderTarget.RenderTargetUsage == RenderTargetUsage.DiscardContents && this.framebufferHelper.SupportsInvalidateFramebuffer)
                    this.framebufferHelper.InvalidateReadFramebuffer();
                if (this._lastRasterizerState.ScissorTestEnable)
                {
                    GL.Enable(EnableCap.ScissorTest);
                    GraphicsExtensions.CheckGLError();
                }
            }
            for (var i = 0; i < this._currentRenderTargetCount; ++i)
            {
                renderTargetBinding = this._currentRenderTargetBindings[i];
                if (renderTargetBinding.RenderTarget.LevelCount > 1)
                {
                    var renderTargetGL = (IRenderTargetGL)renderTargetBinding.RenderTarget;
                    GL.BindTexture(renderTargetGL.GLTarget, renderTargetGL.GLTexture);
                    GraphicsExtensions.CheckGLError();
                    this.framebufferHelper.GenerateMipmap((int)renderTargetGL.GLTarget);
                }
            }
        }

        private IRenderTarget PlatformApplyRenderTargets()
        {
            var glFramebuffer = 0;
            if (!this.glFramebuffers.TryGetValue(this._currentRenderTargetBindings, out glFramebuffer))
            {
                this.framebufferHelper.GenFramebuffer(out glFramebuffer);
                this.framebufferHelper.BindFramebuffer(glFramebuffer);
                var renderTargetBinding = this._currentRenderTargetBindings[0];
                var renderTargetGL = (IRenderTargetGL)renderTargetBinding.RenderTarget;
                this.framebufferHelper.FramebufferRenderbuffer((int)FramebufferAttachment.DepthAttachment, renderTargetGL.GLDepthBuffer, 0);
                this.framebufferHelper.FramebufferRenderbuffer((int)FramebufferAttachment.StencilAttachment, renderTargetGL.GLStencilBuffer, 0);
                for (var i = 0; i < this._currentRenderTargetCount; ++i)
                {
                    renderTargetBinding = this._currentRenderTargetBindings[i];
                    var renderTarget = (IRenderTarget)renderTargetBinding.RenderTarget;
                    renderTargetGL = renderTargetBinding.RenderTarget as IRenderTargetGL;
                    var attachement = (int)(FramebufferAttachment.ColorAttachment0 + i);
                    if (renderTargetGL.GLColorBuffer != renderTargetGL.GLTexture)
                        this.framebufferHelper.FramebufferRenderbuffer(attachement, renderTargetGL.GLColorBuffer, 0);
                    else
                        this.framebufferHelper.FramebufferTexture2D(attachement, (int)renderTargetGL.GetFramebufferTarget(renderTargetBinding.ArraySlice), renderTargetGL.GLTexture, 0, renderTarget.MultiSampleCount);
                }

#if DEBUG
                this.framebufferHelper.CheckFramebufferStatus();
#endif
                this.glFramebuffers.Add((RenderTargetBinding[])_currentRenderTargetBindings.Clone(), glFramebuffer);
            }
            else
            {
                this.framebufferHelper.BindFramebuffer(glFramebuffer);
            }
#if !GLES
            GL.DrawBuffers(this._currentRenderTargetCount, this._drawBuffers);
#endif

            // Reset the raster state because we flip vertices
            // when rendering offscreen and hence the cull direction.
            _rasterizerStateDirty = true;

            // Textures will need to be rebound to render correctly in the new render target.
            Textures.Dirty();

            return _currentRenderTargetBindings[0].RenderTarget as IRenderTarget;
        }

        private static GLPrimitiveType PrimitiveTypeGL(PrimitiveType primitiveType)
        {
            switch (primitiveType)
            {
                case PrimitiveType.PointList:
                    return GLPrimitiveType.Points;
                case PrimitiveType.LineList:
                    return GLPrimitiveType.Lines;
                case PrimitiveType.LineStrip:
                    return GLPrimitiveType.LineStrip;
                case PrimitiveType.TriangleList:
                    return GLPrimitiveType.Triangles;
                case PrimitiveType.TriangleStrip:
                    return GLPrimitiveType.TriangleStrip;
                default:
                    throw new ArgumentException();
            }
        }

        /// <summary>
        /// Activates the Current Vertex/Pixel shader pair into a program.         
        /// </summary>
        private unsafe void ActivateShaderProgram()
        {
            // Lookup the shader program.
            var shaderProgram = _programCache.GetProgram(VertexShader, PixelShader);
            if (shaderProgram.Program == -1)
                return;
            // Set the new program if it has changed.
            if (_shaderProgram != shaderProgram)
            {
                GL.UseProgram(shaderProgram.Program);
                GraphicsExtensions.CheckGLError();
                _shaderProgram = shaderProgram;
            }

            var posFixupLoc = shaderProgram.GetUniformLocation("posFixup");
            if (posFixupLoc == -1)
                return;

            // Apply vertex shader fix:
            // The following two lines are appended to the end of vertex shaders
            // to account for rendering differences between OpenGL and DirectX:
            //
            // gl_Position.y = gl_Position.y * posFixup.y;
            // gl_Position.xy += posFixup.zw * gl_Position.ww;
            //
            // (the following paraphrased from wine, wined3d/state.c and wined3d/glsl_shader.c)
            //
            // - We need to flip along the y-axis in case of offscreen rendering.
            // - D3D coordinates refer to pixel centers while GL coordinates refer
            //   to pixel corners.
            // - D3D has a top-left filling convention. We need to maintain this
            //   even after the y-flip mentioned above.
            // In order to handle the last two points, we translate by
            // (63.0 / 128.0) / VPw and (63.0 / 128.0) / VPh. This is equivalent to
            // translating slightly less than half a pixel. We want the difference to
            // be large enough that it doesn't get lost due to rounding inside the
            // driver, but small enough to prevent it from interfering with any
            // anti-aliasing.
            //
            // OpenGL coordinates specify the center of the pixel while d3d coords specify
            // the corner. The offsets are stored in z and w in posFixup. posFixup.y contains
            // 1.0 or -1.0 to turn the rendering upside down for offscreen rendering. PosFixup.x
            // contains 1.0 to allow a mad.

            _posFixup.X = 1.0f;
            _posFixup.Y = 1.0f;
            if (!UseHalfPixelOffset)
            {
                _posFixup.Z = 0f;
                _posFixup.W = 0f;
            }
            else
            {
                _posFixup.Z =  (63.0f/64.0f)/Viewport.Width;
                _posFixup.W = -(63.0f/64.0f)/Viewport.Height;
            }

            //If we have a render target bound (rendering offscreen)
            if (IsRenderTargetBound)
            {
                //flip vertically
                _posFixup.Y = -_posFixup.Y;
                _posFixup.W = -_posFixup.W;
            }
            
            GL.Uniform4(posFixupLoc, _posFixup);
            GraphicsExtensions.CheckGLError();
        }

        private void PlatformApplyState()
        {
            Threading.EnsureUIThread();

            {
                PlatformApplyBlend();
            }

            if (_depthStencilStateDirty)
            {
                _actualDepthStencilState.PlatformApplyState(this);
                _depthStencilStateDirty = false;
            }

            if (_rasterizerStateDirty)
            {
                _actualRasterizerState.PlatformApplyState(this);
                _rasterizerStateDirty = false;
            }

            if (_scissorRectangleDirty)
            {
                PlatformApplyScissorRectangle();
                _scissorRectangleDirty = false;
            }
        }

        private void PlatformApplyBlend(bool force = false)
        {
            _actualBlendState.PlatformApplyState(this, force);
            ApplyBlendFactor(force);
        }

        private void ApplyBlendFactor(bool force)
        {
            if (force || BlendFactor != _lastBlendState.BlendFactor)
            {
                GL.BlendColor(
                    this.BlendFactor.R/255.0f,
                    this.BlendFactor.G/255.0f,
                    this.BlendFactor.B/255.0f,
                    this.BlendFactor.A/255.0f);
                GraphicsExtensions.CheckGLError();
                _lastBlendState.BlendFactor = this.BlendFactor;
            }
        }

        private void PlatformApplyScissorRectangle()
        {
            var scissorRect = _scissorRectangle;
            if (!IsRenderTargetBound)
                scissorRect.Y = PresentationParameters.BackBufferHeight - (scissorRect.Y + scissorRect.Height);
            GL.Scissor(scissorRect.X, scissorRect.Y, scissorRect.Width, scissorRect.Height);
            GraphicsExtensions.CheckGLError();
            _scissorRectangleDirty = false;
        }

        private void PlatformApplyViewport()
        {
            if (IsRenderTargetBound)
                GL.Viewport(_viewport.X, _viewport.Y, _viewport.Width, _viewport.Height);
            else
                GL.Viewport(_viewport.X, PresentationParameters.BackBufferHeight - _viewport.Y - _viewport.Height, _viewport.Width, _viewport.Height);
            GraphicsExtensions.LogGLError("GraphicsDevice.Viewport_set() GL.Viewport");

            GL.DepthRange(_viewport.MinDepth, _viewport.MaxDepth);
            GraphicsExtensions.LogGLError("GraphicsDevice.Viewport_set() GL.DepthRange");
                
            // In OpenGL we have to re-apply the special "posFixup"
            // vertex shader uniform if the viewport changes.
            _vertexShaderDirty = true;
        }

        private void PlatformApplyIndexBuffer()
        {
            if (_indexBufferDirty)
            {
                if (_indexBuffer != null)
                {
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer.ibo);
                    GraphicsExtensions.CheckGLError();
                }
                _indexBufferDirty = false;
            }
        }

        private void PlatformApplyVertexBuffers()
        {
        }

        private void PlatformApplyShaders()
        {
            if (_vertexShader == null)
                throw new InvalidOperationException("A vertex shader must be set!");
            if (_pixelShader == null)
                throw new InvalidOperationException("A pixel shader must be set!");

            if (_vertexShaderDirty || _pixelShaderDirty)
            {
                ActivateShaderProgram();

                if (_vertexShaderDirty)
                {
                    unchecked { _graphicsMetrics._vertexShaderCount++; }
                }

                if (_pixelShaderDirty)
                {
                    unchecked { _graphicsMetrics._pixelShaderCount++; }
                }

                _vertexShaderDirty = _pixelShaderDirty = false;
            }

            _vertexConstantBuffers.SetConstantBuffers();
            _pixelConstantBuffers.SetConstantBuffers();

            Textures.SetTextures(this);
            SamplerStates.PlatformSetSamplers(this);
        }

        private void PlatformDrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount)
        {
            PlatformApplyState();
            PlatformApplyIndexBuffer();
            PlatformApplyVertexBuffers();
            PlatformApplyShaders();

            var shortIndices = _indexBuffer.IndexElementSize == IndexElementSize.SixteenBits;

			var indexElementType = shortIndices ? DrawElementsType.UnsignedShort : DrawElementsType.UnsignedInt;
            var indexElementSize = shortIndices ? 2 : 4;
			var indexOffsetInBytes = (IntPtr)(startIndex * indexElementSize);
			var indexElementCount = GetElementCountArray(primitiveType, primitiveCount);
			var target = PrimitiveTypeGL(primitiveType);

            ApplyAttribs(_vertexShader, baseVertex);

            GL.DrawElements(target,
                                     indexElementCount,
                                     indexElementType,
                                     indexOffsetInBytes);
            GraphicsExtensions.CheckGLError();
        }

        private void PlatformDrawUserPrimitives<T>(
            PrimitiveType primitiveType, T[] vertexData, int vertexOffset, VertexDeclaration vertexDeclaration, int vertexCount)
            where T : struct
        {
            PlatformApplyState();
            //PlatformApplyIndexBuffer();
            //PlatformApplyVertexBuffers();
            PlatformApplyShaders();

            // Unbind current VBOs.
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GraphicsExtensions.CheckGLError();
            _vertexBuffersDirty = true;
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GraphicsExtensions.CheckGLError();
            _vertexBuffersDirty = true;

            // Pin the buffers.
            var vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
            try
            {
                // Setup the vertex declaration to point at the VB data.
                vertexDeclaration.GraphicsDevice = this;
                vertexDeclaration.Apply(_vertexShader, vbHandle.AddrOfPinnedObject(), ShaderProgramHash);

                //Draw
                GL.DrawArrays(PrimitiveTypeGL(primitiveType),
                              vertexOffset,
                              vertexCount);
                GraphicsExtensions.CheckGLError();
            }
            finally
            {
                // Release the handles.
                vbHandle.Free();
            }
        }

        private void PlatformDrawPrimitives(PrimitiveType primitiveType, int vertexStart, int vertexCount)
        {
            PlatformApplyState();
            PlatformApplyIndexBuffer();
            PlatformApplyVertexBuffers();
            PlatformApplyShaders();

            ApplyAttribs(_vertexShader, 0);

            if (vertexStart < 0)
                vertexStart = 0;

			GL.DrawArrays(PrimitiveTypeGL(primitiveType),
			              vertexStart,
			              vertexCount);
            GraphicsExtensions.CheckGLError();
        }

        private void PlatformDrawUserIndexedPrimitives<T>(
            PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration)
            where T : struct
        {
            PlatformApplyState();
            //PlatformApplyIndexBuffer();
            //PlatformApplyVertexBuffers();
            PlatformApplyShaders();

            // Unbind current VBOs.
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GraphicsExtensions.CheckGLError();
            _vertexBuffersDirty = true;
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GraphicsExtensions.CheckGLError();
            _indexBufferDirty = true;

            // Pin the buffers.
            var vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
            var ibHandle = GCHandle.Alloc(indexData, GCHandleType.Pinned);
            try
            {
                var vertexAddr = (IntPtr)(vbHandle.AddrOfPinnedObject().ToInt64() + vertexDeclaration.VertexStride * vertexOffset);

                // Setup the vertex declaration to point at the VB data.
                vertexDeclaration.GraphicsDevice = this;
                vertexDeclaration.Apply(_vertexShader, vertexAddr, ShaderProgramHash);

                //Draw
                GL.DrawElements(
                    PrimitiveTypeGL(primitiveType),
                    GetElementCountArray(primitiveType, primitiveCount),
                    DrawElementsType.UnsignedShort,
                    (IntPtr)(ibHandle.AddrOfPinnedObject().ToInt64() + (indexOffset * sizeof(short))));
                GraphicsExtensions.CheckGLError();
            }
            finally
            {
                // Release the handles.
                ibHandle.Free();
                vbHandle.Free();
            }
        }

        private void PlatformDrawUserIndexedPrimitives<T>(
            PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, int[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration)
            where T : struct
        {
            PlatformApplyState();
            //PlatformApplyIndexBuffer();
            //PlatformApplyVertexBuffers();
            PlatformApplyShaders();

            // Unbind current VBOs.
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GraphicsExtensions.CheckGLError();
            _vertexBuffersDirty = true;
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GraphicsExtensions.CheckGLError();
            _indexBufferDirty = true;

            // Pin the buffers.
            var vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
            var ibHandle = GCHandle.Alloc(indexData, GCHandleType.Pinned);
            try
            {
                var vertexAddr = (IntPtr)(vbHandle.AddrOfPinnedObject().ToInt64() + vertexDeclaration.VertexStride * vertexOffset);

                // Setup the vertex declaration to point at the VB data.
                vertexDeclaration.GraphicsDevice = this;
                vertexDeclaration.Apply(_vertexShader, vertexAddr, ShaderProgramHash);

                //Draw
                GL.DrawElements(
                    PrimitiveTypeGL(primitiveType),
                    GetElementCountArray(primitiveType, primitiveCount),
                    DrawElementsType.UnsignedInt,
                    (IntPtr)(ibHandle.AddrOfPinnedObject().ToInt64() + (indexOffset * sizeof(int))));
                GraphicsExtensions.CheckGLError();
            }
            finally
            {
                // Release the handles.
                ibHandle.Free();
                vbHandle.Free();
            }
        }

        private void PlatformDrawInstancedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount, int baseInstance, int instanceCount)
        {
            if (!GraphicsCapabilities.SupportsInstancing)
                throw new PlatformNotSupportedException("Instanced geometry drawing requires at least OpenGL 3.2 or GLES 3.2. Try upgrading your graphics card drivers.");
            PlatformApplyState();
            PlatformApplyIndexBuffer();
            PlatformApplyVertexBuffers();
            PlatformApplyShaders();

            var shortIndices = _indexBuffer.IndexElementSize == IndexElementSize.SixteenBits;

            var indexElementType = shortIndices ? DrawElementsType.UnsignedShort : DrawElementsType.UnsignedInt;
            var indexElementSize = shortIndices ? 2 : 4;
            var indexOffsetInBytes = (IntPtr)(startIndex * indexElementSize);
            var indexElementCount = GetElementCountArray(primitiveType, primitiveCount);
            var target = PrimitiveTypeGL(primitiveType);

            ApplyAttribs(_vertexShader, baseVertex);

            if (baseInstance > 0)
            {
                if (!GraphicsCapabilities.SupportsBaseIndexInstancing)
                    throw new PlatformNotSupportedException("Instanced geometry drawing with base instance requires at least OpenGL 4.2. Try upgrading your graphics card drivers.");

                GL.DrawElementsInstancedBaseInstance(target,
                                          indexElementCount,
                                          indexElementType,
                                          indexOffsetInBytes,
                                          instanceCount,
                                          baseInstance);
            }
            else
                GL.DrawElementsInstanced(target,
                                     indexElementCount,
                                     indexElementType,
                                     indexOffsetInBytes,
                                     instanceCount);

            GraphicsExtensions.CheckGLError();
        }

        private void PlatformGetBackBufferData<T>(Rectangle? rectangle, T[] data, int startIndex, int count) where T : struct
        {
            var rect = rectangle ?? new Rectangle(0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);
            var tSize = ReflectionHelpers.SizeOf<T>();
            var flippedY = PresentationParameters.BackBufferHeight - rect.Y - rect.Height;
            GL.ReadPixels(rect.X, flippedY, rect.Width, rect.Height, PixelFormat.Rgba, PixelType.UnsignedByte, data);

            // buffer is returned upside down, so we swap the rows around when copying over
            var rowSize = rect.Width*PresentationParameters.BackBufferFormat.GetSize() / tSize;
            var row = new T[rowSize];
            for (var dy = 0; dy < rect.Height/2; dy++)
            {
                var topRow = startIndex + dy*rowSize;
                var bottomRow = startIndex + (rect.Height - dy - 1)*rowSize;
                // copy the bottom row to buffer
                Array.Copy(data, bottomRow, row, 0, rowSize);
                // copy top row to bottom row
                Array.Copy(data, topRow, data, bottomRow, rowSize);
                // copy buffer to top row
                Array.Copy(row, 0, data, topRow, rowSize);
                count -= rowSize;
            }
        }

        private static Rectangle PlatformGetTitleSafeArea(int x, int y, int width, int height)
        {
            return new Rectangle(x, y, width, height);
        }
        
        internal void PlatformSetMultiSamplingToMaximum(PresentationParameters presentationParameters, out int quality)
        {
            presentationParameters.MultiSampleCount = 4;
            quality = 0;
        }

        internal void OnPresentationChanged()
        {
#if DESKTOPGL
            _currentWindowHandle = SdlGameWindow.Instance.Handle;

            CurrentConcreteContext.MakeCurrent(_currentWindowHandle);
            int swapInterval = ToGLSwapInterval(PresentationParameters.PresentationInterval);
            Sdl.GL.SetSwapInterval(swapInterval);
#endif

            ApplyRenderTargets(null);
        }

        // Holds information for caching
        private class BufferBindingInfo
        {
            public VertexDeclaration.VertexDeclarationAttributeInfo AttributeInfo;
            public IntPtr VertexOffset;
            public int InstanceFrequency;
            public int Vbo;

            public BufferBindingInfo(VertexDeclaration.VertexDeclarationAttributeInfo attributeInfo, IntPtr vertexOffset, int instanceFrequency, int vbo)
            {
                AttributeInfo = attributeInfo;
                VertexOffset = vertexOffset;
                InstanceFrequency = instanceFrequency;
                Vbo = vbo;
            }
        }


#if DESKTOPGL
        /// <summary>
        /// Converts <see cref="PresentInterval"/> to OpenGL swap interval.
        /// </summary>
        /// <returns>A value according to EXT_swap_control</returns>
        /// <param name="interval">The <see cref="PresentInterval"/> to convert.</param>
        private static int ToGLSwapInterval(PresentInterval interval)
        {
            // See http://www.opengl.org/registry/specs/EXT/swap_control.txt
            // and https://www.opengl.org/registry/specs/EXT/glx_swap_control_tear.txt
            // OpenTK checks for EXT_swap_control_tear:
            // if supported, a swap interval of -1 enables adaptive vsync;
            // otherwise -1 is converted to 1 (vsync enabled.)

            switch (interval)
            {
                case PresentInterval.Immediate:
                    return 0;
                case PresentInterval.One:
                    return 1;
                case PresentInterval.Two:
                    return 2;
                case PresentInterval.Default:

                default:
                    return -1;
            }
        }

        private void GetModeSwitchedSize(out int width, out int height)
        {
            var mode = new Sdl.Display.Mode
            {
                Width = PresentationParameters.BackBufferWidth,
                Height = PresentationParameters.BackBufferHeight,
                Format = 0,
                RefreshRate = 0,
                DriverData = IntPtr.Zero
            };
            Sdl.Display.Mode closest;
            Sdl.Display.GetClosestDisplayMode(0, mode, out closest);
            width = closest.Width;
            height = closest.Height;
        }

        private void GetDisplayResolution(out int width, out int height)
        {
            Sdl.Display.Mode mode;
            Sdl.Display.GetCurrentDisplayMode(0, out mode);
            width = mode.Width;
            height = mode.Height;
        }
#endif
    }
}
