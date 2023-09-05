// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteRenderTargetCube : IRenderTargetCubeStrategy, ITextureCubeStrategy
    {
        private RenderTargetUsage _renderTargetUsage;

        internal ConcreteRenderTargetCube(GraphicsContextStrategy contextStrategy, int size, bool mipMap, RenderTargetUsage usage)
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


        #region ITextureCubeStrategy
        public int Size
        {
            get { throw new NotImplementedException(); }
        }

        public void SetData<T>(CubeMapFace face, int level, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            throw new NotImplementedException();
        }

        public void GetData<T>(CubeMapFace face, int level, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            throw new NotImplementedException();
        }
        #endregion #region ITextureCubeStrategy


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

    }
}
