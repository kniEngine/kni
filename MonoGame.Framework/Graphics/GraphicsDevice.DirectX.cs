// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;


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

        /// <summary>
        /// Sends queued-up commands in the command buffer to the graphics processing unit (GPU).
        /// </summary>
        public void Flush()
        {
            ((ConcreteGraphicsContext)CurrentContext.Strategy).D3dContext.Flush();
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


    }
}
