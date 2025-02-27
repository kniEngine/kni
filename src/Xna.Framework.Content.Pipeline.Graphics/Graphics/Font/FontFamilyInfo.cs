﻿// Copyright (C)2023 Nick Kastellanos

using System.Collections.Generic;
using OpenFontStyleFlags = Typography.OpenFont.Extensions.TranslatedOS2FontStyle;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    internal class FontFamilyInfo
    {
        public readonly string FamilyName;
        public readonly List<FontFaceInfo> Faces = new List<FontFaceInfo>();

        public FontFamilyInfo(string familyName)
        {
            FamilyName = familyName;
        }

        public override string ToString()
        {
            return string.Format("{{FamilyName: {0}, FacesCount: {1} }}", FamilyName, Faces.Count);
        }
    }

    internal class FontFaceInfo
    {
        public readonly string FontFile;
        public readonly int FaceIndex;
        public readonly FontDescriptionStyle Style;

        // optional properties
        public readonly string StyleName;
        private readonly SharpFont.FaceFlags FaceFlags;

        public FontFaceInfo(string fontPath, int faceIndex, FontDescriptionStyle style)
        {
            this.FontFile = fontPath;
            this.FaceIndex = faceIndex;
            this.Style = style;
        }

        public FontFaceInfo(string fontPath, int faceIndex, FontDescriptionStyle style, string styleName)
        {
            this.FontFile = fontPath;
            this.FaceIndex = faceIndex;
            this.Style = style;

            this.StyleName = styleName;
        }

        public FontFaceInfo(string fontPath, int faceIndex, FontDescriptionStyle style, string styleName, SharpFont.FaceFlags faceFlags)
        {
            this.FontFile = fontPath;
            this.FaceIndex = faceIndex;
            this.Style = style;

            this.StyleName = styleName;
            this.FaceFlags = faceFlags;
        }

        internal static FontDescriptionStyle ToFontStyle(SharpFont.StyleFlags styleFlags)
        {
            FontDescriptionStyle style = default(FontDescriptionStyle);

            if ((styleFlags & SharpFont.StyleFlags.Bold) == SharpFont.StyleFlags.Bold)
                style |= FontDescriptionStyle.Bold;
            if ((styleFlags & SharpFont.StyleFlags.Italic) == SharpFont.StyleFlags.Italic)
                style |= FontDescriptionStyle.Italic;

            return style;
        }

        internal static FontDescriptionStyle ToFontStyle(OpenFontStyleFlags styleFlags)
        {
            FontDescriptionStyle style = default(FontDescriptionStyle);

            if ((styleFlags & OpenFontStyleFlags.BOLD) == OpenFontStyleFlags.BOLD)
                style |= FontDescriptionStyle.Bold;
            if ((styleFlags & OpenFontStyleFlags.ITALIC) == OpenFontStyleFlags.ITALIC)
                style |= FontDescriptionStyle.Italic;

            // not supported styles
            if ((styleFlags & OpenFontStyleFlags.OBLIQUE) == OpenFontStyleFlags.OBLIQUE)
                style = (FontDescriptionStyle)(-1);

            return style;
        }

        public override string ToString()
        {
            return string.Format("{{FontFile: {0}, FaceIndex: {1}, Style: {2}, StyleName: {3}, FaceFlags: [{4}] }}",
                FontFile, FaceIndex, Style,
                StyleName, FaceFlags);
        }
    }
}