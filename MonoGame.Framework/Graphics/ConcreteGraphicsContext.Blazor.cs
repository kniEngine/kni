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

        internal ShaderProgram PlatformShaderProgram { get { return _shaderProgram; } }


        internal IWebGLRenderingContext GL { get { return Device._glContext; } }


        internal ConcreteGraphicsContext(GraphicsDevice device)
            : base(device)
        {

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
