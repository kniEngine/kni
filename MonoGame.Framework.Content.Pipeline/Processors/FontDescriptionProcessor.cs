// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using MonoGame.Framework.Utilities;
using SharpFont;
using Glyph = Microsoft.Xna.Framework.Content.Pipeline.Graphics.Glyph;


namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    [ContentProcessor(DisplayName = "Sprite Font Description - MonoGame")]
    public class FontDescriptionProcessor : ContentProcessor<FontDescription, SpriteFontContent>
    {
        [DefaultValue(true)]
        public virtual bool PremultiplyAlpha { get; set; }

        [DefaultValue(typeof(TextureProcessorOutputFormat), "Compressed")]
        public virtual TextureProcessorOutputFormat TextureFormat { get; set; }

        public FontDescriptionProcessor()
        {
            PremultiplyAlpha = true;
            TextureFormat = TextureProcessorOutputFormat.Compressed;
        }

        public override SpriteFontContent Process(FontDescription input, ContentProcessorContext context)
        {
            var output = new SpriteFontContent(input);

            var fontFile = FindFont(input.FontName, input.Style.ToString());

            if (string.IsNullOrWhiteSpace(fontFile))
            {
                var directories = new List<string> { Path.GetDirectoryName(input.Identity.SourceFilename) };
                var extensions = new string[] { "", ".ttf", ".ttc", ".otf" };

                // Add special per platform directories
                if (CurrentPlatform.OS == OS.Windows)
                    directories.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts"));
                else if (CurrentPlatform.OS == OS.MacOSX)
                {
                    directories.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Fonts"));
                    directories.Add("/Library/Fonts");
                    directories.Add("/System/Library/Fonts/Supplemental");
                }

                foreach (var dir in directories)
                {
                    foreach (var ext in extensions)
                    {
                        fontFile = Path.Combine(dir, input.FontName + ext);
                        if (File.Exists(fontFile))
                            break;
                    }
                    if (File.Exists(fontFile))
                        break;
                }
            }

            if (!File.Exists(fontFile))
                throw new FileNotFoundException("Could not find \"" + input.FontName + "\" font file.");

            context.Logger.LogMessage("Building Font {0}", fontFile);

            // Get the platform specific texture profile.
            var texProfile = TextureProfile.ForPlatform(context.TargetPlatform);

            {
                if (!File.Exists(fontFile))
                {
                    throw new Exception(string.Format("Could not load {0}", fontFile));
                }

                var characters = new List<char>(input.Characters);
                // add default character
                if (input.DefaultCharacter != null)
                {
                    if (!characters.Contains(input.DefaultCharacter.Value))
                        characters.Add(input.DefaultCharacter.Value);
                }
                characters.Sort();

                float lineSpacing = 0f;
                int minYOffset = 0;
                Dictionary<char, Glyph> glyphs = ImportGlyphs(input, characters, out lineSpacing, out minYOffset, fontFile);

                // Validate.
                if (glyphs.Count == 0)
                    throw new Exception("Font does not contain any glyphs.");

                // Optimize.
                foreach (Glyph glyph in glyphs.Values)
                {
                    glyph.Crop();
                }

                // We need to know how to pack the glyphs.
                bool requiresPot, requiresSquare;
                texProfile.Requirements(context, TextureFormat, out requiresPot, out requiresSquare);

                var face = GlyphPacker.ArrangeGlyphs(glyphs.Values, requiresPot, requiresSquare);

                // Adjust line and character spacing.
                lineSpacing += input.Spacing;
                output.VerticalLineSpacing = (int)lineSpacing;

                foreach (char ch in glyphs.Keys)
                {
                    Glyph glyph = glyphs[ch];

                    output.CharacterMap.Add(ch);

                    var texRect = glyph.Subrect;
                    output.Glyphs.Add(texRect);

                    Rectangle cropping;
                    cropping.X = (int)glyph.XOffset;
                    cropping.Y = (int)(glyph.YOffset + minYOffset);
                    cropping.Width  = (int)glyph.XAdvance;
                    cropping.Height = output.VerticalLineSpacing;
                    output.Cropping.Add(cropping);

                    // Set the optional character kerning.
                    if (input.UseKerning)
                        output.Kerning.Add(glyph.Kerning.ToVector3());
                    else
                        output.Kerning.Add(new Vector3(0, glyph.Width, 0));
                }

                output.Texture.Faces[0].Add(face);
            }

            if (PremultiplyAlpha)
            {
                var bmp = output.Texture.Faces[0][0];
                var data = bmp.GetPixelData();
                var idx = 0;
                for (; idx < data.Length;)
                {
                    var r = data[idx];

                    // Special case of simply copying the R component into the A, since R is the value of white alpha we want
                    data[idx + 0] = r;
                    data[idx + 1] = r;
                    data[idx + 2] = r;
                    data[idx + 3] = r;

                    idx += 4;
                }

                bmp.SetPixelData(data);
            }
            else
            {
                var bmp = output.Texture.Faces[0][0];
                var data = bmp.GetPixelData();
                var idx = 0;
                for (; idx < data.Length;)
                {
                    var r = data[idx];

                    // Special case of simply moving the R component into the A and setting RGB to solid white, since R is the value of white alpha we want
                    data[idx + 0] = 255;
                    data[idx + 1] = 255;
                    data[idx + 2] = 255;
                    data[idx + 3] = r;

                    idx += 4;
                }

                bmp.SetPixelData(data);
            }

            // Perform the final texture conversion.
            texProfile.ConvertTexture(context, output.Texture, TextureFormat, true);

            return output;
        }

        private string FindFont(string name, string style)
        {
            if (CurrentPlatform.OS == OS.Windows)
            {
                var fontDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts");
                foreach (var key in new RegistryKey[] { Registry.LocalMachine, Registry.CurrentUser })
                {
                    var subkey = key.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts", false);
                    foreach (var font in subkey.GetValueNames().OrderBy(x => x))
                    {
                        if (font.StartsWith(name, StringComparison.OrdinalIgnoreCase))
                        {
                            var fontPath = subkey.GetValue(font).ToString();

                            // The registry value might have trailing NUL characters
                            // See https://github.com/MonoGame/MonoGame/issues/4061
                            var nulIndex = fontPath.IndexOf('\0');
                            if (nulIndex != -1)
                                fontPath = fontPath.Substring(0, nulIndex);

                            return Path.IsPathRooted(fontPath) ? fontPath : Path.Combine(fontDirectory, fontPath);
                        }
                    }
                }
            }
            else if (CurrentPlatform.OS == OS.Linux)
            {
                string s, e;
                ExternalTool.Run("/bin/bash", string.Format("-c \"fc-match -f '%{{file}}:%{{family}}\\n' '{0}:style={1}'\"", name, style), out s, out e);
                s = s.Trim();

                var split = s.Split(':');
                if (split.Length < 2)
                    return string.Empty;

                // check font family, fontconfig might return a fallback
                if (split[1].Contains(","))
                {
                    // this file defines multiple family names
                    var families = split[1].Split(',');
                    foreach (var f in families)
                    {
                        if (f.ToLowerInvariant() == name.ToLowerInvariant())
                            return split[0];
                    }
                    // didn't find it
                    return string.Empty;
                }
                else
                {
                    if (split[1].ToLowerInvariant() != name.ToLowerInvariant())
                        return string.Empty;
                }

                return split[0];
            }

            return String.Empty;
        }

        // Uses FreeType to rasterize TrueType fonts into a series of glyph bitmaps.
        private static Dictionary<char, Glyph> ImportGlyphs(FontDescription input, List<char> characters, out float lineSpacing, out int minYOffset, string fontName)
        {
            var TrueTypeFileExtensions = new List<string> { ".ttf", ".ttc", ".otf" };
            string fileExtension = Path.GetExtension(fontName).ToLowerInvariant();
            if (!TrueTypeFileExtensions.Contains(fileExtension))
                throw new PipelineException("Unknown file extension " + fileExtension);

            using (Library sharpFontLib = new Library())
            using (var face = sharpFontLib.NewFace(fontName, 0))
            {
                const uint dpi = 96;
                int fixedSize = ((int)input.Size) << 6;
                face.SetCharSize(0, fixedSize, dpi, dpi);

                if (face.FamilyName == "Microsoft Sans Serif" && input.FontName != "Microsoft Sans Serif")
                    throw new PipelineException(string.Format("Font {0} is not installed on this computer.", input.FontName));

                var glyphMaps = new Dictionary<uint, Glyph>();
                var glyphs = new Dictionary<char,Glyph>();

                // Rasterize each character in turn.
                foreach (char character in characters)
                {
                    uint glyphIndex = face.GetCharIndex(character);
                    if (!glyphMaps.TryGetValue(glyphIndex, out Glyph glyph))
                    {
                        glyph = ImportGlyph(glyphIndex, face);
                        glyphMaps.Add(glyphIndex, glyph);
                    }

                    glyphs.Add(character, glyph);
                }

                // Store the font height.
                lineSpacing = face.Size.Metrics.Height >> 6;

                // The height used to calculate the Y offset for each character.
                minYOffset =  face.Size.Metrics.Ascender >> 6;

                return glyphs;
            }
        }

        // Rasterizes a single character glyph.
        private static Glyph ImportGlyph(uint glyphIndex, Face face)
        {
            face.LoadGlyph(glyphIndex, LoadFlags.Default, LoadTarget.Normal);
            face.Glyph.RenderGlyph(RenderMode.Normal);

            // Render the character.
            BitmapContent glyphBitmap = null;
            if (face.Glyph.Bitmap.Width > 0 && face.Glyph.Bitmap.Rows > 0)
            {
                glyphBitmap = new PixelBitmapContent<byte>(face.Glyph.Bitmap.Width, face.Glyph.Bitmap.Rows);
                byte[] gpixelAlphas = new byte[face.Glyph.Bitmap.Width * face.Glyph.Bitmap.Rows];
                //if the character bitmap has 1bpp we have to expand the buffer data to get the 8bpp pixel data
                //each byte in bitmap.bufferdata contains the value of to 8 pixels in the row
                //if bitmap is of width 10, each row has 2 bytes with 10 valid bits, and the last 6 bits of 2nd byte must be discarded
                if (face.Glyph.Bitmap.PixelMode == PixelMode.Mono)
                {
                    //variables needed for the expansion, amount of written data, length of the data to write
                    int written = 0, length = face.Glyph.Bitmap.Width * face.Glyph.Bitmap.Rows;
                    for (int i = 0; written < length; i++)
                    {
                        //width in pixels of each row
                        int width = face.Glyph.Bitmap.Width;
                        while (width > 0)
                        {
                            //valid data in the current byte
                            int stride = MathHelper.Min(8, width);
                            //copy the valid bytes to pixeldata
                            //System.Array.Copy(ExpandByte(face.Glyph.Bitmap.BufferData[i]), 0, gpixelAlphas, written, stride);
                            ExpandByteAndCopy(face.Glyph.Bitmap.BufferData[i], stride, gpixelAlphas, written);
                            written += stride;
                            width -= stride;
                            if (width > 0)
                                i++;
                        }
                    }
                }
                else
                    Marshal.Copy(face.Glyph.Bitmap.Buffer, gpixelAlphas, 0, gpixelAlphas.Length);
                glyphBitmap.SetPixelData(gpixelAlphas);
            }

            if (glyphBitmap == null)
            {
                var gHA = face.Glyph.Metrics.HorizontalAdvance >> 6;
                var gVA = face.Size.Metrics.Height >> 6;

                gHA = gHA > 0 ? gHA : gVA;
                gVA = gVA > 0 ? gVA : gHA;

                glyphBitmap = new PixelBitmapContent<byte>(gHA, gVA);
            }

            var kerning = new GlyphKerning();
            kerning.LeftBearing  = (face.Glyph.Metrics.HorizontalBearingX >> 6);
            kerning.AdvanceWidth = (face.Glyph.Metrics.Width >> 6);
            kerning.RightBearing = (face.Glyph.Metrics.HorizontalAdvance >> 6) - (kerning.LeftBearing + kerning.AdvanceWidth);
            kerning.LeftBearing  -= face.Glyph.BitmapLeft;
            kerning.AdvanceWidth += face.Glyph.BitmapLeft;

            // Construct the output Glyph object.
            return new Glyph(glyphIndex, glyphBitmap)
            {
                XOffset  =   face.Glyph.BitmapLeft,
                XAdvance =  (face.Glyph.Metrics.HorizontalAdvance >> 6),
                YOffset  = -(face.Glyph.Metrics.HorizontalBearingY >> 6),
                Kerning  = kerning,

#if DEBUG
                GlyphMetricLeftBearing = (face.Glyph.Metrics.HorizontalBearingX >> 6),
                GlyphMetricWidth = (face.Glyph.Metrics.Width >> 6),
                GlyphMetricXAdvance = (face.Glyph.Metrics.HorizontalAdvance >> 6),
                GlyphBitmapLeft = face.Glyph.BitmapLeft,
#endif
            };
        }

        /// <summary>
        /// Reads each individual bit of a byte from left to right and expands it to a full byte, 
        /// ones get byte.maxvalue, and zeros get byte.minvalue.
        /// </summary>
        /// <param name="origin">Byte to expand and copy</param>
        /// <param name="length">Number of Bits of the Byte to copy, from 1 to 8</param>
        /// <param name="destination">Byte array where to copy the results</param>
        /// <param name="startIndex">Position where to begin copying the results in destination</param>
        private static void ExpandByteAndCopy(byte origin, int length, byte[] destination, int startIndex)
        {
            byte tmp;
            for (int i = 7; i > 7 - length; i--)
            {
                tmp = (byte)(1 << i);
                if (origin / tmp == 1)
                {
                    destination[startIndex + 7 - i] = byte.MaxValue;
                    origin -= tmp;
                }
                else
                    destination[startIndex + 7 - i] = byte.MinValue;
            }
        }

    }
}
