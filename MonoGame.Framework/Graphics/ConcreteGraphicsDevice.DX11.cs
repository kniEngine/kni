// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DX = SharpDX;
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

#if WINDOWS_UAP
using System.Runtime.InteropServices;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using SharpDX.Mathematics.Interop;
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


        public override void Reset(PresentationParameters presentationParameters)
        {
            PresentationParameters = presentationParameters;
            Reset();
        }

        public override void Reset()
        {
#if WINDOWS
            CorrectBackBufferSize();
#endif

#if WINDOWS
            if (PresentationParameters.DeviceWindowHandle == IntPtr.Zero)
                throw new ArgumentException("PresentationParameters.DeviceWindowHandle must not be null.");
#endif
#if WINDOWS_UAP
            if (PresentationParameters.DeviceWindowHandle == IntPtr.Zero && PresentationParameters.SwapChainPanel == null)
                throw new ArgumentException("PresentationParameters.DeviceWindowHandle or PresentationParameters.SwapChainPanel must not be null.");
#endif
        }

        public override void Present(Rectangle? sourceRectangle, Rectangle? destinationRectangle, IntPtr overrideWindowHandle)
        {
            throw new NotImplementedException();
        }

        public override void Present()
        {
        }

        public override void GetBackBufferData<T>(Rectangle? rect, T[] data, int startIndex, int elementCount)
        {
        }



        /// <summary>
        /// Creates resources not tied the active graphics device.
        /// </summary>
        internal void CreateDeviceIndependentResources()
        {
#if WINDOWS_UAP

            SharpDX.Direct2D1.DebugLevel debugLevel = SharpDX.Direct2D1.DebugLevel.None;

#if DEBUG && WINDOWS_UAP
            debugLevel |= SharpDX.Direct2D1.DebugLevel.Information;
#endif

            // Dispose previous references.
            if (_d2dFactory != null)
                _d2dFactory.Dispose();
            if (_dwriteFactory != null)
                _dwriteFactory.Dispose();
            if (_wicFactory != null)
                _wicFactory.Dispose();

            // Allocate new references
            _d2dFactory = new SharpDX.Direct2D1.Factory1(SharpDX.Direct2D1.FactoryType.SingleThreaded, debugLevel);
            _dwriteFactory = new SharpDX.DirectWrite.Factory(SharpDX.DirectWrite.FactoryType.Shared);
            _wicFactory = new SharpDX.WIC.ImagingFactory2();
#endif
        }


#if WINDOWS
        internal void CorrectBackBufferSize()
        {
            // Window size can be modified when we're going full screen, we need to take that into account
            // so the back buffer has the right size.
            if (PresentationParameters.IsFullScreen)
            {
                int newWidth, newHeight;
                if (PresentationParameters.HardwareModeSwitch)
                    GetModeSwitchedSize(out newWidth, out newHeight);
                else
                    GetDisplayResolution(out newWidth, out newHeight);

                PresentationParameters.BackBufferWidth = newWidth;
                PresentationParameters.BackBufferHeight = newHeight;
            }
        }

        internal void GetModeSwitchedSize(out int width, out int height)
        {
            DXGI.Output output = null;
            if (_swapChain == null)
            {
                // get the primary output
                using (DXGI.Factory1 factory = new DXGI.Factory1())
                using (DXGI.Adapter1 adapter = factory.GetAdapter1(0))
                    output = adapter.Outputs[0];
            }
            else
            {
                try
                {
                    output = _swapChain.ContainingOutput;
                }
                catch (DX.SharpDXException) { /* ContainingOutput fails on a headless device */ }
            }

            DXGI.Format format = GraphicsExtensions.ToDXFormat(PresentationParameters.BackBufferFormat);
            DXGI.ModeDescription target = new DXGI.ModeDescription
            {
                Format = format,
                Scaling = DXGI.DisplayModeScaling.Unspecified,
                Width = PresentationParameters.BackBufferWidth,
                Height = PresentationParameters.BackBufferHeight,
            };

            if (output == null)
            {
                width = PresentationParameters.BackBufferWidth;
                height = PresentationParameters.BackBufferHeight;
            }
            else
            {
                DXGI.ModeDescription closest;
                output.GetClosestMatchingMode(this.D3DDevice, target, out closest);
                width = closest.Width;
                height = closest.Height;
                output.Dispose();
            }
        }

        internal void GetDisplayResolution(out int width, out int height)
        {
            width = Adapter.CurrentDisplayMode.Width;
            height = Adapter.CurrentDisplayMode.Height;
        }

