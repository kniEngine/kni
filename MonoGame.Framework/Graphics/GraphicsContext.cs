// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Platform.Graphics;
using MonoGame.Framework.Utilities;


namespace Microsoft.Xna.Framework.Graphics
{
    public sealed class GraphicsContext : IDisposable
    {
        private GraphicsContextStrategy _strategy;
        private GraphicsDeviceStrategy _deviceStrategy;
        private bool _isDisposed = false;


        internal GraphicsMetrics _graphicsMetrics;

        internal GraphicsContextStrategy Strategy { get { return _strategy; } }
        internal GraphicsDeviceStrategy DeviceStrategy { get { return _deviceStrategy; } }


        internal GraphicsContext(GraphicsDevice device) : this(device.Strategy)
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
            get { return _graphicsMetrics; }
            set { _graphicsMetrics = value; }
        }

        /// <summary>
        /// Get or set the color a <see cref="RenderTarget2D"/> is cleared to when it is set.
        /// </summary>
        public Color DiscardColor
        {
            get { return Strategy.DiscardColor; }
            set { Strategy.DiscardColor = value; }
        }

        /// <summary>
        /// Access debugging APIs for the graphics subsystem.
        /// </summary>
        public GraphicsDebug GraphicsDebug
        {
            get { return Strategy.GraphicsDebug; }
            set { Strategy.GraphicsDebug = value; }
        }

        public Rectangle ScissorRectangle
        {
            get { return Strategy.ScissorRectangle; }
            set { Strategy.ScissorRectangle = value; }
        }

        public Viewport Viewport
        {
            get { return Strategy.Viewport; }
            set { Strategy.Viewport = value; }
        }

        public BlendState BlendState
        {
            get { return Strategy.BlendState; }
            set { Strategy.BlendState = value; }
        }

        public Color BlendFactor
        {
            get { return Strategy.BlendFactor; }
            set { Strategy.BlendFactor = value; }
        }

        public DepthStencilState DepthStencilState
        {
            get { return Strategy.DepthStencilState; }
            set { Strategy.DepthStencilState = value; }
        }

        public RasterizerState RasterizerState
        {
            get { return Strategy.RasterizerState; }
            set { Strategy.RasterizerState = value; }
        }

        public SamplerStateCollection SamplerStates
        {
            get { return Strategy.SamplerStates; }
        }

        public SamplerStateCollection VertexSamplerStates
        {
            get { return Strategy.VertexSamplerStates; }
        }

        public TextureCollection Textures
        {
            get { return Strategy.Textures; }
        }

        public TextureCollection VertexTextures
        {
            get { return Strategy.VertexTextures; }
        }

        public IndexBuffer Indices
        {
            get { return Strategy.Indices; }
            set { Strategy.Indices = value; }
        }


        public int RenderTargetCount { get { return Strategy.RenderTargetCount; } }
        internal bool IsRenderTargetBound { get { return Strategy.IsRenderTargetBound; } }

        public void Clear(Color color)
        {
            ClearOptions options = ClearOptions.Target
                                 | ClearOptions.DepthBuffer
                                 | ClearOptions.Stencil;
            Strategy.Clear(options, color.ToVector4(), Strategy.Viewport.MaxDepth, 0);

            unchecked { _graphicsMetrics._clearCount++; }
        }

        public void Clear(ClearOptions options, Color color, float depth, int stencil)
        {
            Strategy.Clear(options, color.ToVector4(), depth, stencil);

            unchecked { _graphicsMetrics._clearCount++; }
        }

        public void Clear(ClearOptions options, Vector4 color, float depth, int stencil)
        {
            Strategy.Clear(options, color, depth, stencil);

            unchecked { _graphicsMetrics._clearCount++; }
        }

        public void SetVertexBuffer(VertexBuffer vertexBuffer)
        {
            Strategy.SetVertexBuffer(vertexBuffer);
        }

        public void SetVertexBuffer(VertexBuffer vertexBuffer, int vertexOffset)
        {
            Strategy.SetVertexBuffer(vertexBuffer, vertexOffset);
        }

        public void SetVertexBuffers(VertexBufferBinding[] vertexBuffers)
        {
            Strategy.SetVertexBuffers(vertexBuffers);
        }

        internal VertexShader VertexShader
        {
            get { return Strategy.VertexShader; }
            set { Strategy.VertexShader = value; }
        }

        internal PixelShader PixelShader
        {
            get { return Strategy.PixelShader; }
            set { Strategy.PixelShader = value; }
        }

        public void GetRenderTargets(RenderTargetBinding[] bindings)
        {
            Strategy.GetRenderTargets(bindings);
        }

