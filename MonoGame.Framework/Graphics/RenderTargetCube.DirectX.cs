// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;
using DX = SharpDX;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTargetCube
    {
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (((ConcreteRenderTargetCube)_strategyRenderTargetCube)._renderTargetViews != null)
                {
                    for (int i = 0; i < ((ConcreteRenderTargetCube)_strategyRenderTargetCube)._renderTargetViews.Length; i++)
                        ((ConcreteRenderTargetCube)_strategyRenderTargetCube)._renderTargetViews[i].Dispose();
                    ((ConcreteRenderTargetCube)_strategyRenderTargetCube)._renderTargetViews = null;
                }

                if (((ConcreteRenderTargetCube)_strategyRenderTargetCube)._depthStencilViews != null)
                {
                    for (int i = 0; i < ((ConcreteRenderTargetCube)_strategyRenderTargetCube)._depthStencilViews.Length; i++)
                        if (((ConcreteRenderTargetCube)_strategyRenderTargetCube)._depthStencilViews[i] != null)
                            ((ConcreteRenderTargetCube)_strategyRenderTargetCube)._depthStencilViews[i].Dispose();
                    ((ConcreteRenderTargetCube)_strategyRenderTargetCube)._depthStencilViews = null;
                }

                DX.Utilities.Dispose(ref ((ConcreteRenderTargetCube)_strategyRenderTargetCube)._depthTarget);
            }

            base.Dispose(disposing);
        }
    }
}
