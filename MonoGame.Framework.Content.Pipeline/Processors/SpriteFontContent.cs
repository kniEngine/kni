// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using System.Collections.Generic;


namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
	public class SpriteFontContent
    {
        internal Texture2DContent Texture = new Texture2DContent();
        internal List<Rectangle> Glyphs = new List<Rectangle>();
        internal List<Rectangle> Cropping = new List<Rectangle>();
        internal List<char> CharacterMap = new List<char>();
        internal List<Vector3> Kerning = new List<Vector3>();
        internal int VerticalLineSpacing;
        internal float HorizontalSpacing;

        internal Nullable<char> DefaultCharacter;

        internal SpriteFontContent()
        {
        }

        internal SpriteFontContent(FontDescription desc)
        {
            FontName = desc.FontName;
            Style = desc.Style;
            FontSize = desc.Size;

            CharacterMap = new List<char>(desc.Characters.Count);

            VerticalLineSpacing = (int)desc.Spacing; // Will be replaced in the pipeline.
            HorizontalSpacing = desc.Spacing;

            DefaultCharacter = desc.DefaultCharacter;
        }

        internal string FontName = string.Empty;
        FontDescriptionStyle Style = FontDescriptionStyle.Regular;
        internal float FontSize; 

    }
}
