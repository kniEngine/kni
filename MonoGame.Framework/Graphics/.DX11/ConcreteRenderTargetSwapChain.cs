// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteRenderTargetSwapChain : ConcreteRenderTarget2D,
        IRenderTargetStrategyDX11
    {
        internal IntPtr _windowHandle;
        internal PresentInterval _presentInterval;

        internal DXGI.SwapChain _swapChain;
        internal D3D11.Texture2D _backBuffer;
        //internal D3D11.Texture2D _depthBuffer;


        internal ConcreteRenderTargetSwapChain(GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap, RenderTargetUsage usage,
            IntPtr windowHandle, PresentInterval presentInterval,
            SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
            :base(contextStrategy, width, height, mipMap, 1, false, usage, preferredSurfaceFormat, preferredDepthFormat, preferredMultiSampleCount,
                  Texture2D.SurfaceType.RenderTargetSwapChain)
        {
            _windowHandle = windowHandle;
            _presentInterval = presentInterval;

            int maxMultiSampleCount = contextStrategy.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().GetMaxMultiSampleCount(contextStrategy.Context.DeviceStrategy.PresentationParameters.BackBufferFormat);
            _multiSampleCount = contextStrategy.Context.DeviceStrategy.GetClampedMultiSampleCount(this.Format, preferredMultiSampleCount, maxMultiSampleCount);


            ((ConcreteRenderTarget2D)this)._renderTargetViews = new D3D11.RenderTargetView[1];
            ((ConcreteRenderTarget2D)this)._depthStencilViews = new D3D11.DepthStencilView[1];

            DXGI.Format dxgiFormat = (preferredSurfaceFormat == SurfaceFormat.Color)
                                   ? DXGI.Format.B8G8R8A8_UNorm
                                   : GraphicsExtensions.ToDXFormat(preferredSurfaceFormat);

            DXGI.SampleDescription multisampleDesc = GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().GetSupportedSampleDescription(dxgiFormat, MultiSampleCount);

            D3D11.Device d3dDevice = contextStrategy.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice;

            CreateSwapChainTexture(d3dDevice, width, height, multisampleDesc, dxgiFormat);
            if (preferredDepthFormat != DepthFormat.None)
            {
                DXGI.Format dxgiDepthFormat = GraphicsExtensions.ToDXFormat(preferredDepthFormat);
                CreateSwapChainDepthBuffer(d3dDevice, width, height, multisampleDesc, dxgiDepthFormat);
            }
        }


        D3D11.RenderTargetView IRenderTargetStrategyDX11.GetRenderTargetView(int arraySlice)
        {
            return _renderTargetViews[arraySlice];
        }

        D3D11.DepthStencilView IRenderTargetStrategyDX11.GetDepthStencilView(int arraySlice)
        {
            return _depthStencilViews[0];
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
            swapChainDesc.SwapEffect = GraphicsExtensions.ToDXSwapEffect(_presentInterval);
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
                ((ConcreteRenderTarget2D)this)._renderTargetViews[0] = new D3D11.RenderTargetView(d3dDevice, _backBuffer);
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
                ((ConcreteRenderTarget2D)this)._depthStencilViews[0] = new D3D11.DepthStencilView(d3dDevice, depthBuffer);
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

                if (_shared)
                    texture2DDesc.OptionFlags |= D3D11.ResourceOptionFlags.Shared;
                if (MultiSampleCount == 0 || _shared)
                    texture2DDesc.BindFlags |= D3D11.BindFlags.RenderTarget;
                if (_mipMap)
                    texture2DDesc.OptionFlags |= D3D11.ResourceOptionFlags.GenerateMipMaps;

                return new D3D11.Texture2D(contextStrategy.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture2DDesc);
            }
            else
            {
                System.Diagnostics.Debug.Assert(_backBuffer != null);
                return _backBuffer;
            }
        }

        public void Present()
        {
            lock (GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext)
            {
                D3D11.DeviceContext d3dContext = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                try
                {
                    _swapChain.Present(GraphicsExtensions.ToDXSwapInterval(_presentInterval), DXGI.PresentFlags.None);
                }
                catch (DX.SharpDXException)
                {
                }
            }
        }

        internal override void ResolveSubresource()
        {
            // Using a multisampled SwapChainRenderTarget as a source Texture is not supported.
            //base.ResolveSubresource();
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
