// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using System.Runtime.InteropServices;


namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    internal struct GlyphKerning
    {
        public float LeftBearing;
        public float AdvanceWidth;
        public float RightBearing;

        public Vector3 ToVector3()
        {
            return new Vector3(LeftBearing, AdvanceWidth, RightBearing);
        }
    }

    // Represents a single character within a font.
    internal class Glyph
    {
        // Constructor.
        public Glyph(uint glyphIndex, BitmapContent bitmap)
        {
            GlyphIndex = glyphIndex;
            Bitmap = bitmap;
            Subrect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            Width = bitmap.Width;
            Height = bitmap.Height;
        }

        // Font-specific index of glyph
        public uint GlyphIndex;

        // Glyph image data (may only use a portion of a larger bitmap).
        public BitmapContent Bitmap;
        public Rectangle Subrect;

        // Layout information.
        public float XOffset;
        public float YOffset;
        public int Width;
        public int Height;

        public float XAdvance;

        public GlyphKerning Kerning;

#if DEBUG
        public float GlyphMetricLeftBearing;
        public float GlyphMetricWidth;
        public float GlyphMetricXAdvance;
        public float GlyphBitmapLeft;
#endif

        // Crops unused space from around the edge of a glyph bitmap.
        public void Crop()
        {
            // Crop the top.
            while (Subrect.Height > 1 && IsAlphaEntirely(Bitmap, Subrect.X, Subrect.Y, Subrect.Width, 1))
            {
                Subrect.Y++;
                Subrect.Height--;

                YOffset++;
            }

            // Crop the bottom.
            while (Subrect.Height > 1 && IsAlphaEntirely(Bitmap, Subrect.X, Subrect.Bottom - 1, Subrect.Width, 1))
            {
                Subrect.Height--;
            }

            // Crop the left.
            while (Subrect.Width > 1 && IsAlphaEntirely(Bitmap, Subrect.X, Subrect.Y, 1, Subrect.Height))
            {
                Subrect.X++;
                Subrect.Width--;

                XOffset++;
            }

            // Crop the right.
            while (Subrect.Width > 1 && IsAlphaEntirely(Bitmap, Subrect.Right - 1, Subrect.Y, 1, Subrect.Height))
            {
                Subrect.Width--;

                XAdvance++;
            }
        }

        const byte TransparentAlpha = 0;

        // Checks whether an area of a bitmap contains entirely the specified alpha value.
        public static bool IsAlphaEntirely(BitmapContent bitmap, int rX, int rY, int rW, int rH)
        {
            if (bitmap is PixelBitmapContent<byte>)
            {
                var bmp = bitmap as PixelBitmapContent<byte>;
                for (int y = 0; y < rH; y++)
                {
                    for (int x = 0; x < rW; x++)
                    {
                        var alpha = bmp.GetPixel(rX + x, rY + y);
                        if (alpha != TransparentAlpha)
                            return false;
                    }
                }
                return true;
            }

            if (bitmap is PixelBitmapContent<Color>)
            {
                var bmp = bitmap as PixelBitmapContent<Color>;
                for (int y = 0; y < rH; y++)
                {
                    for (int x = 0; x < rW; x++)
                    {
                        var alpha = bmp.GetPixel(rX + x, rY + y).A;
                        if (alpha != TransparentAlpha)
                            return false;
                    }
                }
                return true;
            }

            throw new ArgumentException("Expected PixelBitmapContent<byte> or PixelBitmapContent<Color>, got " + bitmap.GetType().Name, "bitmap");
        }
    }
}
