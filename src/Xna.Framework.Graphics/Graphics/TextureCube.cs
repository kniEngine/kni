// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;
using Microsoft.Xna.Platform.Graphics.Utilities;


namespace Microsoft.Xna.Framework.Graphics
{
    public class TextureCube : Texture
    {
        protected ITextureCubeStrategy _strategyTextureCube;

        /// <summary>
        /// Gets the width and height of the cube map face in pixels.
        /// </summary>
        /// <value>The width and height of a cube map face in pixels.</value>
        public int Size { get { return _strategyTextureCube.Size; } }
        
        public TextureCube(GraphicsDevice graphicsDevice, int size, bool mipMap, SurfaceFormat format)
            : this(graphicsDevice, size, mipMap, format, true)
        {
            _strategyTextureCube = ((IPlatformGraphicsContext)graphicsDevice.MainContext).Strategy.CreateTextureCubeStrategy(size, mipMap, format);
            _strategyTexture = _strategyTextureCube;
            SetResourceStrategy((IGraphicsResourceStrategy)_strategyTextureCube);
        }

        protected TextureCube(GraphicsDevice graphicsDevice, int size, bool mipMap, SurfaceFormat format,
            bool isInternal)
            : base()
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException("graphicsDevice");
            if (graphicsDevice.GraphicsProfile == GraphicsProfile.Reach && size > 512)
                throw new NotSupportedException("Reach profile supports a maximum TextureCube size of 512");
            if (graphicsDevice.GraphicsProfile == GraphicsProfile.HiDef && size > 4096)
                throw new NotSupportedException("HiDef profile supports a maximum TextureCube size of 4096");
            if (graphicsDevice.GraphicsProfile == GraphicsProfile.FL10_0 && size > 8192)
                throw new NotSupportedException("FL10_0 profile supports a maximum TextureCube size of 8192");
            if (graphicsDevice.GraphicsProfile == GraphicsProfile.FL10_1 && size > 8192)
                throw new NotSupportedException("FL10_1 profile supports a maximum TextureCube size of 8192");
            if (graphicsDevice.GraphicsProfile == GraphicsProfile.FL11_0 && size > 16384)
                throw new NotSupportedException("FL11_0 profile supports a maximum TextureCube size of 16384");
            if (graphicsDevice.GraphicsProfile == GraphicsProfile.FL11_1 && size > 16384)
                throw new NotSupportedException("FL11_1 profile supports a maximum TextureCube size of 16384");
            if (graphicsDevice.GraphicsProfile == GraphicsProfile.Reach && !MathHelper.IsPowerOfTwo(size))
                throw new NotSupportedException("Reach profile requires TextureCube sizes to be powers of two");
            if (graphicsDevice.GraphicsProfile == GraphicsProfile.Reach && (format == SurfaceFormat.NormalizedByte2 || format == SurfaceFormat.NormalizedByte4 || format == SurfaceFormat.Rgba1010102 || format == SurfaceFormat.Rg32 || format == SurfaceFormat.Rgba64 || format == SurfaceFormat.Alpha8 || format == SurfaceFormat.Single || format == SurfaceFormat.Vector2 || format == SurfaceFormat.Vector4 || format == SurfaceFormat.HalfSingle || format == SurfaceFormat.HalfVector2 || format == SurfaceFormat.HalfVector4 || format == SurfaceFormat.HdrBlendable))
                throw new NotSupportedException("Reach profile does not support Texture2D format "+ format);
            if (size <= 0)
                throw new ArgumentOutOfRangeException("size","Cube size must be greater than zero");

        }

        /// <summary>
        /// Gets a copy of cube texture data specifying a cubemap face.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cubeMapFace">The cube map face.</param>
        /// <param name="data">The data.</param>
        public void GetData<T>(CubeMapFace cubeMapFace, T[] data)
            where T : struct
        {
            ValidateArrayBounds<T>(data, 0, data.Length);
            Rectangle textureBounds = new Rectangle(0, 0, Size, Size);
            Rectangle checkedRect;
            ValidateParams<T>(0, textureBounds, data.Length, ref textureBounds, out checkedRect);
            _strategyTextureCube.GetData<T>(cubeMapFace, 0, checkedRect, data, 0, data.Length);
        }

