// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021-2023 Nick Kastellanos

using System;
using System.Collections.Generic;


namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    // Helper for arranging many small bitmaps onto a single larger surface.
    internal static class GlyphPacker
    {
        public static BitmapContent ArrangeGlyphs(ICollection<FontGlyph> glyphs, bool requirePOT, bool requireSquare)
        {
            // Build up a list of all the glyphs needing to be arranged.
            List<ArrangedFontGlyph> arrangedGlyphs = new List<ArrangedFontGlyph>();

            foreach (FontGlyph glyph in glyphs)
            {
                ArrangedFontGlyph arrangedGlyph = new ArrangedFontGlyph(glyph);
                arrangedGlyph.Bounds.Size = glyph.Subrect.Size;

                // Leave a one pixel border around every glyph in the output bitmap.
                arrangedGlyph.Bounds.Width += 2;
                arrangedGlyph.Bounds.Height += 2;

                arrangedGlyphs.Add(arrangedGlyph);
            }

            // Sort so the largest glyphs get arranged first.
            arrangedGlyphs.Sort();

            // Work out how big the output bitmap should be.
            int guessedWidth = GuessOutputWidth(glyphs);
            MaxRectsBin rectPacker = new MaxRectsBin(guessedWidth, 1024 * 2, GrowRule.Height);

            for (int i = 0; i < arrangedGlyphs.Count; i++)
            {
                ArrangedFontGlyph arrangedGlyph = arrangedGlyphs[i];
                Rectangle bounds = rectPacker.Insert(arrangedGlyph.Bounds.Width, arrangedGlyph.Bounds.Height, MaxRectsHeuristic.Bl);
                arrangedGlyph.Bounds.Location = bounds.Location;
            }

            // Create the merged output bitmap.
            int outputWidth = MakeValidTextureSize(rectPacker.UsedWidth, requirePOT);
            int outputHeight = MakeValidTextureSize(rectPacker.UsedHeight, requirePOT);

            if (requireSquare)
            {
                outputHeight = Math.Max(outputWidth, outputHeight);
                outputWidth = outputHeight;
            }

            return CopyGlyphsToOutput(arrangedGlyphs, outputWidth, outputHeight);
        }

        // Once arranging is complete, copies each glyph to its chosen position in the single larger output bitmap.
        static BitmapContent CopyGlyphsToOutput(List<ArrangedFontGlyph> arrangedGlyphs, int width, int height)
        {
            PixelBitmapContent<Vector4> output = new PixelBitmapContent<Vector4>(width, height);

            foreach (ArrangedFontGlyph glyph in arrangedGlyphs)
            {
                FontGlyph sourceGlyph = glyph.Glyph;
                Rectangle sourceRegion = sourceGlyph.Subrect;
                Rectangle destinationRegion = new Rectangle(glyph.Bounds.Location, sourceRegion.Size);
                destinationRegion.Offset(1,1);

                BitmapContent.Copy(sourceGlyph.Bitmap, sourceRegion, output, destinationRegion);

                sourceGlyph.Bitmap = output;
                sourceGlyph.Subrect = destinationRegion;
            }

            return output;
        }


        // Internal helper class keeps track of a glyph while it is being arranged.
        class ArrangedFontGlyph : IComparable<ArrangedFontGlyph>
        {
            public readonly FontGlyph Glyph;
            public Rectangle Bounds;

            public ArrangedFontGlyph(FontGlyph glyph)
            {
                Glyph = glyph;
            }

            int IComparable<ArrangedFontGlyph>.CompareTo(ArrangedFontGlyph other)
            {
                int aSize = this.Bounds.Height << 10  + this.Bounds.Width;
                int bSize = other.Bounds.Height << 10 + other.Bounds.Width;

                return bSize.CompareTo(aSize);
            }
        }

        // Heuristic guesses what might be a good output width for a list of glyphs.
        static int GuessOutputWidth(ICollection<FontGlyph> sourceGlyphs)
        {
            int maxWidth = 0;
            int totalSize = 0;

            foreach (FontGlyph glyph in sourceGlyphs)
            {
                maxWidth = Math.Max(maxWidth, glyph.Bitmap.Width+2);
                totalSize += (glyph.Bitmap.Width+2) * (glyph.Bitmap.Height+2);
            }

            int width = Math.Max((int)Math.Sqrt(totalSize), maxWidth);

            return MakeValidTextureSize(width, true);
        }


        // Rounds a value up to the next larger valid texture size.
        static int MakeValidTextureSize(int value, bool requirePowerOfTwo)
        {
            // In case we want to compress the texture, make sure the size is a multiple of 4.
            const int blockSize = 4;

            if (requirePowerOfTwo)
            {
                // Round up to a power of two.
                int powerOfTwo = blockSize;

                while (powerOfTwo < value)
                    powerOfTwo <<= 1;

                return powerOfTwo;
            }
            else
            {
                // Round up to the specified block size.
                return (value + blockSize - 1) & ~(blockSize - 1);
            }
        }
    }

}

