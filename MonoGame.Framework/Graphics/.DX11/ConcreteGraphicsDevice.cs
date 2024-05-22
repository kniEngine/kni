// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.Utilities;
using DX = SharpDX;
using DXGI = SharpDX.DXGI;
using D2D = SharpDX.Direct2D1;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;

#if UAP || WINUI
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

#if WINDOWSDX
        internal DXGI.SwapChain _swapChain;
#endif

#if UAP || WINUI
        // The swap chain resources.
        internal DXGI.SwapChain1 _swapChain;
        internal D2D.Bitmap1 _bitmapTarget;

        internal SwapChainPanel _swapChainPanel;

        // Declare Direct2D Objects
        internal D2D.Factory1 _d2dFactory;
        internal D2D.Device _d2dDevice;
        internal D2D.DeviceContext _d2dContext;

        // Declare DirectWrite & Windows Imaging Component Objects
        internal SharpDX.DirectWrite.Factory _dwriteFactory;
        internal SharpDX.WIC.ImagingFactory2 _wicFactory;

        // Tearing (disabling V-Sync) support
        internal bool _isTearingSupported;

        internal float _dpi;
#endif

        internal D3D11.Device D3DDevice { get { return _d3dDevice; } }
   

        internal ConcreteGraphicsDevice(GraphicsDevice device, GraphicsAdapter adapter, GraphicsProfile graphicsProfile, bool preferHalfPixelOffset, PresentationParameters presentationParameters)
            : base(device, adapter, graphicsProfile, false, presentationParameters)
        {
        }


        public override void Reset(PresentationParameters presentationParameters)
        {
            this.PresentationParameters = presentationParameters;

#if WINDOWSDX
            if (this.PresentationParameters.IsFullScreen)
            {
                if (!this.PresentationParameters.HardwareModeSwitch)
                {
                    DisplayMode displayMode = Adapter.CurrentDisplayMode;
                    this.PresentationParameters.BackBufferWidth = displayMode.Width;
                    this.PresentationParameters.BackBufferHeight = displayMode.Height;
                }
                else
                {
                    GetModeSwitchedSize();
                }
            }
#endif

            if (this.PresentationParameters.DeviceWindowHandle == IntPtr.Zero)
                throw new ArgumentException("PresentationParameters.DeviceWindowHandle must not be null.");

            CreateSizeDependentResources();
            ((IPlatformGraphicsContext)_mainContext).Strategy.ApplyRenderTargets(null);
        }

        public override void Present(Rectangle? sourceRectangle, Rectangle? destinationRectangle, IntPtr overrideWindowHandle)
        {
            throw new NotImplementedException();
        }

        public override void Present()
        {
            base.Present();

            try
            {
                lock (((IPlatformGraphicsContext)_mainContext).Strategy.SyncHandle)
                {
                    int syncInterval = 0;
                    DXGI.PresentFlags presentFlags = DXGI.PresentFlags.None;

#if WINDOWSDX
                    // The first argument instructs DXGI to block n VSyncs before presenting.
                    syncInterval = PresentationParameters.PresentationInterval.ToDXSwapInterval();
#endif

#if UAP || WINUI
                    // The first argument instructs DXGI to block until VSync, putting the application
                    // to sleep until the next VSync. This ensures we don't waste any cycles rendering
                    // frames that will never be displayed to the screen.
                    if (_isTearingSupported && PresentationParameters.PresentationInterval == PresentInterval.Immediate)
                    {
                        syncInterval = 0;
                        presentFlags = DXGI.PresentFlags.AllowTearing;
                    }
                    else
                    {
                        syncInterval = 1;
                    }
#endif

                    _swapChain.Present(syncInterval, presentFlags);
                }
            }
            catch (DX.SharpDXException ex)
            {
                // TODO: How should we deal with a device lost case here?

#if UAP || WINUI
                /*               
                // If the device was removed either by a disconnect or a driver upgrade, we 
                // must completely reinitialize the renderer.
                if (ex.ResultCode == DXGI.DXGIError.DeviceRemoved ||
                    ex.ResultCode == DXGI.DXGIError.DeviceReset)
                    this.Initialize();
                else
                    throw;
                */
#endif
            }
        }

        public override void GetBackBufferData<T>(Rectangle? rect, T[] data, int startIndex, int elementCount)
        {
            // TODO share code with Texture2D.GetData
            // first set up a staging texture
            const SurfaceFormat format = SurfaceFormat.Color;
            //You can't Map the BackBuffer surface, so we copy to another texture
            using (D3D11.Texture2D backBufferTexture = D3D11.Resource.FromSwapChain<D3D11.Texture2D>(_swapChain, 0))
            {
                D3D11.Texture2DDescription texture2DDesc = backBufferTexture.Description;
                texture2DDesc.SampleDescription = new DXGI.SampleDescription(1, 0);
                texture2DDesc.BindFlags = D3D11.BindFlags.None;
                texture2DDesc.CpuAccessFlags = D3D11.CpuAccessFlags.Read;
                texture2DDesc.Usage = D3D11.ResourceUsage.Staging;
                texture2DDesc.OptionFlags = D3D11.ResourceOptionFlags.None;

                using (D3D11.Texture2D stagingTex = new D3D11.Texture2D(this.D3DDevice, texture2DDesc))
                {
                    lock (((IPlatformGraphicsContext)_mainContext).Strategy.SyncHandle)
                    {
                        // Copy the data from the GPU to the staging texture.
                        // if MSAA is enabled we need to first copy to a resource without MSAA
                        if (backBufferTexture.Description.SampleDescription.Count > 1)
                        {
                            texture2DDesc.Usage = D3D11.ResourceUsage.Default;
                            texture2DDesc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
                            using (D3D11.Texture2D noMsTex = new D3D11.Texture2D(this.D3DDevice, texture2DDesc))
                            {
                                ((IPlatformGraphicsContext)_mainContext).Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext.ResolveSubresource(backBufferTexture, 0, noMsTex, 0, texture2DDesc.Format);
                                if (rect.HasValue)
                                {
                                    Rectangle r = rect.Value;
                                    ((IPlatformGraphicsContext)_mainContext).Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext.CopySubresourceRegion(noMsTex, 0,
                                        new D3D11.ResourceRegion(r.Left, r.Top, 0, r.Right, r.Bottom, 1), stagingTex,
                                        0);
                                }
                                else
                                    ((IPlatformGraphicsContext)_mainContext).Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext.CopyResource(noMsTex, stagingTex);
                            }
                        }
                        else
                        {
                            if (rect.HasValue)
                            {
                                Rectangle r = rect.Value;
                                ((IPlatformGraphicsContext)_mainContext).Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext.CopySubresourceRegion(backBufferTexture, 0,
                                    new D3D11.ResourceRegion(r.Left, r.Top, 0, r.Right, r.Bottom, 1), stagingTex, 0);
                            }
                            else
                                ((IPlatformGraphicsContext)_mainContext).Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext.CopyResource(backBufferTexture, stagingTex);
                        }

                        // Copy the data to the array.
                        DX.DataStream stream = null;
                        try
                        {
                            DX.DataBox databox = ((IPlatformGraphicsContext)_mainContext).Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext.MapSubresource(stagingTex, 0, D3D11.MapMode.Read, D3D11.MapFlags.None, out stream);

                            int elementsInRow, rows;
                            if (rect.HasValue)
                            {
                                elementsInRow = rect.Value.Width;
                                rows = rect.Value.Height;
                            }
                            else
                            {
                                elementsInRow = stagingTex.Description.Width;
                                rows = stagingTex.Description.Height;
                            }
                            int elementSize = format.GetSize();
                            int rowSize = elementSize * elementsInRow;
                            if (rowSize == databox.RowPitch)
                                stream.ReadRange(data, startIndex, elementCount);
                            else
                            {
                                // Some drivers may add pitch to rows.
                                // We need to copy each row separately and skip trailing zeroes.
                                stream.Seek(0, SeekOrigin.Begin);

                                int elementSizeInByte = ReflectionHelpers.SizeOf<T>();
                                for (int row = 0; row < rows; row++)
                                {
                                    int i;
                                    for (i = row * rowSize / elementSizeInByte; i < (row + 1) * rowSize / elementSizeInByte; i++)
                                        data[i + startIndex] = stream.Read<T>();

                                    if (i >= elementCount)
                                        break;

                                    stream.Seek(databox.RowPitch - rowSize, SeekOrigin.Current);
                                }
                            }
                        }
                        finally
                        {
                            DX.Utilities.Dispose( ref stream);
                        }
                    }
                }
            }

        }



        protected override void PlatformSetup(PresentationParameters presentationParameters)
        {
            CreateDeviceIndependentResources();

            // Windows requires BGRA support out of DX.
            D3D11.DeviceCreationFlags creationFlags = D3D11.DeviceCreationFlags.BgraSupport;

            if (presentationParameters.UseDebugLayers)
            {
                creationFlags |= D3D11.DeviceCreationFlags.Debug;
            }

#if DEBUG && (UAP || WINUI)
            creationFlags |= D3D11.DeviceCreationFlags.Debug;
#endif

            // Pass the preferred feature levels based on the
            // target profile that may have been set by the user.
            D3D.FeatureLevel[] featureLevels;
            List<D3D.FeatureLevel> featureLevelsList = new List<D3D.FeatureLevel>();
            // create device with the highest available profile
            featureLevelsList.Add(D3D.FeatureLevel.Level_11_1);
            featureLevelsList.Add(D3D.FeatureLevel.Level_11_0);
            featureLevelsList.Add(D3D.FeatureLevel.Level_10_1);
            featureLevelsList.Add(D3D.FeatureLevel.Level_10_0);
            featureLevelsList.Add(D3D.FeatureLevel.Level_9_3);
            featureLevelsList.Add(D3D.FeatureLevel.Level_9_2);
            featureLevelsList.Add(D3D.FeatureLevel.Level_9_1);
#if DEBUG
            featureLevelsList.Clear();
            // create device specific to profile
            if (GraphicsProfile == GraphicsProfile.FL11_1) featureLevelsList.Add(D3D.FeatureLevel.Level_11_1);
            if (GraphicsProfile == GraphicsProfile.FL11_0) featureLevelsList.Add(D3D.FeatureLevel.Level_11_0);
            if (GraphicsProfile == GraphicsProfile.FL10_1) featureLevelsList.Add(D3D.FeatureLevel.Level_10_1);
            if (GraphicsProfile == GraphicsProfile.FL10_0) featureLevelsList.Add(D3D.FeatureLevel.Level_10_0);
            if (GraphicsProfile == GraphicsProfile.HiDef) featureLevelsList.Add(D3D.FeatureLevel.Level_9_3);
            if (GraphicsProfile == GraphicsProfile.Reach) featureLevelsList.Add(D3D.FeatureLevel.Level_9_1);
#endif
            featureLevels = featureLevelsList.ToArray();

            D3D.DriverType driverType = D3D.DriverType.Hardware;   //Default value

#if WINDOWSDX
            switch (presentationParameters.UseDriverType)
            {
                case PresentationParameters.DriverType.Reference:
                    driverType = D3D.DriverType.Reference;
                    break;

                case PresentationParameters.DriverType.FastSoftware:
                    driverType = D3D.DriverType.Warp;
                    break;
            }
#endif

#if UAP || WINUI
            driverType = GraphicsAdapter.UseReferenceDevice
                       ? D3D.DriverType.Reference
                       : D3D.DriverType.Hardware;
#endif

            try
            {
                // Create the Direct3D device.
                using (D3D11.Device defaultDevice = new D3D11.Device(driverType, creationFlags, featureLevels))
                {
#if WINDOWSDX
                    _d3dDevice = defaultDevice.QueryInterface<D3D11.Device>();
#endif
#if UAP || WINUI
                    _d3dDevice = defaultDevice.QueryInterface<D3D11.Device1>();
#endif
                }

#if UAP || WINUI
                // Necessary to enable video playback
                D3D.DeviceMultithread multithread = _d3dDevice.QueryInterface<D3D.DeviceMultithread>();
                multithread.SetMultithreadProtected(true);
#endif
            }
            catch (DX.SharpDXException)
            {
                // Try again without the debug flag.  This allows debug builds to run
                // on machines that don't have the debug runtime installed.
                creationFlags &= ~D3D11.DeviceCreationFlags.Debug;
                using (D3D11.Device defaultDevice = new D3D11.Device(driverType, creationFlags, featureLevels))
                {
#if WINDOWSDX
                    _d3dDevice = defaultDevice.QueryInterface<D3D11.Device>();
#endif
#if UAP || WINUI
                    _d3dDevice = defaultDevice.QueryInterface<D3D11.Device1>();
#endif
                }
            }

            _mainContext = base.CreateGraphicsContext();

#if UAP || WINUI
            // Create the Direct2D device.
            using (DXGI.Device dxgiDevice = _d3dDevice.QueryInterface<DXGI.Device>())
                _d2dDevice = new D2D.Device(_d2dFactory, dxgiDevice);

            // Create Direct2D context
            _d2dContext = new D2D.DeviceContext(_d2dDevice, D2D.DeviceContextOptions.None);
#endif


            _capabilities = new ConcreteGraphicsCapabilities();
            ((ConcreteGraphicsCapabilities)_capabilities).PlatformInitialize(this);

#if UAP || WINUI
            this.Dpi = DisplayInformation.GetForCurrentView().LogicalDpi;
#endif
        }

        /// <summary>
        /// Creates resources not tied the active graphics device.
        /// </summary>
        private void CreateDeviceIndependentResources()
        {
#if UAP || WINUI

            D2D.DebugLevel debugLevel = D2D.DebugLevel.None;

#if DEBUG && (UAP || WINUI)
            debugLevel |= D2D.DebugLevel.Information;
#endif

            // Allocate new references
            _d2dFactory = new D2D.Factory1(D2D.FactoryType.SingleThreaded, debugLevel);
            _dwriteFactory = new SharpDX.DirectWrite.Factory(SharpDX.DirectWrite.FactoryType.Shared);
            _wicFactory = new SharpDX.WIC.ImagingFactory2();
#endif
        }

        protected override void PlatformInitialize()
        {
#if WINDOWSDX
            if (this.PresentationParameters.IsFullScreen)
            {
                if (!this.PresentationParameters.HardwareModeSwitch)
                {
                    DisplayMode displayMode = Adapter.CurrentDisplayMode;
                    this.PresentationParameters.BackBufferWidth = displayMode.Width;
                    this.PresentationParameters.BackBufferHeight = displayMode.Height;
                }
                else
                {
                    GetModeSwitchedSize();
                }
            }
#endif
            CreateSizeDependentResources();
        }

