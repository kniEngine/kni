
// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;
using DX = SharpDX;
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
            : base(graphicsDevice, width, height, mipMap, surfaceFormat, depthFormat, preferredMultiSampleCount, usage, false, 1)
        {
            this._windowHandle = windowHandle;
            this.PresentInterval = presentInterval;

            DXGI.Format dxgiFormat = (surfaceFormat == SurfaceFormat.Color)
                                   ? DXGI.Format.B8G8R8A8_UNorm
                                   : GraphicsExtensions.ToDXFormat(surfaceFormat);

            D3D11.Device d3dDevice = GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice;

            DXGI.SampleDescription multisampleDesc = GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().GetSupportedSampleDescription(dxgiFormat, MultiSampleCount);

            ((ConcreteRenderTarget2D)base._strategyRenderTarget2D)._renderTargetViews = new D3D11.RenderTargetView[1];
            ((ConcreteRenderTarget2D)base._strategyRenderTarget2D)._depthStencilViews = new D3D11.DepthStencilView[1];

            CreateSwapChainTexture(d3dDevice, width, height, multisampleDesc, dxgiFormat);
            if (depthFormat != DepthFormat.None)
            {
                DXGI.Format dxgiDepthFormat = GraphicsExtensions.ToDXFormat(depthFormat);
                CreateSwapChainDepthBuffer(d3dDevice, width, height, multisampleDesc, dxgiDepthFormat);
            }
        }

        private void CreateSwapChainTexture(D3D11.Device d3dDevice, int width, int height, DXGI.SampleDescription multisampleDesc, DXGI.Format dxgiFormat)
        {
            DXGI.SwapChainDescription swapChainDesc = new DXGI.SwapChainDescription();
            swapChainDesc.ModeDescription.Width = width;
            swapChainDesc.ModeDescription.Height = height;
            swapChainDesc.ModeDescription.Format = dxgiFormat;
            swapChainDesc.ModeDescription.Scaling = DXGI.DisplayModeScaling.Stretched;
            swapChainDesc.OutputHandle = _windowHandle;
            swapChainDesc.SampleDescription = multisampleDesc;
            swapChainDesc.Usage = DXGI.Usage.RenderTargetOutput;
            swapChainDesc.BufferCount = 2;
            swapChainDesc.SwapEffect = GraphicsExtensions.ToDXSwapEffect(PresentInterval);
            swapChainDesc.IsWindowed = true;
            
            // First, retrieve the underlying DXGI Device from the D3D Device.
            // Creates the swap chain 
            using (DXGI.Device1 dxgiDevice = d3dDevice.QueryInterface<DXGI.Device1>())
            using (DXGI.Adapter dxgiAdapter = dxgiDevice.Adapter)
            using (DXGI.Factory1 dxgiFactory = dxgiAdapter.GetParent<DXGI.Factory1>())
            {
                _swapChain = new DXGI.SwapChain(dxgiFactory, dxgiDevice, swapChainDesc);
                // Obtain the backbuffer for this window which will be the final 3D rendertarget.
                _backBuffer = D3D11.Resource.FromSwapChain<D3D11.Texture2D>(_swapChain, 0);
                // Create a view interface on the rendertarget to use on bind.
                ((ConcreteRenderTarget2D)base._strategyRenderTarget2D)._renderTargetViews[0] = new D3D11.RenderTargetView(d3dDevice, _backBuffer);
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
                ((ConcreteRenderTarget2D)base._strategyRenderTarget2D)._depthStencilViews[0] = new D3D11.DepthStencilView(d3dDevice, depthBuffer);
            }
        }

        private D3D11.Resource CreateTexture(GraphicsContextStrategy contextStrategy)
        {
            if (MultiSampleCount > 1)
            {
                DXGI.SampleDescription sampleDesc = new DXGI.SampleDescription(1, 0);
                D3D11.Texture2DDescription texture2DDesc = new D3D11.Texture2DDescription();
                texture2DDesc.Width = this.Width;
                texture2DDesc.Height = this.Height;
                texture2DDesc.MipLevels = this.LevelCount;
                texture2DDesc.ArraySize = this.ArraySize;
                texture2DDesc.Format = GraphicsExtensions.ToDXFormat(this.Format);
                texture2DDesc.BindFlags = D3D11.BindFlags.ShaderResource;
                texture2DDesc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
                texture2DDesc.SampleDescription = sampleDesc;
                texture2DDesc.Usage = D3D11.ResourceUsage.Default;
                texture2DDesc.OptionFlags = D3D11.ResourceOptionFlags.None;

                if (((ConcreteTexture2D)_strategyTexture2D)._shared)
                    texture2DDesc.OptionFlags |= D3D11.ResourceOptionFlags.Shared;
                if (MultiSampleCount == 0 || ((ConcreteTexture2D)_strategyTexture2D)._shared)
                    texture2DDesc.BindFlags |= D3D11.BindFlags.RenderTarget;
                if (((ConcreteTexture2D)_strategyTexture2D)._mipMap)
                    texture2DDesc.OptionFlags |= D3D11.ResourceOptionFlags.GenerateMipMaps;

                return new D3D11.Texture2D(contextStrategy.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture2DDesc);
            }
            else
            {
                System.Diagnostics.Debug.Assert(_backBuffer != null);
                return _backBuffer;
            }
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
                catch (DX.SharpDXException)
                {
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //DX.Utilities.Dispose(ref _backBuffer);
                //DX.Utilities.Dispose(ref _depthBuffer);
                DX.Utilities.Dispose(ref _swapChain);
            }

            base.Dispose(disposing);
        }

    }
}
