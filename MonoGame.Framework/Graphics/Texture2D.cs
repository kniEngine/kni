// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Platform.Graphics;
using MonoGame.Framework.Utilities;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D : Texture
    {
        internal ITexture2DStrategy _strategyTexture2D;

        internal protected enum SurfaceType
        {
            Texture,
            RenderTarget,
            SwapChainRenderTarget,
        }
                
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
            : this(graphicsDevice, width, height, false, SurfaceFormat.Color, false, 1, SurfaceType.Texture)
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
            : this(graphicsDevice, width, height, mipMap, format, false, 1, SurfaceType.Texture)
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
            : this(graphicsDevice, width, height, mipMap, format, false, arraySize, SurfaceType.Texture)
        {
        }

        /// <summary>
        ///  Creates a new texture of a given size with a surface format and optional mipmaps.
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="mipMap"></param>
        /// <param name="format"></param>
        /// <param name="surfaceType"></param>
        protected Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat format, SurfaceType surfaceType)
            : this(graphicsDevice, width, height, mipMap, format, false, 1, surfaceType)
        {
        }
        
        protected Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat format, bool shared, int arraySize, SurfaceType surfaceType)
            : base()
		{
            if (graphicsDevice == null)
                throw new ArgumentNullException("graphicsDevice");
            if (graphicsDevice.Strategy.GraphicsProfile == GraphicsProfile.Reach && (width > 2048 || height > 2048))
                throw new NotSupportedException("Reach profile supports a maximum Texture2D size of 2048");
            if (graphicsDevice.Strategy.GraphicsProfile == GraphicsProfile.HiDef && (width > 4096 || height > 4096))
                throw new NotSupportedException("HiDef profile supports a maximum Texture2D size of 4096");
            if (graphicsDevice.Strategy.GraphicsProfile == GraphicsProfile.FL10_0 && (width > 8192 || height > 8192))
                throw new NotSupportedException("FL10_0 profile supports a maximum Texture2D size of 8192");
            if (graphicsDevice.Strategy.GraphicsProfile == GraphicsProfile.FL10_1 && (width > 8192 || height > 8192))
                throw new NotSupportedException("FL10_1 profile supports a maximum Texture2D size of 8192");
            if (graphicsDevice.Strategy.GraphicsProfile == GraphicsProfile.FL11_0 && (width > 16384 || height > 16384))
                throw new NotSupportedException("FL11_0 profile supports a maximum Texture2D size of 16384");
            if (graphicsDevice.Strategy.GraphicsProfile == GraphicsProfile.FL11_1 && (width > 16384 || height > 16384))
                throw new NotSupportedException("FL11_1 profile supports a maximum Texture2D size of 16384");
            if (graphicsDevice.Strategy.GraphicsProfile == GraphicsProfile.Reach && mipMap && (!MathHelper.IsPowerOfTwo(width) || !MathHelper.IsPowerOfTwo(height)))
                throw new NotSupportedException("Reach profile requires mipmapped Texture2D sizes to be powers of two");            
            if (graphicsDevice.Strategy.GraphicsProfile == GraphicsProfile.Reach && GraphicsExtensions.IsCompressedFormat(format) && (!MathHelper.IsPowerOfTwo(width) || !MathHelper.IsPowerOfTwo(height)))
                throw new NotSupportedException("Reach profile requires compressed Texture2D sizes to be powers of two");
            if (graphicsDevice.Strategy.GraphicsProfile == GraphicsProfile.Reach && (format == SurfaceFormat.Rgba1010102 || format == SurfaceFormat.Rg32 || format == SurfaceFormat.Rgba64 || format == SurfaceFormat.Alpha8 || format == SurfaceFormat.Single || format == SurfaceFormat.Vector2 || format == SurfaceFormat.Vector4 || format == SurfaceFormat.HalfSingle || format == SurfaceFormat.HalfVector2 || format == SurfaceFormat.HalfVector4 || format == SurfaceFormat.HdrBlendable))
                throw new NotSupportedException("Reach profile does not support Texture2D format "+ format);
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width","Texture width must be greater than zero");
            if (height <= 0)
                throw new ArgumentOutOfRangeException("height","Texture height must be greater than zero");
            if (arraySize > 1 && !graphicsDevice.Strategy.Capabilities.SupportsTextureArrays)
                throw new ArgumentException("Texture arrays are not supported on this graphics device", "arraySize");

            _strategyTexture2D = graphicsDevice.Strategy.MainContext.Strategy.CreateTexture2DStrategy(width, height, mipMap, format, arraySize);
            _strategyTexture = _strategyTexture2D;
            SetResourceStrategy((IGraphicsResourceStrategy)_strategyTexture2D);
            SetGraphicsDevice(graphicsDevice);

            this.TexelWidth = 1f / (float)width;
            this.TexelHeight = 1f / (float)height;

            // Texture will be assigned by the swap chain.
		    if (surfaceType == SurfaceType.SwapChainRenderTarget)
		        return;

            PlatformConstructTexture2D(width, height, mipMap, format, surfaceType, shared);
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
            return PlatformGetSharedHandle();
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
            Rectangle checkedRect;
            ValidateParams<T>(level, arraySlice, rect, data, startIndex, elementCount, out checkedRect);
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
            Rectangle checkedRect;
            ValidateParams<T>(level, 0, rect, data, startIndex, elementCount, out checkedRect);
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
            Rectangle checkedRect;
            ValidateParams<T>(0, 0, null, data, startIndex, elementCount, out checkedRect);
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
            Rectangle checkedRect;
            ValidateParams<T>(0, 0, null, data, 0, data.Length, out checkedRect);
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
            Rectangle checkedRect;
            ValidateParams<T>(level, arraySlice, rect, data, startIndex, elementCount, out checkedRect);
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
            this.GetData<T>(level, 0, rect, data, startIndex, elementCount);
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
			this.GetData<T>(0, null, data, startIndex, elementCount);
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
			this.GetData<T>(0, null, data, 0, data.Length);
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
                return PlatformFromStream(graphicsDevice, stream);
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
            PlatformSaveAsJpeg(stream, width, height);
        }

        /// <summary>
        /// Converts the texture to a PNG image
        /// </summary>
        /// <param name="stream">Destination for the image</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SaveAsPng(Stream stream, int width, int height)
        {
            PlatformSaveAsPng(stream, width, height);
        }
        
        private void ValidateParams<T>(int level, int arraySlice, Rectangle? rect, T[] data,
            int startIndex, int elementCount, out Rectangle checkedRect)
            where T : struct
        {
            Rectangle textureBounds = new Rectangle(0, 0, Math.Max(Width >> level, 1), Math.Max(Height >> level, 1));
            checkedRect = rect ?? textureBounds;
            if (level < 0 || level >= LevelCount)
                throw new ArgumentException("level must be smaller than the number of levels in this texture.", "level");
            if (arraySlice > 0 && !GraphicsDevice.Strategy.Capabilities.SupportsTextureArrays)
                throw new ArgumentException("Texture arrays are not supported on this graphics device", "arraySlice");
            if (arraySlice < 0 || arraySlice >= ArraySize)
                throw new ArgumentException("arraySlice must be smaller than the ArraySize of this texture and larger than 0.", "arraySlice");
            if (!textureBounds.Contains(checkedRect) || checkedRect.Width <= 0 || checkedRect.Height <= 0)
                throw new ArgumentException("Rectangle must be inside the texture bounds", "rect");
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

            int dataByteSize;
            if (Format.IsCompressedFormat())
            {
                int blockWidth, blockHeight;
                Format.GetBlockSize(out blockWidth, out blockHeight);
                int blockWidthMinusOne = blockWidth - 1;
                int blockHeightMinusOne = blockHeight - 1;
                // round x and y down to next multiple of block size; width and height up to next multiple of block size
                int roundedWidth = (checkedRect.Width + blockWidthMinusOne) & ~blockWidthMinusOne;
                int roundedHeight = (checkedRect.Height + blockHeightMinusOne) & ~blockHeightMinusOne;
                checkedRect = new Rectangle(checkedRect.X & ~blockWidthMinusOne, checkedRect.Y & ~blockHeightMinusOne,
#if OPENGL
                    // OpenGL only: The last two mip levels require the width and height to be
                    // passed as 2x2 and 1x1, but there needs to be enough data passed to occupy
                    // a full block.
                    checkedRect.Width < blockWidth && textureBounds.Width < blockWidth ? textureBounds.Width : roundedWidth,
                    checkedRect.Height < blockHeight && textureBounds.Height < blockHeight ? textureBounds.Height : roundedHeight);
#else
                    roundedWidth, roundedHeight);
#endif
                if (Format == SurfaceFormat.RgbPvrtc2Bpp || Format == SurfaceFormat.RgbaPvrtc2Bpp)
                {
                    dataByteSize = (Math.Max(checkedRect.Width, 16) * Math.Max(checkedRect.Height, 8) * 2 + 7) / 8;
                }
                else if (Format == SurfaceFormat.RgbPvrtc4Bpp || Format == SurfaceFormat.RgbaPvrtc4Bpp)
                {
                    dataByteSize = (Math.Max(checkedRect.Width, 8) * Math.Max(checkedRect.Height, 8) * 4 + 7) / 8;
                }
                else
                {
                    dataByteSize = roundedWidth * roundedHeight * fSize / (blockWidth * blockHeight);
                }
            }
            else
            {
                dataByteSize = checkedRect.Width * checkedRect.Height * fSize;
            }
            if (elementCount * tSize != dataByteSize)
                throw new ArgumentException(string.Format("elementCount is not the right size, " +
                                            "elementCount * sizeof(T) is {0}, but data size is {1}.",
                                            elementCount * tSize, dataByteSize), "elementCount");
        }

        internal Color[] GetColorData()
        {
            int colorDataLength = Width * Height;
            var colorData = new Color[colorDataLength];

            switch (Format)
            {
                case SurfaceFormat.Single:
                    var floatData = new float[colorDataLength];
                    GetData(floatData);

                    for (int i = 0; i < colorDataLength; i++)
                    {
                        float brightness = floatData[i];
                        // Export as a greyscale image.
                        colorData[i] = new Color(brightness, brightness, brightness);
                    }
                    break;

                case SurfaceFormat.Color:
                    GetData(colorData);
                    break;

                case SurfaceFormat.Alpha8:
                    var alpha8Data = new Alpha8[colorDataLength];
                    GetData(alpha8Data);

                    for (int i = 0; i < colorDataLength; i++)
                    {
                        colorData[i] = new Color(alpha8Data[i].ToVector4());
                    }

                    break;

                case SurfaceFormat.Bgr565:
                    var bgr565Data = new Bgr565[colorDataLength];
                    GetData(bgr565Data);

                    for (int i = 0; i < colorDataLength; i++)
                    {
                        colorData[i] = new Color(bgr565Data[i].ToVector4());
                    }

                    break;

                case SurfaceFormat.Bgra4444:
                    var bgra4444Data = new Bgra4444[colorDataLength];
                    GetData(bgra4444Data);

                    for (int i = 0; i < colorDataLength; i++)
                    {
                        colorData[i] = new Color(bgra4444Data[i].ToVector4());
                    }

                    break;

                case SurfaceFormat.Bgra5551:
                    var bgra5551Data = new Bgra5551[colorDataLength];
                    GetData(bgra5551Data);

                    for (int i = 0; i < colorDataLength; i++)
                    {
                        colorData[i] = new Color(bgra5551Data[i].ToVector4());
                    }
                    break;

                case SurfaceFormat.HalfSingle:
                    var halfSingleData = new HalfSingle[colorDataLength];
                    GetData(halfSingleData);

                    for (int i = 0; i < colorDataLength; i++)
                    {
                        colorData[i] = new Color(halfSingleData[i].ToVector4());
                    }

                    break;

                case SurfaceFormat.HalfVector2:
                    var halfVector2Data = new HalfVector2[colorDataLength];
                    GetData(halfVector2Data);

                    for (int i = 0; i < colorDataLength; i++)
                    {
                        colorData[i] = new Color(halfVector2Data[i].ToVector4());
                    }

                    break;

                case SurfaceFormat.HalfVector4:
                    var halfVector4Data = new HalfVector4[colorDataLength];
                    GetData(halfVector4Data);

                    for (int i = 0; i < colorDataLength; i++)
                    {
                        colorData[i] = new Color(halfVector4Data[i].ToVector4());
                    }

                    break;

                case SurfaceFormat.NormalizedByte2:
                    var normalizedByte2Data = new NormalizedByte2[colorDataLength];
                    GetData(normalizedByte2Data);

                    for (int i = 0; i < colorDataLength; i++)
                    {
                        colorData[i] = new Color(normalizedByte2Data[i].ToVector4());
                    }

                    break;

                case SurfaceFormat.NormalizedByte4:
                    var normalizedByte4Data = new NormalizedByte4[colorDataLength];
                    GetData(normalizedByte4Data);

                    for (int i = 0; i < colorDataLength; i++)
                    {
                        colorData[i] = new Color(normalizedByte4Data[i].ToVector4());
                    }

                    break;

                case SurfaceFormat.Rg32:
                    var rg32Data = new Rg32[colorDataLength];
                    GetData(rg32Data);

                    for (int i = 0; i < colorDataLength; i++)
                    {
                        colorData[i] = new Color(rg32Data[i].ToVector4());
                    }

                    break;

                case SurfaceFormat.Rgba64:
                    var rgba64Data = new Rgba64[colorDataLength];
                    GetData(rgba64Data);

                    for (int i = 0; i < colorDataLength; i++)
                    {
                        colorData[i] = new Color(rgba64Data[i].ToVector4());
                    }

                    break;

                case SurfaceFormat.Rgba1010102:
                    var rgba1010102Data = new Rgba1010102[colorDataLength];
                    GetData(rgba1010102Data);

                    for (int i = 0; i < colorDataLength; i++)
                    {
                        colorData[i] = new Color(rgba1010102Data[i].ToVector4());
                    }

                    break;

                default:
                    throw new Exception("Texture surface format not supported");
            }

            return colorData;
        }
    }
}
