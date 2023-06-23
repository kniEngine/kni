// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteGraphicsContext : GraphicsContextStrategy
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

        internal DepthStencilState _clearDepthStencilState = new DepthStencilState { StencilEnable = true };

        // FBO cache, we create 1 FBO per RenderTargetBinding combination
        internal Dictionary<RenderTargetBinding[], WebGLFramebuffer> _glFramebuffers = new Dictionary<RenderTargetBinding[], WebGLFramebuffer>(new RenderTargetBindingArrayComparer());
        // FBO cache used to resolve MSAA rendertargets, we create 1 FBO per RenderTargetBinding combination
        internal Dictionary<RenderTargetBinding[], WebGLFramebuffer> _glResolveFramebuffers = new Dictionary<RenderTargetBinding[], WebGLFramebuffer>(new RenderTargetBindingArrayComparer());

        internal ShaderProgram ShaderProgram { get { return _shaderProgram; } }


        internal IWebGLRenderingContext GL { get { return Device._glContext; } }

        public override Viewport Viewport
        {
            get { return base.Viewport; }
            set
            {
                base.Viewport = value;
                PlatformApplyViewport();
            }
        }

        internal ConcreteGraphicsContext(GraphicsDevice device)
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
		    var prevScissorRect = ScissorRectangle;
		    var prevDepthStencilState = DepthStencilState;
            var prevBlendState = BlendState;
            ScissorRectangle = _viewport.Bounds;
            // DepthStencilState.Default has the Stencil Test disabled; 
            // make sure stencil test is enabled before we clear since
            // some drivers won't clear with stencil test disabled
            DepthStencilState = _clearDepthStencilState;
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

        internal void PlatformApplyState()
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
                    this.BlendFactor.R / 255.0f,
                    this.BlendFactor.G / 255.0f,
                    this.BlendFactor.B / 255.0f,
                    this.BlendFactor.A / 255.0f);
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
        }

        internal void PlatformApplyViewport()
        {
            if (this.IsRenderTargetBound)
                GL.Viewport(_viewport.X, _viewport.Y, _viewport.Width, _viewport.Height);
            else
                GL.Viewport(_viewport.X, this.Device.PresentationParameters.BackBufferHeight - _viewport.Y - _viewport.Height, _viewport.Width, _viewport.Height);
            GraphicsExtensions.CheckGLError(); // GraphicsExtensions.LogGLError("GraphicsDevice.Viewport_set() GL.Viewport");

            GL.DepthRange(_viewport.MinDepth, _viewport.MaxDepth);
            //GraphicsExtensions.CheckGLError(); // GraphicsExtensions.LogGLError("GraphicsDevice.Viewport_set() GL.DepthRange");

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
                    GL.BindBuffer(WebGLBufferType.ELEMENT_ARRAY, _indexBuffer.ibo);
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
                VertexBufferBinding vertexBufferBinding = _vertexBuffers.Get(slot);
                VertexDeclaration vertexDeclaration = vertexBufferBinding.VertexBuffer.VertexDeclaration;
                var attrInfo = vertexDeclaration.GetAttributeInfo(shader, programHash);

                int vertexStride = vertexDeclaration.VertexStride;
                IntPtr offset = (IntPtr)(vertexDeclaration.VertexStride * (baseVertex + vertexBufferBinding.VertexOffset));

                if (!_attribsDirty &&
                    slot < _activeBufferBindingInfosCount &&
                    _bufferBindingInfos[slot].VertexOffset == offset &&
                    ReferenceEquals(_bufferBindingInfos, attrInfo) &&
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
                    if (this.Device.Capabilities.SupportsInstancing)
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

        internal void PlatformApplyUserVertexDataAttribs(VertexDeclaration vertexDeclaration, Shader shader, int baseVertex)
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
                    (baseVertex + element.Offset));
                GraphicsExtensions.CheckGLError();

                if (this.Device.Capabilities.SupportsInstancing)
                {
                    throw new NotImplementedException();
                    //GL2.VertexAttribDivisor(element.AttributeLocation, 0);
                    //GraphicsExtensions.CheckGLError();
                }
            }

            SetVertexAttributeArray(attrInfo.EnabledAttributes);
            _attribsDirty = true;
        }

        internal static WebGLPrimitiveType PrimitiveTypeGL(PrimitiveType primitiveType)
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThrowIfDisposed();

            }

            base.Dispose(disposing);
        }

        internal static readonly WebGLFramebufferAttachmentPoint[] InvalidateFramebufferAttachements =
        {
            WebGLFramebufferAttachmentPoint.COLOR_ATTACHMENT0,
            WebGLFramebufferAttachmentPoint.DEPTH_ATTACHMENT,
            WebGLFramebufferAttachmentPoint.STENCIL_ATTACHMENT,
        };

        // Holds information for caching
        internal class BufferBindingInfo
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

    }
}
