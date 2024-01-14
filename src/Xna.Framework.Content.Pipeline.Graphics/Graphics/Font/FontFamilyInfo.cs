// Copyright (C)2023 Nick Kastellanos

using System.Collections.Generic;
using SharpFont;

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
        private readonly FaceFlags FaceFlags;

        public bool IsMono { get { return FaceFlags.HasFlag(FaceFlags.FixedWidth); } }

        public FontFaceInfo(string fontPath, int faceIndex, FontDescriptionStyle style)
        {
            this.FontFile = fontPath;
            this.FaceIndex = faceIndex;
            this.Style = style;
        }

        public FontFaceInfo(string fontPath, int faceIndex, FontDescriptionStyle style, string styleName, FaceFlags faceFlags)
        {
            this.FontFile = fontPath;
            this.FaceIndex = faceIndex;
            this.Style = style;

            this.StyleName = styleName;
            this.FaceFlags = faceFlags;
        }

        internal static FontDescriptionStyle ToFontStyle(SharpFont.StyleFlags styleFlags)
        {
            FontDescriptionStyle style = FontDescriptionStyle.Regular;

            if (styleFlags == StyleFlags.None)
                style = FontDescriptionStyle.Regular;
            if (styleFlags == StyleFlags.Italic)
                style = FontDescriptionStyle.Italic;
            if (styleFlags == StyleFlags.Bold)
                style = FontDescriptionStyle.Bold;
            if (styleFlags == (StyleFlags.Italic | StyleFlags.Bold))
                style = (FontDescriptionStyle)(-1);

            return style;
        }

        public override string ToString()
        {
            return string.Format("{{FontFile: {0}, FaceIndex: {1}, Style: {2}, StyleName: {3}, FaceFlags: [{4}], IsMono:{5} }}",
                FontFile, FaceIndex, Style,
                StyleName, FaceFlags,
                IsMono);
        }
    }
}