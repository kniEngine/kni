// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;


namespace Microsoft.Xna.Framework.Graphics
{
	public partial class RenderTarget2D : Texture2D, IRenderTarget
	{
        internal IRenderTarget2DStrategy _strategyRenderTarget2D;

		public DepthFormat DepthStencilFormat { get { return _strategyRenderTarget2D.DepthStencilFormat; } }
		
		public int MultiSampleCount { get { return _strategyRenderTarget2D.MultiSampleCount; } }
		
		public RenderTargetUsage RenderTargetUsage { get { return _strategyRenderTarget2D.RenderTargetUsage; } }

        public bool IsContentLost
        {
            get { throw new NotImplementedException("IsContentLost"); }
        }

        public event EventHandler<EventArgs> ContentLost;


        /// <summary>
        /// Allows child class to specify the surface type, eg: a swap chain.
        /// </summary>
      protected RenderTarget2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared, int arraySize, SurfaceType surfaceType)
            : base(graphicsDevice, width, height, mipMap, QuerySelectedFormat(graphicsDevice, preferredFormat), shared, arraySize, true)
        {
            if (surfaceType != SurfaceType.SwapChainRenderTarget)
                throw new InvalidOperationException();

            SurfaceFormat format = QuerySelectedFormat(graphicsDevice, preferredFormat);
            _strategyRenderTarget2D = graphicsDevice.Strategy.MainContext.Strategy.CreateRenderTarget2DStrategy(width, height, mipMap, arraySize, usage,
                preferredDepthFormat);
            _strategyTexture2D = graphicsDevice.Strategy.MainContext.Strategy.CreateTexture2DStrategy(width, height, mipMap, format, arraySize, shared);
            _strategyTexture = _strategyTexture2D;
            SetResourceStrategy((IGraphicsResourceStrategy)_strategyTexture2D);
            SetGraphicsDevice(graphicsDevice);

            // Texture will be assigned by the swap chain.
            //PlatformConstructTexture2D(graphicsDevice.Strategy.MainContext.Strategy, width, height, mipMap, format, shared);

        }

        public RenderTarget2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared, int arraySize)
	        : base(graphicsDevice, width, height, mipMap, QuerySelectedFormat(graphicsDevice, preferredFormat), shared, arraySize, true)
	    {
            SurfaceFormat format = QuerySelectedFormat(graphicsDevice, preferredFormat);
            _strategyRenderTarget2D = graphicsDevice.Strategy.MainContext.Strategy.CreateRenderTarget2DStrategy(width, height, mipMap, arraySize, usage,
                preferredDepthFormat);
            _strategyTexture2D = graphicsDevice.Strategy.MainContext.Strategy.CreateTexture2DStrategy(width, height, mipMap, format, arraySize, shared);
            _strategyTexture = _strategyTexture2D;
            SetResourceStrategy((IGraphicsResourceStrategy)_strategyTexture2D);
            SetGraphicsDevice(graphicsDevice);

            PlatformConstructTexture2D(graphicsDevice.Strategy.MainContext.Strategy, width, height, mipMap, format, shared);
            PlatformConstructRenderTarget2D(graphicsDevice.Strategy.MainContext.Strategy, width, height, mipMap, preferredDepthFormat, preferredMultiSampleCount, shared);
	    }
        
        protected static SurfaceFormat QuerySelectedFormat(GraphicsDevice graphicsDevice, SurfaceFormat preferredFormat)
        {
			SurfaceFormat selectedFormat = preferredFormat;
			DepthFormat selectedDepthFormat;
			int selectedMultiSampleCount;

            if (graphicsDevice != null)
            {
                graphicsDevice.Adapter.QueryRenderTargetFormat(graphicsDevice.Strategy.GraphicsProfile, preferredFormat, DepthFormat.None, 0,
                    out selectedFormat, out selectedDepthFormat, out selectedMultiSampleCount);
            }
            
            return selectedFormat;
        }

        public RenderTarget2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared)
			: this(graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, preferredMultiSampleCount, usage, shared, 1)
        {
        }

		public RenderTarget2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
			:this(graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, preferredMultiSampleCount, usage, false, 1)
        {
        }

		public RenderTarget2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat)
			:this(graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, 0, RenderTargetUsage.DiscardContents, false, 1) 
		{
        }
		
		public RenderTarget2D(GraphicsDevice graphicsDevice, int width, int height)
			: this(graphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents, false, 1) 
		{
        }

        protected internal override void GraphicsDeviceResetting()
        {
            PlatformGraphicsDeviceResetting();
            base.GraphicsDeviceResetting();
        }        
	}
}