#if WINDOWSDX
        private void GetModeSwitchedSize()
        {
            DXGI.Output output = null;
            if (_swapChain != null)
            {
                try { output = _swapChain.ContainingOutput; }
                catch (DX.SharpDXException) { /* ContainingOutput fails on a headless device */ }
            }
            else
            {
                // get the primary output
                using (DXGI.Factory1 factory = new DXGI.Factory1())
                using (DXGI.Adapter1 adapter = factory.GetAdapter1(0))
                    output = adapter.Outputs[0];
            }

            if (output != null)
            {
                using (output)
                {
                    DXGI.ModeDescription targetModeDesc = new DXGI.ModeDescription();
                    targetModeDesc.Scaling = DXGI.DisplayModeScaling.Unspecified;
                    targetModeDesc.Width = PresentationParameters.BackBufferWidth;
                    targetModeDesc.Height = PresentationParameters.BackBufferHeight;
                    targetModeDesc.Format = PresentationParameters.BackBufferFormat.ToDXFormat();

                    DXGI.ModeDescription closest;
                    output.GetClosestMatchingMode(this.D3DDevice, targetModeDesc, out closest);
                    this.PresentationParameters.BackBufferWidth = closest.Width;
                    this.PresentationParameters.BackBufferHeight = closest.Height;
                }
            }
        }

