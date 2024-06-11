// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Platform.Graphics;
using Microsoft.Xna.Platform.Graphics.Utilities;


namespace Microsoft.Xna.Framework.Graphics
{
    public sealed class GraphicsContext : IDisposable
        , IPlatformGraphicsContext
    {
        private GraphicsContextStrategy _strategy;
        private GraphicsDeviceStrategy _deviceStrategy;
        private bool _isDisposed = false;

        GraphicsContextStrategy IPlatformGraphicsContext.Strategy { get { return _strategy; } }
        GraphicsDeviceStrategy IPlatformGraphicsContext.DeviceStrategy { get { return _deviceStrategy; } }


        internal GraphicsContext(GraphicsDevice device) : this(((IPlatformGraphicsDevice)device).Strategy)
        {
        }

        internal GraphicsContext(GraphicsDeviceStrategy deviceStrategy)
        {
            _deviceStrategy = deviceStrategy;
            _strategy = deviceStrategy.CreateGraphicsContextStrategy(this);
        }


        /// <summary>
        /// The rendering information for debugging and profiling.
        /// The metrics are reset every frame after draw within <see cref="GraphicsDevice.Present"/>. 
        /// </summary>
        public GraphicsMetrics Metrics
        {
            get { return _strategy._graphicsMetrics; }
            set { _strategy._graphicsMetrics = value; }
        }

        /// <summary>
        /// Get or set the color a <see cref="RenderTarget2D"/> is cleared to when it is set.
        /// </summary>
        public Color DiscardColor
        {
            get { return _strategy.DiscardColor; }
            set { _strategy.DiscardColor = value; }
        }

        /// <summary>
        /// Access debugging APIs for the graphics subsystem.
        /// </summary>
        public GraphicsDebug GraphicsDebug
        {
            get { return _strategy.GraphicsDebug; }
            set { _strategy.GraphicsDebug = value; }
        }

        public Rectangle ScissorRectangle
        {
            get { return _strategy.ScissorRectangle; }
            set { _strategy.ScissorRectangle = value; }
        }

        public Viewport Viewport
        {
            get { return _strategy.Viewport; }
            set { _strategy.Viewport = value; }
        }

        public BlendState BlendState
        {
            get { return _strategy.BlendState; }
            set { _strategy.BlendState = value; }
        }

        public Color BlendFactor
        {
            get { return _strategy.BlendFactor; }
            set { _strategy.BlendFactor = value; }
        }

        public DepthStencilState DepthStencilState
        {
            get { return _strategy.DepthStencilState; }
            set { _strategy.DepthStencilState = value; }
        }

        public RasterizerState RasterizerState
        {
            get { return _strategy.RasterizerState; }
            set { _strategy.RasterizerState = value; }
        }

        public SamplerStateCollection SamplerStates
        {
            get { return _strategy.SamplerStates; }
        }

        public SamplerStateCollection VertexSamplerStates
        {
            get { return _strategy.VertexSamplerStates; }
        }

        public TextureCollection Textures
        {
            get { return _strategy.Textures; }
        }

        public TextureCollection VertexTextures
        {
            get { return _strategy.VertexTextures; }
        }

        public IndexBuffer Indices
        {
            get { return _strategy.Indices; }
            set { _strategy.Indices = value; }
        }

        public void Clear(Color color)
        {
            ClearOptions options = ClearOptions.Target
                                 | ClearOptions.DepthBuffer
                                 | ClearOptions.Stencil;
            _strategy.Clear(options, color.ToVector4(), _strategy.Viewport.MaxDepth, 0);
        }

        public void Clear(ClearOptions options, Color color, float depth, int stencil)
        {
            _strategy.Clear(options, color.ToVector4(), depth, stencil);
        }

        public void Clear(ClearOptions options, Vector4 color, float depth, int stencil)
        {
            _strategy.Clear(options, color, depth, stencil);
        }

        public void SetVertexBuffer(VertexBuffer vertexBuffer)
        {
            _strategy.SetVertexBuffer(vertexBuffer);
        }

        public void SetVertexBuffer(VertexBuffer vertexBuffer, int vertexOffset)
        {
            _strategy.SetVertexBuffer(vertexBuffer, vertexOffset);
        }

        public void SetVertexBuffers(VertexBufferBinding[] vertexBuffers)
        {
            _strategy.SetVertexBuffers(vertexBuffers);
        }

        internal VertexShader VertexShader
        {
            get { return _strategy.VertexShader; }
            set { _strategy.VertexShader = value; }
        }

        internal PixelShader PixelShader
        {
            get { return _strategy.PixelShader; }
            set { _strategy.PixelShader = value; }
        }

        public void GetRenderTargets(RenderTargetBinding[] bindings)
        {
            _strategy.GetRenderTargets(bindings);
        }

        public void SetRenderTarget(RenderTarget2D renderTarget)
        {
            if (renderTarget == null)
            {
                SetRenderTargets(null);
            }
            else
            {
                _strategy.SingleRenderTargetBinding[0] = new RenderTargetBinding(renderTarget);
                SetRenderTargets(_strategy.SingleRenderTargetBinding);
            }
        }

        public void SetRenderTarget(RenderTargetCube renderTarget, CubeMapFace cubeMapFace)
        {
            if (renderTarget == null)
            {
                SetRenderTargets(null);
            }
            else
            {
                _strategy.SingleRenderTargetBinding[0] = new RenderTargetBinding(renderTarget, cubeMapFace);
                SetRenderTargets(_strategy.SingleRenderTargetBinding);
            }
        }

        /// <remarks>Only implemented for DirectX </remarks>
        public void SetRenderTarget(RenderTarget2D renderTarget, int arraySlice)
        {
            if (!_deviceStrategy.Capabilities.SupportsTextureArrays)
                throw new InvalidOperationException("Texture arrays are not supported on this graphics device");

            if (renderTarget == null)
            {
                SetRenderTargets(null);
            }
            else
            {
                _strategy.SingleRenderTargetBinding[0] = new RenderTargetBinding(renderTarget, arraySlice);
                SetRenderTargets(_strategy.SingleRenderTargetBinding);
            }
        }

        /// <remarks>Only implemented for DirectX </remarks>
        public void SetRenderTarget(RenderTarget3D renderTarget, int arraySlice)
        {
            if (renderTarget == null)
            {
                SetRenderTargets(null);
            }
            else
            {
                _strategy.SingleRenderTargetBinding[0] = new RenderTargetBinding(renderTarget, arraySlice);
                SetRenderTargets(_strategy.SingleRenderTargetBinding);
            }
        }

        public void SetRenderTargets(params RenderTargetBinding[] renderTargets)
        {
            // Avoid having to check for both null and zero length array.
            int renderTargetCount = 0;
            if (renderTargets != null)
            {
                renderTargetCount = renderTargets.Length;
                if (renderTargetCount == 0)
                    renderTargets = null;
            }

            if (_deviceStrategy.GraphicsProfile == GraphicsProfile.Reach && renderTargetCount > 1)
                throw new NotSupportedException("Reach profile supports a maximum of 1 simultaneous rendertargets");
            if (_deviceStrategy.GraphicsProfile == GraphicsProfile.HiDef && renderTargetCount > 4)
                throw new NotSupportedException("HiDef profile supports a maximum of 4 simultaneous rendertargets");
            if (renderTargetCount > 8)
                throw new NotSupportedException("Current profile supports a maximum of 8 simultaneous rendertargets");

            // Try to early out if the current and new bindings are equal.
            if (_strategy.RenderTargetCount == renderTargetCount)
            {
                bool isEqual = true;
                for (int i = 0; i < _strategy.RenderTargetCount; i++)
                {
                    if (_strategy.CurrentRenderTargetBindings[i].RenderTarget != renderTargets[i].RenderTarget ||
                        _strategy.CurrentRenderTargetBindings[i].ArraySlice != renderTargets[i].ArraySlice)
                    {
                        isEqual = false;
                        break;
                    }
                }

                if (isEqual)
                    return;
            }

            _strategy.ApplyRenderTargets(renderTargets);
        }

        /// <summary>
        /// Draw geometry by indexing into the vertex buffer.
        /// </summary>
        /// <param name="primitiveType">The type of primitives in the index buffer.</param>
        /// <param name="baseVertex">Used to offset the vertex range indexed from the vertex buffer.</param>
        /// <param name="minVertexIndex">This is unused and remains here only for XNA API compatibility.</param>
        /// <param name="numVertices">This is unused and remains here only for XNA API compatibility.</param>
        /// <param name="startIndex">The index within the index buffer to start drawing from.</param>
        /// <param name="primitiveCount">The number of primitives to render from the index buffer.</param>
        /// <remarks>Note that minVertexIndex and numVertices are unused in MonoGame and will be ignored.</remarks>
        public void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int minVertexIndex, int numVertices, int startIndex, int primitiveCount)
        {
            if (_strategy._vertexBuffers.Count == 0)
                throw new InvalidOperationException("Vertex buffer must be set before calling DrawIndexedPrimitives.");

            if (_strategy.Indices == null)
                throw new InvalidOperationException("Index buffer must be set before calling DrawIndexedPrimitives.");

            if (_deviceStrategy.GraphicsProfile == GraphicsProfile.Reach && primitiveCount > 65535)
                throw new NotSupportedException("Reach profile supports a maximum of 65535 primitives per draw call.");
            if (_deviceStrategy.GraphicsProfile == GraphicsProfile.HiDef && primitiveCount > 1048575)
                throw new NotSupportedException("HiDef profile supports a maximum of 1048575 primitives per draw call.");

            if (primitiveCount <= 0)
                throw new ArgumentOutOfRangeException("primitiveCount");

            if (_strategy.VertexShader == null)
                throw new InvalidOperationException("Vertex shader must be set before calling DrawIndexedPrimitives.");
            if (_strategy.PixelShader == null)
                throw new InvalidOperationException("Pixel shader must be set before calling DrawIndexedPrimitives.");

            _strategy.DrawIndexedPrimitives(primitiveType, baseVertex, minVertexIndex, numVertices, startIndex, primitiveCount);
        }

