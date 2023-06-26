// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.OpenGL;


namespace Microsoft.Xna.Platform.Graphics
{
    internal abstract class ConcreteGraphicsContextGL : GraphicsContextStrategy
    {
        // Keeps track of last applied state to avoid redundant OpenGL calls
        internal BlendState _lastBlendState = new BlendState();
        internal bool _lastBlendEnable = false;
        internal DepthStencilState _lastDepthStencilState = new DepthStencilState();
        internal RasterizerState _lastRasterizerState = new RasterizerState();

        internal ShaderProgram _shaderProgram = null;

        internal Vector4 _posFixup;

        internal BufferBindingInfo[] _bufferBindingInfos;
        internal int _activeBufferBindingInfosCount;
        internal bool[] _newEnabledVertexAttributes;
        internal readonly HashSet<int> _enabledVertexAttributes = new HashSet<int>();
        internal bool _attribsDirty;

        // Keeps track of last applied state to avoid redundant OpenGL calls
        internal Vector4 _lastClearColor = Vector4.Zero;
        internal float _lastClearDepth = 1.0f;
        internal int _lastClearStencil = 0;

        internal DepthStencilState _clearDepthStencilState = new DepthStencilState { StencilEnable = true };

        // FBO cache, we create 1 FBO per RenderTargetBinding combination
        internal Dictionary<RenderTargetBinding[], int> _glFramebuffers = new Dictionary<RenderTargetBinding[], int>(new ConcreteGraphicsContextGL.RenderTargetBindingArrayComparer());
        // FBO cache used to resolve MSAA rendertargets, we create 1 FBO per RenderTargetBinding combination
        internal Dictionary<RenderTargetBinding[], int> _glResolveFramebuffers = new Dictionary<RenderTargetBinding[], int>(new ConcreteGraphicsContextGL.RenderTargetBindingArrayComparer());

        internal ShaderProgram ShaderProgram { get { return _shaderProgram; } }

        public override Viewport Viewport
        {
            get { return base.Viewport; }
            set
            {
                base.Viewport = value;
                PlatformApplyViewport();
            }
        }

        internal ConcreteGraphicsContextGL(GraphicsDevice device)
            : base(device)
        {

        }

        public override void Clear(ClearOptions options, Vector4 color, float depth, int stencil)
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
            Rectangle prevScissorRect = ScissorRectangle;
            DepthStencilState prevDepthStencilState = DepthStencilState;
            BlendState prevBlendState = BlendState;
            ScissorRectangle = _viewport.Bounds;
            // DepthStencilState.Default has the Stencil Test disabled; 
            // make sure stencil test is enabled before we clear since
            // some drivers won't clear with stencil test disabled
            DepthStencilState = _clearDepthStencilState;
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

        internal void PlatformApplyState()
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
            Rectangle scissorRect = _scissorRectangle;
            if (!IsRenderTargetBound)
                scissorRect.Y = this.Device.PresentationParameters.BackBufferHeight - (scissorRect.Y + scissorRect.Height);
            GL.Scissor(scissorRect.X, scissorRect.Y, scissorRect.Width, scissorRect.Height);
            GraphicsExtensions.CheckGLError();
            _scissorRectangleDirty = false;
        }

        internal void PlatformApplyViewport()
        {
            if (this.IsRenderTargetBound)
                GL.Viewport(_viewport.X, _viewport.Y, _viewport.Width, _viewport.Height);
            else
                GL.Viewport(_viewport.X, this.Device.PresentationParameters.BackBufferHeight - _viewport.Y - _viewport.Height, _viewport.Width, _viewport.Height);
            GraphicsExtensions.LogGLError("GraphicsDevice.Viewport_set() GL.Viewport");

            GL.DepthRange(_viewport.MinDepth, _viewport.MaxDepth);
            GraphicsExtensions.LogGLError("GraphicsDevice.Viewport_set() GL.DepthRange");

            // In OpenGL we have to re-apply the special "_posFixup"
            // vertex shader uniform if the viewport changes.
            _vertexShaderDirty = true;
        }

