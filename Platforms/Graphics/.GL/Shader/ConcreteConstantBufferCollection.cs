﻿// MonoGame - Copyright (C) The MonoGame Team
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

        internal void Apply(GraphicsContextStrategy contextStrategy)
        {
            uint validMask = _valid;

            for (int slot = 0; validMask != 0 && slot < _buffers.Length; slot++)
            {
                uint mask = ((uint)1) << slot;

                ConstantBuffer constantBuffer = _buffers[slot];
                if (constantBuffer != null && !constantBuffer.IsDisposed)
                {
                    ((IPlatformConstantBuffer)constantBuffer).Strategy.ToConcrete<ConcreteConstantBuffer>().PlatformApply(contextStrategy, slot);
                }

                // clear buffer bit
                validMask &= ~mask;
            }
        }

    }
}
