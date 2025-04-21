// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteConstantBufferCollection : ConstantBufferCollectionStrategy
    {
        private readonly ConstantBuffer[] _buffers;
        private uint _valid;

        public int Length { get { return _buffers.Length; } }

        internal uint InternalValid { get { return this._valid; } }

        internal ConcreteConstantBufferCollection(int capacity)
            : base(capacity)
        {
            // hard limit of 32 because of _valid flags being 32bits.
            if (capacity > 32)
                throw new ArgumentOutOfRangeException("capacity");

            _buffers = new ConstantBuffer[capacity];
            _valid = 0;
        }

        public override ConstantBuffer this[int index]
        {
            get { return _buffers[index]; }
            set
            {
                if (_buffers[index] != value)
                {
                    uint mask = ((uint)1) << index;
                    _buffers[index] = value;

                    if (value != null)
                        _valid |= mask;
                    else
                        _valid &= ~mask;
                }
            }
        }

        public override void Clear()
        {
            for (int slot = 0; slot < _buffers.Length; slot++)
                _buffers[slot] = null;

            _valid = 0;
        }

        internal static void Apply(ConcreteGraphicsContextGL ccontextStrategy, ConcreteConstantBufferCollection cconstantBufferCollection)
        {
            uint validMask = cconstantBufferCollection.InternalValid;

            for (int slot = 0; validMask != 0 && slot < cconstantBufferCollection.Length; slot++)
            {
                uint mask = ((uint)1) << slot;

                ConstantBuffer constantBuffer = cconstantBufferCollection[slot];
                if (constantBuffer != null && !constantBuffer.IsDisposed)
                {
                    ConcreteConstantBuffer constantBufferStrategy = ((IPlatformConstantBuffer)constantBuffer).Strategy.ToConcrete<ConcreteConstantBuffer>();

                    constantBufferStrategy.PlatformApply(ccontextStrategy, slot);
                }

                // clear buffer bit
                validMask &= ~mask;
            }
        }

    }
}
