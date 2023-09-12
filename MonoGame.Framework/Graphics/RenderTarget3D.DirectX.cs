// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Platform.Graphics;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTarget3D
    {
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (((ConcreteRenderTarget3D)_strategyRenderTarget3D)._renderTargetViews != null)
                {
                    for (int i = 0; i < ((ConcreteRenderTarget3D)_strategyRenderTarget3D)._renderTargetViews.Length; i++)
                        DX.Utilities.Dispose(ref ((ConcreteRenderTarget3D)_strategyRenderTarget3D)._renderTargetViews[i]);
                    ((ConcreteRenderTarget3D)_strategyRenderTarget3D)._renderTargetViews = null;
                }
                if (((ConcreteRenderTarget3D)_strategyRenderTarget3D)._depthStencilViews != null)
                {
                    for (int i = 0; i < ((ConcreteRenderTarget3D)_strategyRenderTarget3D)._depthStencilViews.Length; i++)
                        DX.Utilities.Dispose(ref ((ConcreteRenderTarget3D)_strategyRenderTarget3D)._depthStencilViews[i]);
                    ((ConcreteRenderTarget3D)_strategyRenderTarget3D)._depthStencilViews = null;
                }                
            }

            base.Dispose(disposing);
        }

    }
}
