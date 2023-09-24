// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;
using D3D11 = SharpDX.Direct3D11;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConstantBufferCollection
    {
        private readonly ConstantBuffer[] _buffers;
        private uint _valid;

        internal ConstantBufferCollection(int capacity)
        {
            // hard limit of 32 because of _valid flags being 32bits.
            if (capacity > 32)
                throw new ArgumentOutOfRangeException("capacity");

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
            for (int slot = 0; slot < _buffers.Length; slot++)
                _buffers[slot] = null;

            _valid = 0;
        }

        internal void Apply(GraphicsContextStrategy contextStrategy, D3D11.CommonShaderStage shaderStage)
        {
            uint validMask = _valid;

            for (var slot = 0; validMask != 0 && slot < _buffers.Length; slot++)
            {
                var buffer = _buffers[slot];
                if (buffer != null && !buffer.IsDisposed)
                {
                    buffer.Strategy.ToConcrete<ConcreteConstantBuffer>().PlatformApply(contextStrategy, slot, shaderStage);
                }

                uint mask = ((uint)1) << slot;
                // clear buffer bit
                validMask &= ~mask;
            }
        }

    }
}
