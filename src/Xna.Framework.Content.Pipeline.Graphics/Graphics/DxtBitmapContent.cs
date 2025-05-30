﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;
using NVTT = Nvidia.TextureTools;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public abstract class DxtBitmapContent : BitmapContent
    {
        private byte[] _bitmapData;
        private int _blockSize;
        private SurfaceFormat _format;

        private int _nvttWriteOffset;

        protected DxtBitmapContent(int blockSize)
        {
            if (!((blockSize == 8) || (blockSize == 16)))
                throw new ArgumentException("Invalid block size");
            _blockSize = blockSize;
            TryGetFormat(out _format);
        }

        protected DxtBitmapContent(int blockSize, int width, int height)
            : this(blockSize)
        {
            Width = width;
            Height = height;
        }

        public override byte[] GetPixelData()
        {
            return _bitmapData;
        }

        public override void SetPixelData(byte[] sourceData)
        {
            _bitmapData = sourceData;
        }

        private void NvttBeginImageCallback(int size, int width, int height, int depth, int face, int miplevel)
        {
            _bitmapData = new byte[size];
            _nvttWriteOffset = 0;
        }

        private bool NvttWriteImageCallback(IntPtr data, int length)
        {
            Marshal.Copy(data, _bitmapData, _nvttWriteOffset, length);
            _nvttWriteOffset += length;
            return true;
        }

        private void NvttEndImageCallback()
        {
        }
        
        private unsafe static void PrepareNVTT(byte[] data)
        {
            fixed (byte* pdata = data)
            {
                int count = data.Length / 4;
                for (int x = 0; x < count; x++)
                {
                    // NVTT wants BGRA where our source is RGBA so
                    // we swap the red and blue channels.
                    byte r = pdata[x*4+0];
                    byte b = pdata[x*4+2];
                    pdata[x*4+0] = b;
                    pdata[x*4+2] = r;
                }
            }
        }

        private unsafe static bool PrepareNVTT_DXT1(byte[] data)
        {
            bool hasTransparency = false;

            fixed (byte* pdata = data)
            {
                int count = data.Length/4;
                for (int x = 0; x < count; x++)
                {
                    // NVTT wants BGRA where our source is RGBA so
                    // we swap the red and blue channels.
                    byte r = pdata[x*4+0];
                    byte b = pdata[x*4+2];
                    pdata[x*4+0] = b;
                    pdata[x*4+2] = r;

                    // Look for non-opaque pixels.
                    hasTransparency = (hasTransparency || pdata[x*4+3] < 255);
                }
            }

            return hasTransparency;
        }

        protected unsafe override bool TryCopyFrom(BitmapContent sourceBitmap, Rectangle sourceRegion, Rectangle destinationRegion)
        {
            SurfaceFormat sourceFormat;
            if (!sourceBitmap.TryGetFormat(out sourceFormat))
                return false;

            SurfaceFormat format;
            TryGetFormat(out format);

            // A shortcut for copying the entire bitmap to another bitmap of the same type and format
            if (format == sourceFormat && (sourceRegion == new Rectangle(0, 0, Width, Height)) && sourceRegion == destinationRegion)
            {
                SetPixelData(sourceBitmap.GetPixelData());
                return true;
            }

            // TODO: Add a XNA unit test to see what it does
            // my guess is that this is invalid for DXT.
            //
            // Destination region copy is not yet supported
            if (destinationRegion != new Rectangle(0, 0, Width, Height))
                return false;

            // If the source is not Vector4 or requires resizing, send it through BitmapContent.Copy
            if (!(sourceBitmap is PixelBitmapContent<Vector4>) || sourceRegion.Width != destinationRegion.Width || sourceRegion.Height != destinationRegion.Height)
            {
                try
                {
                    BitmapContent.Copy(sourceBitmap, sourceRegion, this, destinationRegion);
                    return true;
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            }

            // NVTT wants 8bit data in BGRA format.
            PixelBitmapContent<Color> colorBitmap = new PixelBitmapContent<Color>(sourceBitmap.Width, sourceBitmap.Height);
            BitmapContent.Copy(sourceBitmap, colorBitmap);
            byte[] sourceData = colorBitmap.GetPixelData();

            NVTT.AlphaMode alphaMode;
            NVTT.Format outputFormat;
            bool alphaDither = false;
            switch (format)
            {
                case SurfaceFormat.Dxt1:
                case SurfaceFormat.Dxt1SRgb:
                {
                    bool hasTransparency = PrepareNVTT_DXT1(sourceData);
                    outputFormat = hasTransparency ? NVTT.Format.DXT1a : NVTT.Format.DXT1;
                    alphaMode = hasTransparency ? NVTT.AlphaMode.Transparency : NVTT.AlphaMode.None;
                    alphaDither = true;
                    break;
                }
                case SurfaceFormat.Dxt3:
                case SurfaceFormat.Dxt3SRgb:
                {
                    PrepareNVTT(sourceData);
                    outputFormat = NVTT.Format.DXT3;
                    alphaMode = NVTT.AlphaMode.Transparency;
                    break;
                }
                case SurfaceFormat.Dxt5:
                case SurfaceFormat.Dxt5SRgb:
                {
                    PrepareNVTT(sourceData);
                    outputFormat = NVTT.Format.DXT5;
                    alphaMode = NVTT.AlphaMode.Transparency;
                    break;
                }
                default:
                    throw new InvalidOperationException("Invalid DXT surface format!");
            }

            using (NVTT.Compressor dxtCompressor = new NVTT.Compressor())
            {
                fixed (byte* pData = &sourceData[0])
                {
                    dxtCompressor.InputOptions.SetTextureLayout(NVTT.TextureType.Texture2D, colorBitmap.Width, colorBitmap.Height, 1);
                    dxtCompressor.InputOptions.SetMipmapData((IntPtr)pData, colorBitmap.Width, colorBitmap.Height, 1, 0, 0);
                    dxtCompressor.InputOptions.SetMipmapGeneration(false);
                    dxtCompressor.InputOptions.SetGamma(1.0f, 1.0f);
                    dxtCompressor.InputOptions.SetAlphaMode(alphaMode);

                    dxtCompressor.CompressionOptions.SetFormat(outputFormat);
                    dxtCompressor.CompressionOptions.SetQuality(NVTT.Quality.Normal);

                    // TODO: This isn't working which keeps us from getting the
                    //       same alpha dither behavior on DXT1 as XNA.
                    //       See: https://github.com/MonoGame/MonoGame/issues/6259
                    //if (alphaDither)
                    //    compressionOptions.SetQuantization(false, false, true);

                    NVTT.OutputOptions.BeginImageDelegate beginImageDelegate = new NVTT.OutputOptions.BeginImageDelegate(NvttBeginImageCallback);
                    NVTT.OutputOptions.WriteDataDelegate  outputDelegate = new NVTT.OutputOptions.WriteDataDelegate(NvttWriteImageCallback);
                    NVTT.OutputOptions.EndImageDelegate   endImageDelegate = new NVTT.OutputOptions.EndImageDelegate(NvttEndImageCallback);
                    GCHandle beginImageHandle = GCHandle.Alloc(beginImageDelegate);
                    GCHandle writeHandle      = GCHandle.Alloc(outputDelegate);
                    GCHandle endImageHandle   = GCHandle.Alloc(endImageDelegate);
                    try
                    {
                        dxtCompressor.OutputOptions.SetOutputHeader(false);
                        dxtCompressor.OutputOptions.SetOutputOptionsOutputHandler(beginImageDelegate, outputDelegate, endImageDelegate);

                        dxtCompressor.Compress();
                    }
                    finally
                    {
                        beginImageHandle.Free();
                        writeHandle.Free();
                        endImageHandle.Free();
                    }
                }
            }

            return true;
        }

        protected override bool TryCopyTo(BitmapContent destinationBitmap, Rectangle sourceRegion, Rectangle destinationRegion)
        {
            SurfaceFormat destinationFormat;
            if (!destinationBitmap.TryGetFormat(out destinationFormat))
                return false;

            SurfaceFormat format;
            TryGetFormat(out format);

            // A shortcut for copying the entire bitmap to another bitmap of the same type and format
            Rectangle fullRegion = new Rectangle(0, 0, Width, Height);
            if ((format == destinationFormat) && (sourceRegion == fullRegion) && (sourceRegion == destinationRegion))
            {
                destinationBitmap.SetPixelData(GetPixelData());
                return true;
            }

            // No other support for copying from a DXT texture yet
            return false;
        }
    }
}
