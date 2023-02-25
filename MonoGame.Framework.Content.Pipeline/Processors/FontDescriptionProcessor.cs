// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021-2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using MonoGame.Framework.Utilities;
using SharpFont;


namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    [ContentProcessor(DisplayName = "Sprite Font Description - MonoGame")]
    public class FontDescriptionProcessor : ContentProcessor<FontDescription, SpriteFontContent>
    {
        SmoothingMode _smoothing = SmoothingMode.Disable;

        [DefaultValue(true)]
        public virtual bool PremultiplyAlpha { get; set; }

        [DefaultValue(typeof(TextureProcessorOutputFormat), "Compressed")]
        public virtual TextureProcessorOutputFormat TextureFormat { get; set; }

        [DefaultValue(typeof(SmoothingMode), "Disable")]
        public virtual SmoothingMode Smoothing
        {
            get { return _smoothing; }
            set { _smoothing = value; }
        }

        public FontDescriptionProcessor()
        {
            PremultiplyAlpha = true;
            TextureFormat = TextureProcessorOutputFormat.Compressed;
        }

        public override SpriteFontContent Process(FontDescription input, ContentProcessorContext context)
        {
            var output = new SpriteFontContent(input);

            var fontFile = FindFont(input, context);

            if (string.IsNullOrWhiteSpace(fontFile))
                fontFile = FindFontFile(input, context);

            if (!File.Exists(fontFile))
                throw new PipelineException("Could not find \"" + input.FontName + "\" font from file \""+ fontFile +"\".");

            var extensions = new List<string> { ".ttf", ".ttc", ".otf" };
            string fileExtension = Path.GetExtension(fontFile).ToLowerInvariant();
            if (!extensions.Contains(fileExtension))
                throw new PipelineException("Unknown file extension " + fileExtension);

            context.Logger.LogMessage("Building Font {0}", fontFile);

            // Get the platform specific texture profile.
            var texProfile = TextureProfile.ForPlatform(context.TargetPlatform);

            var characters = new List<char>(input.Characters);
            // add default character
            if (input.DefaultCharacter != null)
            {
                if (!characters.Contains(input.DefaultCharacter.Value))
                    characters.Add(input.DefaultCharacter.Value);
            }
            characters.Sort();

            FontContent font = ImportFont(input, context, fontFile, characters);
            Dictionary<char, FontGlyph> glyphs = font.Glyphs;

            // Validate.
            if (glyphs.Count == 0)
                throw new PipelineException("Font does not contain any glyphs.");

            // Optimize glyphs.
            foreach (FontGlyph glyph in glyphs.Values)
                glyph.Crop();

            // We need to know how to pack the glyphs.
            bool requiresPot, requiresSquare;
            texProfile.Requirements(context, TextureFormat, out requiresPot, out requiresSquare);

            BitmapContent glyphAtlas = GlyphPacker.ArrangeGlyphs(glyphs.Values, requiresPot, requiresSquare);

            // calculate line spacing.
            output.VerticalLineSpacing = (int)font.MetricsHeight;
            // The LineSpacing from XNA font importer is +1px that SharpFont.
            output.VerticalLineSpacing += 1;

            float glyphHeightEx = -(font.MetricsDescender/2f);
            // The above value of glyphHeightEx match the XNA importer,
            // however the height of MeasureString() does not match VerticalLineSpacing.
            //glyphHeightEx = 0;
            float baseline = font.MetricsHeight + font.MetricsDescender + (font.MetricsDescender/2f) + glyphHeightEx;

            foreach (char ch in glyphs.Keys)
            {
                FontGlyph glyph = glyphs[ch];

                output.CharacterMap.Add(ch);

                var texRect = glyph.Subrect;
                output.Glyphs.Add(texRect);

                Rectangle cropping;
                cropping.X = glyph.XOffset + glyph.FontBitmapLeft;
                cropping.Y = glyph.YOffset + (int)(-glyph.FontBitmapTop + baseline);
                cropping.Width  = (int)glyph.XAdvance;
                cropping.Height = (int)Math.Ceiling(font.MetricsHeight + glyphHeightEx);
                output.Cropping.Add(cropping);

                // Set the optional character kerning.
                if (input.UseKerning)
                    output.Kerning.Add(glyph.Kerning.ToVector3());
                else
                    output.Kerning.Add(new Vector3(0, glyph.Width, 0));
            }

            ProcessPremultiplyAlpha(glyphAtlas);

            output.Texture.Faces[0].Add(glyphAtlas);

            // Perform the final texture conversion.
            texProfile.ConvertTexture(context, output.Texture, TextureFormat, true);

            return output;
        }

        private string FindFont(FontDescription input, ContentProcessorContext context)
        {
            if (CurrentPlatform.OS == OS.Windows)
            {
                var fontsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts");
                foreach (var key in new RegistryKey[] { Registry.LocalMachine, Registry.CurrentUser })
                {
                    var subkey = key.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts", false);
                    foreach (string font in subkey.GetValueNames().OrderBy(x => x))
                    {
                        if (font.StartsWith(input.FontName, StringComparison.OrdinalIgnoreCase))
                        {
                            string fontPath = subkey.GetValue(font).ToString();
                            // The registry value might have trailing NUL characters
                            fontPath.TrimEnd(new char[] { '\0' });

                            return Path.IsPathRooted(fontPath) ? fontPath : Path.Combine(fontsDirectory, fontPath);
                        }
                    }
                }
            }
            else if (CurrentPlatform.OS == OS.Linux)
            {
                string s, e;
                ExternalTool.Run("/bin/bash", string.Format("-c \"fc-match -f '%{{file}}:%{{family}}\\n' '{0}:style={1}'\"", input.FontName, input.Style.ToString()), out s, out e);
                s = s.Trim();

                var split = s.Split(':');
                if (split.Length >= 2)
                {
                    // check font family, fontconfig might return a fallback
                    if (split[1].Contains(","))
                    {
                        // this file defines multiple family names
                        var families = split[1].Split(',');
                        foreach (var f in families)
                        {
                            if (input.FontName.Equals(f, StringComparison.InvariantCultureIgnoreCase))
                                return split[0];
                        }
                    }
                    else
                    {
                        if (input.FontName.Equals(split[1], StringComparison.InvariantCultureIgnoreCase))
                            return split[0];
                    }
                }
            }

            return String.Empty;
        }

        private string FindFontFile(FontDescription input, ContentProcessorContext context)
        {
            var extensions = new string[] { "", ".ttf", ".ttc", ".otf" };
            var directories = new List<string>();

            directories.Add(Path.GetDirectoryName(input.Identity.SourceFilename));

            // Add special per platform directories
            if (CurrentPlatform.OS == OS.Windows)
            {
                var fontsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts");
                directories.Add(fontsDirectory);
            }
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
                    var fontFile = Path.Combine(dir, input.FontName + ext);
                    if (File.Exists(fontFile))
                        return fontFile;
                }
            }

            return String.Empty;
        }

        // Uses FreeType to rasterize TrueType fonts into a series of glyph bitmaps.
        private FontContent ImportFont(FontDescription input, ContentProcessorContext context, string fontName, List<char> characters)
        {
            FontContent fontContent = new FontContent();

            using (Library sharpFontLib = new Library())
            using (var face = sharpFontLib.NewFace(fontName, 0))
            {
                int fixedSize = ((int)input.Size) << 6;
                const uint dpi = 0;
                face.SetCharSize(0, fixedSize, dpi, dpi);

                // Rasterize each character in turn.
                foreach (char character in characters)
                {
                    if (fontContent.Glyphs.ContainsKey(character))
                        continue;

                    var glyph = ImportGlyph(input, context, face, character);
                    fontContent.Glyphs.Add(character, glyph);
                }


                fontContent.MetricsHeight = face.Size.Metrics.Height / 64f;
                fontContent.MetricsAscender  = face.Size.Metrics.Ascender / 64f;
                fontContent.MetricsDescender = face.Size.Metrics.Descender / 64f;

#if DEBUG
                fontContent.FaceUnderlinePosition = face.UnderlinePosition / 64f;
                fontContent.FaceUnderlineThickness = face.UnderlineThickness / 64f;
#endif

                return fontContent;
            }
        }

        // Rasterizes a single character glyph.
        private FontGlyph ImportGlyph(FontDescription input, ContentProcessorContext context, Face face, char character)
        {
            LoadFlags loadFlags   = LoadFlags.Default;
            LoadTarget loadTarget = LoadTarget.Normal;
            RenderMode renderMode = RenderMode.Normal;

            switch (this.Smoothing)
            {
                case SmoothingMode.Disable:
                    //loadTarget = LoadTarget.Mono;
                    renderMode = RenderMode.Mono;
                    break;
                case SmoothingMode.Normal:
                    break;
                case SmoothingMode.Light:
                    loadFlags |= LoadFlags.ForceAutohint;
                    loadTarget = LoadTarget.Light;
                    //renderMode = RenderMode.Light;
                    break;
                case SmoothingMode.AutoHint:
                    loadFlags |= LoadFlags.ForceAutohint;
                    break;


                default:
                    throw new InvalidOperationException("RenderMode");
            }

            uint glyphIndex = face.GetCharIndex(character);
            face.LoadGlyph(glyphIndex, loadFlags, loadTarget);
            face.Glyph.RenderGlyph(renderMode);

            // Render the character.
            BitmapContent glyphBitmap = null;
            if (face.Glyph.Bitmap.Width > 0 && face.Glyph.Bitmap.Rows > 0)
            {
                switch (face.Glyph.Bitmap.PixelMode)
                {
                    case PixelMode.Mono:
                        glyphBitmap = new PixelBitmapContent<Color>(face.Glyph.Bitmap.Width, face.Glyph.Bitmap.Rows);
                        glyphBitmap.SetPixelData(ConvertMonoToColor(face.Glyph));
                        break;

                    case PixelMode.Gray:
                        glyphBitmap = new PixelBitmapContent<Color>(face.Glyph.Bitmap.Width, face.Glyph.Bitmap.Rows);
                        glyphBitmap.SetPixelData(ConvertAlphaToColor(face.Glyph));
                        break;

                    default:
                        throw new PipelineException(string.Format("Glyph PixelMode {0} is not supported.", face.Glyph.Bitmap.PixelMode));
                }
            }
            else // whitespace
            {
                var gHA = face.Glyph.Metrics.HorizontalAdvance / 64f;
                var gVA = face.Size.Metrics.Height / 64f;
                gHA = gHA > 0 ? gHA : gVA;
                gVA = gVA > 0 ? gVA : gHA;
                glyphBitmap = new PixelBitmapContent<Color>((int)gHA, (int)gVA);
            }

            GlyphKerning kerning = new GlyphKerning();
            kerning.LeftBearing  = (face.Glyph.Metrics.HorizontalBearingX / 64f);
            kerning.AdvanceWidth = (face.Glyph.Metrics.Width / 64f);
            kerning.RightBearing = (face.Glyph.Metrics.HorizontalAdvance / 64f) - (kerning.LeftBearing + kerning.AdvanceWidth);
            kerning.LeftBearing  -= face.Glyph.BitmapLeft;
            kerning.AdvanceWidth += face.Glyph.BitmapLeft;

            // Construct the output Glyph object.
            FontGlyph glyph = new FontGlyph(glyphBitmap);
            glyph.FontBitmapLeft = face.Glyph.BitmapLeft;
            glyph.FontBitmapTop = face.Glyph.BitmapTop;

            glyph.XOffset = 0;
            glyph.YOffset = 0;
            glyph.XAdvance = (face.Glyph.Metrics.HorizontalAdvance / 64f);
            glyph.Kerning = kerning;

            glyph.GlyphMetricTopBearing = (face.Glyph.Metrics.HorizontalBearingY / 64f);
#if DEBUG
            glyph.GlyphMetricLeftBearing = (face.Glyph.Metrics.HorizontalBearingX / 64f);
            glyph.GlyphMetricWidth = (face.Glyph.Metrics.Width / 64f);
            glyph.GlyphMetricXAdvance = (face.Glyph.Metrics.HorizontalAdvance / 64f);
#endif

            return glyph;
        }

        private static unsafe byte[] ConvertMonoToColor(GlyphSlot glyph)
        {
            FTBitmap bitmap = glyph.Bitmap;
            int cols = bitmap.Width;
            int rows = bitmap.Rows;
            int stride = bitmap.Pitch;

            // SharpFont 2.5.3 doesn't return the entire bitmapdata when Pitch > 1.
            //System.Diagnostics.Debug.Assert(bitmap.BufferData.Length == rows * bitmap.Pitch);

            byte* pGlyphData = (byte*)bitmap.Buffer.ToPointer();
            byte[] data = new byte[cols * rows * 4];
            fixed (byte* pdata = data)
            {
                for (int y = 0; y < rows; y++)
                {
                    for (int x = 0; x < cols; x++)
                    {
                        byte b = pGlyphData[(x >> 3) + y * stride];
                        b = (byte)(b & (0x80 >> (x & 0x07)));
                        var a = (b == (byte)0) ? (byte)0 : (byte)255;

                        int idx = x + y * cols;
                        pdata[idx * 4 + 0] = 255;
                        pdata[idx * 4 + 1] = 255;
                        pdata[idx * 4 + 2] = 255;
                        pdata[idx * 4 + 3] = a;
                    }
                }
            }

            return data;
        }

        private static unsafe byte[] ConvertAlphaToColor(GlyphSlot glyph)
        {
            FTBitmap bitmap = glyph.Bitmap;
            int cols = bitmap.Width;
            int rows = bitmap.Rows;

            byte* pGlyphData = (byte*)bitmap.Buffer.ToPointer();
            byte[] data = new byte[cols * rows * 4];
            fixed (byte* pdata = data)
            {
                int count = data.Length / 4;
                for (int idx = 0; idx < count; idx++)
                {
                    byte a = pGlyphData[idx];

                    pdata[idx * 4 + 0] = 255;
                    pdata[idx * 4 + 1] = 255;
                    pdata[idx * 4 + 2] = 255;
                    pdata[idx * 4 + 3] = a;
                }
            }

            return data;
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

        public enum SmoothingMode
        {
            [System.ComponentModel.Description("Bitmap with 1bit alpha")]
            Disable,
            [System.ComponentModel.Description("Normal hinting")]
            Normal,
            [System.ComponentModel.Description("Light hinting")]
            Light,
            [System.ComponentModel.Description("Auto-hinter")]
            AutoHint,
        }
    }
}