#endif

#if WINDOWSDX
        internal void SetHardwareFullscreen()
        {
            _swapChain.SetFullscreenState(PresentationParameters.IsFullScreen && PresentationParameters.HardwareModeSwitch, null);
        }

        internal void ClearHardwareFullscreen()
        {
            _swapChain.SetFullscreenState(false, null);
        }
#endif


#if WINDOWSDX
        internal void ResizeTargets()
        {
            DXGI.Format format = PresentationParameters.BackBufferFormat.ToDXFormat();
            DXGI.ModeDescription modeDesc = new DXGI.ModeDescription();
            modeDesc.Format = format;
            modeDesc.Scaling = DXGI.DisplayModeScaling.Unspecified;
            modeDesc.Width = PresentationParameters.BackBufferWidth;
            modeDesc.Height = PresentationParameters.BackBufferHeight;

            _swapChain.ResizeTarget(ref modeDesc);
        }
#endif

#if WINDOWSDX
        internal void RefreshAdapter()
        {
            if (_swapChain == null)
                return;

            DXGI.Output output = null;
            try { output = _swapChain.ContainingOutput; }
            catch (DX.SharpDXException) { /* ContainingOutput fails on a headless device */ }

            if (output != null)
            {
                using (output)
                {
                    foreach (GraphicsAdapter adapter in GraphicsAdapter.Adapters)
                    {
                        if (adapter.DeviceName == output.Description.DeviceName)
                        {
                            Adapter = adapter;
                            break;
                        }
                    }
                }
            }
        }
