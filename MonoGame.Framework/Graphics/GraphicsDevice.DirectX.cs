// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Platform.Graphics;
using MonoGame.Framework.Utilities;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;

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
        // Core Direct3D Objects
        private SharpDX.Direct3D11.Device _d3dDevice;
        private SharpDX.Direct3D11.DeviceContext _d3dContext;
        private SharpDX.Direct3D11.RenderTargetView _renderTargetView;
        private SharpDX.Direct3D11.DepthStencilView _depthStencilView;
        private int _vertexBufferSlotsUsed;

        private PrimitiveType _lastPrimitiveType = (PrimitiveType)(-1);

        internal SharpDX.Direct3D11.Device D3DDevice { get { return _d3dDevice; } }
        internal SharpDX.Direct3D11.DeviceContext CurrentD3DContext { get { return _d3dContext; } }

#if WINDOWS_UAP

        // The swap chain resources.
        SharpDX.DXGI.SwapChain1 _swapChain;
        SharpDX.Direct2D1.Bitmap1 _bitmapTarget;

		SwapChainPanel _swapChainPanel;

        // Declare Direct2D Objects
        SharpDX.Direct2D1.Factory1 _d2dFactory;
        SharpDX.Direct2D1.Device _d2dDevice;
        SharpDX.Direct2D1.DeviceContext _d2dContext;

        // Declare DirectWrite & Windows Imaging Component Objects
        SharpDX.DirectWrite.Factory _dwriteFactory;
        SharpDX.WIC.ImagingFactory2 _wicFactory;

        // Tearing (disabling V-Sync) support
        bool _isTearingSupported;

		float _dpi;
#endif
#if WINDOWS

        SharpDX.DXGI.SwapChain _swapChain;

