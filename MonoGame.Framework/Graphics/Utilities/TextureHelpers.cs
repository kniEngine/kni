// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Microsoft.Xna.Platform.Graphics.Utilities
{
    public static class TextureHelpers
    {

        public static int GetClampedMultiSampleCount(SurfaceFormat surfaceFormat, int multiSampleCount, int maxMultiSampleCount)
        {
            if (multiSampleCount > 1)
            {
                // Round down MultiSampleCount to the nearest power of two
                // hack from http://stackoverflow.com/a/2681094
                // Note: this will return an incorrect, but large value
                // for very large numbers. That doesn't matter because
                // the number will get clamped below anyway in this case.
                int msc = multiSampleCount;
                msc = msc | (msc >> 1);
                msc = msc | (msc >> 2);
                msc = msc | (msc >> 4);
                msc -= (msc >> 1);

                // and clamp it to what the device can handle
                msc = Math.Min(msc, maxMultiSampleCount);

                return msc;
            }
            else return 0;
        }



        public static int CalculateMipLevels(bool mipMap, int width, int height = 0, int depth = 0)
        {
            if (!mipMap)
                return 1;

            int levels = 1;
            int size = Math.Max(Math.Max(width, height), depth);
            while (size > 1)
            {
                size = size / 2;
                levels++;
            }
            return levels;
        }

        public static void GetSizeForLevel(int width, int height, int level, out int w, out int h)
        {
            w = width;
            h = height;
            while (level > 0)
            {
                --level;
                w /= 2;
                h /= 2;
            }
            if (w == 0)
                w = 1;
            if (h == 0)
                h = 1;
        }

        public static void GetSizeForLevel(int width, int height, int depth, int level, out int w, out int h, out int d)
        {
            w = width;
            h = height;
            d = depth;
            while (level > 0)
            {
                --level;
                w /= 2;
                h /= 2;
                d /= 2;
            }
            if (w == 0)
                w = 1;
            if (h == 0)
                h = 1;
            if (d == 0)
                d = 1;
        }
            
        public static Color[] GetColorData(ITexture2DStrategy texture2D)
        {
            int colorDataLength = texture2D.Width * texture2D.Height;
            Color[] colorData = new Color[colorDataLength];

            switch (texture2D.Format)
            {
                case SurfaceFormat.Single:
                    float[] floatData = new float[colorDataLength];
                    texture2D.GetData<float>(0, 0, texture2D.Bounds, floatData, 0, floatData.Length);

                    for (int i = 0; i < colorDataLength; i++)
                    {
                        float brightness = floatData[i];
                        // Export as a greyscale image.
                        colorData[i] = new Color(brightness, brightness, brightness);
                    }
                    break;

                case SurfaceFormat.Color:
                    texture2D.GetData<Color>(0, 0, texture2D.Bounds, colorData, 0, colorData.Length);
                    break;

                case SurfaceFormat.Alpha8:
                    Alpha8[] alpha8Data = new Alpha8[colorDataLength];
                    texture2D.GetData<Alpha8>(0, 0, texture2D.Bounds, alpha8Data, 0, alpha8Data.Length);

                    for (int i = 0; i < colorDataLength; i++)
                        colorData[i] = new Color(alpha8Data[i].ToVector4());
                    break;

                case SurfaceFormat.Bgr565:
                    Bgr565[] bgr565Data = new Bgr565[colorDataLength];
                    texture2D.GetData<Bgr565>(0, 0, texture2D.Bounds, bgr565Data, 0, bgr565Data.Length);

                    for (int i = 0; i < colorDataLength; i++)
                        colorData[i] = new Color(bgr565Data[i].ToVector4());
                    break;

                case SurfaceFormat.Bgra4444:
                    Bgra4444[] bgra4444Data = new Bgra4444[colorDataLength];
                    texture2D.GetData<Bgra4444>(0, 0, texture2D.Bounds, bgra4444Data, 0, bgra4444Data.Length);

                    for (int i = 0; i < colorDataLength; i++)
                        colorData[i] = new Color(bgra4444Data[i].ToVector4());
                    break;

                case SurfaceFormat.Bgra5551:
                    Bgra5551[] bgra5551Data = new Bgra5551[colorDataLength];
                    texture2D.GetData<Bgra5551>(0, 0, texture2D.Bounds, bgra5551Data, 0, bgra5551Data.Length);

                    for (int i = 0; i < colorDataLength; i++)
                        colorData[i] = new Color(bgra5551Data[i].ToVector4());
                    break;

                case SurfaceFormat.HalfSingle:
                    HalfSingle[] halfSingleData = new HalfSingle[colorDataLength];
                    texture2D.GetData<HalfSingle>(0, 0, texture2D.Bounds, halfSingleData, 0, halfSingleData.Length);

                    for (int i = 0; i < colorDataLength; i++)
                        colorData[i] = new Color(halfSingleData[i].ToVector4());
                    break;

                case SurfaceFormat.HalfVector2:
                    HalfVector2[] halfVector2Data = new HalfVector2[colorDataLength];
                    texture2D.GetData<HalfVector2>(0, 0, texture2D.Bounds, halfVector2Data, 0, halfVector2Data.Length);

                    for (int i = 0; i < colorDataLength; i++)
                        colorData[i] = new Color(halfVector2Data[i].ToVector4());
                    break;

                case SurfaceFormat.HalfVector4:
                    HalfVector4[] halfVector4Data = new HalfVector4[colorDataLength];
                    texture2D.GetData<HalfVector4>(0, 0, texture2D.Bounds, halfVector4Data, 0, halfVector4Data.Length);

                    for (int i = 0; i < colorDataLength; i++)
                        colorData[i] = new Color(halfVector4Data[i].ToVector4());
                    break;

                case SurfaceFormat.NormalizedByte2:
                    NormalizedByte2[] normalizedByte2Data = new NormalizedByte2[colorDataLength];
                    texture2D.GetData<NormalizedByte2>(0, 0, texture2D.Bounds, normalizedByte2Data, 0, normalizedByte2Data.Length);

                    for (int i = 0; i < colorDataLength; i++)
                        colorData[i] = new Color(normalizedByte2Data[i].ToVector4());
                    break;

                case SurfaceFormat.NormalizedByte4:
                    NormalizedByte4[] normalizedByte4Data = new NormalizedByte4[colorDataLength];
                    texture2D.GetData<NormalizedByte4>(0, 0, texture2D.Bounds, normalizedByte4Data, 0, normalizedByte4Data.Length);

                    for (int i = 0; i < colorDataLength; i++)
                        colorData[i] = new Color(normalizedByte4Data[i].ToVector4());
                    break;

                case SurfaceFormat.Rg32:
                    Rg32[] rg32Data = new Rg32[colorDataLength];
                    texture2D.GetData<Rg32>(0, 0, texture2D.Bounds, rg32Data, 0, rg32Data.Length);

                    for (int i = 0; i < colorDataLength; i++)
                        colorData[i] = new Color(rg32Data[i].ToVector4());
                    break;

                case SurfaceFormat.Rgba64:
                    Rgba64[] rgba64Data = new Rgba64[colorDataLength];
                    texture2D.GetData<Rgba64>(0, 0, texture2D.Bounds, rgba64Data, 0, rgba64Data.Length);

                    for (int i = 0; i < colorDataLength; i++)
                        colorData[i] = new Color(rgba64Data[i].ToVector4());
                    break;

                case SurfaceFormat.Rgba1010102:
                    Rgba1010102[] rgba1010102Data = new Rgba1010102[colorDataLength];
                    texture2D.GetData<Rgba1010102>(0, 0, texture2D.Bounds, rgba1010102Data, 0, rgba1010102Data.Length);

                    for (int i = 0; i < colorDataLength; i++)
                        colorData[i] = new Color(rgba1010102Data[i].ToVector4());
                    break;

                default:
                    throw new Exception("Texture surface format not supported");
            }

            return colorData;
        }


    }
}