        public void SetRenderTarget(RenderTarget2D renderTarget)
        {
            if (renderTarget == null)
            {
                SetRenderTargets(null);
            }
            else
            {
                Strategy._singleRenderTargetBinding[0] = new RenderTargetBinding(renderTarget);
                SetRenderTargets(Strategy._singleRenderTargetBinding);
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
                Strategy._singleRenderTargetBinding[0] = new RenderTargetBinding(renderTarget, cubeMapFace);
                SetRenderTargets(Strategy._singleRenderTargetBinding);
            }
        }

        /// <remarks>Only implemented for DirectX </remarks>
        public void SetRenderTarget(RenderTarget2D renderTarget, int arraySlice)
        {
            if (!this.DeviceStrategy.Capabilities.SupportsTextureArrays)
                throw new InvalidOperationException("Texture arrays are not supported on this graphics device");

            if (renderTarget == null)
            {
                SetRenderTargets(null);
            }
            else
            {
                Strategy._singleRenderTargetBinding[0] = new RenderTargetBinding(renderTarget, arraySlice);
                SetRenderTargets(Strategy._singleRenderTargetBinding);
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
                Strategy._singleRenderTargetBinding[0] = new RenderTargetBinding(renderTarget, arraySlice);
                SetRenderTargets(Strategy._singleRenderTargetBinding);
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

            if (this.DeviceStrategy.GraphicsProfile == GraphicsProfile.Reach && renderTargetCount > 1)
                throw new NotSupportedException("Reach profile supports a maximum of 1 simultaneous rendertargets");
            if (this.DeviceStrategy.GraphicsProfile == GraphicsProfile.HiDef && renderTargetCount > 4)
                throw new NotSupportedException("HiDef profile supports a maximum of 4 simultaneous rendertargets");
            if (renderTargetCount > 8)
                throw new NotSupportedException("Current profile supports a maximum of 8 simultaneous rendertargets");

            // Try to early out if the current and new bindings are equal.
            if (Strategy._currentRenderTargetCount == renderTargetCount)
            {
                bool isEqual = true;
                for (int i = 0; i < Strategy._currentRenderTargetCount; i++)
                {
                    if (Strategy._currentRenderTargetBindings[i].RenderTarget != renderTargets[i].RenderTarget ||
                        Strategy._currentRenderTargetBindings[i].ArraySlice != renderTargets[i].ArraySlice)
                    {
                        isEqual = false;
                        break;
                    }
                }

                if (isEqual)
                    return;
            }

            ApplyRenderTargets(renderTargets);

            if (renderTargetCount == 0)
                unchecked { _graphicsMetrics._targetCount++; }
            else
                unchecked { _graphicsMetrics._targetCount += renderTargetCount; }
        }

        internal void ApplyRenderTargets(RenderTargetBinding[] renderTargets)
        {
            bool clearTarget = false;

            Strategy.ToConcrete<ConcreteGraphicsContext>().PlatformResolveRenderTargets();

            // Clear the current bindings.
            Array.Clear(Strategy._currentRenderTargetBindings, 0, Strategy._currentRenderTargetBindings.Length);

            int renderTargetWidth;
            int renderTargetHeight;
            if (renderTargets == null)
            {
                Strategy._currentRenderTargetCount = 0;

                Strategy.ToConcrete<ConcreteGraphicsContext>().PlatformApplyDefaultRenderTarget();
                clearTarget = this.DeviceStrategy.PresentationParameters.RenderTargetUsage == RenderTargetUsage.DiscardContents;

                renderTargetWidth = this.DeviceStrategy.PresentationParameters.BackBufferWidth;
                renderTargetHeight = this.DeviceStrategy.PresentationParameters.BackBufferHeight;
            }
            else
            {
                // Copy the new bindings.
                Array.Copy(renderTargets, Strategy._currentRenderTargetBindings, renderTargets.Length);
                Strategy._currentRenderTargetCount = renderTargets.Length;

                IRenderTarget renderTarget = Strategy.ToConcrete<ConcreteGraphicsContext>().PlatformApplyRenderTargets();

                // We clear the render target if asked.
                clearTarget = renderTarget.RenderTargetUsage == RenderTargetUsage.DiscardContents;

                renderTargetWidth = renderTarget.Width;
                renderTargetHeight = renderTarget.Height;
            }

            // Set the viewport to the size of the first render target.
            Strategy.Viewport = new Viewport(0, 0, renderTargetWidth, renderTargetHeight);

            // Set the scissor rectangle to the size of the first render target.
            Strategy.ScissorRectangle = new Rectangle(0, 0, renderTargetWidth, renderTargetHeight);

            if (clearTarget)
                Clear(Strategy.DiscardColor);
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
            DrawIndexedPrimitives(primitiveType, baseVertex, startIndex, primitiveCount);
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
            if (Strategy._vertexBuffers.Count == 0)
                throw new InvalidOperationException("Vertex buffer must be set before calling DrawIndexedPrimitives.");

            if (Strategy.Indices == null)
                throw new InvalidOperationException("Index buffer must be set before calling DrawIndexedPrimitives.");

            if (this.DeviceStrategy.GraphicsProfile == GraphicsProfile.Reach && primitiveCount > 65535)
                throw new NotSupportedException("Reach profile supports a maximum of 65535 primitives per draw call.");
            if (this.DeviceStrategy.GraphicsProfile == GraphicsProfile.HiDef && primitiveCount > 1048575)
                throw new NotSupportedException("HiDef profile supports a maximum of 1048575 primitives per draw call.");
            if (this.DeviceStrategy.GraphicsProfile == GraphicsProfile.Reach)
            {
                for (int i = 0; i < this.DeviceStrategy.Capabilities.MaxTextureSlots; i++)
                {
                    var tx2D = Strategy.Textures[i] as Texture2D;
                    if (tx2D != null)
                    {
                        if (Strategy.SamplerStates[i].AddressU != TextureAddressMode.Clamp && !MathHelper.IsPowerOfTwo(tx2D.Width)
                        ||  Strategy.SamplerStates[i].AddressV != TextureAddressMode.Clamp && !MathHelper.IsPowerOfTwo(tx2D.Height))
                            throw new NotSupportedException("Reach profile support only Clamp mode for non-power of two Textures.");
                    }
                }
            }

            if (primitiveCount <= 0)
                throw new ArgumentOutOfRangeException("primitiveCount");

            if (Strategy.VertexShader == null)
                throw new InvalidOperationException("Vertex shader must be set before calling DrawIndexedPrimitives.");
            if (Strategy.PixelShader == null)
                throw new InvalidOperationException("Pixel shader must be set before calling DrawIndexedPrimitives.");


            Strategy.DrawIndexedPrimitives(primitiveType, baseVertex, startIndex, primitiveCount);

            unchecked { _graphicsMetrics._drawCount++; }
            unchecked { _graphicsMetrics._primitiveCount += primitiveCount; }
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
            DrawUserPrimitives<T>(primitiveType, vertexData, vertexOffset, primitiveCount, VertexDeclarationCache<T>.VertexDeclaration);
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

            if (this.DeviceStrategy.GraphicsProfile == GraphicsProfile.Reach && primitiveCount > 65535)
                throw new NotSupportedException("Reach profile supports a maximum of 65535 primitives per draw call.");
            if (this.DeviceStrategy.GraphicsProfile == GraphicsProfile.HiDef && primitiveCount > 1048575)
                throw new NotSupportedException("HiDef profile supports a maximum of 1048575 primitives per draw call.");
            if (this.DeviceStrategy.GraphicsProfile == GraphicsProfile.Reach)
            {
                for (int i = 0; i < this.DeviceStrategy.Capabilities.MaxTextureSlots; i++)
                {
                    var tx2D = Strategy.Textures[i] as Texture2D;
                    if (tx2D != null)
                    {
                        if (Strategy.SamplerStates[i].AddressU != TextureAddressMode.Clamp && !MathHelper.IsPowerOfTwo(tx2D.Width)
                        ||  Strategy.SamplerStates[i].AddressV != TextureAddressMode.Clamp && !MathHelper.IsPowerOfTwo(tx2D.Height))
                            throw new NotSupportedException("Reach profile support only Clamp mode for non-power of two Textures.");
                    }
                }
            }

            if (primitiveCount <= 0)
                throw new ArgumentOutOfRangeException("primitiveCount");

            var vertexCount = GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount);

            if (vertexOffset + vertexCount > vertexData.Length)
                throw new ArgumentOutOfRangeException("primitiveCount");

            if (vertexDeclaration == null)
                throw new ArgumentNullException("vertexDeclaration");

            if (Strategy.VertexShader == null)
                throw new InvalidOperationException("Vertex shader must be set before calling DrawUserPrimitives.");
            if (Strategy.PixelShader == null)
                throw new InvalidOperationException("Pixel shader must be set before calling DrawUserPrimitives.");


            Strategy.DrawUserPrimitives<T>(primitiveType, vertexData, vertexOffset, vertexDeclaration, vertexCount);

            unchecked { _graphicsMetrics._drawCount++; }
            unchecked { _graphicsMetrics._primitiveCount += primitiveCount; }
        }

