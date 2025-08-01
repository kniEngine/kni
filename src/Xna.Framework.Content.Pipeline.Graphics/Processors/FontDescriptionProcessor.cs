﻿// MonoGame - Copyright (C) The MonoGame Team
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
using Microsoft.Xna.Platform.Utilities;
using SharpFont;


namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    [ContentProcessor(DisplayName = "Sprite Font Description - KNI")]
    public class FontDescriptionProcessor : ContentProcessor<FontDescription, SpriteFontContent>
    {
        private static object _fontFamilyInfoCacheLocker = new object();
        private static Dictionary<string, FontFamilyInfo> _fontFamilyInfoCache;

        List<string> _allowedFontFileExtensions = new List<string> { ".ttf", ".ttc", ".otf" };

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

            FontFaceInfo faceInfo = null;
            faceInfo = FindLocalFontFile(input, context);
            if (faceInfo == null)
                faceInfo = FindFontFile(input, context);

            if (faceInfo == null)
                throw new PipelineException("Could not find \"" + input.FontName + "\" font.");

            context.Logger.LogMessage("Building Font {0}", faceInfo.FontFile);

            List<char> characters = new List<char>(input.Characters);
            // add default character
            if (input.DefaultCharacter != null)
            {
                if (!characters.Contains(input.DefaultCharacter.Value))
                    characters.Add(input.DefaultCharacter.Value);
            }
            characters.Sort();

            FontContent font = ImportFont(input, context, faceInfo, characters);
            Dictionary<char, FontGlyph> glyphs = font.Glyphs;

            // Validate.
            if (glyphs.Count == 0)
                throw new PipelineException("Font does not contain any glyphs.");

            // Optimize glyphs.
            foreach (FontGlyph glyph in glyphs.Values)
                glyph.Crop();

            // We need to know how to pack the glyphs.
            TextureProcessorOutputFormat format = TextureFormat;
            if (format == TextureProcessorOutputFormat.Compressed)
                format = GraphicsUtil.GetTextureFormatForPlatform(format, context.TargetPlatform);
            bool requiresPot = GraphicsUtil.RequiresPowerOfTwo(format, context.TargetPlatform, context.TargetProfile);
            bool requiresSquare = GraphicsUtil.RequiresSquare(format, context.TargetPlatform);

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
                        output.Texture.ConvertBitmapType(typeof(PixelBitmapContent<Color>));
                        break;
                    case TextureProcessorOutputFormat.Color16Bit:
                        // Handle this common compression format.
                        GraphicsUtil.CompressColor16Bit(output.Texture);
                        break;

                    case TextureProcessorOutputFormat.AtcCompressed:
                        {
                            // If sharp alpha is required (for a font texture page), use 16-bit color instead of PVR
                            output.Texture.ConvertBitmapType(typeof(PixelBitmapContent<Vector4>)); // Make sure we're in a floating point format
                            GraphicsUtil.CompressColor16Bit(output.Texture);
                            //GraphicsUtil.CompressAti(input);
                        }
                        break;
                    case TextureProcessorOutputFormat.DxtCompressed:
                        {
                            // Compress the greyscale font texture page using a specially-formulated DXT3 mode
                            output.Texture.ConvertBitmapType(typeof(PixelBitmapContent<Vector4>)); // Make sure we're in a floating point format
                            GraphicsUtil.CompressFontDXT3(output.Texture, context);
                            //GraphicsUtil.CompressDxt(output.Texture, context);
                        }
                        break;
                    case TextureProcessorOutputFormat.Etc1Compressed:
                        {
                            // If sharp alpha is required (for a font texture page), use 16-bit color instead of PVR
                            output.Texture.ConvertBitmapType(typeof(PixelBitmapContent<Vector4>)); // Make sure we're in a floating point format
                            GraphicsUtil.CompressColor16Bit(output.Texture);
                            //GraphicsUtil.CompressEtc1(input, context);
                        }
                        break;
                    case TextureProcessorOutputFormat.PvrCompressed:
                        {
                            // If sharp alpha is required (for a font texture page), use 16-bit color instead of PVR
                            output.Texture.ConvertBitmapType(typeof(PixelBitmapContent<Vector4>)); // Make sure we're in a floating point format
                            GraphicsUtil.CompressColor16Bit(output.Texture);
                            //GraphicsUtil.CompressPvrtc(input, context);
                        }
                        break;
                }
            }
            catch (EntryPointNotFoundException ex)
            {
                context.Logger.LogImportantMessage("Could not find the entry point to compress the texture. " + ex.ToString());
                throw;
            }
            catch (DllNotFoundException ex)
            {
                context.Logger.LogImportantMessage("Could not compress texture. Required shared lib is missing. " + ex.ToString());
                throw;
            }
            catch (Exception ex)
            {
                context.Logger.LogImportantMessage("Could not convert texture. " + ex.ToString());
                throw;
            }

            return output;
        }

        private FontFaceInfo FindLocalFontFile(FontDescription input, ContentProcessorContext context)
        {

            string fontsDirectory = Path.GetDirectoryName(input.Identity.SourceFilename);

            string fontFile = Path.Combine(fontsDirectory, input.FontName);

            if (!File.Exists(fontFile))
                return null;
            string fontFileExtension = Path.GetExtension(fontFile).ToLowerInvariant();
            if (!_allowedFontFileExtensions.Contains(fontFileExtension))
               throw new PipelineException("Unknown file extension " + fontFileExtension);

            Dictionary<string, FontFamilyInfo> fontFamilyInfoCache = new Dictionary<string, FontFamilyInfo>();

            //using (Library sharpFontLib = new Library())
            //    AddSharpFontFaces(fontFamilyInfoCache, fontFile, sharpFontLib);
            AddOpenFontFaces(fontFamilyInfoCache, fontFile);

            foreach(FontFamilyInfo familyInfo in fontFamilyInfoCache.Values)
            {
                foreach (FontFaceInfo faceInfo in familyInfo.Faces)
                {
                    if (faceInfo.Style == input.Style)
                        return faceInfo;
                }
            }

            return null;
        }

        private FontFaceInfo FindFontFile(FontDescription input, ContentProcessorContext context)
        {
            if (CurrentPlatform.OS == OS.Windows)
            {
                lock (_fontFamilyInfoCacheLocker)
                {
                    if (_fontFamilyInfoCache == null)
                    {
                        Dictionary<string, FontFamilyInfo> fontFamilyInfoCache = new Dictionary<string, FontFamilyInfo>();

                        //using (Library sharpFontLib = new Library())
                        {
                            string fontsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts");
                            foreach (RegistryKey key in new RegistryKey[] { Registry.LocalMachine, Registry.CurrentUser })
                            {
                                RegistryKey subkey = key.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts", false);
                                string[] valueNames = subkey.GetValueNames();
                                Array.Sort(valueNames);

                                foreach (string font in valueNames)
                                {
                                    string fontFile = subkey.GetValue(font).ToString();
                                    // The registry value might have trailing NUL characters
                                    fontFile.TrimEnd(new char[] { '\0' });

                                    string fontFileExtension = Path.GetExtension(fontFile).ToLowerInvariant();
                                    if (!_allowedFontFileExtensions.Contains(fontFileExtension))
                                        continue;

                                    if (!Path.IsPathRooted(fontFile))
                                        fontFile = Path.Combine(fontsDirectory, fontFile);
                                    if (!File.Exists(fontFile))
                                        continue;

                                    //AddSharpFontFaces(fontFamilyInfoCache, fontFile, sharpFontLib);
                                    AddOpenFontFaces(fontFamilyInfoCache, fontFile);
                                }
                            }
                        }

                        _fontFamilyInfoCache = fontFamilyInfoCache;
                    }

                    FontFamilyInfo familyInfo;
                    if (_fontFamilyInfoCache.TryGetValue(input.FontName.ToUpperInvariant(), out familyInfo))
                    {
                        foreach (FontFaceInfo faceInfo in familyInfo.Faces)
                        {
                            if (faceInfo.Style == input.Style)
                                return faceInfo;
                        }
                        string errorMsg = String.Format("Font '{0}' does not have style '{1}'.", input.FontName, input.Style);
                        throw new InvalidContentException(errorMsg);
                    }
                }
            }
            else if (CurrentPlatform.OS == OS.MacOSX)
            {
                lock (_fontFamilyInfoCacheLocker)
                {
                    if (_fontFamilyInfoCache == null)
                    {
                        Dictionary<string, FontFamilyInfo> fontFamilyInfoCache = new Dictionary<string, FontFamilyInfo>();

                        //using (Library sharpFontLib = new Library())
                        {
                            List<string> directories = new List<string>();
                            directories.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Fonts"));
                            directories.Add("/Library/Fonts");
                            directories.Add("/System/Library/Fonts/Supplemental");

                            foreach (string fontsDirectory in directories)
                            {
                                foreach (string fontFile in Directory.GetFiles(fontsDirectory))
                                {
                                    string fontFileExtension = Path.GetExtension(fontFile).ToLowerInvariant();
                                    if (!_allowedFontFileExtensions.Contains(fontFileExtension))
                                        continue;

                                    //AddSharpFontFaces(fontFamilyInfoCache, fontFile, sharpFontLib);
                                    AddOpenFontFaces(fontFamilyInfoCache, fontFile);
                                }
                            }
                        }

                        _fontFamilyInfoCache = fontFamilyInfoCache;
                    }

                    FontFamilyInfo familyInfo;
                    if (_fontFamilyInfoCache.TryGetValue(input.FontName.ToUpperInvariant(), out familyInfo))
                    {
                        foreach (FontFaceInfo faceInfo in familyInfo.Faces)
                        {
                            if (faceInfo.Style == input.Style)
                                return faceInfo;
                        }
                        string errorMsg = String.Format("Font '{0}' does not have style '{1}'.", input.FontName, input.Style);
                        throw new InvalidContentException(errorMsg);
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
                    string fontFile = split[0];
                    string fontName = split[1];

                    string fontFileExtension = Path.GetExtension(fontFile).ToLowerInvariant();
                    if (!_allowedFontFileExtensions.Contains(fontFileExtension))
                        return null;

                    // check font family, fontconfig might return a fallback
                    string[] families = new string[] { fontName };
                    if (fontName.Contains(","))
                        families = fontName.Split(','); // this file defines multiple family names
                    foreach (string family in families)
                    {
                        if (input.FontName.Equals(family, StringComparison.InvariantCultureIgnoreCase))
                        {
                            FontFaceInfo fontFaceInfo = new FontFaceInfo(fontFile, 0, input.Style);
                            return fontFaceInfo;
                        }
                    }
                }
            }

            return null;
        }

        private static void AddOpenFontFaces(Dictionary<string, FontFamilyInfo> fontFamilyInfoCache, string fontFile)
        {
            using (Stream fontFileStream = new FileStream(fontFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Typography.OpenFont.OpenFontReader reader = new Typography.OpenFont.OpenFontReader();
                Typography.OpenFont.PreviewFontInfo previewFontInfo = reader.ReadPreview(fontFileStream);

                bool isFontCollection = previewFontInfo.IsFontCollection;
                if (!isFontCollection)
                {
                    int faceIndex = 0;
                    Typography.OpenFont.PreviewFontInfo fontInfo = previewFontInfo;

                    string fontName = fontInfo.Name;
                    string styleName = fontInfo.NameEntry.TypographyicSubfamilyName;
                    FontDescriptionStyle fontStyle = FontFaceInfo.ToFontStyle(fontInfo.OS2TranslatedStyle);

                    FontFaceInfo fontFaceInfo = new FontFaceInfo(fontFile, faceIndex, fontStyle, styleName);

                    CachefontFaceInfo(fontFamilyInfoCache, fontName, fontFaceInfo);
                }
                else
                {
                    for (int faceIndex = 0; faceIndex < previewFontInfo.MemberCount; faceIndex++)
                    {
                        Typography.OpenFont.PreviewFontInfo fontInfo = previewFontInfo.GetMember(faceIndex);

                        string fontName = fontInfo.Name;
                        string styleName = fontInfo.NameEntry.TypographyicSubfamilyName;
                        FontDescriptionStyle fontStyle = FontFaceInfo.ToFontStyle(fontInfo.OS2TranslatedStyle);

                        FontFaceInfo fontFaceInfo = new FontFaceInfo(fontFile, faceIndex, fontStyle, styleName);

                        CachefontFaceInfo(fontFamilyInfoCache, fontName, fontFaceInfo);
                    }
                }
            }

        }

        private static void AddSharpFontFaces(Dictionary<string, FontFamilyInfo> fontFamilyInfoCache, string fontFile, Library sharpFontLib)
        {
            int faceCount = 1;
            for (int faceIndex = 0; faceIndex < faceCount; faceIndex++)
            {
                using (Face face = sharpFontLib.NewFace(fontFile, faceIndex))
                {
                    faceCount = face.FaceCount;

                    string fontName = face.FamilyName;
                    string styleName = face.StyleName;
                    FontDescriptionStyle fontStyle = FontFaceInfo.ToFontStyle(face.StyleFlags);

                    FontFaceInfo fontFaceInfo = new FontFaceInfo(fontFile, face.FaceIndex, fontStyle, styleName, face.FaceFlags);

                    CachefontFaceInfo(fontFamilyInfoCache, fontName, fontFaceInfo);
                }
            }
        }

        private static void CachefontFaceInfo(Dictionary<string, FontFamilyInfo> fontFamilyInfoCache, string fontName, FontFaceInfo fontFaceInfo)
        {
            FontFamilyInfo fontFamilyInfo;
            if (!fontFamilyInfoCache.TryGetValue(fontName.ToUpperInvariant(), out fontFamilyInfo))
            {
                fontFamilyInfo = new FontFamilyInfo(fontName);
                fontFamilyInfoCache.Add(fontName.ToUpperInvariant(), fontFamilyInfo);
            }
            fontFamilyInfo.Faces.Add(fontFaceInfo);
        }

        // Uses FreeType to rasterize TrueType fonts into a series of glyph bitmaps.
        private FontContent ImportFont(FontDescription input, ContentProcessorContext context, FontFaceInfo faceInfo, List<char> characters)
        {
            FontContent fontContent = new FontContent();

            using (Library sharpFontLib = new Library())
            using (Face face = sharpFontLib.NewFace(faceInfo.FontFile, faceInfo.FaceIndex))
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