        internal void PlatformApplyIndexBuffer()
        {
            if (_indexBufferDirty)
            {
                if (_indexBuffer != null)
                {
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer._ibo);
                    GraphicsExtensions.CheckGLError();
                }
                _indexBufferDirty = false;
            }
        }

        internal void PlatformApplyVertexBuffers()
        {
        }

        internal int GetCurrentShaderProgramHash2()
        {
            return _vertexShader.HashKey ^ _pixelShader.HashKey;
        }

        private void SetVertexAttributeArray(bool[] attrs)
        {
            for (int x = 0; x < attrs.Length; x++)
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

        // Get a hashed value based on the currently bound shaders
        // throws an exception if no shaders are bound
        private int GetCurrentShaderProgramHash()
        {
            if (_vertexShader == null && _pixelShader == null)
                throw new InvalidOperationException("There is no shader bound!");
            if (_vertexShader == null)
                return _pixelShader.HashKey;
            if (_pixelShader == null)
                return _vertexShader.HashKey;

            return _vertexShader.HashKey ^ _pixelShader.HashKey;
        }

        internal void PlatformApplyVertexBuffersAttribs(Shader shader, int baseVertex)
        {
            int programHash = GetCurrentShaderProgramHash();
            bool bindingsChanged = false;

            for (int slot = 0; slot < _vertexBuffers.Count; slot++)
            {
                var vertexBufferBinding = _vertexBuffers.Get(slot);
                VertexDeclaration vertexDeclaration = vertexBufferBinding.VertexBuffer.VertexDeclaration;
                var attrInfo = vertexDeclaration.GetAttributeInfo(shader, programHash);

                int vertexStride = vertexDeclaration.VertexStride;
                IntPtr offset = (IntPtr)(vertexDeclaration.VertexStride * (baseVertex + vertexBufferBinding.VertexOffset));

                if (!_attribsDirty &&
                    slot < _activeBufferBindingInfosCount &&
                    _bufferBindingInfos[slot].VertexOffset == offset &&
                    ReferenceEquals(_bufferBindingInfos, attrInfo) &&
                    _bufferBindingInfos[slot].InstanceFrequency == vertexBufferBinding.InstanceFrequency &&
                    _bufferBindingInfos[slot].Vbo == vertexBufferBinding.VertexBuffer._vbo)
                    continue;

                bindingsChanged = true;

                GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferBinding.VertexBuffer._vbo);
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
                    if (this.Device.Capabilities.SupportsInstancing)
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
                _bufferBindingInfos[slot].Vbo = vertexBufferBinding.VertexBuffer._vbo;
            }

            _attribsDirty = false;

            if (bindingsChanged)
            {
                for (int eva = 0; eva < _newEnabledVertexAttributes.Length; eva++)
                    _newEnabledVertexAttributes[eva] = false;
                for (int slot = 0; slot < _vertexBuffers.Count; slot++)
                {
                    for (int e = 0; e < _bufferBindingInfos[slot].AttributeInfo.Elements.Count; e++)
                    {
                        var element = _bufferBindingInfos[slot].AttributeInfo.Elements[e];
                        _newEnabledVertexAttributes[element.AttributeLocation] = true;
                    }
                }
                _activeBufferBindingInfosCount = _vertexBuffers.Count;
            }

            SetVertexAttributeArray(_newEnabledVertexAttributes);
        }

        internal void PlatformApplyUserVertexDataAttribs(VertexDeclaration vertexDeclaration, Shader shader, IntPtr baseVertex)
        {
            int programHash = GetCurrentShaderProgramHash();
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
                    (IntPtr)(baseVertex.ToInt64() + element.Offset));
                GraphicsExtensions.CheckGLError();

#if DESKTOPGL
                if (this.Device.Capabilities.SupportsInstancing)
                {
                    GL.VertexAttribDivisor(element.AttributeLocation, 0);
                    GraphicsExtensions.CheckGLError();
                }
