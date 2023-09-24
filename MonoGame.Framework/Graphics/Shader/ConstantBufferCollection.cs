// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;


namespace Microsoft.Xna.Framework.Graphics
{
    internal sealed class ConstantBufferCollection
    {
        private ShaderStage _stage;

        private readonly ConstantBuffer[] _buffers;
        private uint _valid;

        private ShaderStage Stage { get { return _stage; } }


        internal ConstantBufferCollection(ShaderStage stage, int capacity)
        {
            // hard limit of 32 because of _valid flags being 32bits.
            if (capacity > 32)
                throw new ArgumentOutOfRangeException("capacity");

            _stage = stage;

            _buffers = new ConstantBuffer[capacity];
            _valid = 0;
        }

        public ConstantBuffer this[int index]
        {
            get { return _buffers[index]; }
            set
            {
                if (_buffers[index] != value)
                {
                    _buffers[index] = value;

                    uint mask = ((uint)1) << index;
                    if (value != null)
                        _valid |= mask;
                    else
                        _valid &= ~mask;
                }
            }
        }

        internal void Clear()
        {
            for (int i = 0; i < _buffers.Length; i++)
                _buffers[i] = null;

            _valid = 0;
        }

        internal void Apply(GraphicsContextStrategy contextStrategy)
        {
            uint validMask = _valid;

            for (var i = 0; validMask != 0 && i < _buffers.Length; i++)
            {
                var buffer = _buffers[i];
                if (buffer != null && !buffer.IsDisposed)
                {
                    buffer.Strategy.PlatformApply(contextStrategy, _stage, i);
                }

                uint mask = ((uint)1) << i;
                // clear buffer bit
                validMask &= ~mask;
            }
        }

    }
}