        public void GetData<T>(CubeMapFace cubeMapFace, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            ValidateArrayBounds<T>(data, startIndex, elementCount);
            Rectangle textureBounds = new Rectangle(0, 0, Size, Size);
            Rectangle checkedRect;
            ValidateParams<T>(0, textureBounds, elementCount, ref textureBounds, out checkedRect);
            _strategyTextureCube.GetData<T>(cubeMapFace, 0, checkedRect, data, startIndex, elementCount);
        }

        public void GetData<T>(CubeMapFace cubeMapFace, int level, Rectangle? rect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            ValidateArrayBounds<T>(data, startIndex, elementCount);
            Rectangle textureBounds = new Rectangle(0, 0, Math.Max(Size >> level, 1), Math.Max(Size >> level, 1));
            if (rect == null)
                rect = textureBounds;
            ValidateRect<T>(level, ref textureBounds, rect.Value);
            Rectangle checkedRect;
            ValidateParams<T>(level, rect.Value, elementCount, ref textureBounds, out checkedRect);
            _strategyTextureCube.GetData<T>(cubeMapFace, level, checkedRect, data, startIndex, elementCount);
        }

        public void SetData<T>(CubeMapFace cubeMapFace, T[] data)
            where T : struct
        {
            ValidateArrayBounds<T>(data, 0, data.Length);
            Rectangle textureBounds = new Rectangle(0, 0, Size, Size);
            Rectangle checkedRect;
            ValidateParams<T>(0, textureBounds, data.Length, ref textureBounds, out checkedRect);
            _strategyTextureCube.SetData<T>(cubeMapFace, 0, checkedRect, data, 0, data.Length);
        }

        public void SetData<T>(CubeMapFace cubeMapFace, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            ValidateArrayBounds<T>(data, startIndex, elementCount);
            Rectangle textureBounds = new Rectangle(0, 0, Size, Size);
            Rectangle checkedRect;
            ValidateParams<T>(0, textureBounds, elementCount, ref textureBounds, out checkedRect);
            _strategyTextureCube.SetData<T>(cubeMapFace, 0, checkedRect, data, startIndex, elementCount);
        }
        
        public void SetData<T>(CubeMapFace cubeMapFace, int level, Rectangle? rect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            ValidateArrayBounds<T>(data, startIndex, elementCount);
            Rectangle textureBounds = new Rectangle(0, 0, Math.Max(Size >> level, 1), Math.Max(Size >> level, 1));
            if (rect == null)
                rect = textureBounds;
            ValidateRect<T>(level, ref textureBounds, rect.Value);
            Rectangle checkedRect;
            ValidateParams<T>(level, rect.Value, elementCount, ref textureBounds, out checkedRect);
            _strategyTextureCube.SetData<T>(cubeMapFace, level, checkedRect, data, startIndex, elementCount);
        }

        private void ValidateArrayBounds<T>(T[] data, int startIndex, int elementCount)
            where T : struct
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (startIndex < 0 || startIndex >= data.Length)
                throw new ArgumentException("startIndex must be at least zero and smaller than data.Length.", "startIndex");
            if (data.Length < startIndex + elementCount)
                throw new ArgumentException("The data array is too small.");
        }

        private void ValidateRect<T>(int level, ref Rectangle textureBounds, Rectangle rect)
            where T : struct
        {
            if (level < 0 || level >= LevelCount)
                throw new ArgumentException("level must be smaller than the number of levels in this texture.");
            if (!textureBounds.Contains(rect) || rect.Width <= 0 || rect.Height <= 0)
                throw new ArgumentException("Rectangle must be inside the texture bounds", "rect");
        }

        private unsafe void ValidateParams<T>(int level, Rectangle rect, int elementCount, ref Rectangle textureBounds, out Rectangle checkedRect)
            where T : struct
        {
            int tSize = sizeof(T);
            int fSize = Format.GetSize();
            if (tSize > fSize || fSize % tSize != 0)
                throw new ArgumentException("Type T is of an invalid size for the format of this texture.", "T");

            int dataByteSize;
            if (Format.IsCompressedFormat())
            {
                dataByteSize = _strategyTextureCube.GetCompressedDataByteSize(fSize, rect, ref textureBounds, out checkedRect);
            }
            else
            {
                checkedRect = rect;
                dataByteSize = rect.Width * rect.Height * fSize;
            }

            if (elementCount * tSize != dataByteSize)
                throw new ArgumentException(string.Format("elementCount is not the right size, " +
                                            "elementCount * sizeof(T) is {0}, but data size is {1}.",
                                            elementCount * tSize, dataByteSize), "elementCount");
        }
    }
}

