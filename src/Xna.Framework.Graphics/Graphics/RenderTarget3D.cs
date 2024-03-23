// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;


namespace Microsoft.Xna.Framework.Graphics
{
    public class RenderTarget3D : Texture3D, IRenderTarget
    {
        private IRenderTarget3DStrategy _strategyRenderTarget3D;

        public DepthFormat DepthStencilFormat { get { return _strategyRenderTarget3D.DepthStencilFormat; } }
        
        public int MultiSampleCount { get { return _strategyRenderTarget3D.MultiSampleCount; } }
        
        public RenderTargetUsage RenderTargetUsage { get { return _strategyRenderTarget3D.RenderTargetUsage; } }

        IRenderTargetStrategy IRenderTarget.RenderTargetStrategy { get { return _strategyRenderTarget3D; } }

        public bool IsContentLost { get { return _strategyRenderTarget3D.IsContentLost; } }

        public event EventHandler<EventArgs> ContentLost;



        public RenderTarget3D(GraphicsDevice graphicsDevice, int width, int height, int depth, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
            :base(graphicsDevice, width, height, depth, mipMap, QuerySelectedFormat(graphicsDevice, preferredFormat), true)
        {
            SurfaceFormat format = QuerySelectedFormat(graphicsDevice, preferredFormat);
            _strategyRenderTarget3D = ((IPlatformGraphicsContext)graphicsDevice.MainContext).Strategy.CreateRenderTarget3DStrategy(width, height, depth, mipMap, usage,
                format, preferredDepthFormat, preferredMultiSampleCount);
            _strategyTexture3D = _strategyRenderTarget3D;
            _strategyTexture = _strategyTexture3D;
            SetResourceStrategy((IGraphicsResourceStrategy)_strategyTexture3D);
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

        public RenderTarget3D(GraphicsDevice graphicsDevice, int width, int height, int depth, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat)
            :this(graphicsDevice, width, height, depth, mipMap, preferredFormat, preferredDepthFormat, 0, RenderTargetUsage.DiscardContents) 
        {
        }
        
        public RenderTarget3D(GraphicsDevice graphicsDevice, int width, int height, int depth)
            : this(graphicsDevice, width, height, depth, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents) 
        {
        }
    }
}
