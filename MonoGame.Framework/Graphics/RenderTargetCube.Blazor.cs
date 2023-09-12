// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Platform.Graphics;
using nkast.Wasm.Canvas.WebGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTargetCube
    {
        private void PlatformConstructRenderTargetCube(GraphicsContextStrategy contextStrategy, bool mipMap,
            DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
        {
            ((ConcreteRenderTargetCube)_strategyRenderTargetCube)._multiSampleCount = contextStrategy.Context.DeviceStrategy.GetClampedMultiSampleCount(this.Format, preferredMultiSampleCount);

            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }

            ConcreteTexture.PlatformDeleteRenderTarget((IRenderTargetStrategyGL)this._strategyRenderTargetCube, GraphicsDevice.Strategy);

            base.Dispose(disposing);
        }
    }
}
