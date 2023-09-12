// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Platform.Graphics;
using nkast.Wasm.Canvas.WebGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTarget2D
    {
        private void PlatformGraphicsDeviceResetting()
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }

            ConcreteTexture.PlatformDeleteRenderTarget((IRenderTargetStrategyGL)this._strategyRenderTarget2D, GraphicsDevice.Strategy);

            base.Dispose(disposing);
        }
    }
}