#endif
            }

            SetVertexAttributeArray(attrInfo.EnabledAttributes);
            _attribsDirty = true;
        }

        internal static GLPrimitiveType PrimitiveTypeGL(PrimitiveType primitiveType)
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThrowIfDisposed();

            }

            base.Dispose(disposing);
        }

        static readonly FramebufferAttachment[] InvalidateFramebufferAttachements =
        {
            FramebufferAttachment.ColorAttachment0,
            FramebufferAttachment.DepthAttachment,
            FramebufferAttachment.StencilAttachment,
        };

        internal void PlatformResolveRenderTargets()
        {
            if (!this.IsRenderTargetBound)
                return;

            var renderTargetBinding = _currentRenderTargetBindings[0];
            var renderTarget = renderTargetBinding.RenderTarget as IRenderTarget;
            if (renderTarget.MultiSampleCount > 0 && this.Device._supportsBlitFramebuffer)
            {
                int glResolveFramebuffer = 0;
                if (!_glResolveFramebuffers.TryGetValue(_currentRenderTargetBindings, out glResolveFramebuffer))
                {
                    glResolveFramebuffer = GL.GenFramebuffer();
                    GraphicsExtensions.CheckGLError();
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, glResolveFramebuffer);
                    GraphicsExtensions.CheckGLError();

                    for (int i = 0; i < _currentRenderTargetCount; i++)
                    {
                        var renderTargetGL = (IRenderTargetGL)_currentRenderTargetBindings[i].RenderTarget;

                        FramebufferAttachment attachement = (FramebufferAttachment.ColorAttachment0 + i);
                        TextureTarget target = renderTargetGL.GetFramebufferTarget(renderTargetBinding.ArraySlice);
                        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachement, target, renderTargetGL.GLTexture, 0);
                        GraphicsExtensions.CheckGLError();
                    }
                    _glResolveFramebuffers.Add((RenderTargetBinding[])_currentRenderTargetBindings.Clone(), glResolveFramebuffer);
                }
                else
                {
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, glResolveFramebuffer);
                    GraphicsExtensions.CheckGLError();
                }

                // The only fragment operations which affect the resolve are the pixel ownership test, the scissor test, and dithering.
                if (_lastRasterizerState.ScissorTestEnable)
                {
                    GL.Disable(EnableCap.ScissorTest);
                    GraphicsExtensions.CheckGLError();
                }

                int glFramebuffer = _glFramebuffers[_currentRenderTargetBindings];
                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, glFramebuffer);
                GraphicsExtensions.CheckGLError();

                for (int i = 0; i < _currentRenderTargetCount; i++)
                {
                    renderTargetBinding = _currentRenderTargetBindings[i];
                    renderTarget = renderTargetBinding.RenderTarget as IRenderTarget;

                    GL.ReadBuffer(ReadBufferMode.ColorAttachment0 + i);
                    GraphicsExtensions.CheckGLError();
                    GL.DrawBuffer(DrawBufferMode.ColorAttachment0 + i);
                    GraphicsExtensions.CheckGLError();
                    GL.BlitFramebuffer(0, 0, renderTarget.Width, renderTarget.Height,
                                       0, 0, renderTarget.Width, renderTarget.Height,
                                       ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
                    GraphicsExtensions.CheckGLError();
                }

                if (renderTarget.RenderTargetUsage == RenderTargetUsage.DiscardContents && this.Device._supportsInvalidateFramebuffer)
                {
                    Debug.Assert(this.Device._supportsInvalidateFramebuffer);
                    GL.InvalidateFramebuffer(FramebufferTarget.Framebuffer, 3, InvalidateFramebufferAttachements);
                    GraphicsExtensions.CheckGLError();
                }

                if (_lastRasterizerState.ScissorTestEnable)
                {
                    GL.Enable(EnableCap.ScissorTest);
                    GraphicsExtensions.CheckGLError();
                }
            }

            for (int i = 0; i < _currentRenderTargetCount; i++)
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

        internal void PlatformApplyDefaultRenderTarget()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, this.Device._glDefaultFramebuffer);
            GraphicsExtensions.CheckGLError();

            // Reset the raster state because we flip vertices
            // when rendering offscreen and hence the cull direction.
            _rasterizerStateDirty = true;

            // Textures will need to be rebound to render correctly in the new render target.
            this.Textures.Dirty();
        }

        internal IRenderTarget PlatformApplyRenderTargets()
        {
            int glFramebuffer = 0;
            if (!_glFramebuffers.TryGetValue(_currentRenderTargetBindings, out glFramebuffer))
            {
                glFramebuffer = GL.GenFramebuffer();
                GraphicsExtensions.CheckGLError();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, glFramebuffer);
                GraphicsExtensions.CheckGLError();
                var renderTargetBinding = _currentRenderTargetBindings[0];
                var renderTargetGL = (IRenderTargetGL)renderTargetBinding.RenderTarget;

                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, renderTargetGL.GLDepthBuffer);
                GraphicsExtensions.CheckGLError();
                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.StencilAttachment, RenderbufferTarget.Renderbuffer, renderTargetGL.GLStencilBuffer);
                GraphicsExtensions.CheckGLError();

                for (int i = 0; i < _currentRenderTargetCount; i++)
                {
                    renderTargetBinding = _currentRenderTargetBindings[i];
                    var renderTarget = (IRenderTarget)renderTargetBinding.RenderTarget;
                    renderTargetGL = renderTargetBinding.RenderTarget as IRenderTargetGL;
                    var attachement = (FramebufferAttachment.ColorAttachment0 + i);

                    if (renderTargetGL.GLColorBuffer != renderTargetGL.GLTexture)
                    {
                        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, attachement, RenderbufferTarget.Renderbuffer, renderTargetGL.GLColorBuffer);
                        GraphicsExtensions.CheckGLError();
                    }
                    else
                    {
                        TextureTarget target = renderTargetGL.GetFramebufferTarget(renderTargetBinding.ArraySlice);
                        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachement, target, renderTargetGL.GLTexture, 0);
                        GraphicsExtensions.CheckGLError();
                    }
                }

                GraphicsExtensions.CheckFramebufferStatus();

                _glFramebuffers.Add((RenderTargetBinding[])_currentRenderTargetBindings.Clone(), glFramebuffer);
            }
            else
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, glFramebuffer);
                GraphicsExtensions.CheckGLError();
            }

#if DESKTOPGL
            GL.DrawBuffers(_currentRenderTargetCount, ((ConcreteGraphicsContext)this)._drawBuffers);
#endif

            // Reset the raster state because we flip vertices
            // when rendering offscreen and hence the cull direction.
            _rasterizerStateDirty = true;

            // Textures will need to be rebound to render correctly in the new render target.
            Textures.Dirty();

            return _currentRenderTargetBindings[0].RenderTarget as IRenderTarget;
        }

        // Holds information for caching
        internal class BufferBindingInfo
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

                for (int i = 0; i < first.Length; i++)
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

    }
}
