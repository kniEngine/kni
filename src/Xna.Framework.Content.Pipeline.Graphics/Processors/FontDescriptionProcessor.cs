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
        SmoothingMode _smoothing = SmoothingMode.Normal;

        [DefaultValue(true)]
        public virtual bool PremultiplyAlpha { get; set; }

        [DefaultValue(typeof(TextureProcessorOutputFormat), "Compressed")]
        public virtual TextureProcessorOutputFormat TextureFormat { get; set; }

        [DefaultValue(typeof(SmoothingMode), "Normal")]
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
            SpriteFontContent output = new SpriteFontContent(input);

            string fontFile = FindFont(input, context);

            if (string.IsNullOrWhiteSpace(fontFile))
                fontFile = FindFontFile(input, context);

            if (!File.Exists(fontFile))
                throw new PipelineException("Could not find \"" + input.FontName + "\" font from file \""+ fontFile +"\".");

            List<string> extensions = new List<string> { ".ttf", ".ttc", ".otf" };
            string fileExtension = Path.GetExtension(fontFile).ToLowerInvariant();
            if (!extensions.Contains(fileExtension))
                throw new PipelineException("Unknown file extension " + fileExtension);

            context.Logger.LogMessage("Building Font {0}", fontFile);

            // Get the platform specific texture profile.
            TextureProfile texProfile = TextureProfile.ForPlatform(context.TargetPlatform);

            List<char> characters = new List<char>(input.Characters);
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

                Rectangle texRect = glyph.Subrect;
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
                    output.Kerning.Add(new Vector3(0, glyph.Width + 1, 0));
            }

            if (PremultiplyAlpha)
                TextureProcessor.ProcessPremultiplyAlpha((PixelBitmapContent<Vector4>)glyphAtlas);

            output.Texture.Faces[0].Add(glyphAtlas);

            // Perform the final texture conversion.
            texProfile.ConvertTexture(context, output.Texture, TextureFormat, true);

            return output;
        }

        private string FindFont(FontDescription input, ContentProcessorContext context)
        {
            if (CurrentPlatform.OS == OS.Windows)
            {
                string fontsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts");
                foreach (RegistryKey key in new RegistryKey[] { Registry.LocalMachine, Registry.CurrentUser })
                {
                    RegistryKey subkey = key.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts", false);
                    string[] valueNames = subkey.GetValueNames();
                    Array.Sort(valueNames);

                    foreach (string font in valueNames)
                    {
                        if (font.StartsWith(input.FontName, StringComparison.OrdinalIgnoreCase))
                        {
                            string fontPath = subkey.GetValue(font).ToString();
                            // The registry value might have trailing NUL characters
                            fontPath.TrimEnd(new char[] { '\0' });

                            if (!Path.IsPathRooted(fontPath))
                                fontPath = Path.Combine(fontsDirectory, fontPath);
                            return fontPath;
                        }
                    }
                }
            }
            else if (CurrentPlatform.OS == OS.Linux)
            {
                string stdout, stderr;
                ExternalTool.Run("/bin/bash", string.Format("-c \"fc-match -f '%{{file}}:%{{family}}\\n' '{0}:style={1}'\"", input.FontName, input.Style.ToString()), out stdout, out stderr);
                stdout = stdout.Trim();

                string[] split = stdout.Split(':');
                if (split.Length >= 2)
                {
                    string fontPath = split[0];
                    string fontName = split[1];

                    // check font family, fontconfig might return a fallback
                    if (fontName.Contains(","))
                    {
                        // this file defines multiple family names
                        string[] families = fontName.Split(',');
                        foreach (string family in families)
                        {
                            if (input.FontName.Equals(family, StringComparison.InvariantCultureIgnoreCase))
                                return fontPath;
                        }
                    }
                    else
                    {
                        if (input.FontName.Equals(fontName, StringComparison.InvariantCultureIgnoreCase))
                            return fontPath;
                    }
                }
            }

            return String.Empty;
        }

        private string FindFontFile(FontDescription input, ContentProcessorContext context)
        {
            string[] extensions = new string[] { "", ".ttf", ".ttc", ".otf" };
            List<string> directories = new List<string>();

            directories.Add(Path.GetDirectoryName(input.Identity.SourceFilename));

            // Add special per platform directories
            if (CurrentPlatform.OS == OS.Windows)
            {
                string fontsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts");
                directories.Add(fontsDirectory);
            }
            else if (CurrentPlatform.OS == OS.MacOSX)
            {
                directories.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Fonts"));
                directories.Add("/Library/Fonts");
                directories.Add("/System/Library/Fonts/Supplemental");
            }

            foreach (string dir in directories)
            {
                foreach (string ext in extensions)
                {
                    string fontFile = Path.Combine(dir, input.FontName + ext);
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
            using (Face face = sharpFontLib.NewFace(fontName, 0))
            {
                float size = (96f/72f) * input.Size;
                int fixedSize = (int)(size * 64);
                const uint dpi = 0;
                face.SetCharSize(0, fixedSize, dpi, dpi);

                // Rasterize each character in turn.
                foreach (char ch in characters)
                {
                    if (fontContent.Glyphs.ContainsKey(ch))
                        continue;

                    FontGlyph glyph = ImportGlyph(input, context, face, ch);
                    fontContent.Glyphs.Add(ch, glyph);
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
        private FontGlyph ImportGlyph(FontDescription input, ContentProcessorContext context, Face face, char ch)
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

            uint glyphIndex = face.GetCharIndex(ch);
            face.LoadGlyph(glyphIndex, loadFlags, loadTarget);
            face.Glyph.RenderGlyph(renderMode);

            // Render the character.
            PixelBitmapContent<Vector4> glyphBitmap = null;
            if (face.Glyph.Bitmap.Width > 0 && face.Glyph.Bitmap.Rows > 0)
            {
                switch (face.Glyph.Bitmap.PixelMode)
                {
                    case PixelMode.Mono:
                        glyphBitmap = new PixelBitmapContent<Vector4>(face.Glyph.Bitmap.Width, face.Glyph.Bitmap.Rows);
                        glyphBitmap.SetPixelData(ConvertMonoToVector4(face.Glyph));
                        break;

                    case PixelMode.Gray:
                        glyphBitmap = new PixelBitmapContent<Vector4>(face.Glyph.Bitmap.Width, face.Glyph.Bitmap.Rows);
                        glyphBitmap.SetPixelData(ConvertAlphaToVector4(face.Glyph));
                        break;

                    default:
                        throw new PipelineException(string.Format("Glyph PixelMode {0} is not supported.", face.Glyph.Bitmap.PixelMode));
                }
            }
            else // whitespace
            {
                float gHA = face.Glyph.Metrics.HorizontalAdvance / 64f;
                float gVA = face.Size.Metrics.Height / 64f;
                gHA = (gHA > 0) ? gHA : gVA;
                gVA = (gVA > 0) ? gVA : gHA;
                glyphBitmap = new PixelBitmapContent<Vector4>((int)gHA, (int)gVA);
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

        private static unsafe byte[] ConvertMonoToVector4(GlyphSlot glyph)
        {
            FTBitmap bitmap = glyph.Bitmap;
            int cols = bitmap.Width;
            int rows = bitmap.Rows;
            int stride = bitmap.Pitch;

            // SharpFont 2.5.3 doesn't return the entire bitmapdata when Pitch > 1.
            //System.Diagnostics.Debug.Assert(bitmap.BufferData.Length == rows * bitmap.Pitch);

            byte* pGlyphData = (byte*)bitmap.Buffer.ToPointer();
            byte[] data = new byte[cols * rows * sizeof(Vector4)];
            fixed (byte* pdata = data)
            {
                Vector4* pvdata = (Vector4*)pdata;
                for (int y = 0; y < rows; y++)
                {
                    for (int x = 0; x < cols; x++)
                    {
                        byte b = pGlyphData[(x >> 3) + y * stride];
                        b = (byte)(b & (0x80 >> (x & 0x07)));
                        float a = (b == (byte)0) ? 0f : 1f;

                        int idx = x + y * cols;
                        pvdata[idx].X = 1f;
                        pvdata[idx].Y = 1f;
                        pvdata[idx].Z = 1f;
                        pvdata[idx].W = a;
                    }
                }
            }

            return data;
        }

        private static unsafe byte[] ConvertAlphaToVector4(GlyphSlot glyph)
        {
            FTBitmap bitmap = glyph.Bitmap;
            int cols = bitmap.Width;
            int rows = bitmap.Rows;

            byte* pGlyphData = (byte*)bitmap.Buffer.ToPointer();
            byte[] data = new byte[cols * rows * sizeof(Vector4)];
            fixed (byte* pdata = data)
            {
                Vector4* pvdata = (Vector4*)pdata;
                int count = (cols * rows);
                for (int idx = 0; idx < count; idx++)
                {
                    byte a = pGlyphData[idx];

                    pvdata[idx].X = 1f;
                    pvdata[idx].Y = 1f;
                    pvdata[idx].Z = 1f;
                    pvdata[idx].W = a / (float)byte.MaxValue;
                }
            }

            return data;
        }

        public enum SmoothingMode
        {
            [Description("Normal hinting")]
            Normal,
            [Description("Light hinting")]
            Light,
            [Description("Auto-hinter")]
            AutoHint,
            [Description("Bitmap with 1bit alpha")]
            Disable,
        }
    }
}
