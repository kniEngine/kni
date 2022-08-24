// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos


namespace Microsoft.Xna.Framework.Graphics
{
    internal sealed class ConstantBufferCollection
    {
        private readonly ConstantBuffer[] _buffers;

        private ShaderStage _stage;
        private ShaderStage Stage { get { return this._stage; } }

        private int _valid;

        internal ConstantBufferCollection(ShaderStage stage, int maxBuffers)
        {
            _stage = stage;
            _buffers = new ConstantBuffer[maxBuffers];
            _valid = 0;
        }

        public ConstantBuffer this[int index]
        {
            get { return _buffers[index]; }
            set
            {
                if (_buffers[index] == value)
                    return;

                if (value != null)
                {
                    _buffers[index] = value;
                    _valid |= 1 << index;
                }
                else
                {
                    _buffers[index] = null;
                    _valid &= ~(1 << index);
                }
            }
        }

        internal void Clear()
        {
            for (var i = 0; i < _buffers.Length; i++)
                _buffers[i] = null;

            _valid = 0;
        }

        internal void SetConstantBuffers()
        {
            var validMask = _valid;

            for (var i = 0; i < _buffers.Length && validMask != 0; i++)
            {
                var buffer = _buffers[i];
                if (buffer != null && !buffer.IsDisposed)
                {
                    buffer.PlatformApply(_stage, i);
                }

                // clear buffer bit
                validMask &= ~(1 << i);
            }
        }

    }
}
