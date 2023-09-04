// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteRenderTarget2D : IRenderTarget2DStrategy, ITexture2DStrategy
    {
        private RenderTargetUsage _renderTargetUsage;

        internal ConcreteRenderTarget2D(GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap, int arraySize, RenderTargetUsage usage)
        {
            this._renderTargetUsage = usage;
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


        #region ITexture2DStrategy
        public int Width
        {
            get { throw new NotImplementedException(); }
        }

        public int Height
        {
            get { throw new NotImplementedException(); }
        }

        public int ArraySize
        {
            get { throw new NotImplementedException(); }
        }

        public Rectangle Bounds
        {
            get { throw new NotImplementedException(); }
        }
        #endregion #region ITexture2DStrategy


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
            get { return _renderTargetUsage; }
        }
        #endregion #region IRenderTarget2DStrategy


        internal D3D11.RenderTargetView[] _renderTargetViews;
        internal D3D11.DepthStencilView[] _depthStencilViews;

    }
}
