// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Platform.Graphics;
using nkast.Wasm.Canvas.WebGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTarget3D
    {
        private void PlatformConstructTexture3D_rt(GraphicsContextStrategy contextStrategy, int width, int height, int depth, bool mipMap, SurfaceFormat format)
        {
            ((ConcreteTexture3D)_strategyTexture3D).PlatformConstructTexture3D(contextStrategy, width, height, depth, mipMap, format);
        }

        private void PlatformConstructRenderTarget3D(GraphicsContextStrategy contextStrategy, int width, int height, int depth, bool mipMap,
            DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
        {
            ((ConcreteRenderTarget3D)_strategyRenderTarget3D)._multiSampleCount = contextStrategy.Context.DeviceStrategy.GetClampedMultiSampleCount(this.Format, preferredMultiSampleCount);

            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                
            }

            ConcreteTexture.PlatformDeleteRenderTarget((IRenderTargetStrategyGL)this._strategyRenderTarget3D, GraphicsDevice.Strategy);

            base.Dispose(disposing);
        }

    }
}
