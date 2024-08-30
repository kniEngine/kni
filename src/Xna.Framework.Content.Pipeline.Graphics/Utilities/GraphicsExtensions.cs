// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    static class GraphicsExtensions
    {
        public static int GetSize(this VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                    return 4;
                case VertexElementFormat.Vector2:
                    return 8;
                case VertexElementFormat.Vector3:
                    return 12;
                case VertexElementFormat.Vector4:
                    return 16;
                case VertexElementFormat.Color:
                    return 4;
                case VertexElementFormat.Byte4:
                    return 4;
                case VertexElementFormat.Short2:
                    return 4;
                case VertexElementFormat.Short4:
                    return 8;
                case VertexElementFormat.NormalizedShort2:
                    return 4;
                case VertexElementFormat.NormalizedShort4:
                    return 8;
                case VertexElementFormat.HalfVector2:
                    return 4;
                case VertexElementFormat.HalfVector4:
                    return 8;

                default:
                    return 0;
            }
        }

        public static bool IsCompressedFormat(this SurfaceFormat format)
        {
            switch (format)
            {
                case SurfaceFormat.Dxt1:
                case SurfaceFormat.Dxt1a:
                case SurfaceFormat.Dxt1SRgb:
                case SurfaceFormat.Dxt3:
                case SurfaceFormat.Dxt3SRgb:
                case SurfaceFormat.Dxt5:
                case SurfaceFormat.Dxt5SRgb:
                case SurfaceFormat.RgbaAtcExplicitAlpha:
                case SurfaceFormat.RgbaAtcInterpolatedAlpha:
                case SurfaceFormat.RgbaPvrtc2Bpp:
                case SurfaceFormat.RgbaPvrtc4Bpp:
                case SurfaceFormat.RgbEtc1:
                case SurfaceFormat.Rgb8Etc2:
                case SurfaceFormat.Srgb8Etc2:
                case SurfaceFormat.Rgb8A1Etc2:
                case SurfaceFormat.Srgb8A1Etc2:
                case SurfaceFormat.Rgba8Etc2:
                case SurfaceFormat.SRgb8A8Etc2:
                case SurfaceFormat.RgbPvrtc2Bpp:
                case SurfaceFormat.RgbPvrtc4Bpp:
                    return true;

                default:
                    return false;
            }
        }

        public static int GetSize(this SurfaceFormat surfaceFormat)
        {
            switch (surfaceFormat)
            {
                case SurfaceFormat.Dxt1:
                case SurfaceFormat.Dxt1SRgb:
                case SurfaceFormat.Dxt1a:
                case SurfaceFormat.RgbPvrtc2Bpp:
                case SurfaceFormat.RgbaPvrtc2Bpp:
                case SurfaceFormat.RgbPvrtc4Bpp:
                case SurfaceFormat.RgbaPvrtc4Bpp:
                case SurfaceFormat.RgbEtc1:
                case SurfaceFormat.Rgb8Etc2:
                case SurfaceFormat.Srgb8Etc2:
                case SurfaceFormat.Rgb8A1Etc2:
                case SurfaceFormat.Srgb8A1Etc2:
                    // One texel in DXT1, PVRTC (2bpp and 4bpp) and ETC1 is a minimum 4x4 block (8x4 for PVRTC 2bpp), which is 8 bytes
                    return 8;
                case SurfaceFormat.Dxt3:
                case SurfaceFormat.Dxt3SRgb:
                case SurfaceFormat.Dxt5:
                case SurfaceFormat.Dxt5SRgb:
                case SurfaceFormat.RgbaAtcExplicitAlpha:
                case SurfaceFormat.RgbaAtcInterpolatedAlpha:
                case SurfaceFormat.Rgba8Etc2:
                case SurfaceFormat.SRgb8A8Etc2:
                    // One texel in DXT3 and DXT5 is a minimum 4x4 block, which is 16 bytes
                    return 16;
                case SurfaceFormat.Alpha8:
                    return 1;
                case SurfaceFormat.Bgr565:
                case SurfaceFormat.Bgra4444:
                case SurfaceFormat.Bgra5551:
                case SurfaceFormat.HalfSingle:
                case SurfaceFormat.NormalizedByte2:
                    return 2;
                case SurfaceFormat.Color:
                case SurfaceFormat.ColorSRgb:
                case SurfaceFormat.ColorSRgba:
                case SurfaceFormat.Single:
                case SurfaceFormat.Rg32:
                case SurfaceFormat.HalfVector2:
                case SurfaceFormat.NormalizedByte4:
                case SurfaceFormat.Rgba1010102:
                case SurfaceFormat.Bgra32:
                case SurfaceFormat.Bgra32SRgb:
                case SurfaceFormat.Bgr32:
                case SurfaceFormat.Bgr32SRgb:
                    return 4;
                case SurfaceFormat.HalfVector4:
                case SurfaceFormat.Rgba64:
                case SurfaceFormat.Vector2:
                    return 8;
                case SurfaceFormat.Vector4:
                    return 16;

                default:
                    throw new ArgumentException();
            }
        }

        public static void GetBlockSize(this SurfaceFormat surfaceFormat, out int width, out int height)
        {
            switch (surfaceFormat)
            {
                case SurfaceFormat.RgbPvrtc2Bpp:
                case SurfaceFormat.RgbaPvrtc2Bpp:
                    width = 8;
                    height = 4;
                    break;
                case SurfaceFormat.Dxt1:
                case SurfaceFormat.Dxt1SRgb:
                case SurfaceFormat.Dxt1a:
                case SurfaceFormat.Dxt3:
                case SurfaceFormat.Dxt3SRgb:
                case SurfaceFormat.Dxt5:
                case SurfaceFormat.Dxt5SRgb:
                case SurfaceFormat.RgbPvrtc4Bpp:
                case SurfaceFormat.RgbaPvrtc4Bpp:
                case SurfaceFormat.RgbEtc1:
                case SurfaceFormat.Rgb8Etc2:
                case SurfaceFormat.Srgb8Etc2:
                case SurfaceFormat.Rgb8A1Etc2:
                case SurfaceFormat.Srgb8A1Etc2:
                case SurfaceFormat.Rgba8Etc2:
                case SurfaceFormat.SRgb8A8Etc2:
                case SurfaceFormat.RgbaAtcExplicitAlpha:
                case SurfaceFormat.RgbaAtcInterpolatedAlpha:
                    width = 4;
                    height = 4;
                    break;

                default:
                    width = 1;
                    height = 1;
                    break;
            }
        }

    }
}
