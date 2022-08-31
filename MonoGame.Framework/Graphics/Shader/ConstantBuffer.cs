// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using Microsoft.Xna.Platform.Graphics;
using System;


namespace Microsoft.Xna.Framework.Graphics
{
    internal partial class ConstantBuffer : GraphicsResource
    {
        private ConstantBufferStrategy _strategy;
        
        public ConstantBuffer(GraphicsDevice device,
                              string name,
                              int[] parameterIndexes,
                              int[] parameterOffsets,
                              int sizeInBytes)
        {
            _strategy = new ConcreteConstantBufferStrategy(device);

            GraphicsDevice = device;

            _strategy._name = name;
            _strategy._parameters = parameterIndexes;
            _strategy._offsets = parameterOffsets;
            _strategy._buffer = new byte[sizeInBytes];

            PlatformInitialize();
            _strategy.PlatformInitialize();
        }

        public ConstantBuffer(ConstantBuffer cloneSource)
        {
            GraphicsDevice = cloneSource.GraphicsDevice;

            // Share the immutable types.
            _strategy._name = cloneSource._strategy._name;
            _strategy._parameters = cloneSource._strategy._parameters;
            _strategy._offsets = cloneSource._strategy._offsets;

            // Clone the mutable types.
            _strategy = (ConstantBufferStrategy)cloneSource._strategy.Clone();
            _strategy._buffer = (byte[])cloneSource._strategy._buffer.Clone();
            _strategy.PlatformInitialize();
            PlatformInitialize();
        }


        internal void Apply(ShaderStage stage, int slot)
        {
            _strategy.PlatformApply(stage, slot);
            PlatformApply(stage, slot);
        }

        internal void Clear()
        {
            _strategy.PlatformClear();
            PlatformClear();
        }

        public void Update(EffectParameterCollection parameters)
        {
            // TODO:  We should be doing some sort of dirty state 
            // testing here.
            //
            // It should let us skip all parameter updates if
            // nothing has changed.  It should not be per-parameter
            // as that is why you should use multiple constant
            // buffers.

            // If our state key becomes larger than the 
            // next state key then the keys have rolled 
            // over and we need to reset.
            if (_strategy._stateKey > EffectParameter.NextStateKey)
                _strategy._stateKey = 0;
            
            for (var p = 0; p < _strategy._parameters.Length; p++)
            {
                var index = _strategy._parameters[p];
                var param = parameters[index];

                if (param.StateKey < _strategy._stateKey)
                    continue;

                var offset = _strategy._offsets[p];
                _strategy._dirty = true;

                SetParameter(param, offset);
            }

            _strategy._stateKey = EffectParameter.NextStateKey;
        }

        private int SetParameter(EffectParameter param, int offset)
        {
            const int elementSize = 4;
            const int rowSize = elementSize * 4;

            var elements = param.Elements;
            if (elements.Count > 0)
            {
                int elementByteCount = 0;
                for (var i=0; i < elements.Count; i++)
                {
                    int subElementByteCount = SetParameter(elements[i], offset + elementByteCount);
                    elementByteCount += subElementByteCount;
                }
                return elementByteCount;
            }
            else if (param.Data != null)
            {
                switch (param.ParameterType)
                {
                    case EffectParameterType.Single:
                    case EffectParameterType.Int32:
                    case EffectParameterType.Bool:
                        // HLSL assumes matrices are column-major, whereas in-memory we use row-major.
                        // TODO: HLSL can be told to use row-major. We should handle that too.
                        if (param.ParameterClass == EffectParameterClass.Matrix)
                        {
                            SetData(offset, param.ColumnCount, param.RowCount, param.Data);
                            return (param.ColumnCount * rowSize);
                        }
                        else
                        {
                            SetData(offset, param.RowCount, param.ColumnCount, param.Data);
                            return (param.RowCount * rowSize);
                        }
                    default:
                        throw new NotSupportedException("Not supported!");
                }
            }
            else
            {
                return 0;
            }
        }

        private void SetData(int offset, int rows, int columns, object data)
        {
            // Shader registers are always 4 bytes and all the
            // incoming data objects should be 4 bytes per element.
            const int elementSize = 4;
            const int rowSize = elementSize * 4;

            // Take care of a single element.
            if (rows == 1 && columns == 1)
            {
                // EffectParameter stores all values in arrays by default.
                if (data is Array)
                {
                    Array source = data as Array;
                    Buffer.BlockCopy(source, 0, _strategy._buffer, offset, elementSize);
                }
                else
                {
                    // TODO: When we eventually expose the internal Shader 
                    // API then we will need to deal with non-array elements.
                    throw new NotImplementedException();
                }
            }
            // Take care of the single copy case!
            else if (rows == 1 || (rows == 4 && columns == 4))
            {
                Array source = data as Array;
                int stride = (columns * elementSize);
                Buffer.BlockCopy(source, 0, _strategy._buffer, offset, rows * stride);
            }
            // Take care of Matrix3x3 and Matrix4x3. (unroll loop)
            else if (rows == 3 && (columns == 3 || columns == 4))
            {
                Array source = data as Array;
                int stride = (columns*elementSize);
                Buffer.BlockCopy(source, stride*0, _strategy._buffer, offset + (rowSize*0), stride);
                Buffer.BlockCopy(source, stride*1, _strategy._buffer, offset + (rowSize*1), stride);
                Buffer.BlockCopy(source, stride*2, _strategy._buffer, offset + (rowSize*2), stride);
            }
            else
            {
                Array source = data as Array;
                int stride = (columns*elementSize);
                for (int y = 0; y < rows; y++)
                {
                    Buffer.BlockCopy(source, stride * y, _strategy._buffer, offset + (rowSize * y), stride);
                }
            }
        }


        protected override void Dispose(bool disposing)
        {
            PlatformDispose(disposing);

            if (disposing)
            {
                if (_strategy != null)
                    _strategy.Dispose();

                _strategy = null;
            }

            base.Dispose(disposing);
        }
    }
}
