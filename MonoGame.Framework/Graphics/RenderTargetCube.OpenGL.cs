// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;
using MonoGame.OpenGL;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTargetCube
    {
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (GraphicsDevice != null)
                {
                    ConcreteTexture.PlatformDeleteRenderTarget((IRenderTargetStrategyGL)this._strategyRenderTargetCube, GraphicsDevice.Strategy);
                }
            }

            base.Dispose(disposing);
        }
    }
}
