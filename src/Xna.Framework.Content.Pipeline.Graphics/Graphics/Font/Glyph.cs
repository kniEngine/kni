// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021-2023 Nick Kastellanos

using System;
using System.Collections.Generic;
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

        public override string ToString()
        {
            return String.Format("{{LeftBearing: {0}, AdvanceWidth: {1}, RightBearing: {2} }}",
                LeftBearing, AdvanceWidth, RightBearing);
        }
    }

    // Represents a single character within a font.
    internal class FontGlyph
    {
        // Constructor.
        public FontGlyph(PixelBitmapContent<Vector4> bitmap)
        {
            Bitmap = bitmap;
            Subrect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            Width = bitmap.Width;
            Height = bitmap.Height;
        }

        // Glyph image data (may only use a portion of a larger bitmap).
        public PixelBitmapContent<Vector4> Bitmap;
        public Rectangle Subrect;

        public int FontBitmapLeft;
        public int FontBitmapTop;

        // Layout information.
        public int XOffset;
        public int YOffset;
        public int Width;
        public int Height;

        public float XAdvance;

        public GlyphKerning Kerning;

        public float GlyphMetricTopBearing;
#if DEBUG
        public float GlyphMetricLeftBearing;
        public float GlyphMetricWidth;
        public float GlyphMetricXAdvance;
#endif

        // Crops unused space from around the edge of a glyph bitmap.
        public void Crop()
        {
            // Crop the top.
            while (Subrect.Height > 1 && IsEmptyRow(this.Bitmap, Subrect.X, Subrect.Y, Subrect.Width))
            {
                YOffset++;
                Subrect.Y++;
                Subrect.Height--;
            }

            // Crop the bottom.
            while (Subrect.Height > 1 && IsEmptyRow(this.Bitmap, Subrect.X, Subrect.Bottom - 1, Subrect.Width))
            {
                Subrect.Height--;
            }

            // Crop the left.
            while (Subrect.Width > 1 && IsEmptyColumn(this.Bitmap, Subrect.X, Subrect.Y, Subrect.Height))
            {
                XOffset++;
                Subrect.X++;
                Subrect.Width--;
            }

            // Crop the right.
            while (Subrect.Width > 1 && IsEmptyColumn(this.Bitmap, Subrect.Right - 1, Subrect.Y, Subrect.Height))
            {
                Subrect.Width--;

                XAdvance++;
            }
        }

        const float TransparentAlpha = 0f;

        public static bool IsEmptyRow(PixelBitmapContent<Vector4> bmp, int rX, int rY, int rW)
        {
            for (int x = 0; x < rW; x++)
            {
                float alpha = bmp.GetPixel(rX + x, rY + 0).W;
                if (alpha != TransparentAlpha)
                    return false;
            }

            return true;
        }

        public static bool IsEmptyColumn(PixelBitmapContent<Vector4> bmp, int rX, int rY, int rH)
        {
            for (int y = 0; y < rH; y++)
            {
                float alpha = bmp.GetPixel(rX + 0, rY + y).W;
                if (alpha != TransparentAlpha)
                    return false;
            }

            return true;
        }
    }

    internal class FontContent : ContentItem
    {
        public readonly Dictionary<char, FontGlyph> Glyphs = new Dictionary<char, FontGlyph>();

        public float MetricsHeight;
        public float MetricsAscender;
        public float MetricsDescender;

#if DEBUG
        public float FaceUnderlinePosition;
        public float FaceUnderlineThickness;
#endif
    }
}
