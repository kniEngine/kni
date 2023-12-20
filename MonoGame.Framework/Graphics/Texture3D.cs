// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;
using MonoGame.Framework.Utilities;


namespace Microsoft.Xna.Framework.Graphics
{
    public class Texture3D : Texture
    {
        protected ITexture3DStrategy _strategyTexture3D;

        public int Width { get { return _strategyTexture3D.Width; } }

        public int Height { get { return _strategyTexture3D.Height; } }

        public int Depth { get { return _strategyTexture3D.Depth; } }

        public Texture3D(GraphicsDevice graphicsDevice, int width, int height, int depth, bool mipMap, SurfaceFormat format)
            : this(graphicsDevice, width, height, depth, mipMap, format, true)
        {
            _strategyTexture3D = graphicsDevice.Strategy.MainContext.Strategy.CreateTexture3DStrategy(width, height, depth, mipMap, format);
            _strategyTexture = _strategyTexture3D;
            SetResourceStrategy((IGraphicsResourceStrategy)_strategyTexture3D);
        }

        internal Texture3D(GraphicsDevice graphicsDevice, int width, int height, int depth, bool mipMap, SurfaceFormat format,
            bool isInternal)
            : base()
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException("graphicsDevice");
            if (graphicsDevice.Strategy.GraphicsProfile == GraphicsProfile.Reach)
                throw new NotSupportedException("Reach profile does not support Texture3D");
            if (graphicsDevice.Strategy.GraphicsProfile == GraphicsProfile.HiDef && (width > 256 || height > 256 || height > 256))
                throw new NotSupportedException("HiDef profile supports a maximum Texture3D size of 256");
            if (graphicsDevice.Strategy.GraphicsProfile == GraphicsProfile.FL10_0 && (width > 2048 || height > 2048 || height > 2048))
                throw new NotSupportedException("FL10_0 profile supports a maximum Texture3D size of 2048");
            if (graphicsDevice.Strategy.GraphicsProfile == GraphicsProfile.FL10_1 && (width > 2048 || height > 2048 || height > 2048))
                throw new NotSupportedException("FL10_1 profile supports a maximum Texture3D size of 2048");
            if (graphicsDevice.Strategy.GraphicsProfile == GraphicsProfile.FL11_0 && (width > 2048 || height > 2048 || height > 2048))
                throw new NotSupportedException("FL11_0 profile supports a maximum Texture3D size of 2048");
            if (graphicsDevice.Strategy.GraphicsProfile == GraphicsProfile.FL11_1 && (width > 2048 || height > 2048 || height > 2048))
                throw new NotSupportedException("FL11_1 profile supports a maximum Texture3D size of 2048");
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width","Texture width must be greater than zero");
            if (height <= 0)
                throw new ArgumentOutOfRangeException("height","Texture height must be greater than zero");
            if (depth <= 0)
                throw new ArgumentOutOfRangeException("depth","Texture depth must be greater than zero");

        }

        public void SetData<T>(T[] data)
            where T : struct
        {
            ValidateParams<T>(0, 0, 0, this.Width, this.Height, 0, this.Depth, data, 0, data.Length);
            _strategyTexture3D.SetData<T>(0, 0, 0, this.Width, this.Height, 0, this.Depth, data, 0, data.Length);
        }

        public void SetData<T>(T[] data, int startIndex, int elementCount)
            where T : struct
        {
            ValidateParams<T>(0, 0, 0, this.Width, this.Height, 0, this.Depth, data, startIndex, elementCount);
            _strategyTexture3D.SetData<T>(0, 0, 0, this.Width, this.Height, 0, this.Depth, data, startIndex, elementCount);
        }

        public void SetData<T>(int level, int left, int top, int right, int bottom, int front, int back,
                               T[] data, int startIndex, int elementCount)
            where T : struct
        {
            ValidateParams<T>(level, left, top, right, bottom, front, back, data, startIndex, elementCount);
            _strategyTexture3D.SetData<T>(level, left, top, right, bottom, front, back, data, startIndex, elementCount);
        }

