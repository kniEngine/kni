
// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;


namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// A swap chain used for rendering to a secondary GameWindow.
    /// </summary>
    /// <remarks>
    /// This is an extension and not part of stock XNA.
    /// It is currently implemented for Windows and DirectX only.
    /// </remarks>
    public class SwapChainRenderTarget : RenderTarget2D
    {
        internal ConcreteRenderTargetSwapChain _strategyRenderTargetSwapChain;
        public readonly PresentInterval PresentInterval;

        public SwapChainRenderTarget(GraphicsDevice graphicsDevice, IntPtr windowHandle, int width, int height)
            : this(graphicsDevice, windowHandle, width, height,
                   false, SurfaceFormat.Bgra32, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents,
                   PresentInterval.Default)
        {
        }

        public SwapChainRenderTarget(GraphicsDevice graphicsDevice,
                                     IntPtr windowHandle,
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
                windowHandle, presentInterval,
                format, preferredDepthFormat, preferredMultiSampleCount);
            _strategyRenderTarget2D = _strategyRenderTargetSwapChain;
            _strategyTexture2D = _strategyRenderTarget2D;
            _strategyTexture = _strategyTexture2D;
            SetResourceStrategy((IGraphicsResourceStrategy)_strategyTexture2D);

            this.PresentInterval = presentInterval;
        }

        // TODO: We need to expose the other Present() overloads
        // for passing source/dest rectangles.

        /// <summary>
        /// Displays the contents of the active back buffer to the screen.
        /// </summary>
        public void Present()
        {
            _strategyRenderTargetSwapChain.Present();
        }


    }
}
