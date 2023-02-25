// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021-2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;


namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    [ContentProcessorAttribute(DisplayName = "Font Texture - MonoGame")]
    public class FontTextureProcessor : ContentProcessor<Texture2DContent, SpriteFontContent>
    {
        private Color transparentPixel = Color.Magenta;

        [DefaultValue(' ')]
        public virtual char FirstCharacter { get; set; }

        [DefaultValue(true)]
        public virtual bool PremultiplyAlpha { get; set; }

        public virtual TextureProcessorOutputFormat TextureFormat { get; set; }

        public FontTextureProcessor()
        {
            FirstCharacter = ' ';
            PremultiplyAlpha = true;
        }


        public override SpriteFontContent Process(Texture2DContent input, ContentProcessorContext context)
        {
            var output = new SpriteFontContent();

            BitmapContent fontBitmap = input.Faces[0][0];
            SurfaceFormat faceFormat;
            fontBitmap.TryGetFormat(out faceFormat);
            if (faceFormat != SurfaceFormat.Color)
            {
                var colorFace = new PixelBitmapContent<Color>(fontBitmap.Width, fontBitmap.Height);
                BitmapContent.Copy(fontBitmap, colorFace);
                fontBitmap = colorFace;
            }

            // extract the glyphs from the texture and map them to a list of characters.
            // we need to call GtCharacterForIndex for each glyph in the Texture to 
            // get the char for that glyph, by default we start at ' ' then '!' and then ASCII
            // after that.
            Dictionary<char, FontGlyph> glyphs = ImportFont(input, context, (PixelBitmapContent<Color>)fontBitmap);

            // Optimize glyphs.
            foreach (var glyph in glyphs.Values)
                glyph.Crop();

            // calculate line spacing.
            foreach (var glyph in glyphs.Values)
                output.VerticalLineSpacing = Math.Max(output.VerticalLineSpacing, glyph.Subrect.Height);

            // Get the platform specific texture profile.
            var texProfile = TextureProfile.ForPlatform(context.TargetPlatform);

            // We need to know how to pack the glyphs.
            bool requiresPot, requiresSquare;
            texProfile.Requirements(context, TextureFormat, out requiresPot, out requiresSquare);

            BitmapContent glyphAtlas = GlyphPacker.ArrangeGlyphs(glyphs.Values, requiresPot, requiresSquare);

            foreach (char ch in glyphs.Keys)
            {
                FontGlyph glyph = glyphs[ch];

                output.CharacterMap.Add(ch);

                var texRect = glyph.Subrect;
                output.Glyphs.Add(texRect);

                Rectangle cropping;
                cropping.X = (int)glyph.XOffset;
                cropping.Y = (int)glyph.YOffset;
                cropping.Width = glyph.Width;
                cropping.Height = glyph.Height;
                output.Cropping.Add(cropping);

                output.Kerning.Add(glyph.Kerning.ToVector3());
            }

            ProcessPremultiplyAlpha(glyphAtlas);

            output.Texture.Faces[0].Add(glyphAtlas);

            // Perform the final texture conversion.
            texProfile.ConvertTexture(context, output.Texture, TextureFormat, true);

            return output;
        }

        private Dictionary<char, FontGlyph> ImportFont(Texture2DContent input, ContentProcessorContext context, PixelBitmapContent<Color> bitmap)
        {
            Dictionary<char, FontGlyph> glyphs = new Dictionary<char, FontGlyph>();

            var regions = new List<Rectangle>();
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    if (bitmap.GetPixel(x, y) != transparentPixel)
                    {
                        // if we don't have a region that has this pixel already
                        var re = regions.Find(r =>
                        {
                            return r.Contains(x, y);
                        });
                        if (re == Rectangle.Empty)
                        {
                            // we have found the top, left of a image. 
                            // we now need to scan for the 'bounds'
                            int top = y;
                            int bottom = y;
                            int left = x;
                            int right = x;
                            while (bitmap.GetPixel(right, bottom) != transparentPixel)
                                right++;
                            while (bitmap.GetPixel(left, bottom) != transparentPixel)
                                bottom++;
                            // we got a glyph :)
                            regions.Add(new Rectangle(left, top, right - left, bottom - top));
                            x = right;
                        }
                        else
                        {
                            x += re.Width;
                        }
                    }
                }
            }

            for (int i = 0; i < regions.Count; i++)
            {
                var character = (char)(FirstCharacter + i);

                var rect = regions[i];
                var glyphBitmap = new PixelBitmapContent<Color>(rect.Width, rect.Height);
                BitmapContent.Copy(bitmap, rect, glyphBitmap, new Rectangle(0, 0, rect.Width, rect.Height));

                GlyphKerning kerning = new GlyphKerning();
                kerning.AdvanceWidth = glyphBitmap.Width;

                FontGlyph glyph = new FontGlyph(glyphBitmap);
                glyph.Kerning = kerning;

                glyphs.Add(character, glyph);
                //newbitmap.Save(""+character+".png", System.Drawing.Imaging.ImageFormat.Png);
            }

            return glyphs;
        }

        private unsafe void ProcessPremultiplyAlpha(BitmapContent bmp)
        {
            System.Diagnostics.Debug.Assert(bmp is PixelBitmapContent<Color>);

            if (PremultiplyAlpha)
            {
                byte[] data = bmp.GetPixelData();
                fixed (byte* pdata = data)
                {
                    int count = data.Length / 4;
                    for (int idx = 0; idx < count; idx++)
                    {
                        byte r = pdata[idx * 4 + 0];
                        byte g = pdata[idx * 4 + 1];
                        byte b = pdata[idx * 4 + 2];
                        byte a = pdata[idx * 4 + 3];

                        pdata[idx * 4 + 0] = (byte)((r * a) / 255);
                        pdata[idx * 4 + 1] = (byte)((g * a) / 255);
                        pdata[idx * 4 + 2] = (byte)((b * a) / 255);
                        //pdata[idx * 4 + 3] = a;
                    }
                }
                bmp.SetPixelData(data);
            }
        }
    }
}