        /// <summary>
        /// Draw primitives of the specified type from the currently bound vertexbuffers without indexing.
        /// </summary>
        /// <param name="primitiveType">The type of primitives to draw.</param>
        /// <param name="vertexStart">Index of the vertex to start at.</param>
        /// <param name="primitiveCount">The number of primitives to draw.</param>
        public void DrawPrimitives(PrimitiveType primitiveType, int vertexStart, int primitiveCount)
        {
            if (Strategy._vertexBuffers.Count == 0)
                throw new InvalidOperationException("Vertex buffer must be set before calling DrawPrimitives.");

            if (this.DeviceStrategy.GraphicsProfile == GraphicsProfile.Reach && primitiveCount > 65535)
                throw new NotSupportedException("Reach profile supports a maximum of 65535 primitives per draw call.");
            if (this.DeviceStrategy.GraphicsProfile == GraphicsProfile.HiDef && primitiveCount > 1048575)
                throw new NotSupportedException("HiDef profile supports a maximum of 1048575 primitives per draw call.");
            if (this.DeviceStrategy.GraphicsProfile == GraphicsProfile.Reach)
            {
                for (int i = 0; i < this.DeviceStrategy.Capabilities.MaxTextureSlots; i++)
                {
                    var tx2D = Strategy.Textures[i] as Texture2D;
                    if (tx2D != null)
                    {
                        if (Strategy.SamplerStates[i].AddressU != TextureAddressMode.Clamp && !MathHelper.IsPowerOfTwo(tx2D.Width)
                        ||  Strategy.SamplerStates[i].AddressV != TextureAddressMode.Clamp && !MathHelper.IsPowerOfTwo(tx2D.Height))
                            throw new NotSupportedException("Reach profile support only Clamp mode for non-power of two Textures.");
                    }
                }
            }

            if (primitiveCount <= 0)
                throw new ArgumentOutOfRangeException("primitiveCount");

            if (Strategy.VertexShader == null)
                throw new InvalidOperationException("Vertex shader must be set before calling DrawPrimitives.");
            if (Strategy.PixelShader == null)
                throw new InvalidOperationException("Pixel shader must be set before calling DrawPrimitives.");

            var vertexCount = GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount);


