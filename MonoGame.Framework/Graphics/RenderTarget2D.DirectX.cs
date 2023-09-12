// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using Microsoft.Xna.Platform.Graphics;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTarget2D
    {

        private void PlatformGraphicsDeviceResetting()
        {
            if (((ConcreteRenderTarget2D)_strategyRenderTarget2D)._renderTargetViews != null)
            {
                for (int i = 0; i < ((ConcreteRenderTarget2D)_strategyRenderTarget2D)._renderTargetViews.Length; i++)
                    ((ConcreteRenderTarget2D)_strategyRenderTarget2D)._renderTargetViews[i].Dispose();
                ((ConcreteRenderTarget2D)_strategyRenderTarget2D)._renderTargetViews = null;
            }
            if (((ConcreteRenderTarget2D)_strategyRenderTarget2D)._depthStencilViews != null)
            {
                for (int i = 0; i < ((ConcreteRenderTarget2D)_strategyRenderTarget2D)._depthStencilViews.Length; i++)
                    DX.Utilities.Dispose(ref ((ConcreteRenderTarget2D)_strategyRenderTarget2D)._depthStencilViews[i]);
                ((ConcreteRenderTarget2D)_strategyRenderTarget2D)._depthStencilViews = null;
            }
        }

        internal virtual void ResolveSubresource()
        {
            lock (GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext)
            {
                D3D11.DeviceContext d3dContext = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                System.Diagnostics.Debug.Assert(((ConcreteRenderTarget2D)_strategyRenderTarget2D)._msTexture != null);

                d3dContext.ResolveSubresource(
                    ((ConcreteRenderTarget2D)_strategyRenderTarget2D)._msTexture,
                    0,
                    this.GetTextureStrategy<ConcreteTexture>().GetTexture(),
                    0,
                    GraphicsExtensions.ToDXFormat(this.Format));
            }
        }

    }
}