        /// <summary>
        /// Gets a copy of 3D texture data, specifying a mipMap level, source box, start index, and number of elements.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <param name="level">MipMap level.</param>
        /// <param name="left">Position of the left side of the box on the x-axis.</param>
        /// <param name="top">Position of the top of the box on the y-axis.</param>
        /// <param name="right">Position of the right side of the box on the x-axis.</param>
        /// <param name="bottom">Position of the bottom of the box on the y-axis.</param>
        /// <param name="front">Position of the front of the box on the z-axis.</param>
        /// <param name="back">Position of the back of the box on the z-axis.</param>
        /// <param name="data">Array of data.</param>
        /// <param name="startIndex">Index of the first element to get.</param>
        /// <param name="elementCount">Number of elements to get.</param>
        public void GetData<T>(int level, int left, int top, int right, int bottom, int front, int back,
                               T[] data, int startIndex, int elementCount)
            where T : struct
        {
            ValidateParams<T>(level, left, top, right, bottom, front, back, data, startIndex, elementCount);                       
            _strategyTexture3D.GetData<T>(level, left, top, right, bottom, front, back, data, startIndex, elementCount);
        }

        /// <summary>
        /// Gets a copy of 3D texture data, specifying a start index and number of elements.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <param name="data">Array of data.</param>
        /// <param name="startIndex">Index of the first element to get.</param>
        /// <param name="elementCount">Number of elements to get.</param>
        public void GetData<T>(T[] data, int startIndex, int elementCount)
            where T : struct
        {
            ValidateParams<T>(0, 0, 0, this.Width, this.Height, 0, this.Depth, data, startIndex, elementCount);
            _strategyTexture3D.GetData<T>(0, 0, 0, this.Width, this.Height, 0, this.Depth, data, startIndex, elementCount);
        }

        /// <summary>
        /// Gets a copy of 3D texture data.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <param name="data">Array of data.</param>
        public void GetData<T>(T[] data)
            where T : struct
        {
            ValidateParams<T>(0, 0, 0, this.Width, this.Height, 0, this.Depth, data, 0, data.Length);
            _strategyTexture3D.GetData<T>(0, 0, 0, this.Width, this.Height, 0, this.Depth, data, 0, data.Length);
        }

        private void ValidateParams<T>(int level,
                                       int left, int top, int right, int bottom, int front, int back,
                                       T[] data, int startIndex, int elementCount)
            where T : struct
        {
            int texWidth = Math.Max(Width >> level, 1);
            int texHeight = Math.Max(Height >> level, 1);
            int texDepth = Math.Max(Depth >> level, 1);
            int width = right - left;
            int height = bottom - top;
            int depth = back - front;

            if (left < 0 || top < 0 || back < 0 || right > texWidth || bottom > texHeight || front > texDepth)
                throw new ArgumentException("Area must remain inside texture bounds");
            // Disallow negative box size
            if (left >= right || top >= bottom || front >= back)
                throw new ArgumentException("Neither box size nor box position can be negative");
            if (level < 0 || level >= LevelCount)
                throw new ArgumentException("level must be smaller than the number of levels in this texture.");
            if (data == null)
                throw new ArgumentNullException("data");
            int tSize = ReflectionHelpers.SizeOf<T>();
            int fSize = Format.GetSize();
            if (tSize > fSize || fSize % tSize != 0)
                throw new ArgumentException("Type T is of an invalid size for the format of this texture.", "T");
            if (startIndex < 0 || startIndex >= data.Length)
                throw new ArgumentException("startIndex must be at least zero and smaller than data.Length.", "startIndex");
            if (data.Length < startIndex + elementCount)
                throw new ArgumentException("The data array is too small.");

            int dataByteSize = width*height*depth*fSize;
            if (elementCount * tSize != dataByteSize)
                throw new ArgumentException(string.Format("elementCount is not the right size, " +
                                            "elementCount * sizeof(T) is {0}, but data size is {1}.",
                                            elementCount * tSize, dataByteSize), "elementCount");
        }
    }
}

