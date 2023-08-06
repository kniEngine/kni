// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

#if WINDOWS_UAP
using System.Runtime.InteropServices;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
#endif


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteGraphicsDevice : GraphicsDeviceStrategy
    {
        // Core Direct3D Objects
        internal D3D11.Device _d3dDevice;
        internal D3D11.RenderTargetView _renderTargetView;
        internal D3D11.DepthStencilView _depthStencilView;

#if WINDOWS
        internal DXGI.SwapChain _swapChain;
#endif

#if WINDOWS_UAP
        // The swap chain resources.
        internal DXGI.SwapChain1 _swapChain;
        internal SharpDX.Direct2D1.Bitmap1 _bitmapTarget;

        internal SwapChainPanel _swapChainPanel;

        // Declare Direct2D Objects
        internal SharpDX.Direct2D1.Factory1 _d2dFactory;
        internal SharpDX.Direct2D1.Device _d2dDevice;
        internal SharpDX.Direct2D1.DeviceContext _d2dContext;

        // Declare DirectWrite & Windows Imaging Component Objects
        internal SharpDX.DirectWrite.Factory _dwriteFactory;
        internal SharpDX.WIC.ImagingFactory2 _wicFactory;

        // Tearing (disabling V-Sync) support
        internal bool _isTearingSupported;

        internal float _dpi;
#endif

        internal D3D11.Device D3DDevice { get { return _d3dDevice; } }
   

        internal ConcreteGraphicsDevice(GraphicsAdapter adapter, GraphicsProfile graphicsProfile, bool preferHalfPixelOffset, PresentationParameters presentationParameters)
            : base(adapter, graphicsProfile, false, presentationParameters)
        {
        }


        internal override TextureCollectionStrategy CreateTextureCollectionStrategy(GraphicsDevice device, GraphicsContext context, int capacity)
        {
            return new ConcreteTextureCollection(device, context, capacity);
        }

        internal override SamplerStateCollectionStrategy CreateSamplerStateCollectionStrategy(GraphicsDevice device, GraphicsContext context, int capacity)
        {
            return new ConcreteSamplerStateCollection(device, context, capacity);
        }

        internal override GraphicsDebugStrategy CreateGraphicsDebugStrategy(GraphicsDevice device)
        {
            return new ConcreteGraphicsDebug(device);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }

            base.Dispose(disposing);
        }

    }
}
