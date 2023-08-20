// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Platform.Graphics;
using SharpDX;
using SharpDX.Direct3D;
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

#if WINDOWS_UAP
using Windows.Graphics.Display;
#endif


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class GraphicsDevice
    {
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
            ((ConcreteGraphicsDevice)_strategy).CreateDeviceIndependentResources();
			CreateDeviceResources();

            _strategy._capabilities = new GraphicsCapabilities();
            _strategy._capabilities.PlatformInitialize(this);

#if WINDOWS_UAP
            ((ConcreteGraphicsDevice)_strategy).Dpi = DisplayInformation.GetForCurrentView().LogicalDpi;
#endif
        }

        private void PlatformInitialize()
        {
#if WINDOWS
            ((ConcreteGraphicsDevice)_strategy).CorrectBackBufferSize();
#endif
            ((ConcreteGraphicsDevice)_strategy).CreateSizeDependentResources();
        }


        /// <summary>
        /// Create graphics device specific resources.
        /// </summary>
        protected virtual void CreateDeviceResources()
        {
            // Dispose previous references.
            if (((ConcreteGraphicsDevice)_strategy)._d3dDevice != null)
                ((ConcreteGraphicsDevice)_strategy)._d3dDevice.Dispose();
            if (_strategy._mainContext != null)
                _strategy._mainContext.Dispose();
            _strategy._mainContext = null;


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

            _strategy._mainContext = new GraphicsContext(this);


#if WINDOWS
            // Create a new instance of GraphicsDebug because we support it on Windows platforms.
            _strategy._mainContext.Strategy.GraphicsDebug = new GraphicsDebug(this);
#endif

#if WINDOWS_UAP
            // Create the Direct2D device.
            using (DXGI.Device dxgiDevice = ((ConcreteGraphicsDevice)_strategy)._d3dDevice.QueryInterface<DXGI.Device>())
                ((ConcreteGraphicsDevice)_strategy)._d2dDevice = new SharpDX.Direct2D1.Device(((ConcreteGraphicsDevice)_strategy)._d2dFactory, dxgiDevice);

            // Create Direct2D context
            ((ConcreteGraphicsDevice)_strategy)._d2dContext = new SharpDX.Direct2D1.DeviceContext(((ConcreteGraphicsDevice)_strategy)._d2dDevice, SharpDX.Direct2D1.DeviceContextOptions.None);
#endif
        }

        internal void OnPresentationChanged()
        {
            ((ConcreteGraphicsDevice)_strategy).CreateSizeDependentResources();
            _strategy._mainContext.ApplyRenderTargets(null);
        }


        private void PlatformDispose()
        {
            // make sure to release full screen or this might cause issues on exit
            if (((ConcreteGraphicsDevice)_strategy)._swapChain != null && ((ConcreteGraphicsDevice)_strategy)._swapChain.IsFullScreen)
                ((ConcreteGraphicsDevice)_strategy)._swapChain.SetFullscreenState(false, null);

            SharpDX.Utilities.Dispose(ref ((ConcreteGraphicsDevice)_strategy)._renderTargetView);
            SharpDX.Utilities.Dispose(ref ((ConcreteGraphicsDevice)_strategy)._depthStencilView);

            if (((ConcreteGraphicsContext)_strategy._mainContext.Strategy)._userIndexBuffer16 != null)
                ((ConcreteGraphicsContext)_strategy._mainContext.Strategy)._userIndexBuffer16.Dispose();
            if (((ConcreteGraphicsContext)_strategy._mainContext.Strategy)._userIndexBuffer32 != null)
                ((ConcreteGraphicsContext)_strategy._mainContext.Strategy)._userIndexBuffer32.Dispose();

            foreach (DynamicVertexBuffer vb in ((ConcreteGraphicsContext)_strategy._mainContext.Strategy)._userVertexBuffers.Values)
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

            if (_strategy._mainContext != null)
                _strategy._mainContext.Dispose();
            _strategy._mainContext = null;
            SharpDX.Utilities.Dispose(ref ((ConcreteGraphicsDevice)_strategy)._d3dDevice);
        }

        /// <summary>
        /// Sends queued-up commands in the command buffer to the graphics processing unit (GPU).
        /// </summary>
        public void Flush()
        {
            ((ConcreteGraphicsContext)CurrentContext.Strategy).D3dContext.Flush();
        }

    }
}
