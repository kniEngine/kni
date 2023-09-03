
// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;


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
        private IntPtr _windowHandle;

        private DXGI.SwapChain _swapChain;
        private D3D11.Texture2D _backBuffer;
        //private D3D11.Texture2D _depthBuffer;

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
                                     SurfaceFormat surfaceFormat,
                                     DepthFormat depthFormat,
                                     int preferredMultiSampleCount,
                                     RenderTargetUsage usage,
                                     PresentInterval presentInterval)
            : base(graphicsDevice, width, height, mipMap, surfaceFormat, depthFormat, preferredMultiSampleCount, usage,
                   SurfaceType.SwapChainRenderTarget)
        {
            this._windowHandle = windowHandle;
            this.PresentInterval = presentInterval;

            DXGI.Format dxgiFormat = (surfaceFormat == SurfaceFormat.Color)
                                   ? DXGI.Format.B8G8R8A8_UNorm
                                   : GraphicsExtensions.ToDXFormat(surfaceFormat);

            D3D11.Device d3dDevice = GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice;

            DXGI.SampleDescription multisampleDesc = GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().GetSupportedSampleDescription(dxgiFormat, MultiSampleCount);

            base._renderTargetViews = new D3D11.RenderTargetView[1];
            base._depthStencilViews = new D3D11.DepthStencilView[1];

            CreateSwapChainTexture(d3dDevice, width, height, multisampleDesc, dxgiFormat);
            if (depthFormat != DepthFormat.None)
            {
                DXGI.Format dxgiDepthFormat = GraphicsExtensions.ToDXFormat(depthFormat);
                CreateSwapChainDepthBuffer(d3dDevice, width, height, multisampleDesc, dxgiDepthFormat);
            }
        }

        private void CreateSwapChainTexture(D3D11.Device d3dDevice, int width, int height, DXGI.SampleDescription multisampleDesc, DXGI.Format dxgiFormat)
        {
            DXGI.SwapChainDescription desc = new DXGI.SwapChainDescription()
            {
                ModeDescription =
                {
                    Width  = width,
                    Height = height,
                    Format = dxgiFormat,
                    Scaling = DXGI.DisplayModeScaling.Stretched,
                },

                OutputHandle = _windowHandle,
                SampleDescription = multisampleDesc,
                Usage = DXGI.Usage.RenderTargetOutput,
                BufferCount = 2,
                SwapEffect = GraphicsExtensions.ToDXSwapEffect(PresentInterval),
                IsWindowed = true,
            };
            
            // First, retrieve the underlying DXGI Device from the D3D Device.
            // Creates the swap chain 
            using (DXGI.Device1 dxgiDevice = d3dDevice.QueryInterface<DXGI.Device1>())
            using (DXGI.Adapter dxgiAdapter = dxgiDevice.Adapter)
            using (DXGI.Factory1 dxgiFactory = dxgiAdapter.GetParent<DXGI.Factory1>())
            {
                _swapChain = new DXGI.SwapChain(dxgiFactory, dxgiDevice, desc);
                // Obtain the backbuffer for this window which will be the final 3D rendertarget.
                _backBuffer = D3D11.Resource.FromSwapChain<D3D11.Texture2D>(_swapChain, 0);
                // Create a view interface on the rendertarget to use on bind.
                base._renderTargetViews[0] = new D3D11.RenderTargetView(d3dDevice, _backBuffer);
            }
        }

        private void CreateSwapChainDepthBuffer(D3D11.Device d3dDevice, int width, int height, DXGI.SampleDescription multisampleDesc, DXGI.Format dxgiDepthFormat)
        {
            // Allocate a 2-D surface as the depth/stencil buffer.
            D3D11.Texture2DDescription textureDescription = new D3D11.Texture2DDescription()
            {
                Format = dxgiDepthFormat,
                ArraySize = 1,
                MipLevels = 1,
                Width = width,
                Height = height,
                SampleDescription = multisampleDesc,
                Usage = D3D11.ResourceUsage.Default,
                BindFlags = D3D11.BindFlags.DepthStencil,
            };

            using (D3D11.Texture2D depthBuffer = new D3D11.Texture2D(d3dDevice, textureDescription))
            {
                // Create a DepthStencil view on this surface to use on bind.
                _depthStencilViews[0] = new D3D11.DepthStencilView(d3dDevice, depthBuffer);
            }
        }
        
        internal override D3D11.Resource CreateTexture()
        {
            return (MultiSampleCount > 1) ? base.CreateTexture() : _backBuffer;
        }

        internal override void ResolveSubresource()
        {
            // Using a multisampled SwapChainRenderTarget as a source Texture is not supported.
            //base.ResolveSubresource();
        }

        // TODO: We need to expose the other Present() overloads
        // for passing source/dest rectangles.

        /// <summary>
        /// Displays the contents of the active back buffer to the screen.
        /// </summary>
        public void Present()
        {
            lock (GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext)
            {
                D3D11.DeviceContext d3dContext = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                try
                {
                    _swapChain.Present(GraphicsExtensions.ToDXSwapInterval(PresentInterval), DXGI.PresentFlags.None);
                }
                catch (SharpDX.SharpDXException)
                {
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //SharpDX.Utilities.Dispose(ref _backBuffer);
                //SharpDX.Utilities.Dispose(ref _depthBuffer);
                SharpDX.Utilities.Dispose(ref _swapChain);
            }

            base.Dispose(disposing);
        }

    }
}
