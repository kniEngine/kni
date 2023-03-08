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
        private readonly ShaderStage _stage = ShaderStage.Pixel;
        private readonly Texture[] _textures;
        private int _dirty;

        internal TextureCollection(GraphicsDevice device, int maxTextures, ShaderStage stage)
        {
            _device = device;
            _stage = stage;
            _textures = new Texture[maxTextures];
            Dirty();
            PlatformInit();
        }

        public Texture this[int index]
        {
            get
            {
                return _textures[index];
            }
            set
            {
                if (_stage != ShaderStage.Vertex || _device.GraphicsCapabilities.SupportsVertexTextures)
                {
                    if (_textures[index] != value)
                    {
                        int mask = 1 << index;
                        _textures[index] = value;
                        _dirty |= mask;
                    }
                }
                else
                    throw new NotSupportedException("Vertex textures are not supported on this device.");
            }
        }

        internal void Clear()
        {
            for (var i = 0; i < _textures.Length; i++)
                _textures[i] = null;

            PlatformClear();
            Dirty();
        }

        /// <summary>
        /// Marks all texture slots as dirty.
        /// </summary>
        internal void Dirty()
        {
            for (var i = 0; i < _textures.Length; i++)
                _dirty |= (1 << i);
        }

        internal void Apply()
        {
            if (_stage != ShaderStage.Vertex || _device.GraphicsCapabilities.SupportsVertexTextures)
            {
                PlatformApply();
            }
        }
    }
}
