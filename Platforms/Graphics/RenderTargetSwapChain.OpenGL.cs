// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;


namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// A RenderTarget used for rendering to a swap chain.
    /// </summary>
    /// <remarks>
    /// This is an extension and not part of stock XNA.
    /// </remarks>
    internal class RenderTargetSwapChain : RenderTarget2D
    {
        internal ConcreteRenderTargetSwapChain _strategyRenderTargetSwapChain;
        public readonly PresentInterval PresentInterval;

        public RenderTargetSwapChain(GraphicsDevice graphicsDevice,
                                     IntPtr glTextureHandle,
                                     int width, int height,
                                     bool mipMap,
                                     SurfaceFormat preferredFormat,
                                     DepthFormat preferredDepthFormat,
                                     int preferredMultiSampleCount,
                                     RenderTargetUsage usage,
                                     PresentInterval presentInterval)
            : base(graphicsDevice, width, height, mipMap, QuerySelectedFormat(graphicsDevice, preferredFormat), false, 1, true)
        {

            SurfaceFormat format = QuerySelectedFormat(graphicsDevice, preferredFormat);
            _strategyRenderTargetSwapChain = new ConcreteRenderTargetSwapChain(((IPlatformGraphicsContext)((IPlatformGraphicsDevice)graphicsDevice).Strategy.MainContext).Strategy, width, height, mipMap, usage,
                glTextureHandle, presentInterval,
                format, preferredDepthFormat, preferredMultiSampleCount);
            _strategyRenderTarget2D = _strategyRenderTargetSwapChain;
            _strategyTexture2D = _strategyRenderTarget2D;
            _strategyTexture = _strategyTexture2D;
            SetResourceStrategy((IGraphicsResourceStrategy)_strategyTexture2D);

            this.PresentInterval = presentInterval;
        }

        /// <summary>
        /// Displays the contents of the active back buffer to the screen.
        /// </summary>
        public void Present()
        {
            _strategyRenderTargetSwapChain.Present();
        }


    }
}