            Strategy.DrawPrimitives(primitiveType, vertexStart, vertexCount);

            unchecked { _graphicsMetrics._drawCount++; }
            unchecked { _graphicsMetrics._primitiveCount += primitiveCount; }
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
            DrawUserIndexedPrimitives<T>(primitiveType, vertexData, vertexOffset, numVertices, indexData, indexOffset, primitiveCount, VertexDeclarationCache<T>.VertexDeclaration);
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

            if (this.DeviceStrategy.GraphicsProfile == GraphicsProfile.Reach && primitiveCount > 65535)
                throw new NotSupportedException("Reach profile supports a maximum of 65535 primitives per draw call.");
            if (this.DeviceStrategy.GraphicsProfile == GraphicsProfile.HiDef && primitiveCount > 1048575)
                throw new NotSupportedException("HiDef profile supports a maximum of 1048575 primitives per draw call.");
            if (this.DeviceStrategy.GraphicsProfile == GraphicsProfile.Reach)
            {
                for (int i = 0; i < this.DeviceStrategy.Capabilities.MaxTextureSlots; i++)
                {
                    var tx2D = Strategy.Textures[i] as Texture2D;
                    if (tx2D != null)
                    {
                        if (Strategy.SamplerStates[i].AddressU != TextureAddressMode.Clamp && !MathHelper.IsPowerOfTwo(tx2D.Width)
                        ||  Strategy.SamplerStates[i].AddressV != TextureAddressMode.Clamp && !MathHelper.IsPowerOfTwo(tx2D.Height))
                            throw new NotSupportedException("Reach profile support only Clamp mode for non-power of two Textures.");
                    }
                }
            }

            if (primitiveCount <= 0)
                throw new ArgumentOutOfRangeException("primitiveCount");

