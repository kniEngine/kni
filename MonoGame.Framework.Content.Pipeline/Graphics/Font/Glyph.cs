// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System.Runtime.InteropServices;


namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ABCFloat
    {
        public float A;
        public float B;
        public float C;

        public Vector3 ToVector3()
        {
            return new Vector3(A, B, C);
        }
    }

    // Represents a single character within a font.
    internal class Glyph
    {
        // Constructor.
        public Glyph(uint glyphIndex, BitmapContent bitmap, Rectangle? subrect = null)
        {
            GlyphIndex = glyphIndex;
            Bitmap = bitmap;
            Subrect = subrect.GetValueOrDefault(new Rectangle(0, 0, bitmap.Width, bitmap.Height));
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

        public ABCFloat CharacterWidths;
    }
}