#endif

#if UAP || WINUI
        internal void SetMultiSamplingToMaximum(PresentationParameters presentationParameters, out int quality)
        {
            quality = (int)D3D11.StandardMultisampleQualityLevels.StandardMultisamplePattern;
        }
#endif

        internal void Internal_CreateSizeDependentResources()
        {
            this.CreateSizeDependentResources();
        }

        private void CreateSizeDependentResources()
        {
            // Clamp MultiSampleCount
            int maxMultiSampleCount = GetMaxMultiSampleCount(this.PresentationParameters.BackBufferFormat);
            PresentationParameters.MultiSampleCount = TextureHelpers.GetClampedMultiSampleCount(PresentationParameters.BackBufferFormat, PresentationParameters.MultiSampleCount, maxMultiSampleCount);

            ((IPlatformGraphicsContext)_mainContext).Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext.OutputMerger.SetTargets((D3D11.DepthStencilView)null,
                                                                                                (D3D11.RenderTargetView)null);

            if (_renderTargetView != null)
            {
                _renderTargetView.Dispose();
                _renderTargetView = null;
            }
            if (_depthStencilView != null)
            {
                _depthStencilView.Dispose();
                _depthStencilView = null;
            }

#if UAP || WINUI
            if (_bitmapTarget != null)
            {
                _bitmapTarget.Dispose();
                _bitmapTarget = null;
            }
            _d2dContext.Target = null;
#endif

            // Clear the current render targets.
            ((IPlatformGraphicsContext)_mainContext).Strategy.ToConcrete<ConcreteGraphicsContext>().ClearCurrentRenderTargets();

            // Make sure all pending rendering commands are flushed.
            ((IPlatformGraphicsContext)_mainContext).Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext.Flush();

            // We need presentation parameters to continue here.
            if (PresentationParameters == null || PresentationParameters.DeviceWindowHandle == IntPtr.Zero)
            {
                if (_swapChain != null)
                {
                    _swapChain.Dispose();
                    _swapChain = null;
                }

                return;
            }

            DXGI.Format format = PresentationParameters.BackBufferFormat.ToDXFormat();
            DXGI.SampleDescription multisampleDesc = GetSupportedSampleDescription(
                format,
                PresentationParameters.MultiSampleCount);

            DXGI.SwapChainFlags swapChainFlags = DXGI.SwapChainFlags.None;

#if WINDOWSDX

            swapChainFlags = DXGI.SwapChainFlags.AllowModeSwitch;

            // If the swap chain already exists... update it.
            if (_swapChain != null
                // check if multisampling hasn't changed
            && _swapChain.Description.SampleDescription.Count == multisampleDesc.Count
            && _swapChain.Description.SampleDescription.Quality == multisampleDesc.Quality
               )
            {
                _swapChain.ResizeBuffers(2,
                                         PresentationParameters.BackBufferWidth,
                                         PresentationParameters.BackBufferHeight,
                                         format,
                                         swapChainFlags);
            }            
            else // Otherwise, create a new swap chain.
            {
                bool wasFullScreen = false;
                // Dispose of old swap chain if exists
                if (_swapChain != null)
                {
                    wasFullScreen = _swapChain.IsFullScreen;
                    // Before releasing a swap chain, first switch to windowed mode
                    _swapChain.SetFullscreenState(false, null);
                    _swapChain.Dispose();
                }

                // SwapChain description
                DXGI.SwapChainDescription swapChainDesc = new DXGI.SwapChainDescription();
                swapChainDesc.ModeDescription.Format = format;
                swapChainDesc.ModeDescription.Scaling = DXGI.DisplayModeScaling.Unspecified;
                swapChainDesc.ModeDescription.Width = PresentationParameters.BackBufferWidth;
                swapChainDesc.ModeDescription.Height = PresentationParameters.BackBufferHeight;

                swapChainDesc.OutputHandle = PresentationParameters.DeviceWindowHandle;
                swapChainDesc.IsWindowed = true;

                swapChainDesc.SampleDescription = multisampleDesc;
                swapChainDesc.Usage = DXGI.Usage.RenderTargetOutput;
                swapChainDesc.BufferCount = 2;
                swapChainDesc.SwapEffect = PresentationParameters.PresentationInterval.ToDXSwapEffect();
                swapChainDesc.Flags = swapChainFlags;

                // Once the desired swap chain description is configured, it must be created on the same adapter as our D3D Device

                // First, retrieve the underlying DXGI Device from the D3D Device.
                // Creates the swap chain 
                using (DXGI.Device1 dxgiDevice = this.D3DDevice.QueryInterface<DXGI.Device1>())
                using (DXGI.Adapter dxgiAdapter = dxgiDevice.Adapter)
                using (DXGI.Factory1 dxgiFactory = dxgiAdapter.GetParent<DXGI.Factory1>())
                {
                    _swapChain = new DXGI.SwapChain(dxgiFactory, dxgiDevice, swapChainDesc);
                    RefreshAdapter();
                    dxgiFactory.MakeWindowAssociation(PresentationParameters.DeviceWindowHandle, DXGI.WindowAssociationFlags.IgnoreAll);
                    // To reduce latency, ensure that DXGI does not queue more than one frame at a time.
                    // Docs: https://msdn.microsoft.com/en-us/library/windows/desktop/ff471334(v=vs.85).aspx
                    dxgiDevice.MaximumFrameLatency = 1;
                }
                // Preserve full screen state, after swap chain is re-created 
                if (PresentationParameters.HardwareModeSwitch
                    && wasFullScreen)
                    SetHardwareFullscreen();
            }
#endif


#if UAP || WINUI

            _isTearingSupported = IsTearingSupported();
            if (_isTearingSupported)
            {
                swapChainFlags = DXGI.SwapChainFlags.AllowTearing;
            }

            // If the swap chain already exists... update it.
            if (_swapChain != null
               )
            {
                _swapChain.ResizeBuffers(2,
                                         PresentationParameters.BackBufferWidth,
                                         PresentationParameters.BackBufferHeight,
                                         format,
                                         swapChainFlags);
            }            
            else // Otherwise, create a new swap chain.
            {
                // SwapChain description
                DXGI.SwapChainDescription1 swapChainDesc = new DXGI.SwapChainDescription1();
                // Automatic sizing
                swapChainDesc.Width = PresentationParameters.BackBufferWidth;
                swapChainDesc.Height = PresentationParameters.BackBufferHeight;
                swapChainDesc.Format = format;
                swapChainDesc.Stereo = false;
                // By default we scale the backbuffer to the window 
                // rectangle to function more like a WP7 game.
                swapChainDesc.Scaling = DXGI.Scaling.Stretch;

                swapChainDesc.SampleDescription = multisampleDesc;
                swapChainDesc.Usage = DXGI.Usage.RenderTargetOutput;
                swapChainDesc.BufferCount = 2;
                swapChainDesc.SwapEffect = PresentationParameters.PresentationInterval.ToDXSwapEffect();
                swapChainDesc.Flags = swapChainFlags;

                // Once the desired swap chain description is configured, it must be created on the same adapter as our D3D Device

                // First, retrieve the underlying DXGI Device from the D3D Device.
                // Creates the swap chain 
                using (DXGI.Device2 dxgiDevice2 = this.D3DDevice.QueryInterface<DXGI.Device2>())
                using (DXGI.Adapter dxgiAdapter = dxgiDevice2.Adapter)
                using (DXGI.Factory2 dxgiFactory2 = dxgiAdapter.GetParent<DXGI.Factory2>())
                {
                    _swapChainPanel = UAPGameWindow.FromHandle(PresentationParameters.DeviceWindowHandle).SwapChainPanel;
                    if (_swapChainPanel != null)
                    {
                        using (DXGI.ISwapChainPanelNative nativePanel = DX.ComObject.As<DXGI.ISwapChainPanelNative>(_swapChainPanel))
                        {
                            _swapChain = new DXGI.SwapChain1(dxgiFactory2, dxgiDevice2, ref swapChainDesc, null);
                            nativePanel.SwapChain = _swapChain;

                            // update swapChain2.MatrixTransform on SizeChanged of SwapChainPanel
                            // sometimes window.SizeChanged and SwapChainPanel.SizeChanged are not synced
                            _swapChainPanel.SizeChanged += (sender, e) =>
                            {
                                try
                                {
                                    using (DXGI.SwapChain2 swapChain2 = _swapChain.QueryInterface<DXGI.SwapChain2>())
                                    {
                                        RawMatrix3x2 inverseScale = new RawMatrix3x2();
                                        inverseScale.M11 = (float)_swapChainPanel.ActualWidth / PresentationParameters.BackBufferWidth;
                                        inverseScale.M22 = (float)_swapChainPanel.ActualHeight / PresentationParameters.BackBufferHeight;
                                        swapChain2.MatrixTransform = inverseScale;
                                    };
                                }
                                catch (Exception) { }
                            };
                        }
                    }
                    else // (PresentationParameters.DeviceWindowHandle != IntPtr.Zero)
                    {
                        // Creates a SwapChain from a CoreWindow pointer.
                        CoreWindow coreWindow = Marshal.GetObjectForIUnknown(PresentationParameters.DeviceWindowHandle) as CoreWindow;
                        using (DX.ComObject comWindow = new DX.ComObject(coreWindow))
                            _swapChain = new DXGI.SwapChain1(dxgiFactory2, dxgiDevice2, comWindow, ref swapChainDesc);
                    }

                    // Ensure that DXGI does not queue more than one frame at a time. This both reduces 
                    // latency and ensures that the application will only render after each VSync, minimizing 
                    // power consumption.
                    dxgiDevice2.MaximumFrameLatency = 1;
                }
            }

            _swapChain.Rotation = DXGI.DisplayModeRotation.Identity;

            // Counter act the composition scale of the render target as 
            // we already handle this in the platform window code.
            if (_swapChainPanel != null)
            {
                Windows.Foundation.IAsyncAction asyncResult = _swapChainPanel.Dispatcher.RunIdleAsync((e) =>
                {
                    RawMatrix3x2 inverseScale = new RawMatrix3x2();
                    inverseScale.M11 = (float)_swapChainPanel.ActualWidth  / PresentationParameters.BackBufferWidth;
                    inverseScale.M22 = (float)_swapChainPanel.ActualHeight / PresentationParameters.BackBufferHeight;
                    using (DXGI.SwapChain2 swapChain2 = _swapChain.QueryInterface<DXGI.SwapChain2>())
                        swapChain2.MatrixTransform = inverseScale;
                });
            }
#endif

            // Obtain the backbuffer for this window which will be the final 3D rendertarget.
            Point targetSize;
            using (D3D11.Texture2D backBuffer = D3D11.Texture2D.FromSwapChain<D3D11.Texture2D>(_swapChain, 0))
            {
                // Create a view interface on the rendertarget to use on bind.
                _renderTargetView = new D3D11.RenderTargetView(this.D3DDevice, backBuffer);

                // Get the rendertarget dimensions for later.
                D3D11.Texture2DDescription backBufferDesc = backBuffer.Description;
                targetSize = new Point(backBufferDesc.Width, backBufferDesc.Height);
            }

            // Create the depth buffer if we need it.
            if (PresentationParameters.DepthStencilFormat != DepthFormat.None)
            {
                DXGI.Format depthFormat = PresentationParameters.DepthStencilFormat.ToDXFormat();

                // Allocate a 2-D surface as the depth/stencil buffer.
                D3D11.Texture2DDescription texture2DDesc = new D3D11.Texture2DDescription();
                texture2DDesc.Format = depthFormat;
                texture2DDesc.ArraySize = 1;
                texture2DDesc.MipLevels = 1;
                texture2DDesc.Width = targetSize.X;
                texture2DDesc.Height = targetSize.Y;
                texture2DDesc.SampleDescription = multisampleDesc;
                texture2DDesc.Usage = D3D11.ResourceUsage.Default;
                texture2DDesc.BindFlags = D3D11.BindFlags.DepthStencil;
                using (D3D11.Texture2D depthBuffer = new D3D11.Texture2D(this.D3DDevice, texture2DDesc))
                {
                    // Create a DepthStencil view on this surface to use on bind.
                    _depthStencilView = new D3D11.DepthStencilView(this.D3DDevice, depthBuffer);
                }

            }

            // Set the current viewport.
            _mainContext.Viewport = new Viewport
            {
                X = 0,
                Y = 0,
                Width = targetSize.X,
                Height = targetSize.Y,
                MinDepth = 0.0f,
                MaxDepth = 1.0f
            };

#if UAP || WINUI
            // Now we set up the Direct2D render target bitmap linked to the swapchain. 
            // Whenever we render to this bitmap, it will be directly rendered to the 
            // swapchain associated with the window.
            D2D.BitmapProperties1 bitmapProperties = new D2D.BitmapProperties1(
                new D2D.PixelFormat(format, D2D.AlphaMode.Premultiplied),
                _dpi, _dpi,
                D2D.BitmapOptions.Target | D2D.BitmapOptions.CannotDraw);

            // Direct2D needs the dxgi version of the backbuffer surface pointer.
            // Get a D2D surface from the DXGI back buffer to use as the D2D render target.
            using (DXGI.Surface dxgiBackBuffer = _swapChain.GetBackBuffer<DXGI.Surface>(0))
                _bitmapTarget = new D2D.Bitmap1(_d2dContext, dxgiBackBuffer, bitmapProperties);

            // So now we can set the Direct2D render target.
            _d2dContext.Target = _bitmapTarget;

            // Set D2D text anti-alias mode to Grayscale to 
            // ensure proper rendering of text on intermediate surfaces.
            _d2dContext.TextAntialiasMode = D2D.TextAntialiasMode.Grayscale;
#endif
        }

        internal int GetMaxMultiSampleCount(SurfaceFormat surfaceFormat)
        {
            DXGI.Format format = surfaceFormat.ToDXFormat();

            // Find the maximum supported level starting with the game's requested multisampling level
            // and halving each time until reaching 0 (meaning no multisample support).
            int qualityLevels = 0;
            int maxLevel = 32; // The highest possible multisampling level
            while (maxLevel > 0)
            {
                qualityLevels = this.D3DDevice.CheckMultisampleQualityLevels(format, maxLevel);
                if (qualityLevels > 0)
                    break;
                maxLevel /= 2;
            }
            return maxLevel;
        }

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
            int qualityLevels = this.D3DDevice.CheckMultisampleQualityLevels(format, multiSampleCount) - 1;

            return Math.Max(qualityLevels, 0); // clamp minimum to 0
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


