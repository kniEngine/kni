// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Platform.Graphics;
using Microsoft.Xna.Platform.Graphics.Utilities;


namespace Microsoft.Xna.Framework.Graphics
{
    public class Texture2D : Texture
    {
        protected internal ITexture2DStrategy _strategyTexture2D;

        internal float TexelWidth { get; private set; }
        internal float TexelHeight { get; private set; }

        /// <summary>
        /// Gets the width of the texture in pixels.
        /// </summary>
        public int Width { get { return _strategyTexture2D.Width; } }

        /// <summary>
        /// Gets the height of the texture in pixels.
        /// </summary>
        public int Height { get { return _strategyTexture2D.Height; } }

        internal int ArraySize { get { return _strategyTexture2D.ArraySize; } }

        /// <summary>
        /// Gets the dimensions of the texture
        /// </summary>
        public Rectangle Bounds { get { return _strategyTexture2D.Bounds; } }

        /// <summary>
        /// Creates a new texture of the given size
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public Texture2D(GraphicsDevice graphicsDevice, int width, int height)
            : this(graphicsDevice, width, height, false, SurfaceFormat.Color, false, 1)
        {
        }

        /// <summary>
        /// Creates a new texture of a given size with a surface format and optional mipmaps 
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="mipMap"></param>
        /// <param name="format"></param>
        public Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat format)
            : this(graphicsDevice, width, height, mipMap, format, false, 1)
        {
        }

