﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;


namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Represents a texture cube that can be used as a render target.
    /// </summary>
    public class RenderTargetCube : TextureCube, IRenderTarget
    {
        private IRenderTargetCubeStrategy _strategyRenderTargetCube;

        /// <summary>
        /// Gets the depth-stencil buffer format of this render target.
        /// </summary>
        /// <value>The format of the depth-stencil buffer.</value>
        public DepthFormat DepthStencilFormat { get { return _strategyRenderTargetCube.DepthStencilFormat; } }

        /// <summary>
        /// Gets the number of multisample locations.
        /// </summary>
        /// <value>The number of multisample locations.</value>
        public int MultiSampleCount { get { return _strategyRenderTargetCube.MultiSampleCount; } }

        /// <summary>
        /// Gets the usage mode of this render target.
        /// </summary>
        /// <value>The usage mode of the render target.</value>
        public RenderTargetUsage RenderTargetUsage { get { return _strategyRenderTargetCube.RenderTargetUsage; } }

        IRenderTargetStrategy IRenderTarget.RenderTargetStrategy { get { return _strategyRenderTargetCube; } }

        /// <inheritdoc/>
        int IRenderTarget.Width { get { return this.Size; } }

        /// <inheritdoc/>
        int IRenderTarget.Height { get { return this.Size; } }

        public bool IsContentLost { get { return _strategyRenderTargetCube.IsContentLost; } }

        public event EventHandler<EventArgs> ContentLost;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RenderTargetCube"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="size">The width and height of a texture cube face in pixels.</param>
        /// <param name="mipMap"><see langword="true"/> to generate a full mipMap chain; otherwise <see langword="false"/>.</param>
        /// <param name="preferredFormat">The preferred format of the surface.</param>
        /// <param name="preferredDepthFormat">The preferred format of the depth-stencil buffer.</param>
        public RenderTargetCube(GraphicsDevice graphicsDevice, int size, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat)
            : this(graphicsDevice, size, mipMap, preferredFormat, preferredDepthFormat, 0, RenderTargetUsage.DiscardContents)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderTargetCube"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="size">The width and height of a texture cube face in pixels.</param>
        /// <param name="mipMap"><see langword="true"/> to generate a full mipMap chain; otherwise <see langword="false"/>.</param>
        /// <param name="preferredFormat">The preferred format of the surface.</param>
        /// <param name="preferredDepthFormat">The preferred format of the depth-stencil buffer.</param>
        /// <param name="preferredMultiSampleCount">The preferred number of multisample locations.</param>
        /// <param name="usage">The usage mode of the render target.</param>
        public RenderTargetCube(GraphicsDevice graphicsDevice, int size, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
            : base(graphicsDevice, size, mipMap, QuerySelectedFormat(graphicsDevice, preferredFormat), true)
        {
            SurfaceFormat format = QuerySelectedFormat(graphicsDevice, preferredFormat);
            _strategyRenderTargetCube = ((IPlatformGraphicsContext)graphicsDevice.CurrentContext).Strategy.CreateRenderTargetCubeStrategy(size, mipMap, usage,
                format, preferredDepthFormat, preferredMultiSampleCount);
            _strategyTextureCube = _strategyRenderTargetCube;
            _strategyTexture = _strategyTextureCube;
            SetResourceStrategy((IGraphicsResourceStrategy)_strategyTextureCube);
       }
        
        protected static SurfaceFormat QuerySelectedFormat(GraphicsDevice graphicsDevice, SurfaceFormat preferredFormat)
        {
            SurfaceFormat selectedFormat = preferredFormat;
            DepthFormat selectedDepthFormat;
            int selectedMultiSampleCount;

            if (graphicsDevice != null)
            {
                graphicsDevice.Adapter.QueryRenderTargetFormat(((IPlatformGraphicsDevice)graphicsDevice).Strategy.GraphicsProfile, preferredFormat, DepthFormat.None, 0, 
                    out selectedFormat, out selectedDepthFormat, out selectedMultiSampleCount);
            }
            
            return selectedFormat;
        }
    }
}