#endif

#if WINDOWS
        internal void SetHardwareFullscreen()
        {
            _swapChain.SetFullscreenState(PresentationParameters.IsFullScreen && PresentationParameters.HardwareModeSwitch, null);
        }

        internal void ClearHardwareFullscreen()
        {
            _swapChain.SetFullscreenState(false, null);
        }
#endif


#if WINDOWS
        internal void ResizeTargets()
        {
            DXGI.Format format = GraphicsExtensions.ToDXFormat(PresentationParameters.BackBufferFormat);
            DXGI.ModeDescription descr = new DXGI.ModeDescription
            {
                Format = format,
                Scaling = DXGI.DisplayModeScaling.Unspecified,
                Width = PresentationParameters.BackBufferWidth,
                Height = PresentationParameters.BackBufferHeight,
            };

            _swapChain.ResizeTarget(ref descr);
        }
#endif

#if WINDOWS
        internal void RefreshAdapter()
        {
            if (_swapChain == null)
                return;

            DXGI.Output output = null;
            try
            {
                output = _swapChain.ContainingOutput;
            }
            catch (DX.SharpDXException) { /* ContainingOutput fails on a headless device */ }

            if (output != null)
            {
                foreach (GraphicsAdapter adapter in GraphicsAdapter.Adapters)
                {
                    if (adapter.DeviceName == output.Description.DeviceName)
                    {
                        Adapter = adapter;
                        break;
                    }
                }

                output.Dispose();
            }
        }
#endif

#if WINDOWS_UAP
        internal void SetMultiSamplingToMaximum(PresentationParameters presentationParameters, out int quality)
        {
            quality = (int)D3D11.StandardMultisampleQualityLevels.StandardMultisamplePattern;
        }
#endif





        /// <summary>
        /// Get highest multisample quality level for specified format and multisample count.
        /// Returns 0 if multisampling is not supported for input parameters.
        /// </summary>
        /// <param name="format">The texture format.</param>
        /// <param name="multiSampleCount">The number of samples during multisampling.</param>
        /// <returns>
        /// Higher than zero if multiSampleCount is supported. 
        /// Zero if multiSampleCount is not supported.
        /// </returns>
        private int GetMultiSamplingQuality(DXGI.Format format, int multiSampleCount)
        {
            // The valid range is between zero and one less than the level returned by CheckMultisampleQualityLevels
            // https://msdn.microsoft.com/en-us/library/windows/desktop/bb173072(v=vs.85).aspx
            int quality = this.D3DDevice.CheckMultisampleQualityLevels(format, multiSampleCount) - 1;
            // NOTE: should we always return highest quality?
            return Math.Max(quality, 0); // clamp minimum to 0
        }

        internal DXGI.SampleDescription GetSupportedSampleDescription(DXGI.Format format, int multiSampleCount)
        {
            DXGI.SampleDescription multisampleDesc = new DXGI.SampleDescription(1, 0);

            if (multiSampleCount > 1)
            {
                int quality = GetMultiSamplingQuality(format, multiSampleCount);

                multisampleDesc.Count = multiSampleCount;
                multisampleDesc.Quality = quality;
            }

            return multisampleDesc;
        }


#if WINDOWS_UAP

        internal void Trim()
        {
            using (DXGI.Device3 dxgiDevice3 = this.D3DDevice.QueryInterface<DXGI.Device3>())
                dxgiDevice3.Trim();
        }

        internal float Dpi
        {
            get { return _dpi; }
            set
            {
                if (_dpi == value)
                    return;

                _dpi = value;
                _d2dContext.DotsPerInch = new DX.Size2F(_dpi, _dpi);

                //if (OnDpiChanged != null)
                //    OnDpiChanged(this);
            }
        }

        internal bool IsTearingSupported()
        {
            RawBool allowTearing;
            using (DXGI.Factory2 dxgiFactory2 = new DXGI.Factory2())
            {
                unsafe
                {
                    DXGI.Factory5 factory5 = dxgiFactory2.QueryInterface<DXGI.Factory5>();
                    try
                    {
                        factory5.CheckFeatureSupport(DXGI.Feature.PresentAllowTearing, new IntPtr(&allowTearing), sizeof(RawBool));

                        return allowTearing;
                    }
                    catch (DX.SharpDXException ex)
                    {
                        // can't request feature
                    }
                }
            }

            return false;
        }

#endif


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
