// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteTextureCube : ConcreteTexture, ITextureCubeStrategy
    {
        internal ConcreteTextureCube(GraphicsContextStrategy contextStrategy, int size, bool mipMap, SurfaceFormat format)
            : base(contextStrategy, format, Texture.CalculateMipLevels(mipMap, size))
        {

        }


        #region ITextureCubeStrategy
        public int Size
        {
            get { throw new PlatformNotSupportedException(); }
        }
        public void SetData<T>(CubeMapFace face, int level, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            throw new PlatformNotSupportedException();
        }

        public void GetData<T>(CubeMapFace face, int level, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            throw new PlatformNotSupportedException();
        }
        #endregion #region ITextureCubeStrategy

    }
}
