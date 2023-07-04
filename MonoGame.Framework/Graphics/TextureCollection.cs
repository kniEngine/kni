// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;


namespace Microsoft.Xna.Framework.Graphics
{
    public sealed partial class TextureCollection
    {
        private readonly GraphicsDevice _device;
        private readonly GraphicsContext _context;
        private readonly ShaderStage _stage = ShaderStage.Pixel;

        private readonly Texture[] _textures;
        private uint _dirty;


        internal TextureCollection(GraphicsDevice device, GraphicsContext context, ShaderStage stage, int capacity)
        {
            // hard limit of 32 because of _dirty flags being 32bits.
            if (capacity > 32)
                throw new ArgumentOutOfRangeException("capacity");
            
            _device = device;
            _context = context;
            _stage = stage;

            _textures = new Texture[capacity];

            Dirty();

            PlatformInit(capacity);
            PlatformClear();
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

        internal void Clear()
        {
            for (var i = 0; i < _textures.Length; i++)
                _textures[i] = null;

            Dirty();

            PlatformClear();
        }

        /// <summary>
        /// Marks all texture slots as dirty.
        /// </summary>
        internal void Dirty()
        {
            for (var i = 0; i < _textures.Length; i++)
                _dirty |= (((uint)1) << i);
        }

    }
}
