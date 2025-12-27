// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021-2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    [ContentProcessorAttribute(DisplayName = "Font Texture - KNI")]
    public class FontTextureProcessor : ContentProcessor<Texture2DContent, SpriteFontContent>
    {
        private Vector4 transparentPixel = Color.Magenta.ToVector4();
        private bool _generateMipmaps = false;

        [DefaultValue(' ')]
        public virtual char FirstCharacter { get; set; }

        [DefaultValue(true)]
        public virtual bool PremultiplyAlpha { get; set; }

        public virtual TextureProcessorOutputFormat TextureFormat { get; set; }

        [DefaultValue(false)]
        [DisplayName("Generate Mipmaps")]
        public virtual bool GenerateMipmaps
        {
            get { return _generateMipmaps; }
            set { _generateMipmaps = value; }
        }

        public FontTextureProcessor()
        {
            FirstCharacter = ' ';
            PremultiplyAlpha = true;
        }


        public override SpriteFontContent Process(Texture2DContent input, ContentProcessorContext context)
        {
            SpriteFontContent output = new SpriteFontContent();

            BitmapContent fontBitmap = input.Faces[0][0];
            SurfaceFormat faceFormat;
            fontBitmap.TryGetFormat(out faceFormat);
            if (faceFormat != SurfaceFormat.Vector4)
            {
                PixelBitmapContent<Vector4> colorFace = new PixelBitmapContent<Vector4>(fontBitmap.Width, fontBitmap.Height);
                BitmapContent.Copy(fontBitmap, colorFace);
                fontBitmap = colorFace;
            }

            // extract the glyphs from the texture and map them to a list of characters.
            // we need to call GtCharacterForIndex for each glyph in the Texture to 
            // get the char for that glyph, by default we start at ' ' then '!' and then ASCII
            // after that.
            Dictionary<char, FontGlyph> glyphs = ImportFont(input, context, (PixelBitmapContent<Vector4>)fontBitmap);

            // Optimize glyphs.
            foreach (FontGlyph glyph in glyphs.Values)
                glyph.Crop();

            // calculate line spacing.
            foreach (FontGlyph glyph in glyphs.Values)
                output.VerticalLineSpacing = Math.Max(output.VerticalLineSpacing, glyph.Subrect.Height);

            // We need to know how to pack the glyphs.
            TextureProcessorOutputFormat format = TextureFormat;
            if (format == TextureProcessorOutputFormat.Compressed)
                format = GraphicsUtil.GetTextureFormatForPlatform(format, context.TargetPlatform);
            bool requiresPot = GraphicsUtil.RequiresPowerOfTwo(format, GenerateMipmaps ,context.TargetPlatform, context.TargetProfile);
            bool requiresSquare = GraphicsUtil.RequiresSquare(format, context.TargetPlatform);

            BitmapContent glyphAtlas = GlyphPacker.ArrangeGlyphs(glyphs.Values, requiresPot, requiresSquare);

            foreach (char ch in glyphs.Keys)
            {
                FontGlyph glyph = glyphs[ch];

                output.CharacterMap.Add(ch);

                Rectangle texRect = glyph.Subrect;
                output.Glyphs.Add(texRect);

                Rectangle cropping;
                cropping.X = (int)glyph.XOffset;
                cropping.Y = (int)glyph.YOffset;
                cropping.Width = glyph.Width;
                cropping.Height = glyph.Height;
                output.Cropping.Add(cropping);

                output.Kerning.Add(glyph.Kerning.ToVector3());
            }

            if (PremultiplyAlpha)
                TextureProcessor.ProcessPremultiplyAlpha((PixelBitmapContent<Vector4>)glyphAtlas);

            output.Texture.Faces[0].Add(glyphAtlas);

            if (GenerateMipmaps)
                output.Texture.GenerateMipmaps(false);

            // Perform the final texture conversion.
            try
            {
                TextureProcessorOutputFormat textureFormat = GraphicsUtil.GetTextureFormatForPlatform(TextureFormat, context.TargetPlatform);

                switch (textureFormat)
                {
                    case TextureProcessorOutputFormat.NoChange:
                        // We do nothing in this case.
                        break;
                    case TextureProcessorOutputFormat.Color:
                        // If this is color just make sure the format is right and return it.
                        input.ConvertBitmapType(typeof(PixelBitmapContent<Color>));
                        break;
                    case TextureProcessorOutputFormat.Color16Bit:
                        // Handle this common compression format.
                        GraphicsUtil.CompressColor16Bit(input);
                        break;

                    case TextureProcessorOutputFormat.AtcCompressed:
                        {
                            // If sharp alpha is required (for a font texture page), use 16-bit color instead of PVR
                            input.ConvertBitmapType(typeof(PixelBitmapContent<Vector4>)); // Make sure we're in a floating point format
                            GraphicsUtil.CompressColor16Bit(input);
                            //GraphicsUtil.CompressAti(input);
                        }
                        break;
                    case TextureProcessorOutputFormat.DxtCompressed:
                        {
                            // Compress the greyscale font texture page using a specially-formulated DXT3 mode
                            input.ConvertBitmapType(typeof(PixelBitmapContent<Vector4>)); // Make sure we're in a floating point format
                            GraphicsUtil.CompressFontDXT3(input, context);
                            //GraphicsUtil.CompressDxt(input, context);
                        }
                        break;
                    case TextureProcessorOutputFormat.Etc1Compressed:
                        {
                            // If sharp alpha is required (for a font texture page), use 16-bit color instead of PVR
                            input.ConvertBitmapType(typeof(PixelBitmapContent<Vector4>)); // Make sure we're in a floating point format
                            GraphicsUtil.CompressColor16Bit(input);
                            //GraphicsUtil.CompressEtc1(input, context);
                        }
                        break;
                    case TextureProcessorOutputFormat.PvrCompressed:
                        {
                            // If sharp alpha is required (for a font texture page), use 16-bit color instead of PVR
                            input.ConvertBitmapType(typeof(PixelBitmapContent<Vector4>)); // Make sure we're in a floating point format
                            GraphicsUtil.CompressColor16Bit(input);
                            //GraphicsUtil.CompressPvrtc(input, context);
                        }
                        break;
                }
            }
            catch (EntryPointNotFoundException ex)
            {
                context.Logger.LogImportantMessage("Could not find the entry point to compress the texture. " + ex.ToString());
                throw ex;
            }
            catch (DllNotFoundException ex)
            {
                context.Logger.LogImportantMessage("Could not compress texture. Required shared lib is missing. " + ex.ToString());
                throw ex;
            }
            catch (Exception ex)
            {
                context.Logger.LogImportantMessage("Could not convert texture. " + ex.ToString());
                throw ex;
            }

            return output;
        }

        private Dictionary<char, FontGlyph> ImportFont(Texture2DContent input, ContentProcessorContext context, PixelBitmapContent<Vector4> bitmap)
        {
            Dictionary<char, FontGlyph> glyphs = new Dictionary<char, FontGlyph>();

            List<Rectangle> regions = new List<Rectangle>();
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    if (bitmap.GetPixel(x, y) != transparentPixel)
                    {
                        // if we don't have a region that has this pixel already
                        Rectangle rect = regions.Find(r =>
                        {
                            return r.Contains(x, y);
                        });
                        if (rect == Rectangle.Empty)
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
                            x += rect.Width;
                        }
                    }
                }
            }

            for (int i = 0; i < regions.Count; i++)
            {
                char ch = (char)(FirstCharacter + i);

                Rectangle rect = regions[i];
                PixelBitmapContent<Vector4> glyphBitmap = new PixelBitmapContent<Vector4>(rect.Width, rect.Height);
                BitmapContent.Copy(bitmap, rect, glyphBitmap, new Rectangle(0, 0, rect.Width, rect.Height));

                GlyphKerning kerning = new GlyphKerning();
                kerning.AdvanceWidth = glyphBitmap.Width;

                FontGlyph glyph = new FontGlyph(glyphBitmap);
                glyph.Kerning = kerning;

                glyphs.Add(ch, glyph);
                //newbitmap.Save(""+ch+".png", System.Drawing.Imaging.ImageFormat.Png);
            }

            return glyphs;
        }

    }
}
