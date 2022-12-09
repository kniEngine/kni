// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;


namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
	// Crops unused space from around the edge of a glyph bitmap.
	internal static class GlyphCropper
	{
		public static void Crop(Glyph glyph)
		{
			// Crop the top.
			while (glyph.Subrect.Height > 1 && IsAlphaEntirely(glyph.Bitmap, glyph.Subrect.X, glyph.Subrect.Y, glyph.Subrect.Width, 1))
			{
				glyph.Subrect.Y++;
				glyph.Subrect.Height--;

				glyph.YOffset++;
			}

			// Crop the bottom.
			while (glyph.Subrect.Height > 1 && IsAlphaEntirely(glyph.Bitmap, glyph.Subrect.X, glyph.Subrect.Bottom - 1, glyph.Subrect.Width, 1))
			{
				glyph.Subrect.Height--;
			}

			// Crop the left.
			while (glyph.Subrect.Width > 1 && IsAlphaEntirely(glyph.Bitmap, glyph.Subrect.X, glyph.Subrect.Y, 1, glyph.Subrect.Height))
			{
				glyph.Subrect.X++;
				glyph.Subrect.Width--;

				glyph.XOffset++;
			}

			// Crop the right.
			while (glyph.Subrect.Width > 1 && IsAlphaEntirely(glyph.Bitmap, glyph.Subrect.Right - 1, glyph.Subrect.Y, 1, glyph.Subrect.Height))
			{
				glyph.Subrect.Width--;

				glyph.XAdvance++;
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
