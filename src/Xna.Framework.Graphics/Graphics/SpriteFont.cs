// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

// Original code from SilverSprite Project
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics 
{

    public sealed class SpriteFont 
    {
        private readonly char[] _characters;
        private readonly Glyph[] _glyphs;
        private readonly CharacterRegion[] _regions;
        private char? _defaultCharacter;
        private int _defaultGlyphIndex = -1;
        
        private readonly Texture2D _texture;

        ///<remarks>SpriteBatcher need direct accest to the Glyph array</remarks>
        internal Glyph[] InternalGlyphs { get { return _glyphs; } }

        class CharComparer: IEqualityComparer<char>
        {
            public bool Equals(char x, char y)
            {
                return x == y;
            }

            public int GetHashCode(char b)
            {
                return (b);
            }

            static public readonly CharComparer Default = new CharComparer();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteFont" /> class.
        /// </summary>
        /// <param name="texture">The font texture.</param>
        /// <param name="glyphBounds">The rectangles in the font texture containing letters.</param>
        /// <param name="cropping">The cropping rectangles, which are applied to the corresponding glyphBounds to calculate the bounds of the actual character.</param>
        /// <param name="characters">The characters.</param>
        /// <param name="lineSpacing">The line spacing (the distance from baseline to baseline) of the font.</param>
        /// <param name="spacing">The spacing (tracking) between characters in the font.</param>
        /// <param name="kerning">The letters kernings (X - left side bearing, Y - width and Z - right side bearing).</param>
        /// <param name="defaultCharacter">The character that will be substituted when a given character is not included in the font.</param>
        public SpriteFont(
            Texture2D texture, List<Rectangle> glyphBounds, List<Rectangle> cropping, List<char> characters,
            int lineSpacing, float spacing, List<Vector3> kerning, char? defaultCharacter)
        {
            _texture = texture;
            LineSpacing = lineSpacing;
            Spacing = spacing;

            _characters = characters.ToArray();
            _glyphs = new Glyph[characters.Count];
            var regions = new Stack<CharacterRegion>();

            for (int i = 0; i < characters.Count; i++) 
            {
                _glyphs[i] = new Glyph 
                {
                    BoundsInTexture = glyphBounds[i],
                    Cropping = cropping[i],

                    LeftSideBearing = kerning[i].X,
                    Width = kerning[i].Y,
                    RightSideBearing = kerning[i].Z,

                    WidthIncludingBearings = kerning[i].X + kerning[i].Y + kerning[i].Z,
                    
                    TexCoordTL = new Vector2( glyphBounds[i].X * _texture.TexelWidth, 
                                              glyphBounds[i].Y * _texture.TexelHeight),
                    TexCoordBR = new Vector2( (glyphBounds[i].X + glyphBounds[i].Width) * _texture.TexelWidth, 
                                              (glyphBounds[i].Y + glyphBounds[i].Height) * _texture.TexelHeight),
                };
                
                if(regions.Count == 0 || characters[i] > (regions.Peek().End+1))
                {
                    // Start a new region
                    regions.Push(new CharacterRegion(characters[i], i));
                } 
                else if(characters[i] == (regions.Peek().End+1))
                {
                    CharacterRegion currentRegion = regions.Pop();
                    // include character in currentRegion
                    currentRegion.End++;
                    regions.Push(currentRegion);
                }
                else // characters[i] < (regions.Peek().End+1)
                {
                    throw new InvalidOperationException("Invalid SpriteFont. Character map must be in ascending order.");
                }
            }

            _regions = regions.ToArray();
            Array.Reverse(_regions);

            Glyphs = new GlyphCollection(this);

            DefaultCharacter = defaultCharacter;
        }

        /// <summary>
        /// Gets the texture that this SpriteFont draws from.
        /// </summary>
        /// <remarks>Can be used to implement custom rendering of a SpriteFont</remarks>
        public Texture2D Texture { get { return _texture; } }

        /// <summary>
        /// Gets a collection of the characters in the font.
        /// </summary>
        public ReadOnlyCollection<char> Characters
        {
            get { return (ReadOnlyCollection<char>)((IDictionary<char, Glyph>)Glyphs).Keys; }
        }

        /// <summary>
        /// The glyphs in this SpriteFont.
        /// </summary>
        ///
        public readonly GlyphCollection Glyphs;

        /// <summary>
        /// Gets or sets the character that will be substituted when a
        /// given character is not included in the font.
        /// </summary>
        public char? DefaultCharacter
        {
            get { return _defaultCharacter; }
            set
            {   
                // Get the default glyph index here once.
                if (value.HasValue)
                {
                    if(!TryGetGlyphIndex(value.Value, out _defaultGlyphIndex))
                        throw new ArgumentException(UnresolvableCharacterErrorMessage(value.Value));
                }
                else
                    _defaultGlyphIndex = -1;

                _defaultCharacter = value;
            }
        }

        /// <summary>
        /// Gets or sets the line spacing (the distance from baseline
        /// to baseline) of the font.
        /// </summary>
        public int LineSpacing { get; set; }

        /// <summary>
        /// Gets or sets the spacing (tracking) between characters in
        /// the font.
        /// </summary>
        public float Spacing { get; set; }

        /// <summary>
        /// Returns the size of a string when rendered in this font.
        /// </summary>
        /// <param name="text">The text to measure.</param>
        /// <returns>The size, in pixels, of 'text' when rendered in
        /// this font.</returns>
        public unsafe Vector2 MeasureString(string text)
        {
            char* pChars = stackalloc char[text.Length];
            int* pGlyphIndices = stackalloc int[text.Length];
            GetGlyphIndexes(text, pChars, pGlyphIndices, text.Length);
            return MeasureString(pChars, pGlyphIndices, text.Length);
        }

        /// <summary>
        /// Returns the size of the contents of a StringBuilder when
        /// rendered in this font.
        /// </summary>
        /// <param name="text">The text to measure.</param>
        /// <returns>The size, in pixels, of 'text' when rendered in
        /// this font.</returns>
        public unsafe Vector2 MeasureString(StringBuilder text)
        {
            char* pChars = stackalloc char[text.Length];
            int* pGlyphIndices = stackalloc int[text.Length];
            GetGlyphIndexes(text, pChars, pGlyphIndices, text.Length);
            return MeasureString(pChars, pGlyphIndices, text.Length);
        }

        internal unsafe Vector2 MeasureString(char* pChars, int* pGlyphIndices, int charsCount)
        {
            if (charsCount == 0)
                return Vector2.Zero;

            float width = 0.0f;
            float finalLineHeight = (float)LineSpacing;
            
            Vector2 offset = Vector2.Zero;
            bool firstGlyphOfLine = true;

            fixed (Glyph* pGlyphs = InternalGlyphs)
            for (int i = 0; i < charsCount; i++)
            {
                char c = pChars[i];

                if (c == '\r')
                    continue;

                if (c == '\n')
                {
                    finalLineHeight = LineSpacing;

                    offset.X = 0;
                    offset.Y += LineSpacing;
                    firstGlyphOfLine = true;
                    continue;
                }

                int currentGlyphIndex = pGlyphIndices[i];
                if (currentGlyphIndex == -1)
                    throw new ArgumentException(UnresolvableCharacterErrorMessage(c), "text");

                Debug.Assert(currentGlyphIndex >= 0 && currentGlyphIndex < InternalGlyphs.Length, "currentGlyphIndex was outside the bounds of the array.");
                Glyph* pCurrentGlyph = pGlyphs + currentGlyphIndex;

                // The first character on a line might have a negative left side bearing.
                // In this scenario, SpriteBatch/SpriteFont normally offset the text to the right,
                //  so that text does not hang off the left side of its rectangle.
                if (firstGlyphOfLine)
                {
                    offset.X = Math.Max(pCurrentGlyph->LeftSideBearing, 0);
                    firstGlyphOfLine = false;
                }
                else
                {
                    offset.X += Spacing + pCurrentGlyph->LeftSideBearing;
                }

                offset.X += pCurrentGlyph->Width;

                float proposedWidth = offset.X + Math.Max(pCurrentGlyph->RightSideBearing, 0);
                if (proposedWidth > width)
                    width = proposedWidth;

                offset.X += pCurrentGlyph->RightSideBearing;

                if (pCurrentGlyph->Cropping.Height > finalLineHeight)
                    finalLineHeight = pCurrentGlyph->Cropping.Height;
            }

            float height = offset.Y + finalLineHeight;
            return new Vector2(width, height);
        }
        
        internal unsafe bool TryGetGlyphIndex(char c, out int index)
        {
            fixed (CharacterRegion* pRegions = _regions)
                return TryGetGlyphIndex(pRegions, c, out index);
        }
        
        private unsafe bool TryGetGlyphIndex(CharacterRegion* pRegions, char c, out int index)
        {
            // Get region Index
            int regionIdx = -1;
            int l = 0;
            int r = _regions.Length - 1;
            while (l <= r)
            {
                int m = (l + r) >> 1;
                Debug.Assert(m >= 0 && m < _regions.Length, "Index was outside the bounds of the array.");
                if (pRegions[m].End < c)
                {
                    l = m + 1;
                }
                else if (pRegions[m].Start > c)
                {
                    r = m - 1;
                }
                else
                {
                    regionIdx = m;
                    break;
                }
            }

            if (regionIdx != -1)
            {
                index = pRegions[regionIdx].StartIndex + (c - pRegions[regionIdx].Start);
                return true;
            }
            else
            {
                index = -1;
                return false;
            }
        }

        internal unsafe void GetGlyphIndexes(StringBuilder text, char* pChars, int* pGlyphIndices, int charsCount)
        {
            fixed (CharacterRegion* pRegions = _regions)
            {
                for (int i = 0; i < charsCount; i++)
                {
                    pChars[i] = text[i];
                    if (!TryGetGlyphIndex(pRegions, pChars[i], out pGlyphIndices[i]))
                    {
                        pGlyphIndices[i] = _defaultGlyphIndex;
                    }
                }
            }
        }

        internal unsafe void GetGlyphIndexes(string text, char* pChars, int* pGlyphIndices, int charsCount)
        {
            fixed (CharacterRegion* pRegions = _regions)
            {
                for (int i = 0; i < charsCount; i++)
                {
                    pChars[i] = text[i];
                    if (!TryGetGlyphIndex(pRegions, pChars[i], out pGlyphIndices[i]))
                    {
                        pGlyphIndices[i] = _defaultGlyphIndex;
                    }
                }
            }
        }

        internal static string UnresolvableCharacterErrorMessage(char ch)
        {
            return String.Format("Character '{0}' (#{1}) cannot be resolved by this SpriteFont.", ch,(int)ch);
        }

        /// <summary>
        /// Struct that defines the spacing, Kerning, and bounds of a character.
        /// </summary>
        /// <remarks>Provides the data necessary to implement custom SpriteFont rendering.</remarks>
        public struct Glyph 
        {
            /// <summary>
            /// Rectangle in the font texture where this letter exists.
            /// </summary>
            public Rectangle BoundsInTexture;
            /// <summary>
            /// Cropping applied to the BoundsInTexture to calculate the bounds of the actual character.
            /// </summary>
            public Rectangle Cropping;
            /// <summary>
            /// The amount of space between the left side of the character and its first pixel in the X dimension.
            /// </summary>
            public float LeftSideBearing;
            /// <summary>
            /// The amount of space between the right side of the character and its last pixel in the X dimension.
            /// </summary>
            public float RightSideBearing;
            /// <summary>
            /// Width of the character before kerning is applied. 
            /// </summary>
            public float Width;
            /// <summary>
            /// Width of the character before kerning is applied. 
            /// </summary>
            public float WidthIncludingBearings;

            internal Vector2 TexCoordTL;
            internal Vector2 TexCoordBR;

            public static readonly Glyph Empty = new Glyph();

            public override string ToString ()
            {
                return "Glyph=" + BoundsInTexture + ", Cropping=" + Cropping + ", Kerning=" + LeftSideBearing + "," + Width + "," + RightSideBearing;
            }
        }

        private struct CharacterRegion
        {
            public char Start;
            public char End;
            public int StartIndex;

            public CharacterRegion(char start, int startIndex)
            {
                this.Start = start;                
                this.End = start;
                this.StartIndex = startIndex;
            }
        }

        public class GlyphCollection : IDictionary<char, Glyph>
        {
            private readonly SpriteFont _spriteFont;
            private readonly ReadOnlyCollection<char> _keys;
            private readonly ReadOnlyCollection<Glyph> _values;

            internal GlyphCollection(SpriteFont spriteFont)
            {
                _spriteFont = spriteFont;
                _keys = new ReadOnlyCollection<char>(_spriteFont._characters);
                _values = new ReadOnlyCollection<Glyph>(_spriteFont._glyphs);
            }

            public Glyph this[char key]
            {
                get
                {
                    int glyphIdx;
                    if (_spriteFont.TryGetGlyphIndex(key, out glyphIdx))
                        return _spriteFont._glyphs[glyphIdx];
                    else
                        throw new KeyNotFoundException();
                }
                set { throw new NotSupportedException(); }
            }

            public int Count { get { return _spriteFont._glyphs.Length; } }

            public bool IsReadOnly { get { return true; } }

            ICollection<char> IDictionary<char, Glyph>.Keys { get { return _keys; } }

            ICollection<Glyph> IDictionary<char, Glyph>.Values { get { return _values; } }

            public bool ContainsKey(char key)
            {
                int glyphIdx;
                return _spriteFont.TryGetGlyphIndex(key, out glyphIdx);
            }

            public bool TryGetValue(char key, out Glyph value)
            {
                int glyphIdx;
                bool isFound = _spriteFont.TryGetGlyphIndex(key, out glyphIdx);
                value = (isFound) ? _spriteFont._glyphs[glyphIdx] : default(Glyph);
                return isFound;
            }

            bool ICollection<KeyValuePair<char, Glyph>>.Contains(KeyValuePair<char, Glyph> item)
            {
                return (_keys.Contains(item.Key) && this[item.Key].Equals(item.Value));
            }

            void ICollection<KeyValuePair<char, Glyph>>.CopyTo(KeyValuePair<char, Glyph>[] array, int arrayIndex)
            {
                foreach (var keyValue in this)
                    array[arrayIndex++] = keyValue;
            }

            IEnumerator<KeyValuePair<char, Glyph>> IEnumerable<KeyValuePair<char, Glyph>>.GetEnumerator()
            {
                return new CharGlyphPairEnumerator(this, _keys);
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return ((IEnumerable<KeyValuePair<char, Glyph>>)this).GetEnumerator();
            }

            void IDictionary<char, Glyph>.Add(char key, Glyph value)
            {
                throw new NotSupportedException();
            }

            bool IDictionary<char, Glyph>.Remove(char key)
            {
                throw new NotSupportedException();
            }

            void ICollection<KeyValuePair<char, Glyph>>.Add(KeyValuePair<char, Glyph> item)
            {
                throw new NotSupportedException();
            }

            bool ICollection<KeyValuePair<char, Glyph>>.Remove(KeyValuePair<char, Glyph> item)
            {
                throw new NotSupportedException();
            }

            void ICollection<KeyValuePair<char, Glyph>>.Clear()
            {
                throw new NotSupportedException();
            }

            public struct CharGlyphPairEnumerator : IEnumerator<KeyValuePair<char, Glyph>>
            {
                private GlyphCollection _collection;
                private ReadOnlyCollection<char> _keys;
                private int i;

                public CharGlyphPairEnumerator(GlyphCollection collection, ReadOnlyCollection<char> keys)
                {
                    _collection = collection;
                    _keys = keys;
                    i = -1;
                }

                #region IEnumerator<KeyValuePair<char, Glyph>>

                public KeyValuePair<char, Glyph> Current
                {
                    get
                    {
                        char key = _keys[i];
                        return new KeyValuePair<char, Glyph>(key, _collection[key]);
                    }
                }

                #endregion IEnumerator<KeyValuePair<char, Glyph>>

                #region IEnumerator

                public bool MoveNext()
                {
                    return (++i < _keys.Count);
                }

                object IEnumerator.Current
                {
                    get
                    {
                        char key = _keys[i];
                        return new KeyValuePair<char, Glyph>(key, _collection[key]);
                    }
                }

                void IDisposable.Dispose()
                {
                        _collection = null;
                        _keys = null;
                        i = -1;
                }

                void IEnumerator.Reset()
                {
                    i = -1;
                }

                #endregion IEnumerator
            }
        }

    }
}