#endif

        // The active render targets.
        readonly SharpDX.Direct3D11.RenderTargetView[] _currentRenderTargets = new SharpDX.Direct3D11.RenderTargetView[8];

        // The active depth view.
        SharpDX.Direct3D11.DepthStencilView _currentDepthStencilView;

        private readonly Dictionary<VertexDeclaration, DynamicVertexBuffer> _userVertexBuffers = new Dictionary<VertexDeclaration, DynamicVertexBuffer>();
        private DynamicIndexBuffer _userIndexBuffer16;
        private DynamicIndexBuffer _userIndexBuffer32;

        /// <summary>
        /// Returns a handle to internal device object. Valid only on DirectX platforms.
        /// For usage, convert this to SharpDX.Direct3D11.Device.
        /// </summary>
        public object Handle
        {
            get
            {
                return _d3dDevice;
            }
        }

        private void PlatformSetup()
        {
            MaxTextureSlots = 16;
            MaxVertexTextureSlots = 16;

			CreateDeviceIndependentResources();
			CreateDeviceResources();

#if WINDOWS_UAP
			Dpi = DisplayInformation.GetForCurrentView().LogicalDpi;
#endif

            _maxVertexBufferSlots = 16;
            if (this.GraphicsProfile >= GraphicsProfile.FL10_1) _maxVertexBufferSlots = 32;
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

            var debugLevel = SharpDX.Direct2D1.DebugLevel.None;

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

        /// <summary>
        /// Create graphics device specific resources.
        /// </summary>
        protected virtual void CreateDeviceResources()
        {
            // Dispose previous references.
            if (_d3dDevice != null)
                _d3dDevice.Dispose();
            if (_d3dContext != null)
                _d3dContext.Dispose();


#if WINDOWS_UAP
            if (_d2dDevice != null)
                _d2dDevice.Dispose();
            if (_d2dContext != null)
                _d2dContext.Dispose();
#endif

            // Windows requires BGRA support out of DX.
            var creationFlags = SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport;

            if (GraphicsAdapter.UseDebugLayers)
            {
                creationFlags |= SharpDX.Direct3D11.DeviceCreationFlags.Debug;
            }

#if DEBUG && WINDOWS_UAP
            creationFlags |= SharpDX.Direct3D11.DeviceCreationFlags.Debug;
#endif
            
            // Pass the preferred feature levels based on the
            // target profile that may have been set by the user.
            FeatureLevel[] featureLevels;
            var featureLevelsList = new List<FeatureLevel>();
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

            var driverType = DriverType.Hardware;   //Default value

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
                using (var defaultDevice = new SharpDX.Direct3D11.Device(driverType, creationFlags, featureLevels))
                {
#if WINDOWS
                    _d3dDevice = defaultDevice.QueryInterface<SharpDX.Direct3D11.Device>();
#endif
#if WINDOWS_UAP
                    _d3dDevice = defaultDevice.QueryInterface<SharpDX.Direct3D11.Device1>();
#endif
                }

#if WINDOWS_UAP
                // Necessary to enable video playback
                var multithread = _d3dDevice.QueryInterface<SharpDX.Direct3D.DeviceMultithread>();
                multithread.SetMultithreadProtected(true);
#endif
            }
            catch (SharpDXException)
            {
                // Try again without the debug flag.  This allows debug builds to run
                // on machines that don't have the debug runtime installed.
                creationFlags &= ~SharpDX.Direct3D11.DeviceCreationFlags.Debug;
                using (var defaultDevice = new SharpDX.Direct3D11.Device(driverType, creationFlags, featureLevels))
                {
#if WINDOWS
                    _d3dDevice = defaultDevice.QueryInterface<SharpDX.Direct3D11.Device>();
#endif
#if WINDOWS_UAP
                    _d3dDevice = defaultDevice.QueryInterface<SharpDX.Direct3D11.Device1>();
#endif
                }
            }

            // Get Direct3D 11.1 context
#if WINDOWS
            var d3dContext = _d3dDevice.ImmediateContext.QueryInterface<SharpDX.Direct3D11.DeviceContext>();
#endif
#if WINDOWS_UAP
            var d3dContext = _d3dDevice.ImmediateContext.QueryInterface<SharpDX.Direct3D11.DeviceContext1>();
#endif
            _d3dContext = d3dContext;


#if WINDOWS
            // Create a new instance of GraphicsDebug because we support it on Windows platforms.
            _graphicsDebug = new GraphicsDebug(this);
#endif

#if WINDOWS_UAP
            // Create the Direct2D device.
            using (var dxgiDevice = _d3dDevice.QueryInterface<SharpDX.DXGI.Device>())
                _d2dDevice = new SharpDX.Direct2D1.Device(_d2dFactory, dxgiDevice);

            // Create Direct2D context
            _d2dContext = new SharpDX.Direct2D1.DeviceContext(_d2dDevice, SharpDX.Direct2D1.DeviceContextOptions.None);
#endif
        }

        internal void CreateSizeDependentResources()
        {
            // Clamp MultiSampleCount
            PresentationParameters.MultiSampleCount =
                GetClampedMultisampleCount(PresentationParameters.MultiSampleCount);

            CurrentD3DContext.OutputMerger.SetTargets((SharpDX.Direct3D11.DepthStencilView)null,
                                                (SharpDX.Direct3D11.RenderTargetView)null);

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

#if WINDOWS_UAP
            if (_bitmapTarget != null)
            {
                _bitmapTarget.Dispose();
                _bitmapTarget = null;
            }
            _d2dContext.Target = null;
#endif

            // Clear the current render targets.
            _currentDepthStencilView = null;
            Array.Clear(_currentRenderTargets, 0, _currentRenderTargets.Length);
            Array.Clear(_currentRenderTargetBindings, 0, _currentRenderTargetBindings.Length);
            _currentRenderTargetCount = 0;

            // Make sure all pending rendering commands are flushed.
            CurrentD3DContext.Flush();

#if WINDOWS
            // We need presentation parameters to continue here.
            if (PresentationParameters == null
            ||  (PresentationParameters.DeviceWindowHandle == IntPtr.Zero)
               )
            {
                if (_swapChain != null)
                {
                    _swapChain.Dispose();
                    _swapChain = null;
                }

                return;
            }

            var format = GraphicsExtensions.ToDXFormat(PresentationParameters.BackBufferFormat);
            var multisampleDesc = GetSupportedSampleDescription(
                format,
                PresentationParameters.MultiSampleCount);

            var swapChainFlags = SwapChainFlags.None;

            swapChainFlags = SwapChainFlags.AllowModeSwitch;

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

            // Otherwise, create a new swap chain.
            else
            {
                var wasFullScreen = false;
                // Dispose of old swap chain if exists
                if (_swapChain != null)
                {
                    wasFullScreen = _swapChain.IsFullScreen;
                    // Before releasing a swap chain, first switch to windowed mode
                    _swapChain.SetFullscreenState(false, null);
                    _swapChain.Dispose();
                }

                // SwapChain description
                var desc = new SharpDX.DXGI.SwapChainDescription()
                {
                    ModeDescription =
                    {
                        Format = format,
#if WINDOWS_UAP
                        Scaling = DisplayModeScaling.Stretched,
#else
                        Scaling = DisplayModeScaling.Unspecified,
#endif
                        Width = PresentationParameters.BackBufferWidth,
                        Height = PresentationParameters.BackBufferHeight,
                    },

                    OutputHandle = PresentationParameters.DeviceWindowHandle,
                    IsWindowed = true,

                    SampleDescription = multisampleDesc,
                    Usage = SharpDX.DXGI.Usage.RenderTargetOutput,
                    BufferCount = 2,
                    SwapEffect = GraphicsExtensions.ToDXSwapEffect(PresentationParameters.PresentationInterval),
                    Flags = swapChainFlags
                };

                // Once the desired swap chain description is configured, it must be created on the same adapter as our D3D Device

                // First, retrieve the underlying DXGI Device from the D3D Device.
                // Creates the swap chain 
                using (var dxgiDevice = D3DDevice.QueryInterface<SharpDX.DXGI.Device1>())
                using (var dxgiAdapter = dxgiDevice.Adapter)
                using (var dxgiFactory = dxgiAdapter.GetParent<SharpDX.DXGI.Factory1>())
                {
                    _swapChain = new SwapChain(dxgiFactory, dxgiDevice, desc);
                    RefreshAdapter();
                    dxgiFactory.MakeWindowAssociation(PresentationParameters.DeviceWindowHandle, WindowAssociationFlags.IgnoreAll);
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
                if (_swapChain != null)
                {
                    _swapChain.Dispose();
                    _swapChain = null;
                }

                return;
            }

            // Did we change swap panels?
            if (PresentationParameters.SwapChainPanel != _swapChainPanel)
            {
                _swapChainPanel = null;

                if (_swapChain != null)
                {
                    _swapChain.Dispose();
                    _swapChain = null;
                }
            }

            var format = GraphicsExtensions.ToDXFormat(PresentationParameters.BackBufferFormat);
            var multisampleDesc = GetSupportedSampleDescription(
                format,
                PresentationParameters.MultiSampleCount);

            var swapChainFlags = SwapChainFlags.None;

            _isTearingSupported = IsTearingSupported();
            if (_isTearingSupported)
            {
                swapChainFlags = SwapChainFlags.AllowTearing;
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

            // Otherwise, create a new swap chain.
            else
            {
                // SwapChain description
                var desc = new SharpDX.DXGI.SwapChainDescription1()
                {
                    // Automatic sizing
                    Width = PresentationParameters.BackBufferWidth,
                    Height = PresentationParameters.BackBufferHeight,
                    Format = format,
                    Stereo = false,
                    // By default we scale the backbuffer to the window 
                    // rectangle to function more like a WP7 game.
                    Scaling = SharpDX.DXGI.Scaling.Stretch,

                    SampleDescription = multisampleDesc,
                    Usage = SharpDX.DXGI.Usage.RenderTargetOutput,
                    BufferCount = 2,
                    SwapEffect = GraphicsExtensions.ToDXSwapEffect(PresentationParameters.PresentationInterval),
                    Flags = swapChainFlags
                };

                // Once the desired swap chain description is configured, it must be created on the same adapter as our D3D Device

                // First, retrieve the underlying DXGI Device from the D3D Device.
                // Creates the swap chain 
                using (var dxgiDevice2 = D3DDevice.QueryInterface<SharpDX.DXGI.Device2>())
                using (var dxgiAdapter = dxgiDevice2.Adapter)
                using (var dxgiFactory2 = dxgiAdapter.GetParent<SharpDX.DXGI.Factory2>())
                {
                    if (PresentationParameters.DeviceWindowHandle != IntPtr.Zero)
                    {
                        // Creates a SwapChain from a CoreWindow pointer.
                        var coreWindow = Marshal.GetObjectForIUnknown(PresentationParameters.DeviceWindowHandle) as CoreWindow;
                        using (var comWindow = new ComObject(coreWindow))
                            _swapChain = new SwapChain1(dxgiFactory2, dxgiDevice2, comWindow, ref desc);
                    }
                    else
                    {
                        _swapChainPanel = PresentationParameters.SwapChainPanel;
                        using (var nativePanel = ComObject.As<SharpDX.DXGI.ISwapChainPanelNative>(PresentationParameters.SwapChainPanel))
                        {
                            _swapChain = new SwapChain1(dxgiFactory2, dxgiDevice2, ref desc, null);
                            nativePanel.SwapChain = _swapChain;

                            // update swapChain2.MatrixTransform on SizeChanged of SwapChainPanel
                            // sometimes window.SizeChanged and SwapChainPanel.SizeChanged are not synced
                            PresentationParameters.SwapChainPanel.SizeChanged += (sender, e) =>
                            {
                                try
                                {
                                    using (var swapChain2 = _swapChain.QueryInterface<SwapChain2>())
                                    {
                                        var inverseScale = new RawMatrix3x2();
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

            _swapChain.Rotation = SharpDX.DXGI.DisplayModeRotation.Identity;

            // Counter act the composition scale of the render target as 
            // we already handle this in the platform window code. 
            if (PresentationParameters.SwapChainPanel != null)
            {
                var asyncResult = PresentationParameters.SwapChainPanel.Dispatcher.RunIdleAsync((e) =>
                {
                    var inverseScale = new RawMatrix3x2();
                    inverseScale.M11 = (float)PresentationParameters.SwapChainPanel.ActualWidth  / PresentationParameters.BackBufferWidth;
                    inverseScale.M22 = (float)PresentationParameters.SwapChainPanel.ActualHeight / PresentationParameters.BackBufferHeight;
                    using (var swapChain2 = _swapChain.QueryInterface<SwapChain2>())
                        swapChain2.MatrixTransform = inverseScale;
                });
            }
#endif

            // Obtain the backbuffer for this window which will be the final 3D rendertarget.
            Point targetSize;
            using (var backBuffer = SharpDX.Direct3D11.Texture2D.FromSwapChain<SharpDX.Direct3D11.Texture2D>(_swapChain, 0))
            {
                // Create a view interface on the rendertarget to use on bind.
                _renderTargetView = new SharpDX.Direct3D11.RenderTargetView(D3DDevice, backBuffer);

                // Get the rendertarget dimensions for later.
                var backBufferDesc = backBuffer.Description;
                targetSize = new Point(backBufferDesc.Width, backBufferDesc.Height);
            }

            // Create the depth buffer if we need it.
            if (PresentationParameters.DepthStencilFormat != DepthFormat.None)
            {
                var depthFormat = GraphicsExtensions.ToDXFormat(PresentationParameters.DepthStencilFormat);

                // Allocate a 2-D surface as the depth/stencil buffer.
                using (var depthBuffer = new SharpDX.Direct3D11.Texture2D(D3DDevice, new SharpDX.Direct3D11.Texture2DDescription()
                    {
                        Format = depthFormat,
                        ArraySize = 1,
                        MipLevels = 1,
                        Width = targetSize.X,
                        Height = targetSize.Y,
                        SampleDescription = multisampleDesc,
                        Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                        BindFlags = SharpDX.Direct3D11.BindFlags.DepthStencil,
                    }))
                {
                    // Create a DepthStencil view on this surface to use on bind.
                    _depthStencilView = new SharpDX.Direct3D11.DepthStencilView(D3DDevice, depthBuffer);
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
            var bitmapProperties = new SharpDX.Direct2D1.BitmapProperties1(
                new SharpDX.Direct2D1.PixelFormat(format, SharpDX.Direct2D1.AlphaMode.Premultiplied),
                _dpi, _dpi,
                SharpDX.Direct2D1.BitmapOptions.Target | SharpDX.Direct2D1.BitmapOptions.CannotDraw);

            // Direct2D needs the dxgi version of the backbuffer surface pointer.
            // Get a D2D surface from the DXGI back buffer to use as the D2D render target.
            using (var dxgiBackBuffer = _swapChain.GetBackBuffer<SharpDX.DXGI.Surface>(0))
                _bitmapTarget = new SharpDX.Direct2D1.Bitmap1(_d2dContext, dxgiBackBuffer, bitmapProperties);

            // So now we can set the Direct2D render target.
            _d2dContext.Target = _bitmapTarget;

            // Set D2D text anti-alias mode to Grayscale to 
            // ensure proper rendering of text on intermediate surfaces.
            _d2dContext.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Grayscale;
#endif
        }

        internal void OnPresentationChanged()
        {
            CreateSizeDependentResources();
            ApplyRenderTargets(null);
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
            Output output = null;
            if (_swapChain == null)
            {
                // get the primary output
                using (var factory = new SharpDX.DXGI.Factory1())
                using (var adapter = factory.GetAdapter1(0))
                    output = adapter.Outputs[0];
            }
            else
            {
                try
                {
                    output = _swapChain.ContainingOutput;
                }
                catch (SharpDXException) { /* ContainingOutput fails on a headless device */ }
            }

            var format = GraphicsExtensions.ToDXFormat(PresentationParameters.BackBufferFormat);
            var target = new ModeDescription
            {
                Format = format,
                Scaling = DisplayModeScaling.Unspecified,
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
                ModeDescription closest;
                output.GetClosestMatchingMode(D3DDevice, target, out closest);
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
            var format = GraphicsExtensions.ToDXFormat(PresentationParameters.BackBufferFormat);
            var descr = new ModeDescription
            {
                Format = format,
                Scaling = DisplayModeScaling.Unspecified,
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

            Output output = null;
            try
            {
                output = _swapChain.ContainingOutput;
            }
            catch (SharpDXException) { /* ContainingOutput fails on a headless device */ }

            if (output != null)
            {
                foreach (var adapter in GraphicsAdapter.Adapters)
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
        private void SetMultiSamplingToMaximum(PresentationParameters presentationParameters, out int quality)
        {
            quality = (int)SharpDX.Direct3D11.StandardMultisampleQualityLevels.StandardMultisamplePattern;
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
        private int GetMultiSamplingQuality(Format format, int multiSampleCount)
        {
            // The valid range is between zero and one less than the level returned by CheckMultisampleQualityLevels
            // https://msdn.microsoft.com/en-us/library/windows/desktop/bb173072(v=vs.85).aspx
            var quality = D3DDevice.CheckMultisampleQualityLevels(format, multiSampleCount) - 1;
            // NOTE: should we always return highest quality?
            return Math.Max(quality, 0); // clamp minimum to 0 
        }

        internal SampleDescription GetSupportedSampleDescription(Format format, int multiSampleCount)
        {
            var multisampleDesc = new SharpDX.DXGI.SampleDescription(1, 0);

            if (multiSampleCount > 1)
            {
                var quality = GetMultiSamplingQuality(format, multiSampleCount);

                multisampleDesc.Count = multiSampleCount;
                multisampleDesc.Quality = quality;
            }

            return multisampleDesc;
        }

        private void PlatformClear(ClearOptions options, Vector4 color, float depth, int stencil)
        {
            // Clear options for depth/stencil buffer if not attached.
            if (_currentDepthStencilView != null)
            {
                if (_currentDepthStencilView.Description.Format != SharpDX.DXGI.Format.D24_UNorm_S8_UInt)
                    options &= ~ClearOptions.Stencil;
            }
            else
            {
                options &= ~ClearOptions.DepthBuffer;
                options &= ~ClearOptions.Stencil;
            }

            lock (CurrentD3DContext)
            {
                // Clear the diffuse render buffer.
                if ((options & ClearOptions.Target) == ClearOptions.Target)
                {
                    foreach (var view in _currentRenderTargets)
                    {
                        if (view != null)
                            CurrentD3DContext.ClearRenderTargetView(view, new RawColor4(color.X, color.Y, color.Z, color.W));
                    }
                }

                // Clear the depth/stencil render buffer.
                SharpDX.Direct3D11.DepthStencilClearFlags flags = 0;
                if ((options & ClearOptions.DepthBuffer) == ClearOptions.DepthBuffer)
                    flags |= SharpDX.Direct3D11.DepthStencilClearFlags.Depth;
                if ((options & ClearOptions.Stencil) == ClearOptions.Stencil)
                    flags |= SharpDX.Direct3D11.DepthStencilClearFlags.Stencil;

                if (flags != 0)
                    CurrentD3DContext.ClearDepthStencilView(_currentDepthStencilView, flags, depth, (byte)stencil);
            }
        }

        private void PlatformDispose()
        {
            // make sure to release full screen or this might cause issues on exit
            if (_swapChain != null && _swapChain.IsFullScreen)
                _swapChain.SetFullscreenState(false, null);

            SharpDX.Utilities.Dispose(ref _renderTargetView);
            SharpDX.Utilities.Dispose(ref _depthStencilView);

            if (_userIndexBuffer16 != null)
                _userIndexBuffer16.Dispose();
            if (_userIndexBuffer32 != null)
                _userIndexBuffer32.Dispose();

            foreach (var vb in _userVertexBuffers.Values)
                vb.Dispose();

            SharpDX.Utilities.Dispose(ref _swapChain);

#if WINDOWS_UAP

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

            SharpDX.Utilities.Dispose(ref _d3dContext);
            SharpDX.Utilities.Dispose(ref _d3dDevice);
        }

        private void PlatformPresent()
        {
            try
            {
                lock (CurrentD3DContext)
                {
                    var syncInterval = 0;
                    var presentFlags = PresentFlags.None;

#if WINDOWS
                    // The first argument instructs DXGI to block n VSyncs before presenting.
                    syncInterval = GraphicsExtensions.ToDXSwapInterval(PresentationParameters.PresentationInterval);
#endif

#if WINDOWS_UAP
                    // The first argument instructs DXGI to block until VSync, putting the application
                    // to sleep until the next VSync. This ensures we don't waste any cycles rendering
                    // frames that will never be displayed to the screen.
                    if (_isTearingSupported && PresentationParameters.PresentationInterval == PresentInterval.Immediate)
                    {
                        syncInterval = 0;
                        presentFlags = PresentFlags.AllowTearing;
                    }
                    else
                    {
                        syncInterval = 1;
                    }
#endif

                    _swapChain.Present(syncInterval, presentFlags);
                }
            }
            catch (SharpDX.SharpDXException ex)
            {
                // TODO: How should we deal with a device lost case here?

#if WINDOWS_UAP
                /*               
                // If the device was removed either by a disconnect or a driver upgrade, we 
                // must completely reinitialize the renderer.
                if (    ex.ResultCode == SharpDX.DXGI.DXGIError.DeviceRemoved ||
                        ex.ResultCode == SharpDX.DXGI.DXGIError.DeviceReset)
                    this.Initialize();
                else
                    throw;
                */
#endif
            }
        }

        private void PlatformSetViewport(ref Viewport value)
        {
            if (CurrentD3DContext != null)
            {
				var viewport = new RawViewportF
				{
					X = _viewport.X,
					Y = _viewport.Y,
					Width = (float)_viewport.Width,
					Height = (float)_viewport.Height,
					MinDepth = _viewport.MinDepth,
					MaxDepth = _viewport.MaxDepth
				};
                lock (CurrentD3DContext)
                    CurrentD3DContext.Rasterizer.SetViewport(viewport);
            }
        }

#if WINDOWS_UAP
        internal void ResetRenderTargets()
        {
            if (CurrentD3DContext != null)
            {
                lock (CurrentD3DContext)
                {
					var viewport = new RawViewportF
					{
						X = _viewport.X,
						Y = _viewport.Y,
						Width = _viewport.Width,
						Height = _viewport.Height,
						MinDepth = _viewport.MinDepth,
						MaxDepth = _viewport.MaxDepth
					};
                    CurrentD3DContext.Rasterizer.SetViewport(viewport);
                    CurrentD3DContext.OutputMerger.SetTargets(_currentDepthStencilView, _currentRenderTargets);
                }
            }

            Textures.Dirty();
            SamplerStates.Dirty();
            _depthStencilStateDirty = true;
            _blendStateDirty = true;
            _indexBufferDirty = true;
            _vertexBuffersDirty = true;
            _pixelShaderDirty = true;
            _vertexShaderDirty = true;
            _rasterizerStateDirty = true;
            _scissorRectangleDirty = true;            
        }
#endif

        // Only implemented for DirectX right now, so not in GraphicsDevice.cs
        public void SetRenderTarget(RenderTarget2D renderTarget, int arraySlice)
        {
            if (!GraphicsCapabilities.SupportsTextureArrays)
                throw new InvalidOperationException("Texture arrays are not supported on this graphics device");

            if (renderTarget == null)
                SetRenderTarget(null);
            else
            {
                _tempRenderTargetBinding[0] = new RenderTargetBinding(renderTarget, arraySlice);
                SetRenderTargets(_tempRenderTargetBinding);
            }
        }

        // Only implemented for DirectX right now, so not in GraphicsDevice.cs
        public void SetRenderTarget(RenderTarget3D renderTarget, int arraySlice)
        {
            if (renderTarget == null)
                SetRenderTarget(null);
            else
            {
                _tempRenderTargetBinding[0] = new RenderTargetBinding(renderTarget, arraySlice);
                SetRenderTargets(_tempRenderTargetBinding);
            }
        }

        private void PlatformApplyDefaultRenderTarget()
        {
            // Set the default swap chain.
            Array.Clear(_currentRenderTargets, 0, _currentRenderTargets.Length);
            _currentRenderTargets[0] = _renderTargetView;
            _currentDepthStencilView = _depthStencilView;

            lock (CurrentD3DContext)
                CurrentD3DContext.OutputMerger.SetTargets(_currentDepthStencilView, _currentRenderTargets);
        }

        internal void PlatformResolveRenderTargets()
        {
            for (var i = 0; i < _currentRenderTargetCount; i++)
            {
                var renderTargetBinding = _currentRenderTargetBindings[i];

                // Resolve MSAA render targets
                var renderTarget = renderTargetBinding.RenderTarget as RenderTarget2D;
                if (renderTarget != null && renderTarget.MultiSampleCount > 1)
                    renderTarget.ResolveSubresource();

                // Generate mipmaps.
                if (renderTargetBinding.RenderTarget.LevelCount > 1)
                {
                    lock (CurrentD3DContext)
                        CurrentD3DContext.GenerateMips(renderTargetBinding.RenderTarget.GetShaderResourceView());
                }
            }
        }

        private IRenderTarget PlatformApplyRenderTargets()
        {
            // Clear the current render targets.
            Array.Clear(_currentRenderTargets, 0, _currentRenderTargets.Length);
            _currentDepthStencilView = null;

            // Make sure none of the new targets are bound
            // to the device as a texture resource.
            lock (CurrentD3DContext)
            {
                VertexTextures.ClearTargets(this, _currentRenderTargetBindings);
                Textures.ClearTargets(this, _currentRenderTargetBindings);
            }

            for (var i = 0; i < _currentRenderTargetCount; i++)
            {
                var binding = _currentRenderTargetBindings[i];
                var targetDX = (IRenderTargetDX11)binding.RenderTarget;
                _currentRenderTargets[i] = targetDX.GetRenderTargetView(binding.ArraySlice);
            }

            // Use the depth from the first target.
            var renderTargetDX = (IRenderTargetDX11)_currentRenderTargetBindings[0].RenderTarget;
            _currentDepthStencilView = renderTargetDX.GetDepthStencilView(_currentRenderTargetBindings[0].ArraySlice);

            // Set the targets.
            lock (CurrentD3DContext)
                CurrentD3DContext.OutputMerger.SetTargets(_currentDepthStencilView, _currentRenderTargets);

            return (IRenderTarget)_currentRenderTargetBindings[0].RenderTarget;
        }

        private static PrimitiveTopology ToPrimitiveTopology(PrimitiveType primitiveType)
        {
            switch (primitiveType)
            {
                case PrimitiveType.LineList:
                    return PrimitiveTopology.LineList;
                case PrimitiveType.LineStrip:
                    return PrimitiveTopology.LineStrip;
                case PrimitiveType.TriangleList:
                    return PrimitiveTopology.TriangleList;
                case PrimitiveType.TriangleStrip:
                    return PrimitiveTopology.TriangleStrip;
                case PrimitiveType.PointList:
                    return PrimitiveTopology.PointList;
                default:
                    throw new ArgumentException();
            }
        }

        private void PlatformApplyState()
        {
            Debug.Assert(CurrentD3DContext != null, "The d3d context is null!");

            if (_blendFactorDirty || _blendStateDirty)
            {
                PlatformApplyBlend();
                _blendFactorDirty = false;
                _blendStateDirty = false;
            }

            if (_depthStencilStateDirty)
            {
                _actualDepthStencilState.PlatformApplyState(this);
                _depthStencilStateDirty = false;
            }

            if (_rasterizerStateDirty)
            {
                _actualRasterizerState.PlatformApplyState(this);
                _rasterizerStateDirty = false;
            }

            if (_scissorRectangleDirty)
            {
                PlatformApplyScissorRectangle();
                _scissorRectangleDirty = false;
            }
        }

        private void PlatformApplyBlend()
        {
            var state = _actualBlendState.GetDxState(this);
            var factor = ToDXColor(BlendFactor);
            CurrentD3DContext.OutputMerger.SetBlendState(state, factor);
        }

        private SharpDX.Mathematics.Interop.RawColor4 ToDXColor(Color blendFactor)
        {
			return new SharpDX.Mathematics.Interop.RawColor4(
                    blendFactor.R / 255.0f,
                    blendFactor.G / 255.0f,
                    blendFactor.B / 255.0f,
                    blendFactor.A / 255.0f);
        }

        private void PlatformApplyScissorRectangle()
        {
            // NOTE: This code assumes CurrentD3DContext has been locked by the caller.

            CurrentD3DContext.Rasterizer.SetScissorRectangle(
                _scissorRectangle.X,
                _scissorRectangle.Y,
                _scissorRectangle.Right,
                _scissorRectangle.Bottom);
            _scissorRectangleDirty = false;
        }

        private void PlatformApplyIndexBuffer()
        {
            // NOTE: This code assumes CurrentD3DContext has been locked by the caller.

            if (_indexBufferDirty)
            {
                if (_indexBuffer != null)
                {
                    CurrentD3DContext.InputAssembler.SetIndexBuffer(
                        _indexBuffer.Buffer,
                        _indexBuffer.IndexElementSize == IndexElementSize.SixteenBits ?
                            SharpDX.DXGI.Format.R16_UInt : SharpDX.DXGI.Format.R32_UInt,
                        0);
                }
                _indexBufferDirty = false;
            }
        }

        private void PlatformApplyVertexBuffers()
        {
            // NOTE: This code assumes CurrentD3DContext has been locked by the caller.

            if (_vertexBuffersDirty)
            {
                if (_vertexBuffers.Count > 0)
                {
                    for (int slot = 0; slot < _vertexBuffers.Count; slot++)
                    {
                        var vertexBufferBinding = _vertexBuffers.Get(slot);
                        var vertexBuffer = vertexBufferBinding.VertexBuffer;
                        var vertexDeclaration = vertexBuffer.VertexDeclaration;
                        int vertexStride = vertexDeclaration.VertexStride;
                        int vertexOffsetInBytes = vertexBufferBinding.VertexOffset * vertexStride;
                        CurrentD3DContext.InputAssembler.SetVertexBuffers(
                            slot, new SharpDX.Direct3D11.VertexBufferBinding(vertexBuffer.Buffer, vertexStride, vertexOffsetInBytes));
                    }
                    _vertexBufferSlotsUsed = _vertexBuffers.Count;
                }
                else
                {
                    for (int slot = 0; slot < _vertexBufferSlotsUsed; slot++)
                        CurrentD3DContext.InputAssembler.SetVertexBuffers(slot, new SharpDX.Direct3D11.VertexBufferBinding());

                    _vertexBufferSlotsUsed = 0;
                }
            }
        }

        private void PlatformApplyShaders()
        {
            // NOTE: This code assumes CurrentD3DContext has been locked by the caller.

            if (_vertexShader == null)
                throw new InvalidOperationException("A vertex shader must be set!");
            if (_pixelShader == null)
                throw new InvalidOperationException("A pixel shader must be set!");

            if (_vertexShaderDirty)
            {
                CurrentD3DContext.VertexShader.Set(_vertexShader.VertexShader);

                unchecked { _graphicsMetrics._vertexShaderCount++; }
            }
            if (_vertexShaderDirty || _vertexBuffersDirty)
            {
                CurrentD3DContext.InputAssembler.InputLayout = _vertexShader.InputLayouts.GetOrCreate(_vertexBuffers);
                _vertexShaderDirty = _vertexBuffersDirty = false;
            }

            if (_pixelShaderDirty)
            {
                CurrentD3DContext.PixelShader.Set(_pixelShader.PixelShader);
                _pixelShaderDirty = false;

                unchecked { _graphicsMetrics._pixelShaderCount++; }
            }

            _vertexConstantBuffers.SetConstantBuffers();
            _pixelConstantBuffers.SetConstantBuffers();

            VertexTextures.SetTextures(this);
            VertexSamplerStates.PlatformSetSamplers(this);
            Textures.SetTextures(this);
            SamplerStates.PlatformSetSamplers(this);
        }

        private int SetUserVertexBuffer<T>(T[] vertexData, int vertexOffset, int vertexCount, VertexDeclaration vertexDecl)
            where T : struct
        {
            DynamicVertexBuffer buffer;

            if (!_userVertexBuffers.TryGetValue(vertexDecl, out buffer) || buffer.VertexCount < vertexCount)
            {
                // Dispose the previous buffer if we have one.
                if (buffer != null)
                    buffer.Dispose();

                var requiredVertexCount = Math.Max(vertexCount, 4 * 256);
                requiredVertexCount = (requiredVertexCount + 255) & (~255); // grow in chunks of 256.
                buffer = new DynamicVertexBuffer(this, vertexDecl, requiredVertexCount, BufferUsage.WriteOnly);
                _userVertexBuffers[vertexDecl] = buffer;
            }

            var startVertex = buffer.UserOffset;


            if ((vertexCount + buffer.UserOffset) < buffer.VertexCount)
            {
                buffer.UserOffset += vertexCount;
                buffer.SetData(startVertex * vertexDecl.VertexStride, vertexData, vertexOffset, vertexCount, vertexDecl.VertexStride, SetDataOptions.NoOverwrite);
            }
            else
            {
                buffer.UserOffset = vertexCount;
                buffer.SetData(vertexData, vertexOffset, vertexCount, SetDataOptions.Discard);
                startVertex = 0;
            }

            SetVertexBuffer(buffer);

            return startVertex;
        }

        private int SetUserIndexBuffer<T>(T[] indexData, int indexOffset, int indexCount)
            where T : struct
        {
            DynamicIndexBuffer buffer;

            var indexSize = ReflectionHelpers.SizeOf<T>();
            var indexElementSize = indexSize == 2 ? IndexElementSize.SixteenBits : IndexElementSize.ThirtyTwoBits;

            var requiredIndexCount = Math.Max(indexCount, 6 * 512);
            requiredIndexCount = (requiredIndexCount + 511) & (~511); // grow in chunks of 512.
            if (indexElementSize == IndexElementSize.SixteenBits)
            {
                if (_userIndexBuffer16 == null || _userIndexBuffer16.IndexCount < requiredIndexCount)
                {
                    if (_userIndexBuffer16 != null)
                        _userIndexBuffer16.Dispose();

                    _userIndexBuffer16 = new DynamicIndexBuffer(this, indexElementSize, requiredIndexCount, BufferUsage.WriteOnly);
                }

                buffer = _userIndexBuffer16;
            }
            else
            {
                if (_userIndexBuffer32 == null || _userIndexBuffer32.IndexCount < requiredIndexCount)
                {
                    if (_userIndexBuffer32 != null)
                        _userIndexBuffer32.Dispose();

                    _userIndexBuffer32 = new DynamicIndexBuffer(this, indexElementSize, requiredIndexCount, BufferUsage.WriteOnly);
                }

                buffer = _userIndexBuffer32;                
            }

            var startIndex = buffer.UserOffset;

            if ((indexCount + buffer.UserOffset) < buffer.IndexCount)
            {
                buffer.UserOffset += indexCount;
                buffer.SetData(startIndex * indexSize, indexData, indexOffset, indexCount, SetDataOptions.NoOverwrite);
            }
            else
            {
                startIndex = 0;
                buffer.UserOffset = indexCount;
                buffer.SetData(indexData, indexOffset, indexCount, SetDataOptions.Discard);
            }

            Indices = buffer;

            return startIndex;
        }

        private void PlatformApplyPrimitiveType(PrimitiveType primitiveType)
        {
            if (_lastPrimitiveType == primitiveType)
                return;

            CurrentD3DContext.InputAssembler.PrimitiveTopology = ToPrimitiveTopology(primitiveType);
            _lastPrimitiveType = primitiveType;
        }

        private void PlatformDrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount)
        {
            lock (CurrentD3DContext)
            {
                PlatformApplyState();
                PlatformApplyIndexBuffer();
                PlatformApplyVertexBuffers();
                PlatformApplyShaders();

                PlatformApplyPrimitiveType(primitiveType);
                var indexCount = GetElementCountArray(primitiveType, primitiveCount);
                CurrentD3DContext.DrawIndexed(indexCount, startIndex, baseVertex);
            }
        }

        private void PlatformDrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, VertexDeclaration vertexDeclaration, int vertexCount) where T : struct
        {
            // TODO: Do not set public VertexBuffers.
            //       Bind directly to d3dContext and set dirty flags.
            var startVertex = SetUserVertexBuffer(vertexData, vertexOffset, vertexCount, vertexDeclaration);

            lock (CurrentD3DContext)
            {
                PlatformApplyState();
                //PlatformApplyIndexBuffer();
                PlatformApplyVertexBuffers(); // SetUserVertexBuffer() overwrites the vertexBuffer
                PlatformApplyShaders();

                PlatformApplyPrimitiveType(primitiveType);
                CurrentD3DContext.Draw(vertexCount, startVertex);
            }
        }

        private void PlatformDrawPrimitives(PrimitiveType primitiveType, int vertexStart, int vertexCount)
        {
            lock (CurrentD3DContext)
            {
                PlatformApplyState();
                //PlatformApplyIndexBuffer();
                PlatformApplyVertexBuffers();
                PlatformApplyShaders();

                PlatformApplyPrimitiveType(primitiveType);
                CurrentD3DContext.Draw(vertexCount, vertexStart);
            }
        }

        private void PlatformDrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
        {
            // TODO: Do not set public VertexBuffers and Indices.
            //       Bind directly to d3dContext and set dirty flags.
            var indexCount = GetElementCountArray(primitiveType, primitiveCount);
            var startVertex = SetUserVertexBuffer(vertexData, vertexOffset, numVertices, vertexDeclaration);
            var startIndex = SetUserIndexBuffer(indexData, indexOffset, indexCount);

            lock (CurrentD3DContext)
            {
                PlatformApplyState();
                PlatformApplyIndexBuffer(); // SetUserIndexBuffer() overwrites the indexbuffer
                PlatformApplyVertexBuffers(); // SetUserVertexBuffer() overwrites the vertexBuffer
                PlatformApplyShaders();

                PlatformApplyPrimitiveType(primitiveType);
                CurrentD3DContext.DrawIndexed(indexCount, startIndex, startVertex);
            }
        }

        private void PlatformDrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, int[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
        {
            // TODO: Do not set public VertexBuffers and Indices.
            //       Bind directly to d3dContext and set dirty flags.
            var indexCount = GetElementCountArray(primitiveType, primitiveCount);
            var startVertex = SetUserVertexBuffer(vertexData, vertexOffset, numVertices, vertexDeclaration);
            var startIndex = SetUserIndexBuffer(indexData, indexOffset, indexCount);

            lock (CurrentD3DContext)
            {
                PlatformApplyState();
                PlatformApplyIndexBuffer(); // SetUserIndexBuffer() overwrites the indexbuffer
                PlatformApplyVertexBuffers(); // SetUserVertexBuffer() overwrites the vertexBuffer
                PlatformApplyShaders();

                PlatformApplyPrimitiveType(primitiveType);
                CurrentD3DContext.DrawIndexed(indexCount, startIndex, startVertex);
            }
        }

        private void PlatformDrawInstancedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex,
            int primitiveCount, int baseInstance, int instanceCount)
        {
            lock (CurrentD3DContext)
            {
                PlatformApplyState();
                PlatformApplyIndexBuffer();
                PlatformApplyVertexBuffers();
                PlatformApplyShaders();

                PlatformApplyPrimitiveType(primitiveType);
                int indexCount = GetElementCountArray(primitiveType, primitiveCount);

                if (baseInstance > 0)
                {
                    if (!GraphicsCapabilities.SupportsBaseIndexInstancing)
                        throw new PlatformNotSupportedException("Instanced geometry drawing with base instance not supported.");

                    CurrentD3DContext.DrawIndexedInstanced(indexCount, instanceCount, startIndex, baseVertex, baseInstance);
                }
                else
                {
                    CurrentD3DContext.DrawIndexedInstanced(indexCount, instanceCount, startIndex, baseVertex, 0);
                }
            }
        }

        private void PlatformGetBackBufferData<T>(Rectangle? rect, T[] data, int startIndex, int count) where T : struct
        {
            // TODO share code with Texture2D.GetData and do pooling for staging textures
            // first set up a staging texture
            const SurfaceFormat format = SurfaceFormat.Color;
            //You can't Map the BackBuffer surface, so we copy to another texture
            using (var backBufferTexture = SharpDX.Direct3D11.Resource.FromSwapChain<SharpDX.Direct3D11.Texture2D>(_swapChain, 0))
            {
                var desc = backBufferTexture.Description;
                desc.SampleDescription = new SampleDescription(1, 0);
                desc.BindFlags = BindFlags.None;
                desc.CpuAccessFlags = CpuAccessFlags.Read;
                desc.Usage = ResourceUsage.Staging;
                desc.OptionFlags = ResourceOptionFlags.None;

                using (var stagingTex = new SharpDX.Direct3D11.Texture2D(D3DDevice, desc))
                {
                    lock (CurrentD3DContext)
                    {
                        // Copy the data from the GPU to the staging texture.
                        // if MSAA is enabled we need to first copy to a resource without MSAA
                        if (backBufferTexture.Description.SampleDescription.Count > 1)
                        {
                            desc.Usage = ResourceUsage.Default;
                            desc.CpuAccessFlags = CpuAccessFlags.None;
                            using (var noMsTex = new SharpDX.Direct3D11.Texture2D(D3DDevice, desc))
                            {
                                CurrentD3DContext.ResolveSubresource(backBufferTexture, 0, noMsTex, 0, desc.Format);
                                if (rect.HasValue)
                                {
                                    var r = rect.Value;
                                    CurrentD3DContext.CopySubresourceRegion(noMsTex, 0,
                                        new ResourceRegion(r.Left, r.Top, 0, r.Right, r.Bottom, 1), stagingTex,
                                        0);
                                }
                                else
                                    CurrentD3DContext.CopyResource(noMsTex, stagingTex);
                            }
                        }
                        else
                        {
                            if (rect.HasValue)
                            {
                                var r = rect.Value;
                                CurrentD3DContext.CopySubresourceRegion(backBufferTexture, 0,
                                    new ResourceRegion(r.Left, r.Top, 0, r.Right, r.Bottom, 1), stagingTex, 0);
                            }
                            else
                                CurrentD3DContext.CopyResource(backBufferTexture, stagingTex);
                        }

                        // Copy the data to the array.
                        DataStream stream = null;
                        try
                        {
                            var databox = CurrentD3DContext.MapSubresource(stagingTex, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None, out stream);

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
                            var elementSize = format.GetSize();
                            var rowSize = elementSize * elementsInRow;
                            if (rowSize == databox.RowPitch)
                                stream.ReadRange(data, startIndex, count);
                            else
                            {
                                // Some drivers may add pitch to rows.
                                // We need to copy each row separately and skip trailing zeroes.
                                stream.Seek(0, SeekOrigin.Begin);

                                var elementSizeInByte = ReflectionHelpers.SizeOf<T>();
                                for (var row = 0; row < rows; row++)
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
            CurrentD3DContext.Flush();
        }

        private static Rectangle PlatformGetTitleSafeArea(int x, int y, int width, int height)
        {
            return new Rectangle(x, y, width, height);
        }

#if WINDOWS_UAP

        internal void Trim()
        {
            using (var dxgiDevice3 = D3DDevice.QueryInterface<SharpDX.DXGI.Device3>())
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
                _d2dContext.DotsPerInch = new Size2F(_dpi, _dpi);

                //if (OnDpiChanged != null)
                //    OnDpiChanged(this);
            }
        }

        private bool IsTearingSupported()
        {
            RawBool allowTearing;
            using (var dxgiFactory2 = new Factory2())
            {
                unsafe
                {
                    var factory5 = dxgiFactory2.QueryInterface<Factory5>();
                    try
                    {
                        factory5.CheckFeatureSupport(SharpDX.DXGI.Feature.PresentAllowTearing, new IntPtr(&allowTearing), sizeof(RawBool));

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
