// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    public interface IPlatformTextureCollection
    {
        TextureCollectionStrategy Strategy { get; }
    }

    public abstract class TextureCollectionStrategy
    {   
        protected readonly GraphicsContextStrategy _contextStrategy;

        protected readonly Texture[] _textures;
        protected uint _dirty;

        protected TextureCollectionStrategy(GraphicsContextStrategy contextStrategy, int capacity)
        {
            // hard limit of 32 because of _dirty flags being 32bits.
            if (capacity > 32)
                throw new ArgumentOutOfRangeException("capacity");

            _contextStrategy = contextStrategy;

            _textures = new Texture[capacity];

            Dirty();
        }

        public int Length { get { return _textures.Length; } }

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

        public virtual void Clear()
        {
            for (int i = 0; i < _textures.Length; i++)
            {
                _textures[i] = null;
            }

            Dirty();
        }

        public void Dirty(int index)
        {
            uint mask = ((uint)1) << index;
            _dirty |= mask;
        }

        /// <summary>
        /// Marks all texture slots as dirty.
        /// </summary>
        public void Dirty()
        {
            for (int i = 0; i < _textures.Length; i++)
                _dirty |= (((uint)1) << i);
        }

        public T ToConcrete<T>() where T : TextureCollectionStrategy
        {
            return (T)this;
        }

    }
}