            if (indexOffset + GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount) > indexData.Length)
                throw new ArgumentOutOfRangeException("primitiveCount");

            if (vertexDeclaration == null)
                throw new ArgumentNullException("vertexDeclaration");

            if (vertexDeclaration.VertexStride < ReflectionHelpers.SizeOf<T>())
                throw new ArgumentOutOfRangeException("vertexDeclaration", "Vertex stride of vertexDeclaration should be at least as big as the stride of the actual vertices.");

            if (Strategy.VertexShader == null)
                throw new InvalidOperationException("Vertex shader must be set before calling DrawUserIndexedPrimitives.");
            if (Strategy.PixelShader == null)
                throw new InvalidOperationException("Pixel shader must be set before calling DrawUserIndexedPrimitives.");


            Strategy.DrawUserIndexedPrimitives<T>(primitiveType, vertexData, vertexOffset, numVertices, indexData, indexOffset, primitiveCount, vertexDeclaration);

            unchecked { _graphicsMetrics._drawCount++; }
            unchecked { _graphicsMetrics._primitiveCount += primitiveCount; }
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
            DrawUserIndexedPrimitives<T>(primitiveType, vertexData, vertexOffset, numVertices, indexData, indexOffset, primitiveCount, VertexDeclarationCache<T>.VertexDeclaration);
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
            if (this.DeviceStrategy.GraphicsProfile == GraphicsProfile.Reach)
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

            if (this.DeviceStrategy.GraphicsProfile == GraphicsProfile.Reach && primitiveCount > 65535)
                throw new NotSupportedException("Reach profile supports a maximum of 65535 primitives per draw call.");
            if (this.DeviceStrategy.GraphicsProfile == GraphicsProfile.HiDef && primitiveCount > 1048575)
                throw new NotSupportedException("HiDef profile supports a maximum of 1048575 primitives per draw call.");
            if (this.DeviceStrategy.GraphicsProfile == GraphicsProfile.Reach)
            {
                for (int i = 0; i < this.DeviceStrategy.Capabilities.MaxTextureSlots; i++)
                {
                    var tx2D = Strategy.Textures[i] as Texture2D;
                    if (tx2D != null)
                    {
                        if (Strategy.SamplerStates[i].AddressU != TextureAddressMode.Clamp && !MathHelper.IsPowerOfTwo(tx2D.Width)
                        ||  Strategy.SamplerStates[i].AddressV != TextureAddressMode.Clamp && !MathHelper.IsPowerOfTwo(tx2D.Height))
                            throw new NotSupportedException("Reach profile support only Clamp mode for non-power of two Textures.");
                    }
                }
            }

            if (primitiveCount <= 0)
                throw new ArgumentOutOfRangeException("primitiveCount");

            if (indexOffset + GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount) > indexData.Length)
                throw new ArgumentOutOfRangeException("primitiveCount");

            if (vertexDeclaration == null)
                throw new ArgumentNullException("vertexDeclaration");

            if (vertexDeclaration.VertexStride < ReflectionHelpers.SizeOf<T>())
                throw new ArgumentOutOfRangeException("vertexDeclaration", "Vertex stride of vertexDeclaration should be at least as big as the stride of the actual vertices.");

            if (Strategy.VertexShader == null)
                throw new InvalidOperationException("Vertex shader must be set before calling DrawUserIndexedPrimitives.");
            if (Strategy.PixelShader == null)
                throw new InvalidOperationException("Pixel shader must be set before calling DrawUserIndexedPrimitives.");


            Strategy.DrawUserIndexedPrimitives<T>(primitiveType, vertexData, vertexOffset, numVertices, indexData, indexOffset, primitiveCount, vertexDeclaration);

            unchecked { _graphicsMetrics._drawCount++; }
            unchecked { _graphicsMetrics._primitiveCount += primitiveCount; }
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
            if (this.DeviceStrategy.GraphicsProfile == GraphicsProfile.Reach)
                throw new NotSupportedException("Reach profile does not support Instancing.");

            if (Strategy._vertexBuffers.Count == 0)
                throw new InvalidOperationException("Vertex buffer must be set before calling DrawInstancedPrimitives.");

            if (Strategy.Indices == null)
                throw new InvalidOperationException("Index buffer must be set before calling DrawInstancedPrimitives.");

            if (primitiveCount <= 0)
                throw new ArgumentOutOfRangeException("primitiveCount");

            if (Strategy.VertexShader == null)
                throw new InvalidOperationException("Vertex shader must be set before calling DrawInstancedPrimitives.");
            if (Strategy.PixelShader == null)
                throw new InvalidOperationException("Pixel shader must be set before calling DrawInstancedPrimitives.");


            Strategy.DrawInstancedPrimitives(primitiveType, baseVertex, startIndex, primitiveCount, baseInstance, instanceCount);

            unchecked { _graphicsMetrics._drawCount++; }
            unchecked { _graphicsMetrics._primitiveCount += (primitiveCount * instanceCount); }
        }

        /// <summary>
        /// Sends queued-up commands in the command buffer to the graphics processing unit (GPU).
        /// </summary>
        public void Flush()
        {
            Strategy.Flush();
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
