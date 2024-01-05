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

        public FontFaceInfo(string fontPath, int faceIndex, FontDescriptionStyle style)
        {
            this.FontFile = fontPath;
            this.FaceIndex = faceIndex;
            this.Style = style;
        }

        public FontFaceInfo(string fontPath, int faceIndex, StyleFlags styleFlags)
        {
            this.FontFile = fontPath;
            this.FaceIndex = faceIndex;

            if (styleFlags == StyleFlags.None)
                this.Style = FontDescriptionStyle.Regular;
            if (styleFlags == StyleFlags.Italic)
                this.Style = FontDescriptionStyle.Italic;
            if (styleFlags == StyleFlags.Bold)
                this.Style = FontDescriptionStyle.Bold;
            if (styleFlags == (StyleFlags.Italic | StyleFlags.Bold))
                this.Style = (FontDescriptionStyle)(-1);
        }


        public override string ToString()
        {
            return string.Format("{{FontFile: {0}, FaceIndex: {1}, Style: {2} }}", FontFile, FaceIndex, Style);
        }
    }
}