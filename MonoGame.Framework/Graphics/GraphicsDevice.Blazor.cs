// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Platform.Graphics;
using nkast.Wasm.Canvas.WebGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class GraphicsDevice
    {
        // TODO: make private
        internal IWebGLRenderingContext _glContext;

        private IWebGLRenderingContext GL { get { return _glContext; } }


        private ShaderProgramCache _programCache;
        private ShaderProgram _shaderProgram = null;

        Vector4 _posFixup;

        private BufferBindingInfo[] _bufferBindingInfos;
        private int _activeBufferBindingInfosCount;
        private bool[] _newEnabledVertexAttributes;
        private readonly HashSet<int> _enabledVertexAttributes = new HashSet<int>();
        private bool _attribsDirty;

        internal FramebufferHelper _framebufferHelper;

        private const WebGLFramebuffer _glDefaultFramebuffer = null;
        internal int MaxVertexAttributes;
        internal int _maxTextureSize = 0;

        // Keeps track of last applied state to avoid redundant OpenGL calls
        internal bool _lastBlendEnable = false;
        internal BlendState _lastBlendState = new BlendState();
        internal DepthStencilState _lastDepthStencilState = new DepthStencilState();
        internal RasterizerState _lastRasterizerState = new RasterizerState();

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

        private int ShaderProgramHash2
        {
            get { return _vertexShader.HashKey ^ _pixelShader.HashKey; }
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

        private void PlatformApplyVertexBuffersAttribs(Shader shader, int baseVertex)
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

                GL.BindBuffer(WebGLBufferType.ARRAY, vertexBufferBinding.VertexBuffer.vbo);
                GraphicsExtensions.CheckGLError();

                for (int e = 0; e < attrInfo.Elements.Count; e++)
                {
                    var element = attrInfo.Elements[e];
                    GL.VertexAttribPointer(element.AttributeLocation,
                        element.NumberOfElements,
                        element.VertexAttribPointerType,
                        element.Normalized,
                        vertexStride,
                        ((IntPtr)(offset.ToInt64() + element.Offset)).ToInt32());
                    GraphicsExtensions.CheckGLError();

                    // only set the divisor if instancing is supported
                    if (GraphicsCapabilities.SupportsInstancing)
                    {
                        throw new NotImplementedException();
                        //GL2.VertexAttribDivisor(element.AttributeLocation, vertexBufferBinding.InstanceFrequency);
                        //GraphicsExtensions.CheckGLError();
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

        private void PlatformApplyUserVertexDataAttribs(VertexDeclaration vertexDeclaration, Shader shader, int baseVertex)
        {
            var programHash = ShaderProgramHash;
            var attrInfo = vertexDeclaration.GetAttributeInfo(shader, programHash);

            // Apply the vertex attribute info
            for (int i = 0; i < attrInfo.Elements.Count; i++)
            {
                var element = attrInfo.Elements[i];
                GL.VertexAttribPointer(element.AttributeLocation,
                    element.NumberOfElements,
                    element.VertexAttribPointerType,
                    element.Normalized,
                    vertexDeclaration.VertexStride,
                    (baseVertex + element.Offset));
                GraphicsExtensions.CheckGLError();

                if (GraphicsCapabilities.SupportsInstancing)
                {
                    throw new NotImplementedException();
                    //GL2.VertexAttribDivisor(element.AttributeLocation, 0);
                    //GraphicsExtensions.CheckGLError();
                }
            }
            SetVertexAttributeArray(attrInfo.EnabledAttributes);
            _attribsDirty = true;
        }

        private void PlatformSetup()
        {
            _programCache = new ShaderProgramCache(this);

            var handle = PresentationParameters.DeviceWindowHandle;
            var gameWindow = BlazorGameWindow.FromHandle(handle);
            var canvas = gameWindow._canvas;

            // create context.
            _glContext = canvas.GetContext<IWebGLRenderingContext>();
            var contextStrategy = new ConcreteGraphicsContext(this);
            _mainContext = new GraphicsContext(this, contextStrategy);
            GraphicsExtensions.GL = _glContext; // for GraphicsExtensions.CheckGLError()
            //_glContext = new LogContent(_glContext);


            MaxTextureSlots = 4;

            _maxTextureSize = 2048;

            MaxVertexAttributes = 16;
            if (this.GraphicsProfile >= GraphicsProfile.FL10_1) _maxVertexBufferSlots = 32;

            _maxVertexBufferSlots = MaxVertexAttributes;
            _newEnabledVertexAttributes = new bool[MaxVertexAttributes];


        }

        private void PlatformInitialize()
        {
            // set actual backbuffer size
            PresentationParameters.BackBufferWidth = _glContext.Canvas.Width;
            PresentationParameters.BackBufferHeight = _glContext.Canvas.Height;

            _viewport = new Viewport(0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);

            // Ensure the vertex attributes are reset
            _enabledVertexAttributes.Clear();

            // Free all the cached shader programs. 
            _programCache.DisposePrograms();
            _shaderProgram = null;

            _framebufferHelper = new FramebufferHelper(this);

            // Force resetting states
            this._actualBlendState.PlatformApplyState(this, true);
            this.DepthStencilState.PlatformApplyState(this, true);
            this.RasterizerState.PlatformApplyState(this, true);

            _bufferBindingInfos = new BufferBindingInfo[_maxVertexBufferSlots];
            for (int i = 0; i < _bufferBindingInfos.Length; i++)
                _bufferBindingInfos[i] = new BufferBindingInfo(null, IntPtr.Zero, 0,  null);
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

            WebGLBufferBits bb = default(WebGLBufferBits);
            if ((options & ClearOptions.Target) != 0)
            {
                GL.ClearColor(color.X, color.Y, color.Z, color.W);
                GraphicsExtensions.CheckGLError();
                bb |= WebGLBufferBits.COLOR;
            }
            if ((options & ClearOptions.DepthBuffer) != 0)
            {
                GL.ClearDepth(depth);
                GraphicsExtensions.CheckGLError();
                bb |= WebGLBufferBits.DEPTH;
            }
            if ((options & ClearOptions.Stencil) != 0)
            {
                GL.ClearStencil(stencil);
                GraphicsExtensions.CheckGLError();
                bb |= WebGLBufferBits.STENCIL;
            }

            GL.Clear(bb);
            GraphicsExtensions.CheckGLError();

            // Restore the previous render state.
		    ScissorRectangle = prevScissorRect;
		    DepthStencilState = prevDepthStencilState;
		    BlendState = prevBlendState;
        }

        private void PlatformDispose()
        {
        }

        internal void PlatformPresent()
        {
        }

        private void PlatformApplyDefaultRenderTarget()
        {
            GL.BindFramebuffer(WebGLFramebufferType.FRAMEBUFFER, _glDefaultFramebuffer);
            GraphicsExtensions.CheckGLError();

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

                for (var i = 0; i < first.Length; i++)
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
        private Dictionary<RenderTargetBinding[], WebGLFramebuffer> _glFramebuffers = new Dictionary<RenderTargetBinding[], WebGLFramebuffer>(new RenderTargetBindingArrayComparer());
        // FBO cache used to resolve MSAA rendertargets, we create 1 FBO per RenderTargetBinding combination
        private Dictionary<RenderTargetBinding[], WebGLFramebuffer> _glResolveFramebuffers = new Dictionary<RenderTargetBinding[], WebGLFramebuffer>(new RenderTargetBindingArrayComparer());

        internal void PlatformCreateRenderTarget(IRenderTarget renderTarget, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
        {
            WebGLTexture color = null;
            WebGLRenderbuffer depth = null;
            WebGLRenderbuffer stencil = null;
            
            if (preferredMultiSampleCount > 0 && _framebufferHelper.SupportsBlitFramebuffer)
            {
                throw new NotImplementedException();
            }

            if (preferredDepthFormat != DepthFormat.None)
            {
                var depthInternalFormat = WebGLRenderbufferInternalFormat.DEPTH_COMPONENT16;
                var stencilInternalFormat = (WebGLRenderbufferInternalFormat)0;
                switch (preferredDepthFormat)
                {
                    case DepthFormat.Depth16: 
                        depthInternalFormat = WebGLRenderbufferInternalFormat.DEPTH_COMPONENT16;
                        break;
                    case DepthFormat.Depth24:
                        depthInternalFormat = WebGLRenderbufferInternalFormat.DEPTH_COMPONENT16;
                        break;
                    case DepthFormat.Depth24Stencil8:
                        depthInternalFormat = WebGLRenderbufferInternalFormat.DEPTH_COMPONENT16;
                        stencilInternalFormat = WebGLRenderbufferInternalFormat.STENCIL_INDEX8;
                        break;
                }

                if (depthInternalFormat != 0)
                {
                    depth = GL.CreateRenderbuffer();
                    GraphicsExtensions.CheckGLError();
                    GL.BindRenderbuffer(WebGLRenderbufferType.RENDERBUFFER, depth);
                    GraphicsExtensions.CheckGLError();
                    if (preferredMultiSampleCount > 0 /*&& GL.RenderbufferStorageMultisample != null*/)
                        throw new NotImplementedException();
                    else
                        GL.RenderbufferStorage(WebGLRenderbufferType.RENDERBUFFER, depthInternalFormat, width, height);
                    GraphicsExtensions.CheckGLError();
                    if (preferredDepthFormat == DepthFormat.Depth24Stencil8)
                    {
                        stencil = depth;
                        if (stencilInternalFormat != 0)
                        {
                            stencil = GL.CreateRenderbuffer();
                            GraphicsExtensions.CheckGLError();
                            GL.BindRenderbuffer(WebGLRenderbufferType.RENDERBUFFER, stencil);
                            GraphicsExtensions.CheckGLError();
                            if (preferredMultiSampleCount > 0 /*&& GL.RenderbufferStorageMultisample != null*/)
                                throw new NotImplementedException();
                            else
                                GL.RenderbufferStorage(WebGLRenderbufferType.RENDERBUFFER, stencilInternalFormat, width, height);
                            GraphicsExtensions.CheckGLError();
                        }
                    }
                }
            }

            var renderTargetGL = (IRenderTargetGL)renderTarget;
            if (color != null)
                renderTargetGL.GLColorBuffer = color;
            else
                renderTargetGL.GLColorBuffer = renderTargetGL.GLTexture;
            renderTargetGL.GLDepthBuffer = depth;
            renderTargetGL.GLStencilBuffer = stencil;
        }

        internal void PlatformDeleteRenderTarget(IRenderTarget renderTarget)
        {
            WebGLTexture color = null;
            WebGLRenderbuffer depth = null;
            WebGLRenderbuffer stencil = null;

            var renderTargetGL = (IRenderTargetGL)renderTarget;
            color = renderTargetGL.GLColorBuffer;
            depth = renderTargetGL.GLDepthBuffer;
            stencil = renderTargetGL.GLStencilBuffer;
            bool colorIsRenderbuffer = renderTargetGL.GLColorBuffer != renderTargetGL.GLTexture;

            if (color != null)
            {
                if (colorIsRenderbuffer)
                {
                    throw new NotImplementedException();
                }
                if (stencil != null && stencil != depth)
                {
                    stencil.Dispose();
                    GraphicsExtensions.CheckGLError();
                }
                if (depth != null)
                {
                    depth.Dispose();
                    GraphicsExtensions.CheckGLError();
                }

                var bindingsToDelete = new List<RenderTargetBinding[]>();
                foreach (var bindings in _glFramebuffers.Keys)
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
                    WebGLFramebuffer fbo = null;
                    if (_glFramebuffers.TryGetValue(bindings, out fbo))
                    {
                        fbo.Dispose();
                        GraphicsExtensions.CheckGLError();
                        _glFramebuffers.Remove(bindings);
                    }
                    if (_glResolveFramebuffers.TryGetValue(bindings, out fbo))
                    {
                        fbo.Dispose();
                        GraphicsExtensions.CheckGLError();

                        _glResolveFramebuffers.Remove(bindings);
                    }
                }
            }
        }

        internal static readonly WebGLFramebufferAttachmentPoint[] InvalidateFramebufferAttachements =
        {
            WebGLFramebufferAttachmentPoint.COLOR_ATTACHMENT0,
            WebGLFramebufferAttachmentPoint.DEPTH_ATTACHMENT,
            WebGLFramebufferAttachmentPoint.STENCIL_ATTACHMENT,
        };

        private void PlatformResolveRenderTargets()
        {
            if (!this.IsRenderTargetBound)
                return;

            var renderTargetBinding = _currentRenderTargetBindings[0];
            var renderTarget = renderTargetBinding.RenderTarget as IRenderTarget;
            if (renderTarget.MultiSampleCount > 0 && _framebufferHelper.SupportsBlitFramebuffer)
            {
                throw new NotImplementedException();
            }

            for (var i = 0; i < _currentRenderTargetCount; i++)
            {
                renderTargetBinding = _currentRenderTargetBindings[i];
                if (renderTargetBinding.RenderTarget.LevelCount > 1)
                {
                    var renderTargetGL = (IRenderTargetGL)renderTargetBinding.RenderTarget;
                    GL.BindTexture(renderTargetGL.GLTarget, renderTargetGL.GLTexture);
                    GraphicsExtensions.CheckGLError();
                    GL.GenerateMipmap(renderTargetGL.GLTarget);
                    GraphicsExtensions.CheckGLError();
                }
            }
        }

        private IRenderTarget PlatformApplyRenderTargets()
        {
            WebGLFramebuffer glFramebuffer = null;
            if (!_glFramebuffers.TryGetValue(_currentRenderTargetBindings, out glFramebuffer))
            {
                glFramebuffer = GL.CreateFramebuffer();
                GraphicsExtensions.CheckGLError();
                GL.BindFramebuffer(WebGLFramebufferType.FRAMEBUFFER, glFramebuffer);
                GraphicsExtensions.CheckGLError();
                var renderTargetBinding = _currentRenderTargetBindings[0];
                var renderTargetGL = (IRenderTargetGL)renderTargetBinding.RenderTarget;             
                GL.FramebufferRenderbuffer(WebGLFramebufferType.FRAMEBUFFER, WebGLFramebufferAttachmentPoint.DEPTH_ATTACHMENT, WebGLRenderbufferType.RENDERBUFFER, renderTargetGL.GLDepthBuffer);
                GraphicsExtensions.CheckGLError();
                GL.FramebufferRenderbuffer(WebGLFramebufferType.FRAMEBUFFER, WebGLFramebufferAttachmentPoint.STENCIL_ATTACHMENT, WebGLRenderbufferType.RENDERBUFFER, renderTargetGL.GLStencilBuffer);
                GraphicsExtensions.CheckGLError();

                for (var i = 0; i < _currentRenderTargetCount; i++)
                {
                    renderTargetBinding = _currentRenderTargetBindings[i];
                    var renderTarget = (IRenderTarget)renderTargetBinding.RenderTarget;
                    renderTargetGL = renderTargetBinding.RenderTarget as IRenderTargetGL;
                    var attachement = (WebGLFramebufferAttachmentPoint.COLOR_ATTACHMENT0 + i);
                    if (renderTargetGL.GLColorBuffer != renderTargetGL.GLTexture)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        WebGLTextureTarget target = renderTargetGL.GetFramebufferTarget(renderTargetBinding.ArraySlice);
                        GL.FramebufferTexture2D(WebGLFramebufferType.FRAMEBUFFER, attachement, target, renderTargetGL.GLTexture);
                        GraphicsExtensions.CheckGLError();
                    }
                }

                GraphicsExtensions.CheckFramebufferStatus();

                _glFramebuffers.Add((RenderTargetBinding[])_currentRenderTargetBindings.Clone(), glFramebuffer);
            }
            else
            {
                GL.BindFramebuffer(WebGLFramebufferType.FRAMEBUFFER, glFramebuffer);
                GraphicsExtensions.CheckGLError();
            }
#if !GLES
            //GL.DrawBuffers(_currentRenderTargetCount, _drawBuffers);
#endif

            // Reset the raster state because we flip vertices
            // when rendering offscreen and hence the cull direction.
            _rasterizerStateDirty = true;

            // Textures will need to be rebound to render correctly in the new render target.
            Textures.Dirty();

            return _currentRenderTargetBindings[0].RenderTarget as IRenderTarget;
        }

        private static WebGLPrimitiveType PrimitiveTypeGL(PrimitiveType primitiveType)
        {
            switch (primitiveType)
            {
                case PrimitiveType.PointList:
                    throw new NotSupportedException();
                case PrimitiveType.LineList:
                    return WebGLPrimitiveType.LINES;
                case PrimitiveType.LineStrip:
                    return WebGLPrimitiveType.LINE_STRIP;
                case PrimitiveType.TriangleList:
                    return WebGLPrimitiveType.TRIANGLES;
                case PrimitiveType.TriangleStrip:
                    return WebGLPrimitiveType.TRIANGLE_STRIP;
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
            var shaderProgram = _programCache.GetProgram(VertexShader, PixelShader, ShaderProgramHash2);
            if (shaderProgram.Program == null)
                return;

            // Set the new program if it has changed.
            if (_shaderProgram != shaderProgram)
            {
                GL.UseProgram(shaderProgram.Program);
                GraphicsExtensions.CheckGLError();
                _shaderProgram = shaderProgram;
            }

            var posFixupLoc = shaderProgram.GetUniformLocation("posFixup");
            if (posFixupLoc == null)
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
            
            GL.Uniform4f(posFixupLoc, _posFixup.X, _posFixup.Y, _posFixup.Z, _posFixup.W);
            GraphicsExtensions.CheckGLError();
        }

        private void PlatformApplyState()
        {
            // Threading.EnsureUIThread();

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

        private void PlatformApplyBlend()
        {
            if (_blendStateDirty)            
            {
                _actualBlendState.PlatformApplyState(this);
                _blendStateDirty = false;
            }

            if (_blendFactorDirty)
            {
                GL.BlendColor(
                    this.BlendFactor.R/255.0f,
                    this.BlendFactor.G/255.0f,
                    this.BlendFactor.B/255.0f,
                    this.BlendFactor.A/255.0f);
                GraphicsExtensions.CheckGLError();
                _blendFactorDirty = false;
            }
        }

        private void PlatformApplyScissorRectangle()
        {
            var scissorRect = _scissorRectangle;
            if (!IsRenderTargetBound)
                scissorRect.Y = PresentationParameters.BackBufferHeight - (scissorRect.Y + scissorRect.Height);
            GL.Scissor(scissorRect.X, scissorRect.Y, scissorRect.Width, scissorRect.Height);
            GraphicsExtensions.CheckGLError();
        }

        private void PlatformApplyViewport()
        {
            if (IsRenderTargetBound)
                GL.Viewport(_viewport.X, _viewport.Y, _viewport.Width, _viewport.Height);
            else
                GL.Viewport(_viewport.X, PresentationParameters.BackBufferHeight - _viewport.Y - _viewport.Height, _viewport.Width, _viewport.Height);
            GraphicsExtensions.CheckGLError(); // GraphicsExtensions.LogGLError("GraphicsDevice.Viewport_set() GL.Viewport");

            GL.DepthRange(_viewport.MinDepth, _viewport.MaxDepth);
            //GraphicsExtensions.CheckGLError(); // GraphicsExtensions.LogGLError("GraphicsDevice.Viewport_set() GL.DepthRange");
                
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
                    GL.BindBuffer(WebGLBufferType.ELEMENT_ARRAY, _indexBuffer.ibo);
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

                _vertexShaderDirty = false;
                _pixelShaderDirty = false;
            }

            _vertexConstantBuffers.Apply();
            _pixelConstantBuffers.Apply();

            Textures.Apply();
            SamplerStates.PlatformApply();
        }

        private void PlatformDrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount)
        {
            PlatformApplyState();
            PlatformApplyIndexBuffer();
            PlatformApplyVertexBuffers();
            PlatformApplyShaders();

            var shortIndices = _indexBuffer.IndexElementSize == IndexElementSize.SixteenBits;

			var indexElementType = shortIndices ? WebGLDataType.USHORT : WebGLDataType.UINT;
            var indexElementSize = shortIndices ? 2 : 4;
			var indexOffsetInBytes = (startIndex * indexElementSize);
			var indexElementCount = GetElementCountArray(primitiveType, primitiveCount);
			var target = PrimitiveTypeGL(primitiveType);

            PlatformApplyVertexBuffersAttribs(_vertexShader, baseVertex);

            GL.DrawElements(target,
                                     indexElementCount,
                                     indexElementType,
                                     indexOffsetInBytes);
            GraphicsExtensions.CheckGLError();
        }

        private void PlatformDrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, VertexDeclaration vertexDeclaration, int vertexCount) where T : struct
        {
            PlatformApplyState();
            //PlatformApplyIndexBuffer();
            //PlatformApplyVertexBuffers();
            PlatformApplyShaders();

            // TODO: reimplement without creating new buffers

            // create and bind vertexBuffer
            var vbo = GL.CreateBuffer();
            GraphicsExtensions.CheckGLError();
            GL.BindBuffer(WebGLBufferType.ARRAY, vbo);
            GraphicsExtensions.CheckGLError();
            GL.BufferData(WebGLBufferType.ARRAY,
                          (vertexDeclaration.VertexStride * vertexData.Length),
                          (false) ? WebGLBufferUsageHint.STREAM_DRAW : WebGLBufferUsageHint.STATIC_DRAW);
            GraphicsExtensions.CheckGLError();
            // mark the default Vertex buffers for rebinding
            _vertexBuffersDirty = true;

            //set vertex data
            GL.BufferSubData(WebGLBufferType.ARRAY, 0, vertexData, vertexData.Length);
            GraphicsExtensions.CheckGLError();

            // Setup the vertex declaration to point at the VB data.
            vertexDeclaration.GraphicsDevice = this;
            PlatformApplyUserVertexDataAttribs(vertexDeclaration, _vertexShader, vertexOffset);

            var target = PrimitiveTypeGL(primitiveType);

            GL.DrawArrays(PrimitiveTypeGL(primitiveType),
                          vertexOffset,
                          vertexCount);
            GraphicsExtensions.CheckGLError();

            //GL.BindBuffer(WebGLBufferType.ARRAY, null);
            //GraphicsExtensions.CheckGLError();
            //GL.BindBuffer(WebGLBufferType.ELEMENT_ARRAY, null);
            //GraphicsExtensions.CheckGLError();

            vbo.Dispose();
        }

        private void PlatformDrawPrimitives(PrimitiveType primitiveType, int vertexStart, int vertexCount)
        {
            PlatformApplyState();
            //PlatformApplyIndexBuffer();
            PlatformApplyVertexBuffers();
            PlatformApplyShaders();

            PlatformApplyVertexBuffersAttribs(_vertexShader, 0);

            if (vertexStart < 0)
                vertexStart = 0;

            GL.DrawArrays(PrimitiveTypeGL(primitiveType),
			              vertexStart,
			              vertexCount);
            GraphicsExtensions.CheckGLError();
        }

        private void PlatformDrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
        {
            PlatformApplyState();
            //PlatformApplyIndexBuffer();
            //PlatformApplyVertexBuffers();
            PlatformApplyShaders();

            // TODO: reimplement without creating new buffers

            // create and bind vertexBuffer
            var vbo = GL.CreateBuffer();
            GraphicsExtensions.CheckGLError();
            GL.BindBuffer(WebGLBufferType.ARRAY, vbo);
            GraphicsExtensions.CheckGLError();
            GL.BufferData(WebGLBufferType.ARRAY,
                          (vertexDeclaration.VertexStride * vertexData.Length),
                          (false) ? WebGLBufferUsageHint.STREAM_DRAW : WebGLBufferUsageHint.STATIC_DRAW);
            GraphicsExtensions.CheckGLError();
            // mark the default Vertex buffers for rebinding
            _vertexBuffersDirty = true;

            //set vertex data
            GL.BufferSubData(WebGLBufferType.ARRAY, 0, vertexData, vertexData.Length);
            GraphicsExtensions.CheckGLError();

            // create and bind index buffer
            var ibo = GL.CreateBuffer();
            GraphicsExtensions.CheckGLError();
            GL.BindBuffer(WebGLBufferType.ELEMENT_ARRAY, ibo);
            GraphicsExtensions.CheckGLError();
            GL.BufferData(WebGLBufferType.ELEMENT_ARRAY,
                          (indexData.Length * sizeof(short)),
                          (false) ? WebGLBufferUsageHint.STREAM_DRAW : WebGLBufferUsageHint.STATIC_DRAW);
            GraphicsExtensions.CheckGLError();
            // mark the default index buffer for rebinding
            _indexBufferDirty = true;

            // set index buffer
            GL.BufferSubData(WebGLBufferType.ELEMENT_ARRAY, 0, indexData, indexData.Length);
            GraphicsExtensions.CheckGLError();

            // Setup the vertex declaration to point at the VB data.
            vertexDeclaration.GraphicsDevice = this;
            PlatformApplyUserVertexDataAttribs(vertexDeclaration, _vertexShader, vertexOffset);


            var indexElementCount = GetElementCountArray(primitiveType, primitiveCount);
            var indexOffsetInBytes = (indexOffset * sizeof(short));
            var target = PrimitiveTypeGL(primitiveType);

            GL.DrawElements(target,
                                     indexElementCount,
                                     WebGLDataType.USHORT,
                                     indexOffsetInBytes);
            GraphicsExtensions.CheckGLError();

            //GL.BindBuffer(WebGLBufferType.ARRAY, null);
            //GraphicsExtensions.CheckGLError();
            //GL.BindBuffer(WebGLBufferType.ELEMENT_ARRAY, null);
            //GraphicsExtensions.CheckGLError();

            ibo.Dispose();
            vbo.Dispose();
        }

        private void PlatformDrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, int[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
        {
            PlatformApplyState();
            //PlatformApplyIndexBuffer();
            //PlatformApplyVertexBuffers();
            PlatformApplyShaders();

            throw new NotImplementedException();
        }

        private void PlatformDrawInstancedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount, int baseInstance, int instanceCount)
        {
            if (!GraphicsCapabilities.SupportsInstancing)
                throw new PlatformNotSupportedException("Instanced geometry drawing requires at least OpenGL 3.2 or GLES 3.2. Try upgrading your graphics card drivers.");

            PlatformApplyState();
            PlatformApplyIndexBuffer();
            PlatformApplyVertexBuffers();
            PlatformApplyShaders();

            throw new NotImplementedException();
        }

        private void PlatformGetBackBufferData<T>(Rectangle? rect, T[] data, int startIndex, int count) where T : struct
        {
            throw new NotImplementedException();
        }

        private static Rectangle PlatformGetTitleSafeArea(int x, int y, int width, int height)
        {
            return new Rectangle(x, y, width, height);
        }
        
        internal void PlatformSetMultiSamplingToMaximum(PresentationParameters presentationParameters, out int quality)
        {
            presentationParameters.MultiSampleCount = 0;
            quality = 0;
        }

        internal void OnPresentationChanged()
        {
            ApplyRenderTargets(null);
        }

        // Holds information for caching
        private class BufferBindingInfo
        {
            public VertexDeclaration.VertexDeclarationAttributeInfo AttributeInfo;
            public IntPtr VertexOffset;
            public int InstanceFrequency;
            public WebGLBuffer Vbo;

            public BufferBindingInfo(VertexDeclaration.VertexDeclarationAttributeInfo attributeInfo, IntPtr vertexOffset, int instanceFrequency, WebGLBuffer vbo)
            {
                AttributeInfo = attributeInfo;
                VertexOffset = vertexOffset;
                InstanceFrequency = instanceFrequency;
                Vbo = vbo;
            }
        }

    }
}
