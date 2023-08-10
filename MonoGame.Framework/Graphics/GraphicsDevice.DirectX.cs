// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Platform.Graphics;
using MonoGame.Framework.Utilities;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Mathematics.Interop;
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

#if WINDOWS_UAP
using System.Runtime.InteropServices;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
#endif


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class GraphicsDevice
    {
        internal D3D11.DeviceContext CurrentD3DContext
        {
            get { return ((ConcreteGraphicsContext)CurrentContext.Strategy).D3dContext; }
        }


        /// <summary>
        /// Returns a handle to internal device object. Valid only on DirectX platforms.
        /// For usage, convert this to SharpDX.Direct3D11.Device.
        /// </summary>
        public object Handle
        {
            get { return ((ConcreteGraphicsDevice)_strategy)._d3dDevice; }
        }

        private void PlatformSetup()
        {
			CreateDeviceIndependentResources();
			CreateDeviceResources();

            _strategy._capabilities = new GraphicsCapabilities();
            _strategy._capabilities.PlatformInitialize(this);

#if WINDOWS_UAP
			Dpi = DisplayInformation.GetForCurrentView().LogicalDpi;
#endif
        }

        private void PlatformInitialize()
        {
#if WINDOWS
            CorrectBackBufferSize();
#endif
            CreateSizeDependentResources();
        }

        /// <summary>
        /// Creates resources not tied the active graphics device.
        /// </summary>
        protected void CreateDeviceIndependentResources()
        {
#if WINDOWS_UAP

            SharpDX.Direct2D1.DebugLevel debugLevel = SharpDX.Direct2D1.DebugLevel.None;

#if DEBUG && WINDOWS_UAP
            debugLevel |= SharpDX.Direct2D1.DebugLevel.Information;
#endif

            // Dispose previous references.
            if (((ConcreteGraphicsDevice)_strategy)._d2dFactory != null)
                ((ConcreteGraphicsDevice)_strategy)._d2dFactory.Dispose();
            if (((ConcreteGraphicsDevice)_strategy)._dwriteFactory != null)
                ((ConcreteGraphicsDevice)_strategy)._dwriteFactory.Dispose();
            if (((ConcreteGraphicsDevice)_strategy)._wicFactory != null)
                ((ConcreteGraphicsDevice)_strategy)._wicFactory.Dispose();

            // Allocate new references
            ((ConcreteGraphicsDevice)_strategy)._d2dFactory = new SharpDX.Direct2D1.Factory1(SharpDX.Direct2D1.FactoryType.SingleThreaded, debugLevel);
            ((ConcreteGraphicsDevice)_strategy)._dwriteFactory = new SharpDX.DirectWrite.Factory(SharpDX.DirectWrite.FactoryType.Shared);
            ((ConcreteGraphicsDevice)_strategy)._wicFactory = new SharpDX.WIC.ImagingFactory2();
#endif
        }

        /// <summary>
        /// Create graphics device specific resources.
        /// </summary>
        protected virtual void CreateDeviceResources()
        {
            // Dispose previous references.
            if (((ConcreteGraphicsDevice)_strategy)._d3dDevice != null)
                ((ConcreteGraphicsDevice)_strategy)._d3dDevice.Dispose();
            if (_mainContext != null)
                _mainContext.Dispose();
            _mainContext = null;


#if WINDOWS_UAP
            if (((ConcreteGraphicsDevice)_strategy)._d2dDevice != null)
                ((ConcreteGraphicsDevice)_strategy)._d2dDevice.Dispose();
            if (((ConcreteGraphicsDevice)_strategy)._d2dContext != null)
                ((ConcreteGraphicsDevice)_strategy)._d2dContext.Dispose();
#endif

            // Windows requires BGRA support out of DX.
            D3D11.DeviceCreationFlags creationFlags = D3D11.DeviceCreationFlags.BgraSupport;

            if (GraphicsAdapter.UseDebugLayers)
            {
                creationFlags |= D3D11.DeviceCreationFlags.Debug;
            }

#if DEBUG && WINDOWS_UAP
            creationFlags |= D3D11.DeviceCreationFlags.Debug;
#endif
            
            // Pass the preferred feature levels based on the
            // target profile that may have been set by the user.
            FeatureLevel[] featureLevels;
            List<FeatureLevel> featureLevelsList = new List<FeatureLevel>();
            // create device with the highest available profile
            featureLevelsList.Add(FeatureLevel.Level_11_1);
            featureLevelsList.Add(FeatureLevel.Level_11_0);
            featureLevelsList.Add(FeatureLevel.Level_10_1);
            featureLevelsList.Add(FeatureLevel.Level_10_0);
            featureLevelsList.Add(FeatureLevel.Level_9_3);
            featureLevelsList.Add(FeatureLevel.Level_9_2);
            featureLevelsList.Add(FeatureLevel.Level_9_1);
#if DEBUG
            featureLevelsList.Clear();
            // create device specific to profile
            if (GraphicsProfile == GraphicsProfile.FL11_1) featureLevelsList.Add(FeatureLevel.Level_11_1);
            if (GraphicsProfile == GraphicsProfile.FL11_0) featureLevelsList.Add(FeatureLevel.Level_11_0);
            if (GraphicsProfile == GraphicsProfile.FL10_1) featureLevelsList.Add(FeatureLevel.Level_10_1);
            if (GraphicsProfile == GraphicsProfile.FL10_0) featureLevelsList.Add(FeatureLevel.Level_10_0);
            if (GraphicsProfile == GraphicsProfile.HiDef) featureLevelsList.Add(FeatureLevel.Level_9_3);
            if (GraphicsProfile == GraphicsProfile.Reach) featureLevelsList.Add(FeatureLevel.Level_9_1);
#endif
            featureLevels = featureLevelsList.ToArray();

            DriverType driverType = DriverType.Hardware;   //Default value

#if WINDOWS
            switch (GraphicsAdapter.UseDriverType)
            {
                case GraphicsAdapter.DriverType.Reference:
                    driverType = DriverType.Reference;
                    break;

                case GraphicsAdapter.DriverType.FastSoftware:
                    driverType = DriverType.Warp;
                    break;
            }
#endif

#if WINDOWS_UAP
            driverType = GraphicsAdapter.UseReferenceDevice
                       ? DriverType.Reference
                       : DriverType.Hardware;
#endif

            try
            {
                // Create the Direct3D device.
                using (D3D11.Device defaultDevice = new D3D11.Device(driverType, creationFlags, featureLevels))
                {
#if WINDOWS
                    ((ConcreteGraphicsDevice)_strategy)._d3dDevice = defaultDevice.QueryInterface<D3D11.Device>();
#endif
#if WINDOWS_UAP
                    ((ConcreteGraphicsDevice)_strategy)._d3dDevice = defaultDevice.QueryInterface<D3D11.Device1>();
#endif
                }

#if WINDOWS_UAP
                // Necessary to enable video playback
                SharpDX.Direct3D.DeviceMultithread multithread = ((ConcreteGraphicsDevice)_strategy)._d3dDevice.QueryInterface<SharpDX.Direct3D.DeviceMultithread>();
                multithread.SetMultithreadProtected(true);
#endif
            }
            catch (SharpDXException)
            {
                // Try again without the debug flag.  This allows debug builds to run
                // on machines that don't have the debug runtime installed.
                creationFlags &= ~D3D11.DeviceCreationFlags.Debug;
                using (D3D11.Device defaultDevice = new D3D11.Device(driverType, creationFlags, featureLevels))
                {
#if WINDOWS
                    ((ConcreteGraphicsDevice)_strategy)._d3dDevice = defaultDevice.QueryInterface<D3D11.Device>();
#endif
#if WINDOWS_UAP
                    ((ConcreteGraphicsDevice)_strategy)._d3dDevice = defaultDevice.QueryInterface<D3D11.Device1>();
#endif
                }
            }

            // Get Direct3D 11.1 context
#if WINDOWS
            D3D11.DeviceContext d3dContext = ((ConcreteGraphicsDevice)_strategy)._d3dDevice.ImmediateContext.QueryInterface<D3D11.DeviceContext>();
#endif
#if WINDOWS_UAP
            D3D11.DeviceContext1 d3dContext = ((ConcreteGraphicsDevice)_strategy)._d3dDevice.ImmediateContext.QueryInterface<D3D11.DeviceContext1>();
#endif

            GraphicsContextStrategy contextStrategy = new ConcreteGraphicsContext(this, d3dContext);
            _mainContext = new GraphicsContext(this, contextStrategy);


#if WINDOWS
            // Create a new instance of GraphicsDebug because we support it on Windows platforms.
            _mainContext.Strategy.GraphicsDebug = new GraphicsDebug(this);
#endif

#if WINDOWS_UAP
            // Create the Direct2D device.
            using (DXGI.Device dxgiDevice = ((ConcreteGraphicsDevice)_strategy)._d3dDevice.QueryInterface<DXGI.Device>())
                ((ConcreteGraphicsDevice)_strategy)._d2dDevice = new SharpDX.Direct2D1.Device(((ConcreteGraphicsDevice)_strategy)._d2dFactory, dxgiDevice);

            // Create Direct2D context
            ((ConcreteGraphicsDevice)_strategy)._d2dContext = new SharpDX.Direct2D1.DeviceContext(((ConcreteGraphicsDevice)_strategy)._d2dDevice, SharpDX.Direct2D1.DeviceContextOptions.None);
#endif
        }

        internal void CreateSizeDependentResources()
        {
            // Clamp MultiSampleCount
            PresentationParameters.MultiSampleCount =
                GetClampedMultisampleCount(PresentationParameters.MultiSampleCount);

            ((ConcreteGraphicsContext)_mainContext.Strategy).D3dContext.OutputMerger.SetTargets((D3D11.DepthStencilView)null,
                                                                                                (D3D11.RenderTargetView)null);

            if (((ConcreteGraphicsDevice)_strategy)._renderTargetView != null)
            {
                ((ConcreteGraphicsDevice)_strategy)._renderTargetView.Dispose();
                ((ConcreteGraphicsDevice)_strategy)._renderTargetView = null;
            }
            if (((ConcreteGraphicsDevice)_strategy)._depthStencilView != null)
            {
                ((ConcreteGraphicsDevice)_strategy)._depthStencilView.Dispose();
                ((ConcreteGraphicsDevice)_strategy)._depthStencilView = null;
            }

#if WINDOWS_UAP
            if (((ConcreteGraphicsDevice)_strategy)._bitmapTarget != null)
            {
                ((ConcreteGraphicsDevice)_strategy)._bitmapTarget.Dispose();
                ((ConcreteGraphicsDevice)_strategy)._bitmapTarget = null;
            }
            ((ConcreteGraphicsDevice)_strategy)._d2dContext.Target = null;
#endif

            // Clear the current render targets.
            ((ConcreteGraphicsContext)_mainContext.Strategy)._currentDepthStencilView = null;
            Array.Clear(((ConcreteGraphicsContext)_mainContext.Strategy)._currentRenderTargets, 0, ((ConcreteGraphicsContext)_mainContext.Strategy)._currentRenderTargets.Length);
            Array.Clear(_mainContext.Strategy._currentRenderTargetBindings, 0, _mainContext.Strategy._currentRenderTargetBindings.Length);
            _mainContext.Strategy._currentRenderTargetCount = 0;

            // Make sure all pending rendering commands are flushed.
            ((ConcreteGraphicsContext)_mainContext.Strategy).D3dContext.Flush();

#if WINDOWS
            // We need presentation parameters to continue here.
            if (PresentationParameters == null
            ||  (PresentationParameters.DeviceWindowHandle == IntPtr.Zero)
               )
            {
                if (((ConcreteGraphicsDevice)_strategy)._swapChain != null)
                {
                    ((ConcreteGraphicsDevice)_strategy)._swapChain.Dispose();
                    ((ConcreteGraphicsDevice)_strategy)._swapChain = null;
                }

                return;
            }

            DXGI.Format format = GraphicsExtensions.ToDXFormat(PresentationParameters.BackBufferFormat);
            DXGI.SampleDescription multisampleDesc = GetSupportedSampleDescription(
                format,
                PresentationParameters.MultiSampleCount);

            DXGI.SwapChainFlags swapChainFlags = DXGI.SwapChainFlags.None;

            swapChainFlags = DXGI.SwapChainFlags.AllowModeSwitch;

            // If the swap chain already exists... update it.
            if (((ConcreteGraphicsDevice)_strategy)._swapChain != null
                // check if multisampling hasn't changed
                && ((ConcreteGraphicsDevice)_strategy)._swapChain.Description.SampleDescription.Count == multisampleDesc.Count
                && ((ConcreteGraphicsDevice)_strategy)._swapChain.Description.SampleDescription.Quality == multisampleDesc.Quality
               )
            {
                ((ConcreteGraphicsDevice)_strategy)._swapChain.ResizeBuffers(2,
                                        PresentationParameters.BackBufferWidth,
                                        PresentationParameters.BackBufferHeight,
                                        format,
                                        swapChainFlags);
            }

            // Otherwise, create a new swap chain.
            else
            {
                bool wasFullScreen = false;
                // Dispose of old swap chain if exists
                if (((ConcreteGraphicsDevice)_strategy)._swapChain != null)
                {
                    wasFullScreen = ((ConcreteGraphicsDevice)_strategy)._swapChain.IsFullScreen;
                    // Before releasing a swap chain, first switch to windowed mode
                    ((ConcreteGraphicsDevice)_strategy)._swapChain.SetFullscreenState(false, null);
                    ((ConcreteGraphicsDevice)_strategy)._swapChain.Dispose();
                }

                // SwapChain description
                DXGI.SwapChainDescription desc = new DXGI.SwapChainDescription()
                {
                    ModeDescription =
                    {
                        Format = format,
                        Scaling = DXGI.DisplayModeScaling.Unspecified,
                        Width = PresentationParameters.BackBufferWidth,
                        Height = PresentationParameters.BackBufferHeight,
                    },

                    OutputHandle = PresentationParameters.DeviceWindowHandle,
                    IsWindowed = true,

                    SampleDescription = multisampleDesc,
                    Usage = DXGI.Usage.RenderTargetOutput,
                    BufferCount = 2,
                    SwapEffect = GraphicsExtensions.ToDXSwapEffect(PresentationParameters.PresentationInterval),
                    Flags = swapChainFlags
                };

                // Once the desired swap chain description is configured, it must be created on the same adapter as our D3D Device

                // First, retrieve the underlying DXGI Device from the D3D Device.
                // Creates the swap chain 
                using (DXGI.Device1 dxgiDevice = ((ConcreteGraphicsDevice)_strategy).D3DDevice.QueryInterface<DXGI.Device1>())
                using (DXGI.Adapter dxgiAdapter = dxgiDevice.Adapter)
                using (DXGI.Factory1 dxgiFactory = dxgiAdapter.GetParent<DXGI.Factory1>())
                {
                    ((ConcreteGraphicsDevice)_strategy)._swapChain = new DXGI.SwapChain(dxgiFactory, dxgiDevice, desc);
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

#if WINDOWS_UAP
            // We need presentation parameters to continue here.
            if (PresentationParameters == null ||
                   (PresentationParameters.DeviceWindowHandle == IntPtr.Zero && PresentationParameters.SwapChainPanel == null)
               )
            {
                if (((ConcreteGraphicsDevice)_strategy)._swapChain != null)
                {
                    ((ConcreteGraphicsDevice)_strategy)._swapChain.Dispose();
                    ((ConcreteGraphicsDevice)_strategy)._swapChain = null;
                }

                return;
            }

            // Did we change swap panels?
            if (PresentationParameters.SwapChainPanel != ((ConcreteGraphicsDevice)_strategy)._swapChainPanel)
            {
                ((ConcreteGraphicsDevice)_strategy)._swapChainPanel = null;

                if (((ConcreteGraphicsDevice)_strategy)._swapChain != null)
                {
                    ((ConcreteGraphicsDevice)_strategy)._swapChain.Dispose();
                    ((ConcreteGraphicsDevice)_strategy)._swapChain = null;
                }
            }

            DXGI.Format format = GraphicsExtensions.ToDXFormat(PresentationParameters.BackBufferFormat);
            DXGI.SampleDescription multisampleDesc = GetSupportedSampleDescription(
                format,
                PresentationParameters.MultiSampleCount);

            DXGI.SwapChainFlags swapChainFlags = DXGI.SwapChainFlags.None;

            ((ConcreteGraphicsDevice)_strategy)._isTearingSupported = IsTearingSupported();
            if (((ConcreteGraphicsDevice)_strategy)._isTearingSupported)
            {
                swapChainFlags = DXGI.SwapChainFlags.AllowTearing;
            }

            // If the swap chain already exists... update it.
            if (((ConcreteGraphicsDevice)_strategy)._swapChain != null
               )
            {
                ((ConcreteGraphicsDevice)_strategy)._swapChain.ResizeBuffers(2,
                                        PresentationParameters.BackBufferWidth,
                                        PresentationParameters.BackBufferHeight,
                                        format,
                                        swapChainFlags);
            }

            // Otherwise, create a new swap chain.
            else
            {
                // SwapChain description
                DXGI.SwapChainDescription1 desc = new DXGI.SwapChainDescription1()
                {
                    // Automatic sizing
                    Width = PresentationParameters.BackBufferWidth,
                    Height = PresentationParameters.BackBufferHeight,
                    Format = format,
                    Stereo = false,
                    // By default we scale the backbuffer to the window 
                    // rectangle to function more like a WP7 game.
                    Scaling = DXGI.Scaling.Stretch,

                    SampleDescription = multisampleDesc,
                    Usage = DXGI.Usage.RenderTargetOutput,
                    BufferCount = 2,
                    SwapEffect = GraphicsExtensions.ToDXSwapEffect(PresentationParameters.PresentationInterval),
                    Flags = swapChainFlags
                };

                // Once the desired swap chain description is configured, it must be created on the same adapter as our D3D Device

                // First, retrieve the underlying DXGI Device from the D3D Device.
                // Creates the swap chain 
                using (DXGI.Device2 dxgiDevice2 = ((ConcreteGraphicsDevice)_strategy).D3DDevice.QueryInterface<DXGI.Device2>())
                using (DXGI.Adapter dxgiAdapter = dxgiDevice2.Adapter)
                using (DXGI.Factory2 dxgiFactory2 = dxgiAdapter.GetParent<DXGI.Factory2>())
                {
                    if (PresentationParameters.DeviceWindowHandle != IntPtr.Zero)
                    {
                        // Creates a SwapChain from a CoreWindow pointer.
                        CoreWindow coreWindow = Marshal.GetObjectForIUnknown(PresentationParameters.DeviceWindowHandle) as CoreWindow;
                        using (ComObject comWindow = new ComObject(coreWindow))
                            ((ConcreteGraphicsDevice)_strategy)._swapChain = new DXGI.SwapChain1(dxgiFactory2, dxgiDevice2, comWindow, ref desc);
                    }
                    else
                    {
                        ((ConcreteGraphicsDevice)_strategy)._swapChainPanel = PresentationParameters.SwapChainPanel;
                        using (DXGI.ISwapChainPanelNative nativePanel = ComObject.As<DXGI.ISwapChainPanelNative>(PresentationParameters.SwapChainPanel))
                        {
                            ((ConcreteGraphicsDevice)_strategy)._swapChain = new DXGI.SwapChain1(dxgiFactory2, dxgiDevice2, ref desc, null);
                            nativePanel.SwapChain = ((ConcreteGraphicsDevice)_strategy)._swapChain;

                            // update swapChain2.MatrixTransform on SizeChanged of SwapChainPanel
                            // sometimes window.SizeChanged and SwapChainPanel.SizeChanged are not synced
                            PresentationParameters.SwapChainPanel.SizeChanged += (sender, e) =>
                            {
                                try
                                {
                                    using (DXGI.SwapChain2 swapChain2 = ((ConcreteGraphicsDevice)_strategy)._swapChain.QueryInterface<DXGI.SwapChain2>())
                                    {
                                        RawMatrix3x2 inverseScale = new RawMatrix3x2();
                                        inverseScale.M11 = (float)PresentationParameters.SwapChainPanel.ActualWidth / PresentationParameters.BackBufferWidth;
                                        inverseScale.M22 = (float)PresentationParameters.SwapChainPanel.ActualHeight / PresentationParameters.BackBufferHeight;
                                        swapChain2.MatrixTransform = inverseScale;
                                    };
                                }
                                catch (Exception) { }
                            };
                        }
                    }

                    // Ensure that DXGI does not queue more than one frame at a time. This both reduces 
                    // latency and ensures that the application will only render after each VSync, minimizing 
                    // power consumption.
                    dxgiDevice2.MaximumFrameLatency = 1;
                }
            }

            ((ConcreteGraphicsDevice)_strategy)._swapChain.Rotation = DXGI.DisplayModeRotation.Identity;

            // Counter act the composition scale of the render target as 
            // we already handle this in the platform window code. 
            if (PresentationParameters.SwapChainPanel != null)
            {
                Windows.Foundation.IAsyncAction asyncResult = PresentationParameters.SwapChainPanel.Dispatcher.RunIdleAsync((e) =>
                {
                    RawMatrix3x2 inverseScale = new RawMatrix3x2();
                    inverseScale.M11 = (float)PresentationParameters.SwapChainPanel.ActualWidth  / PresentationParameters.BackBufferWidth;
                    inverseScale.M22 = (float)PresentationParameters.SwapChainPanel.ActualHeight / PresentationParameters.BackBufferHeight;
                    using (DXGI.SwapChain2 swapChain2 = ((ConcreteGraphicsDevice)_strategy)._swapChain.QueryInterface<DXGI.SwapChain2>())
                        swapChain2.MatrixTransform = inverseScale;
                });
            }
#endif

            // Obtain the backbuffer for this window which will be the final 3D rendertarget.
            Point targetSize;
            using (D3D11.Texture2D backBuffer = D3D11.Texture2D.FromSwapChain<D3D11.Texture2D>(((ConcreteGraphicsDevice)_strategy)._swapChain, 0))
            {
                // Create a view interface on the rendertarget to use on bind.
                ((ConcreteGraphicsDevice)_strategy)._renderTargetView = new D3D11.RenderTargetView(((ConcreteGraphicsDevice)_strategy).D3DDevice, backBuffer);

                // Get the rendertarget dimensions for later.
                D3D11.Texture2DDescription backBufferDesc = backBuffer.Description;
                targetSize = new Point(backBufferDesc.Width, backBufferDesc.Height);
            }

            // Create the depth buffer if we need it.
            if (PresentationParameters.DepthStencilFormat != DepthFormat.None)
            {
                DXGI.Format depthFormat = GraphicsExtensions.ToDXFormat(PresentationParameters.DepthStencilFormat);

                // Allocate a 2-D surface as the depth/stencil buffer.
                using (D3D11.Texture2D depthBuffer = new D3D11.Texture2D(((ConcreteGraphicsDevice)_strategy).D3DDevice, new D3D11.Texture2DDescription()
                    {
                        Format = depthFormat,
                        ArraySize = 1,
                        MipLevels = 1,
                        Width = targetSize.X,
                        Height = targetSize.Y,
                        SampleDescription = multisampleDesc,
                        Usage = D3D11.ResourceUsage.Default,
                        BindFlags = D3D11.BindFlags.DepthStencil,
                    }))
                {
                    // Create a DepthStencil view on this surface to use on bind.
                    ((ConcreteGraphicsDevice)_strategy)._depthStencilView = new D3D11.DepthStencilView(((ConcreteGraphicsDevice)_strategy).D3DDevice, depthBuffer);
                }

            }

            // Set the current viewport.
            Viewport = new Viewport
            {
                X = 0,
                Y = 0,
                Width = targetSize.X,
                Height = targetSize.Y,
                MinDepth = 0.0f,
                MaxDepth = 1.0f
            };

#if WINDOWS_UAP
            // Now we set up the Direct2D render target bitmap linked to the swapchain. 
            // Whenever we render to this bitmap, it will be directly rendered to the 
            // swapchain associated with the window.
            SharpDX.Direct2D1.BitmapProperties1 bitmapProperties = new SharpDX.Direct2D1.BitmapProperties1(
                new SharpDX.Direct2D1.PixelFormat(format, SharpDX.Direct2D1.AlphaMode.Premultiplied),
                ((ConcreteGraphicsDevice)_strategy)._dpi, ((ConcreteGraphicsDevice)_strategy)._dpi,
                SharpDX.Direct2D1.BitmapOptions.Target | SharpDX.Direct2D1.BitmapOptions.CannotDraw);

            // Direct2D needs the dxgi version of the backbuffer surface pointer.
            // Get a D2D surface from the DXGI back buffer to use as the D2D render target.
            using (DXGI.Surface dxgiBackBuffer = ((ConcreteGraphicsDevice)_strategy)._swapChain.GetBackBuffer<DXGI.Surface>(0))
                ((ConcreteGraphicsDevice)_strategy)._bitmapTarget = new SharpDX.Direct2D1.Bitmap1(((ConcreteGraphicsDevice)_strategy)._d2dContext, dxgiBackBuffer, bitmapProperties);

            // So now we can set the Direct2D render target.
            ((ConcreteGraphicsDevice)_strategy)._d2dContext.Target = ((ConcreteGraphicsDevice)_strategy)._bitmapTarget;

            // Set D2D text anti-alias mode to Grayscale to 
            // ensure proper rendering of text on intermediate surfaces.
            ((ConcreteGraphicsDevice)_strategy)._d2dContext.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Grayscale;
#endif
        }

        internal void OnPresentationChanged()
        {
            CreateSizeDependentResources();
            _mainContext.ApplyRenderTargets(null);
        }

        partial void PlatformReset()
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

#if WINDOWS
        private void CorrectBackBufferSize()
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
            if (((ConcreteGraphicsDevice)_strategy)._swapChain == null)
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
                    output = ((ConcreteGraphicsDevice)_strategy)._swapChain.ContainingOutput;
                }
                catch (SharpDXException) { /* ContainingOutput fails on a headless device */ }
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
                output.GetClosestMatchingMode(((ConcreteGraphicsDevice)_strategy).D3DDevice, target, out closest);
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
            ((ConcreteGraphicsDevice)_strategy)._swapChain.SetFullscreenState(PresentationParameters.IsFullScreen && PresentationParameters.HardwareModeSwitch, null);
        }

        internal void ClearHardwareFullscreen()
        {
            ((ConcreteGraphicsDevice)_strategy)._swapChain.SetFullscreenState(false, null);
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

            ((ConcreteGraphicsDevice)_strategy)._swapChain.ResizeTarget(ref descr);
        }
#endif

#if WINDOWS
        internal void RefreshAdapter()
        {
            if (((ConcreteGraphicsDevice)_strategy)._swapChain == null)
                return;

            DXGI.Output output = null;
            try
            {
                output = ((ConcreteGraphicsDevice)_strategy)._swapChain.ContainingOutput;
            }
            catch (SharpDXException) { /* ContainingOutput fails on a headless device */ }

            if (output != null)
            {
                foreach (GraphicsAdapter adapter in GraphicsAdapter.Adapters)
                {
                    if (adapter.DeviceName == output.Description.DeviceName)
                    {
                        _strategy.Adapter = adapter;
                        break;
                    }
                }

                output.Dispose();
            }
        }
#endif

#if WINDOWS_UAP
        private void SetMultiSamplingToMaximum(PresentationParameters presentationParameters, out int quality)
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
            int quality = ((ConcreteGraphicsDevice)_strategy).D3DDevice.CheckMultisampleQualityLevels(format, multiSampleCount) - 1;
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

        private void PlatformDispose()
        {
            // make sure to release full screen or this might cause issues on exit
            if (((ConcreteGraphicsDevice)_strategy)._swapChain != null && ((ConcreteGraphicsDevice)_strategy)._swapChain.IsFullScreen)
                ((ConcreteGraphicsDevice)_strategy)._swapChain.SetFullscreenState(false, null);

            SharpDX.Utilities.Dispose(ref ((ConcreteGraphicsDevice)_strategy)._renderTargetView);
            SharpDX.Utilities.Dispose(ref ((ConcreteGraphicsDevice)_strategy)._depthStencilView);

            if (((ConcreteGraphicsContext)_mainContext.Strategy)._userIndexBuffer16 != null)
                ((ConcreteGraphicsContext)_mainContext.Strategy)._userIndexBuffer16.Dispose();
            if (((ConcreteGraphicsContext)_mainContext.Strategy)._userIndexBuffer32 != null)
                ((ConcreteGraphicsContext)_mainContext.Strategy)._userIndexBuffer32.Dispose();

            foreach (DynamicVertexBuffer vb in ((ConcreteGraphicsContext)_mainContext.Strategy)._userVertexBuffers.Values)
                vb.Dispose();

            SharpDX.Utilities.Dispose(ref ((ConcreteGraphicsDevice)_strategy)._swapChain);

#if WINDOWS_UAP

            if (((ConcreteGraphicsDevice)_strategy)._bitmapTarget != null)
            {
                ((ConcreteGraphicsDevice)_strategy)._bitmapTarget.Dispose();
                ((ConcreteGraphicsDevice)_strategy)._depthStencilView = null;
            }
            if (((ConcreteGraphicsDevice)_strategy)._d2dDevice != null)
            {
                ((ConcreteGraphicsDevice)_strategy)._d2dDevice.Dispose();
                ((ConcreteGraphicsDevice)_strategy)._d2dDevice = null;
            }
            if (((ConcreteGraphicsDevice)_strategy)._d2dContext != null)
            {
                ((ConcreteGraphicsDevice)_strategy)._d2dContext.Target = null;
                ((ConcreteGraphicsDevice)_strategy)._d2dContext.Dispose();
                ((ConcreteGraphicsDevice)_strategy)._d2dContext = null;
            }
            if (((ConcreteGraphicsDevice)_strategy)._d2dFactory != null)
            {
                ((ConcreteGraphicsDevice)_strategy)._d2dFactory.Dispose();
                ((ConcreteGraphicsDevice)_strategy)._d2dFactory = null;
            }
            if (((ConcreteGraphicsDevice)_strategy)._dwriteFactory != null)
            {
                ((ConcreteGraphicsDevice)_strategy)._dwriteFactory.Dispose();
                ((ConcreteGraphicsDevice)_strategy)._dwriteFactory = null;
            }
            if (((ConcreteGraphicsDevice)_strategy)._wicFactory != null)
            {
                ((ConcreteGraphicsDevice)_strategy)._wicFactory.Dispose();
                ((ConcreteGraphicsDevice)_strategy)._wicFactory = null;
            }

#endif

            if (_mainContext != null)
                _mainContext.Dispose();
            _mainContext = null;
            SharpDX.Utilities.Dispose(ref ((ConcreteGraphicsDevice)_strategy)._d3dDevice);
        }

        private void PlatformPresent()
        {
            try
            {
                lock (((ConcreteGraphicsContext)_mainContext.Strategy).D3dContext)
                {
                    int syncInterval = 0;
                    DXGI.PresentFlags presentFlags = DXGI.PresentFlags.None;

#if WINDOWS
                    // The first argument instructs DXGI to block n VSyncs before presenting.
                    syncInterval = GraphicsExtensions.ToDXSwapInterval(PresentationParameters.PresentationInterval);
#endif

#if WINDOWS_UAP
                    // The first argument instructs DXGI to block until VSync, putting the application
                    // to sleep until the next VSync. This ensures we don't waste any cycles rendering
                    // frames that will never be displayed to the screen.
                    if (((ConcreteGraphicsDevice)_strategy)._isTearingSupported && PresentationParameters.PresentationInterval == PresentInterval.Immediate)
                    {
                        syncInterval = 0;
                        presentFlags = DXGI.PresentFlags.AllowTearing;
                    }
                    else
                    {
                        syncInterval = 1;
                    }
#endif

                    ((ConcreteGraphicsDevice)_strategy)._swapChain.Present(syncInterval, presentFlags);
                }
            }
            catch (SharpDX.SharpDXException ex)
            {
                // TODO: How should we deal with a device lost case here?

#if WINDOWS_UAP
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

        private void PlatformGetBackBufferData<T>(Rectangle? rect, T[] data, int startIndex, int count) where T : struct
        {
            // TODO share code with Texture2D.GetData and do pooling for staging textures
            // first set up a staging texture
            const SurfaceFormat format = SurfaceFormat.Color;
            //You can't Map the BackBuffer surface, so we copy to another texture
            using (D3D11.Texture2D backBufferTexture = D3D11.Resource.FromSwapChain<D3D11.Texture2D>(((ConcreteGraphicsDevice)_strategy)._swapChain, 0))
            {
                D3D11.Texture2DDescription desc = backBufferTexture.Description;
                desc.SampleDescription = new DXGI.SampleDescription(1, 0);
                desc.BindFlags = D3D11.BindFlags.None;
                desc.CpuAccessFlags = D3D11.CpuAccessFlags.Read;
                desc.Usage = D3D11.ResourceUsage.Staging;
                desc.OptionFlags = D3D11.ResourceOptionFlags.None;

                using (D3D11.Texture2D stagingTex = new D3D11.Texture2D(((ConcreteGraphicsDevice)_strategy).D3DDevice, desc))
                {
                    lock (((ConcreteGraphicsContext)_mainContext.Strategy).D3dContext)
                    {
                        // Copy the data from the GPU to the staging texture.
                        // if MSAA is enabled we need to first copy to a resource without MSAA
                        if (backBufferTexture.Description.SampleDescription.Count > 1)
                        {
                            desc.Usage = D3D11.ResourceUsage.Default;
                            desc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
                            using (D3D11.Texture2D noMsTex = new D3D11.Texture2D(((ConcreteGraphicsDevice)_strategy).D3DDevice, desc))
                            {
                                ((ConcreteGraphicsContext)_mainContext.Strategy).D3dContext.ResolveSubresource(backBufferTexture, 0, noMsTex, 0, desc.Format);
                                if (rect.HasValue)
                                {
                                    Rectangle r = rect.Value;
                                    ((ConcreteGraphicsContext)_mainContext.Strategy).D3dContext.CopySubresourceRegion(noMsTex, 0,
                                        new D3D11.ResourceRegion(r.Left, r.Top, 0, r.Right, r.Bottom, 1), stagingTex,
                                        0);
                                }
                                else
                                    ((ConcreteGraphicsContext)_mainContext.Strategy).D3dContext.CopyResource(noMsTex, stagingTex);
                            }
                        }
                        else
                        {
                            if (rect.HasValue)
                            {
                                Rectangle r = rect.Value;
                                ((ConcreteGraphicsContext)_mainContext.Strategy).D3dContext.CopySubresourceRegion(backBufferTexture, 0,
                                    new D3D11.ResourceRegion(r.Left, r.Top, 0, r.Right, r.Bottom, 1), stagingTex, 0);
                            }
                            else
                                ((ConcreteGraphicsContext)_mainContext.Strategy).D3dContext.CopyResource(backBufferTexture, stagingTex);
                        }

                        // Copy the data to the array.
                        DataStream stream = null;
                        try
                        {
                            DataBox databox = ((ConcreteGraphicsContext)_mainContext.Strategy).D3dContext.MapSubresource(stagingTex, 0, D3D11.MapMode.Read, D3D11.MapFlags.None, out stream);

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
                                stream.ReadRange(data, startIndex, count);
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

                                    if (i >= count)
                                        break;

                                    stream.Seek(databox.RowPitch - rowSize, SeekOrigin.Current);
                                }
                            }
                        }
                        finally
                        {
                            SharpDX.Utilities.Dispose( ref stream);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sends queued-up commands in the command buffer to the graphics processing unit (GPU).
        /// </summary>
        public void Flush()
        {
            ((ConcreteGraphicsContext)CurrentContext.Strategy).D3dContext.Flush();
        }

#if WINDOWS_UAP

        internal void Trim()
        {
            using (DXGI.Device3 dxgiDevice3 = ((ConcreteGraphicsDevice)_strategy).D3DDevice.QueryInterface<DXGI.Device3>())
                dxgiDevice3.Trim();
        }

        internal float Dpi
        {
            get { return ((ConcreteGraphicsDevice)_strategy)._dpi; }
            set
            {
                if (((ConcreteGraphicsDevice)_strategy)._dpi == value)
                    return;

                ((ConcreteGraphicsDevice)_strategy)._dpi = value;
                ((ConcreteGraphicsDevice)_strategy)._d2dContext.DotsPerInch = new Size2F(((ConcreteGraphicsDevice)_strategy)._dpi, ((ConcreteGraphicsDevice)_strategy)._dpi);

                //if (OnDpiChanged != null)
                //    OnDpiChanged(this);
            }
        }

        private bool IsTearingSupported()
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
                    catch (SharpDXException ex)
                    {
                        // can't request feature
                    }
                }
            }

            return false;
        }

#endif

    }
}
