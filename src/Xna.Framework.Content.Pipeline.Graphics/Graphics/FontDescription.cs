﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using System.Collections.Generic;


namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
	/// <summary>
	/// Provides information to the FontDescriptionProcessor describing which font to rasterize, which font size to utilize, and which Unicode characters to include in the processor output.
	/// </summary>
	public class FontDescription : ContentItem
	{
        private string _fontName;
        private float _size;
        private float _spacing;
        private FontDescriptionStyle _style;
        private bool _useKerning;
	    private ICollection<char> _characters = new HashSet<char>();
        private char? _defaultCharacter;

		/// <summary>
		/// Gets or sets the name of the font, such as "Times New Roman" or "Arial". This value cannot be null or empty.
		/// </summary>
        [ContentSerializer(AllowNull = false)]
		public string FontName
		{
			get { return _fontName; }
			set
			{
				if (string.IsNullOrEmpty(value))
					throw new ArgumentNullException("FontName is null or an empty string.");
				_fontName = value;
			}
		}

		/// <summary>
		/// Gets or sets the size, in points, of the font.
		/// </summary>
		public float Size
		{
			get { return _size; }
			set
			{
				if (value <= 0.0f)
					throw new ArgumentOutOfRangeException("Size must be greater than zero.");
				_size = value;
			}
		}

		/// <summary>
		/// Gets or sets the amount of space, in pixels, to insert between letters in a string.
		/// </summary>
        [ContentSerializer(Optional = true)]
		public float Spacing
		{
			get { return _spacing; }
			set { _spacing = value; }
		}

        /// <summary>
        /// Indicates if kerning information is used when drawing characters.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public bool UseKerning
        {
            get { return _useKerning; }
            set { _useKerning = value; }
        }

		/// <summary>
		/// Gets or sets the style of the font, expressed as a combination of one or more FontDescriptionStyle flags.
		/// </summary>
		public FontDescriptionStyle Style
		{
			get { return _style; }
			set { _style = value; }
		}

        /// <summary>
        /// Gets or sets the default character for the font.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public Nullable<char> DefaultCharacter
        {
            get { return _defaultCharacter; }
            set { _defaultCharacter = value; }
        }

        [ContentSerializer(CollectionItemName = "CharacterRegion")]
        internal CharacterRegion[] CharacterRegions
        {
            get
            {
                List<CharacterRegion> regions = new List<CharacterRegion>();
                List<char> chars = new List<char>(Characters);
                chars.Sort();

                char start = chars[0];
                char end = chars[0];

                for (int i=1; i < chars.Count; i++)
                {
                    if (chars[i] != (end+1))
                    {
                        regions.Add(new CharacterRegion(start, end));
                        start = chars[i];
                    }
                    end = chars[i];
                }

                regions.Add(new CharacterRegion(start, end));

                return regions.ToArray();
            }

            set
            {
                for (int index = 0; index < value.Length; ++index)
                {
                    CharacterRegion characterRegion = value[index];
                    if (characterRegion.End < characterRegion.Start)
                        throw new ArgumentException("CharacterRegion.End must be greater than CharacterRegion.Start");

                    for (char start = characterRegion.Start; start <= characterRegion.End; start++)
                        Characters.Add(start);
                }
            }
        }
		
	    [ContentSerializerIgnore]
	    public ICollection<char> Characters
	    {
	        get { return _characters; } 
            internal set { _characters = new HashSet<char>(value); }
	    }

        internal FontDescription()
        {
        }

		/// <summary>
		/// Initializes a new instance of FontDescription and initializes its members to the specified font, size, and spacing, using FontDescriptionStyle.Regular as the default value for Style.
		/// </summary>
		/// <param name="fontName">The name of the font, such as Times New Roman.</param>
		/// <param name="size">The size, in points, of the font.</param>
		/// <param name="spacing">The amount of space, in pixels, to insert between letters in a string.</param>
		public FontDescription(string fontName, float size, float spacing)
			: this(fontName, size, spacing, FontDescriptionStyle.Regular, false)
		{
		}

		/// <summary>
		/// Initializes a new instance of FontDescription and initializes its members to the specified font, size, spacing, and style.
		/// </summary>
		/// <param name="fontName">The name of the font, such as Times New Roman.</param>
		/// <param name="size">The size, in points, of the font.</param>
		/// <param name="spacing">The amount of space, in pixels, to insert between letters in a string.</param>
		/// <param name="fontStyle">The font style for the font.</param>
		public FontDescription(string fontName, float size, float spacing, FontDescriptionStyle fontStyle)
            : this(fontName, size, spacing, fontStyle, false)
		{
		}

		/// <summary>
		/// Initializes a new instance of FontDescription using the specified values.
		/// </summary>
		/// <param name="fontName">The name of the font, such as Times New Roman.</param>
		/// <param name="size">The size, in points, of the font.</param>
		/// <param name="spacing">The amount of space, in pixels, to insert between letters in a string.</param>
		/// <param name="fontStyle">The font style for the font.</param>
		/// <param name="useKerning">true if kerning information is used when drawing characters; false otherwise.</param>
		public FontDescription(string fontName, float size, float spacing, FontDescriptionStyle fontStyle, bool useKerning)            
		{
			// Write to the properties so the validation is run
			this.FontName = fontName;
            this.Size = size;
            this.Spacing = spacing;
            this.Style = fontStyle;
            this.UseKerning = useKerning;			
		}
	}
}
