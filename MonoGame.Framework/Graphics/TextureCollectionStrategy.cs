﻿// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    public abstract class TextureCollectionStrategy
    {   
        protected readonly GraphicsContextStrategy _contextStrategy;

        internal readonly Texture[] _textures;
        internal uint _dirty;

        internal TextureCollectionStrategy(GraphicsContextStrategy contextStrategy, int capacity)
        {
            // hard limit of 32 because of _dirty flags being 32bits.
            if (capacity > 32)
                throw new ArgumentOutOfRangeException("capacity");

            _contextStrategy = contextStrategy;

            _textures = new Texture[capacity];

            Dirty();
        }

        public Texture this[int index]
        {
            get { return _textures[index]; }
            set
            {
                if (_textures[index] != value)
                {
                    uint mask = ((uint)1) << index;
                    _textures[index] = value;
                    _dirty |= mask;
                }
            }
        }

        internal virtual void Clear()
        {
            for (int i = 0; i < _textures.Length; i++)
            {
                _textures[i] = null;
            }

            Dirty();
        }

        /// <summary>
        /// Marks all texture slots as dirty.
        /// </summary>
        internal void Dirty()
        {
            for (int i = 0; i < _textures.Length; i++)
                _dirty |= (((uint)1) << i);
        }

    }
}
