// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteRenderTarget3D : IRenderTarget3DStrategy, ITexture3DStrategy
    {
        internal ConcreteRenderTarget3D(GraphicsContextStrategy contextStrategy, int width, int height, int depth, bool mipMap)
        {

        }


        #region ITextureStrategy
        public SurfaceFormat Format
        {
            get { throw new NotImplementedException(); }
        }

        public int LevelCount
        {
            get { throw new NotImplementedException(); }
        }
        #endregion #region ITextureStrategy


        #region ITexture3DStrategy
        public int Width
        {
            get { throw new NotImplementedException(); }
        }

        public int Height
        {
            get { throw new NotImplementedException(); }
        }

        public int Depth
        {
            get { throw new NotImplementedException(); }
        }
        #endregion #region ITexture3DStrategy


        #region IRenderTargetStrategy
        public DepthFormat DepthStencilFormat
        {
            get { throw new NotImplementedException(); }
        }

        public int MultiSampleCount
        {
            get { throw new NotImplementedException(); }
        }

        public RenderTargetUsage RenderTargetUsage
        {
            get { throw new NotImplementedException(); }
        }
        #endregion #region IRenderTarget2DStrategy

    }
}