        /// <summary>
        /// Draw geometry by indexing into the vertex buffer.
        /// </summary>
        /// <param name="primitiveType">The type of primitives in the index buffer.</param>
        /// <param name="baseVertex">Used to offset the vertex range indexed from the vertex buffer.</param>
        /// <param name="startIndex">The index within the index buffer to start drawing from.</param>
        /// <param name="primitiveCount">The number of primitives to render from the index buffer.</param>
        public void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount)
        {
            if (_strategy._vertexBuffers.Count == 0)
                throw new InvalidOperationException("Vertex buffer must be set before calling DrawIndexedPrimitives.");

            if (_strategy.Indices == null)
                throw new InvalidOperationException("Index buffer must be set before calling DrawIndexedPrimitives.");

            if (_deviceStrategy.GraphicsProfile == GraphicsProfile.Reach && primitiveCount > 65535)
                throw new NotSupportedException("Reach profile supports a maximum of 65535 primitives per draw call.");
            if (_deviceStrategy.GraphicsProfile == GraphicsProfile.HiDef && primitiveCount > 1048575)
                throw new NotSupportedException("HiDef profile supports a maximum of 1048575 primitives per draw call.");

            if (primitiveCount <= 0)
                throw new ArgumentOutOfRangeException("primitiveCount");

            if (_strategy.VertexShader == null)
                throw new InvalidOperationException("Vertex shader must be set before calling DrawIndexedPrimitives.");
            if (_strategy.PixelShader == null)
                throw new InvalidOperationException("Pixel shader must be set before calling DrawIndexedPrimitives.");


            _strategy.DrawIndexedPrimitives(primitiveType, baseVertex, startIndex, primitiveCount);
        }

        // internal DrawIndexedPrimitives without checks, used by SpriteBatcher.
        void IPlatformGraphicsContext.SB_DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int minVertexIndex, int numVertices, int startIndex, int primitiveCount)
        {
            _strategy.DrawIndexedPrimitives(primitiveType, baseVertex, minVertexIndex, numVertices, startIndex, primitiveCount);
        }

        /// <summary>
        /// Draw primitives of the specified type from the data in an array of vertices without indexing.
        /// </summary>
        /// <typeparam name="T">The type of the vertices.</typeparam>
        /// <param name="primitiveType">The type of primitives to draw with the vertices.</param>
        /// <param name="vertexData">An array of vertices to draw.</param>
        /// <param name="vertexOffset">The index in the array of the first vertex that should be rendered.</param>
        /// <param name="primitiveCount">The number of primitives to draw.</param>
        /// <remarks>The <see cref="VertexDeclaration"/> will be found by getting <see cref="IVertexType.VertexDeclaration"/>
        /// from an instance of <typeparamref name="T"/> and cached for subsequent calls.</remarks>
        public void DrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int primitiveCount) where T : struct, IVertexType
        {
            VertexDeclaration vertexDeclaration = VertexDeclarationCache<T>.VertexDeclaration;
            DrawUserPrimitives<T>(primitiveType, vertexData, vertexOffset, primitiveCount, vertexDeclaration);
        }

        /// <summary>
        /// Draw primitives of the specified type from the data in the given array of vertices without indexing.
        /// </summary>
        /// <typeparam name="T">The type of the vertices.</typeparam>
        /// <param name="primitiveType">The type of primitives to draw with the vertices.</param>
        /// <param name="vertexData">An array of vertices to draw.</param>
        /// <param name="vertexOffset">The index in the array of the first vertex that should be rendered.</param>
        /// <param name="primitiveCount">The number of primitives to draw.</param>
        /// <param name="vertexDeclaration">The layout of the vertices.</param>
        public void DrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
        {
            if (vertexData == null)
                throw new ArgumentNullException("vertexData");

            if (vertexData.Length == 0)
                throw new ArgumentOutOfRangeException("vertexData");

            if (vertexOffset < 0 || vertexOffset >= vertexData.Length)
                throw new ArgumentOutOfRangeException("vertexOffset");

            if (_deviceStrategy.GraphicsProfile == GraphicsProfile.Reach && primitiveCount > 65535)
                throw new NotSupportedException("Reach profile supports a maximum of 65535 primitives per draw call.");
            if (_deviceStrategy.GraphicsProfile == GraphicsProfile.HiDef && primitiveCount > 1048575)
                throw new NotSupportedException("HiDef profile supports a maximum of 1048575 primitives per draw call.");

            if (primitiveCount <= 0)
                throw new ArgumentOutOfRangeException("primitiveCount");

            int vertexCount = GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount);

            if (vertexOffset + vertexCount > vertexData.Length)
                throw new ArgumentOutOfRangeException("primitiveCount");

            if (vertexDeclaration == null)
                throw new ArgumentNullException("vertexDeclaration");

            if (_strategy.VertexShader == null)
                throw new InvalidOperationException("Vertex shader must be set before calling DrawUserPrimitives.");
            if (_strategy.PixelShader == null)
                throw new InvalidOperationException("Pixel shader must be set before calling DrawUserPrimitives.");


            _strategy.DrawUserPrimitives<T>(primitiveType, vertexData, vertexOffset, primitiveCount, vertexDeclaration, vertexCount);
        }

        /// <summary>
        /// Draw primitives of the specified type from the currently bound vertexbuffers without indexing.
        /// </summary>
        /// <param name="primitiveType">The type of primitives to draw.</param>
        /// <param name="vertexStart">Index of the vertex to start at.</param>
        /// <param name="primitiveCount">The number of primitives to draw.</param>
        public void DrawPrimitives(PrimitiveType primitiveType, int vertexStart, int primitiveCount)
        {
            if (_strategy._vertexBuffers.Count == 0)
                throw new InvalidOperationException("Vertex buffer must be set before calling DrawPrimitives.");

            if (_deviceStrategy.GraphicsProfile == GraphicsProfile.Reach && primitiveCount > 65535)
                throw new NotSupportedException("Reach profile supports a maximum of 65535 primitives per draw call.");
            if (_deviceStrategy.GraphicsProfile == GraphicsProfile.HiDef && primitiveCount > 1048575)
                throw new NotSupportedException("HiDef profile supports a maximum of 1048575 primitives per draw call.");

            if (primitiveCount <= 0)
                throw new ArgumentOutOfRangeException("primitiveCount");

            if (_strategy.VertexShader == null)
                throw new InvalidOperationException("Vertex shader must be set before calling DrawPrimitives.");
            if (_strategy.PixelShader == null)
                throw new InvalidOperationException("Pixel shader must be set before calling DrawPrimitives.");

            int vertexCount = GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount);


            _strategy.DrawPrimitives(primitiveType, vertexStart, primitiveCount, vertexCount);
        }

        /// <summary>
        /// Draw primitives of the specified type by indexing into the given array of vertices with 16-bit indices.
        /// </summary>
        /// <typeparam name="T">The type of the vertices.</typeparam>
        /// <param name="primitiveType">The type of primitives to draw with the vertices.</param>
        /// <param name="vertexData">An array of vertices to draw.</param>
        /// <param name="vertexOffset">The index in the array of the first vertex to draw.</param>
        /// <param name="indexOffset">The index in the array of indices of the first index to use</param>
        /// <param name="primitiveCount">The number of primitives to draw.</param>
        /// <param name="numVertices">The number of vertices to draw.</param>
        /// <param name="indexData">The index data.</param>
        /// <remarks>The <see cref="VertexDeclaration"/> will be found by getting <see cref="IVertexType.VertexDeclaration"/>
        /// from an instance of <typeparamref name="T"/> and cached for subsequent calls.</remarks>
        /// <remarks>All indices in the vertex buffer are interpreted relative to the specified <paramref name="vertexOffset"/>.
        /// For example a value of zero in the array of indices points to the vertex at index <paramref name="vertexOffset"/>
        /// in the array of vertices.</remarks>
        public void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount) where T : struct, IVertexType
        {
            VertexDeclaration vertexDeclaration = VertexDeclarationCache<T>.VertexDeclaration;
            DrawUserIndexedPrimitives<T>(primitiveType, vertexData, vertexOffset, numVertices, indexData, indexOffset, primitiveCount, vertexDeclaration);
        }

        /// <summary>
        /// Draw primitives of the specified type by indexing into the given array of vertices with 16-bit indices.
        /// </summary>
        /// <typeparam name="T">The type of the vertices.</typeparam>
        /// <param name="primitiveType">The type of primitives to draw with the vertices.</param>
        /// <param name="vertexData">An array of vertices to draw.</param>
        /// <param name="vertexOffset">The index in the array of the first vertex to draw.</param>
        /// <param name="indexOffset">The index in the array of indices of the first index to use</param>
        /// <param name="primitiveCount">The number of primitives to draw.</param>
        /// <param name="numVertices">The number of vertices to draw.</param>
        /// <param name="indexData">The index data.</param>
        /// <param name="vertexDeclaration">The layout of the vertices.</param>
        /// <remarks>All indices in the vertex buffer are interpreted relative to the specified <paramref name="vertexOffset"/>.
        /// For example a value of zero in the array of indices points to the vertex at index <paramref name="vertexOffset"/>
        /// in the array of vertices.</remarks>
        public void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
        {
            if (vertexData == null || vertexData.Length == 0)
                throw new ArgumentNullException("vertexData");

            if (vertexOffset < 0 || vertexOffset >= vertexData.Length)
                throw new ArgumentOutOfRangeException("vertexOffset");

            if (numVertices <= 0 || numVertices > vertexData.Length)
                throw new ArgumentOutOfRangeException("numVertices");

            if (vertexOffset + numVertices > vertexData.Length)
                throw new ArgumentOutOfRangeException("numVertices");

            if (indexData == null || indexData.Length == 0)
                throw new ArgumentNullException("indexData");

            if (indexOffset < 0 || indexOffset >= indexData.Length)
                throw new ArgumentOutOfRangeException("indexOffset");

            if (_deviceStrategy.GraphicsProfile == GraphicsProfile.Reach && primitiveCount > 65535)
                throw new NotSupportedException("Reach profile supports a maximum of 65535 primitives per draw call.");
            if (_deviceStrategy.GraphicsProfile == GraphicsProfile.HiDef && primitiveCount > 1048575)
                throw new NotSupportedException("HiDef profile supports a maximum of 1048575 primitives per draw call.");

            if (primitiveCount <= 0)
                throw new ArgumentOutOfRangeException("primitiveCount");

            if (indexOffset + GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount) > indexData.Length)
                throw new ArgumentOutOfRangeException("primitiveCount");

            if (vertexDeclaration == null)
                throw new ArgumentNullException("vertexDeclaration");

            if (vertexDeclaration.VertexStride < ReflectionHelpers.SizeOf<T>())
                throw new ArgumentOutOfRangeException("vertexDeclaration", "Vertex stride of vertexDeclaration should be at least as big as the stride of the actual vertices.");

            if (_strategy.VertexShader == null)
                throw new InvalidOperationException("Vertex shader must be set before calling DrawUserIndexedPrimitives.");
            if (_strategy.PixelShader == null)
                throw new InvalidOperationException("Pixel shader must be set before calling DrawUserIndexedPrimitives.");


            _strategy.DrawUserIndexedPrimitives<T>(primitiveType, vertexData, vertexOffset, numVertices, indexData, indexOffset, primitiveCount, vertexDeclaration);
        }

        /// <summary>
        /// Draw primitives of the specified type by indexing into the given array of vertices with 32-bit indices.
        /// </summary>
        /// <typeparam name="T">The type of the vertices.</typeparam>
        /// <param name="primitiveType">The type of primitives to draw with the vertices.</param>
        /// <param name="vertexData">An array of vertices to draw.</param>
        /// <param name="vertexOffset">The index in the array of the first vertex to draw.</param>
        /// <param name="indexOffset">The index in the array of indices of the first index to use</param>
        /// <param name="primitiveCount">The number of primitives to draw.</param>
        /// <param name="numVertices">The number of vertices to draw.</param>
        /// <param name="indexData">The index data.</param>
        /// <remarks>The <see cref="VertexDeclaration"/> will be found by getting <see cref="IVertexType.VertexDeclaration"/>
        /// from an instance of <typeparamref name="T"/> and cached for subsequent calls.</remarks>
        /// <remarks>All indices in the vertex buffer are interpreted relative to the specified <paramref name="vertexOffset"/>.
        /// For example a value of zero in the array of indices points to the vertex at index <paramref name="vertexOffset"/>
        /// in the array of vertices.</remarks>
        public void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, int[] indexData, int indexOffset, int primitiveCount) where T : struct, IVertexType
        {
            VertexDeclaration vertexDeclaration = VertexDeclarationCache<T>.VertexDeclaration;
            DrawUserIndexedPrimitives<T>(primitiveType, vertexData, vertexOffset, numVertices, indexData, indexOffset, primitiveCount, vertexDeclaration);
        }

        /// <summary>
        /// Draw primitives of the specified type by indexing into the given array of vertices with 32-bit indices.
        /// </summary>
        /// <typeparam name="T">The type of the vertices.</typeparam>
        /// <param name="primitiveType">The type of primitives to draw with the vertices.</param>
        /// <param name="vertexData">An array of vertices to draw.</param>
        /// <param name="vertexOffset">The index in the array of the first vertex to draw.</param>
        /// <param name="indexOffset">The index in the array of indices of the first index to use</param>
        /// <param name="primitiveCount">The number of primitives to draw.</param>
        /// <param name="numVertices">The number of vertices to draw.</param>
        /// <param name="indexData">The index data.</param>
        /// <param name="vertexDeclaration">The layout of the vertices.</param>
        /// <remarks>All indices in the vertex buffer are interpreted relative to the specified <paramref name="vertexOffset"/>.
        /// For example value of zero in the array of indices points to the vertex at index <paramref name="vertexOffset"/>
        /// in the array of vertices.</remarks>
        public void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, int[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
        {
            if (_deviceStrategy.GraphicsProfile == GraphicsProfile.Reach)
                throw new NotSupportedException("Reach profile does not support 32 bit indices");

            if (vertexData == null || vertexData.Length == 0)
                throw new ArgumentNullException("vertexData");

            if (vertexOffset < 0 || vertexOffset >= vertexData.Length)
                throw new ArgumentOutOfRangeException("vertexOffset");

            if (numVertices <= 0 || numVertices > vertexData.Length)
                throw new ArgumentOutOfRangeException("numVertices");

            if (vertexOffset + numVertices > vertexData.Length)
                throw new ArgumentOutOfRangeException("numVertices");

            if (indexData == null || indexData.Length == 0)
                throw new ArgumentNullException("indexData");

            if (indexOffset < 0 || indexOffset >= indexData.Length)
                throw new ArgumentOutOfRangeException("indexOffset");

            if (_deviceStrategy.GraphicsProfile == GraphicsProfile.Reach && primitiveCount > 65535)
                throw new NotSupportedException("Reach profile supports a maximum of 65535 primitives per draw call.");
            if (_deviceStrategy.GraphicsProfile == GraphicsProfile.HiDef && primitiveCount > 1048575)
                throw new NotSupportedException("HiDef profile supports a maximum of 1048575 primitives per draw call.");

            if (primitiveCount <= 0)
                throw new ArgumentOutOfRangeException("primitiveCount");

            if (indexOffset + GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount) > indexData.Length)
                throw new ArgumentOutOfRangeException("primitiveCount");

            if (vertexDeclaration == null)
                throw new ArgumentNullException("vertexDeclaration");

            if (vertexDeclaration.VertexStride < ReflectionHelpers.SizeOf<T>())
                throw new ArgumentOutOfRangeException("vertexDeclaration", "Vertex stride of vertexDeclaration should be at least as big as the stride of the actual vertices.");

            if (_strategy.VertexShader == null)
                throw new InvalidOperationException("Vertex shader must be set before calling DrawUserIndexedPrimitives.");
            if (_strategy.PixelShader == null)
                throw new InvalidOperationException("Pixel shader must be set before calling DrawUserIndexedPrimitives.");


            _strategy.DrawUserIndexedPrimitives<T>(primitiveType, vertexData, vertexOffset, numVertices, indexData, indexOffset, primitiveCount, vertexDeclaration);
        }

        /// <summary>
        /// Draw instanced geometry from the bound vertex buffers and index buffer.
        /// </summary>
        /// <param name="primitiveType">The type of primitives in the index buffer.</param>
        /// <param name="baseVertex">Used to offset the vertex range indexed from the vertex buffer.</param>
        /// <param name="minVertexIndex">This is unused and remains here only for XNA API compatibility.</param>
        /// <param name="numVertices">This is unused and remains here only for XNA API compatibility.</param>
        /// <param name="startIndex">The index within the index buffer to start drawing from.</param>
        /// <param name="primitiveCount">The number of primitives in a single instance.</param>
        /// <param name="instanceCount">The number of instances to render.</param>
        /// <remarks>Note that minVertexIndex and numVertices are unused in MonoGame and will be ignored.</remarks>
        public void DrawInstancedPrimitives(PrimitiveType primitiveType, int baseVertex, int minVertexIndex, int numVertices, int startIndex, int primitiveCount, int instanceCount)
        {
            DrawInstancedPrimitives(primitiveType, baseVertex, startIndex, primitiveCount, 0, instanceCount);
        }

        /// <summary>
        /// Draw instanced geometry from the bound vertex buffers and index buffer.
        /// </summary>
        /// <param name="primitiveType">The type of primitives in the index buffer.</param>
        /// <param name="baseVertex">Used to offset the vertex range indexed from the vertex buffer.</param>
        /// <param name="startIndex">The index within the index buffer to start drawing from.</param>
        /// <param name="primitiveCount">The number of primitives in a single instance.</param>
        /// <param name="instanceCount">The number of instances to render.</param>
        /// <remarks>Draw geometry with data from multiple bound vertex streams at different frequencies.</remarks>
        public void DrawInstancedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount, int instanceCount)
        {
            DrawInstancedPrimitives(primitiveType, baseVertex, startIndex, primitiveCount, 0, instanceCount);
        }

        /// <summary>
        /// Draw instanced geometry from the bound vertex buffers and index buffer.
        /// </summary>
        /// <param name="primitiveType">The type of primitives in the index buffer.</param>
        /// <param name="baseVertex">Used to offset the vertex range indexed from the vertex buffer.</param>
        /// <param name="startIndex">The index within the index buffer to start drawing from.</param>
        /// <param name="primitiveCount">The number of primitives in a single instance.</param>
        /// <param name="baseInstance">Used to offset the instance range indexed from the instance buffer.</param>
        /// <param name="instanceCount">The number of instances to render.</param>
        /// <remarks>Draw geometry with data from multiple bound vertex streams at different frequencies.</remarks>
        public void DrawInstancedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount, int baseInstance, int instanceCount)
        {
            if (_deviceStrategy.GraphicsProfile == GraphicsProfile.Reach)
                throw new NotSupportedException("Reach profile does not support Instancing.");

            if (_strategy._vertexBuffers.Count == 0)
                throw new InvalidOperationException("Vertex buffer must be set before calling DrawInstancedPrimitives.");

            if (_strategy.Indices == null)
                throw new InvalidOperationException("Index buffer must be set before calling DrawInstancedPrimitives.");

            if (primitiveCount <= 0)
                throw new ArgumentOutOfRangeException("primitiveCount");

            if (_strategy.VertexShader == null)
                throw new InvalidOperationException("Vertex shader must be set before calling DrawInstancedPrimitives.");
            if (_strategy.PixelShader == null)
                throw new InvalidOperationException("Pixel shader must be set before calling DrawInstancedPrimitives.");


            _strategy.DrawInstancedPrimitives(primitiveType, baseVertex, startIndex, primitiveCount, baseInstance, instanceCount);
        }

        /// <summary>
        /// Sends queued-up commands in the command buffer to the graphics processing unit (GPU).
        /// </summary>
        public void Flush()
        {
            _strategy.Flush();
        }


        #region IDisposable Members

        ~GraphicsContext()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            ThrowIfDisposed();
            {
                Dispose(true);
                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_strategy != null)
                    _strategy.Dispose();

                _strategy = null;
                _deviceStrategy = null;
            }
        }

        //[Conditional("DEBUG")]
        private void ThrowIfDisposed()
        {
            if (!_isDisposed)
                return;

            throw new ObjectDisposedException("Object is Disposed.");
        }

        #endregion IDisposable Members

    }
}
