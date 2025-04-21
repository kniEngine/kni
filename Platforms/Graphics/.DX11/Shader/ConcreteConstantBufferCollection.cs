// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;
using D3D11 = SharpDX.Direct3D11;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteConstantBufferCollection : ConstantBufferCollectionStrategy
    {
        private readonly ConstantBuffer[] _buffers;
        private uint _valid;
        private uint _dirty;

        public int Length { get { return _buffers.Length; } }

        internal uint InternalValid { get { return this._valid; } }

        internal uint InternalDirty
        {
            get { return this._dirty; }
            set { this._dirty = value; }
        }

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

                    _dirty |= mask;
                }
            }
        }

        public override void Clear()
        {
            for (int slot = 0; slot < _buffers.Length; slot++)
                _buffers[slot] = null;

            _valid = 0;
        }
    }
}