        /// <summary>
        /// Creates a new texture array of a given size with a surface format and optional mipmaps.
        /// Throws ArgumentException if the current GraphicsDevice can't work with texture arrays
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="mipMap"></param>
        /// <param name="format"></param>
        /// <param name="arraySize"></param>
        public Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat format, int arraySize)
            : this(graphicsDevice, width, height, mipMap, format, false, arraySize)
        {
        }
        
        private Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat format, bool shared, int arraySize)
            : this(graphicsDevice, width, height, mipMap, format, shared, arraySize, true)
        {
            _strategyTexture2D = ((IPlatformGraphicsContext)graphicsDevice.MainContext).Strategy.CreateTexture2DStrategy(width, height, mipMap, format, arraySize, shared);
            _strategyTexture = _strategyTexture2D;
            SetResourceStrategy((IGraphicsResourceStrategy)_strategyTexture2D);
        }

        protected Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat format, bool shared, int arraySize,
            bool isInternal)
            : base()
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException("graphicsDevice");
            if (graphicsDevice.GraphicsProfile == GraphicsProfile.Reach && (width > 2048 || height > 2048))
                throw new NotSupportedException("Reach profile supports a maximum Texture2D size of 2048");
            if (graphicsDevice.GraphicsProfile == GraphicsProfile.HiDef && (width > 4096 || height > 4096))
                throw new NotSupportedException("HiDef profile supports a maximum Texture2D size of 4096");
            if (graphicsDevice.GraphicsProfile == GraphicsProfile.FL10_0 && (width > 8192 || height > 8192))
                throw new NotSupportedException("FL10_0 profile supports a maximum Texture2D size of 8192");
            if (graphicsDevice.GraphicsProfile == GraphicsProfile.FL10_1 && (width > 8192 || height > 8192))
                throw new NotSupportedException("FL10_1 profile supports a maximum Texture2D size of 8192");
            if (graphicsDevice.GraphicsProfile == GraphicsProfile.FL11_0 && (width > 16384 || height > 16384))
                throw new NotSupportedException("FL11_0 profile supports a maximum Texture2D size of 16384");
            if (graphicsDevice.GraphicsProfile == GraphicsProfile.FL11_1 && (width > 16384 || height > 16384))
                throw new NotSupportedException("FL11_1 profile supports a maximum Texture2D size of 16384");
            if (graphicsDevice.GraphicsProfile == GraphicsProfile.Reach && mipMap && (!MathHelper.IsPowerOfTwo(width) || !MathHelper.IsPowerOfTwo(height)))
                throw new NotSupportedException("Reach profile requires mipmapped Texture2D sizes to be powers of two");            
            if (graphicsDevice.GraphicsProfile == GraphicsProfile.Reach && GraphicsExtensions.IsCompressedFormat(format) && (!MathHelper.IsPowerOfTwo(width) || !MathHelper.IsPowerOfTwo(height)))
                throw new NotSupportedException("Reach profile requires compressed Texture2D sizes to be powers of two");
            if (graphicsDevice.GraphicsProfile == GraphicsProfile.Reach && (format == SurfaceFormat.Rgba1010102 || format == SurfaceFormat.Rg32 || format == SurfaceFormat.Rgba64 || format == SurfaceFormat.Alpha8 || format == SurfaceFormat.Single || format == SurfaceFormat.Vector2 || format == SurfaceFormat.Vector4 || format == SurfaceFormat.HalfSingle || format == SurfaceFormat.HalfVector2 || format == SurfaceFormat.HalfVector4 || format == SurfaceFormat.HdrBlendable))
                throw new NotSupportedException("Reach profile does not support Texture2D format "+ format);
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width","Texture width must be greater than zero");
            if (height <= 0)
                throw new ArgumentOutOfRangeException("height","Texture height must be greater than zero");
            if (arraySize > 1 && !((IPlatformGraphicsDevice)graphicsDevice).Strategy.Capabilities.SupportsTextureArrays)
                throw new ArgumentException("Texture arrays are not supported on this graphics device", "arraySize");

            this.TexelWidth = 1f / (float)width;
            this.TexelHeight = 1f / (float)height;

        }

        private Texture2D(GraphicsContext context, Stream stream)
        {
            _strategyTexture2D = ((IPlatformGraphicsContext)context).Strategy.CreateTexture2DStrategy(stream);
            _strategyTexture = _strategyTexture2D;
            SetResourceStrategy((IGraphicsResourceStrategy)_strategyTexture2D);

            this.TexelWidth = 1f / (float)this.Width;
            this.TexelHeight = 1f / (float)this.Height;
        }


        /// <summary>
        /// Gets the handle to a shared resource.
        /// </summary>
        /// <returns>
        /// The handle of the shared resource, or <see cref="IntPtr.Zero"/> if the texture was not
        /// created as a shared resource.
        /// </returns>
        public IntPtr GetSharedHandle()
        {
            return _strategyTexture2D.GetSharedHandle();
        }


        /// <summary>
        /// Changes the pixels of the texture
        /// Throws ArgumentNullException if data is null
        /// Throws ArgumentException if arraySlice is greater than 0, and the GraphicsDevice does not support texture arrays
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="level">Layer of the texture to modify</param>
        /// <param name="arraySlice">Index inside the texture array</param>
        /// <param name="rect">Area to modify</param>
        /// <param name="data">New data for the texture</param>
        /// <param name="startIndex">Start position of data</param>
        /// <param name="elementCount"></param>
        public void SetData<T>(int level, int arraySlice, Rectangle? rect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            ValidateArrayBounds<T>(arraySlice, data, startIndex, elementCount);
            Rectangle textureBounds = new Rectangle(0, 0, Math.Max(Width >> level, 1), Math.Max(Height >> level, 1));
            if (rect == null)
                rect = textureBounds;
            ValidateRect<T>(level, ref textureBounds, rect.Value);
            Rectangle checkedRect;
            ValidateParams<T>(level, rect.Value, elementCount, ref textureBounds, out checkedRect);
            _strategyTexture2D.SetData<T>(level, arraySlice, checkedRect, data, startIndex, elementCount);
        }

        /// <summary>
        /// Changes the pixels of the texture
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="level">Layer of the texture to modify</param>
        /// <param name="rect">Area to modify</param>
        /// <param name="data">New data for the texture</param>
        /// <param name="startIndex">Start position of data</param>
        /// <param name="elementCount"></param>
        public void SetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount)
            where T : struct 
        {
            ValidateArrayBounds<T>(0, data, startIndex, elementCount);
            Rectangle textureBounds = new Rectangle(0, 0, Math.Max(Width >> level, 1), Math.Max(Height >> level, 1));
            if (rect == null)
                rect = textureBounds;
            ValidateRect<T>(level, ref textureBounds, rect.Value);
            Rectangle checkedRect;
            ValidateParams<T>(level, rect.Value, elementCount, ref textureBounds, out checkedRect);
            if (rect.HasValue)
                _strategyTexture2D.SetData<T>(level, 0, checkedRect, data, startIndex, elementCount);
            else           
                _strategyTexture2D.SetData<T>(level, data, startIndex, elementCount);
        }

        /// <summary>
        /// Changes the texture's pixels
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">New data for the texture</param>
        /// <param name="startIndex">Start position of data</param>
        /// <param name="elementCount"></param>
        public void SetData<T>(T[] data, int startIndex, int elementCount)
            where T : struct
        {
            ValidateArrayBounds<T>(0, data, startIndex, elementCount);
            Rectangle textureBounds = new Rectangle(0, 0, Width, Height);
            Rectangle checkedRect;
            ValidateParams<T>(0, textureBounds, elementCount, ref textureBounds, out checkedRect);
            _strategyTexture2D.SetData<T>(0, data, startIndex, elementCount);
        }

        /// <summary>
        /// Changes the texture's pixels
        /// </summary>
        /// <typeparam name="T">New data for the texture</typeparam>
        /// <param name="data"></param>
        public void SetData<T>(T[] data)
            where T : struct
        {
            ValidateArrayBounds<T>(0, data, 0, data.Length);
            Rectangle textureBounds = new Rectangle(0, 0,Width, Height);
            Rectangle checkedRect;
            ValidateParams<T>(0, textureBounds, data.Length, ref textureBounds, out checkedRect);
            _strategyTexture2D.SetData<T>(0, data, 0, data.Length);
        }

        /// <summary>
        /// Retrieves the contents of the texture
        /// Throws ArgumentException if data is null, data.length is too short or
        /// if arraySlice is greater than 0 and the GraphicsDevice doesn't support texture arrays
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="level">Layer of the texture</param>
        /// <param name="arraySlice">Index inside the texture array</param>
        /// <param name="rect">Area of the texture to retrieve</param>
        /// <param name="data">Destination array for the data</param>
        /// <param name="startIndex">Starting index of data where to write the pixel data</param>
        /// <param name="elementCount">Number of pixels to read</param>
        public void GetData<T>(int level, int arraySlice, Rectangle? rect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            ValidateArrayBounds<T>(arraySlice, data, startIndex, elementCount);
            Rectangle textureBounds = new Rectangle(0, 0, Math.Max(Width >> level, 1), Math.Max(Height >> level, 1));
            if (rect == null)
                rect = textureBounds;
            ValidateRect<T>(level, ref textureBounds, rect.Value);
            Rectangle checkedRect;
            ValidateParams<T>(level, rect.Value, elementCount, ref textureBounds, out checkedRect);
            _strategyTexture2D.GetData<T>(level, arraySlice, checkedRect, data, startIndex, elementCount);
        }

        /// <summary>
        /// Retrieves the contents of the texture
        /// Throws ArgumentException if data is null, data.length is too short or
        /// if arraySlice is greater than 0 and the GraphicsDevice doesn't support texture arrays
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="level">Layer of the texture</param>
        /// <param name="rect">Area of the texture</param>
        /// <param name="data">Destination array for the texture data</param>
        /// <param name="startIndex">First position in data where to write the pixel data</param>
        /// <param name="elementCount">Number of pixels to read</param>
        public void GetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            ValidateArrayBounds<T>(0, data, startIndex, elementCount);
            Rectangle textureBounds = new Rectangle(0, 0, Math.Max(Width >> level, 1), Math.Max(Height >> level, 1));
            if (rect == null)
                rect = textureBounds;
            ValidateRect<T>(level, ref textureBounds, rect.Value);
            Rectangle checkedRect;
            ValidateParams<T>(level, rect.Value, elementCount, ref textureBounds, out checkedRect);
            _strategyTexture2D.GetData<T>(level, 0, checkedRect, data, startIndex, elementCount);
        }

        /// <summary>
        /// Retrieves the contents of the texture
        /// Throws ArgumentException if data is null, data.length is too short or
        /// if arraySlice is greater than 0 and the GraphicsDevice doesn't support texture arrays
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">Destination array for the texture data</param>
        /// <param name="startIndex">First position in data where to write the pixel data</param>
        /// <param name="elementCount">Number of pixels to read</param>
        public void GetData<T>(T[] data, int startIndex, int elementCount)
            where T : struct
        {
            ValidateArrayBounds<T>(0, data, startIndex, elementCount);
            Rectangle textureBounds = new Rectangle(0, 0, Width, Height);
            Rectangle checkedRect;
            ValidateParams<T>(0, textureBounds, elementCount, ref textureBounds, out checkedRect);
            _strategyTexture2D.GetData<T>(0, 0, checkedRect, data, startIndex, elementCount);
        }

        /// <summary>
        /// Retrieves the contents of the texture
        /// Throws ArgumentException if data is null, data.length is too short or
        /// if arraySlice is greater than 0 and the GraphicsDevice doesn't support texture arrays
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">Destination array for the texture data</param>
        public void GetData<T>(T[] data)
            where T : struct
        {
            ValidateArrayBounds<T>(0, data, 0, data.Length);
            Rectangle textureBounds = new Rectangle(0, 0, Width, Height);
            Rectangle checkedRect;
            ValidateParams<T>(0, textureBounds, data.Length, ref textureBounds, out checkedRect);
            _strategyTexture2D.GetData<T>(0, 0, checkedRect, data, 0, data.Length);
        }

        private void ValidateArrayBounds<T>(int arraySlice, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            if (arraySlice > 0 && !((IPlatformGraphicsDevice)base.GraphicsDevice).Strategy.Capabilities.SupportsTextureArrays)
                throw new ArgumentException("Texture arrays are not supported on this graphics device", "arraySlice");
            if (arraySlice < 0 || arraySlice >= ArraySize)
                throw new ArgumentException("arraySlice must be smaller than the ArraySize of this texture and larger than 0.", "arraySlice");

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
                throw new ArgumentException("level must be smaller than the number of levels in this texture.", "level");
            if (!textureBounds.Contains(rect) || rect.Width <= 0 || rect.Height <= 0)
                throw new ArgumentException("Rectangle must be inside the texture bounds", "rect");
        }

        private void ValidateParams<T>(int level, Rectangle rect, int elementCount, ref Rectangle textureBounds, out Rectangle checkedRect)
            where T : struct
        {
            int tSize = ReflectionHelpers.SizeOf<T>();
            int fSize = Format.GetSize();
            if (tSize > fSize || fSize % tSize != 0)
                throw new ArgumentException("Type T is of an invalid size for the format of this texture.", "T");

            int dataByteSize;
            if (Format.IsCompressedFormat())
            {
                dataByteSize = _strategyTexture2D.GetCompressedDataByteSize(fSize, rect, ref textureBounds, out checkedRect);
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

        /// <summary>
        /// Creates a <see cref="Texture2D"/> from a stream, supported formats bmp, gif, jpg, png, tif and dds (only for simple textures).
        /// May work with other formats, but will not work with tga files.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device to use to create the texture.</param>
        /// <param name="stream">The stream from which to read the image data.</param>
        /// <returns>The <see cref="Texture2D"/> created from the image stream.</returns>
        /// <remarks>Note that different image decoders may generate slight differences between platforms, but perceptually 
        /// the images should be identical.  This call does not premultiply the image alpha, but areas of zero alpha will
        /// result in black color data.
        /// </remarks>
        public static Texture2D FromStream(GraphicsDevice graphicsDevice, Stream stream)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException("graphicsDevice");
            if (stream == null)
                throw new ArgumentNullException("stream");

            try
            {
                return new Texture2D(graphicsDevice.MainContext, stream);
            }
            catch(Exception e)
            {
                throw new InvalidOperationException("This image format is not supported", e);
            }
        }

        /// <summary>
        /// Converts the texture to a JPG image
        /// </summary>
        /// <param name="stream">Destination for the image</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SaveAsJpeg(Stream stream, int width, int height)
        {
            _strategyTexture2D.SaveAsJpeg(stream, width, height);
        }

        /// <summary>
        /// Converts the texture to a PNG image
        /// </summary>
        /// <param name="stream">Destination for the image</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SaveAsPng(Stream stream, int width, int height)
        {
            _strategyTexture2D.SaveAsPng(stream, width, height);
        }
        
    }
}