#if UAP || WINUI

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


        public override GraphicsContextStrategy CreateGraphicsContextStrategy(GraphicsContext context)
        {
            // Get Direct3D 11.1 context
#if WINDOWSDX
            D3D11.DeviceContext d3dContext = _d3dDevice.ImmediateContext.QueryInterface<D3D11.DeviceContext>();
#endif
#if UAP || WINUI
            D3D11.DeviceContext1 d3dContext = _d3dDevice.ImmediateContext.QueryInterface<D3D11.DeviceContext1>();
#endif

            return new ConcreteGraphicsContext(context, d3dContext);
        }

        public override System.Reflection.Assembly ConcreteAssembly
        {
            get { return ReflectionHelpers.GetAssembly(typeof(ConcreteGraphicsDevice)); }
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // make sure to release full screen or this might cause issues on exit
                if (_swapChain != null && _swapChain.IsFullScreen)
                    _swapChain.SetFullscreenState(false, null);

                DX.Utilities.Dispose(ref _renderTargetView);
                DX.Utilities.Dispose(ref _depthStencilView);


                DX.Utilities.Dispose(ref _swapChain);

#if UAP || WINUI
                if (_bitmapTarget != null)
                {
                    _bitmapTarget.Dispose();
                    _depthStencilView = null;
                }
                if (_d2dDevice != null)
                {
                    _d2dDevice.Dispose();
                    _d2dDevice = null;
                }
                if (_d2dContext != null)
                {
                    _d2dContext.Target = null;
                    _d2dContext.Dispose();
                    _d2dContext = null;
                }
                if (_d2dFactory != null)
                {
                    _d2dFactory.Dispose();
                    _d2dFactory = null;
                }
                if (_dwriteFactory != null)
                {
                    _dwriteFactory.Dispose();
                    _dwriteFactory = null;
                }
                if (_wicFactory != null)
                {
                    _wicFactory.Dispose();
                    _wicFactory = null;
                }
#endif

                DX.Utilities.Dispose(ref _d3dDevice);

            }

            base.Dispose(disposing);
        }

    }
}
