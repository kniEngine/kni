﻿// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteRenderTarget3D : IRenderTarget3DStrategy, ITexture3DStrategy, ITextureStrategy
    {
        internal ConcreteRenderTarget3D(GraphicsContextStrategy contextStrategy)
        {

        }


        #region ITextureStrategy
        public SurfaceFormat Format
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public int LevelCount
        {
            get { throw new PlatformNotSupportedException(); }
        }
        #endregion #region ITextureStrategy


        #region ITexture3DStrategy
        public int Width
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public int Height
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public int Depth
        {
            get { throw new PlatformNotSupportedException(); }
        }
        #endregion #region ITexture3DStrategy


        #region IRenderTargetStrategy
        public DepthFormat DepthStencilFormat
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public int MultiSampleCount
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public RenderTargetUsage RenderTargetUsage
        {
            get { throw new PlatformNotSupportedException(); }
        }
        #endregion #region IRenderTarget2DStrategy

    }
}
