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


        internal ConcreteGraphicsContext(GraphicsDevice device)
            : base(device)
        {

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

        internal void PlatformApplyBlend()
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

        internal void PlatformApplyScissorRectangle()
        {
            Rectangle scissorRect = _scissorRectangle;
            if (!IsRenderTargetBound)
                scissorRect.Y = this.Device.PresentationParameters.BackBufferHeight - (scissorRect.Y + scissorRect.Height);
            GL.Scissor(scissorRect.X, scissorRect.Y, scissorRect.Width, scissorRect.Height);
            GraphicsExtensions.CheckGLError();
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

        internal int ShaderProgramHash2
        {
            get { return _vertexShader.HashKey ^ _pixelShader.HashKey; }
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

        internal void PlatformApplyVertexBuffersAttribs(Shader shader, int baseVertex)
        {
            int programHash = this.ShaderProgramHash;
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
            int programHash = this.ShaderProgramHash;
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
