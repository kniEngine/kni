// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;


namespace Microsoft.Xna.Framework.Graphics
{
    internal class ConstantBuffer : GraphicsResource
    {
        private ConstantBufferStrategy _strategy;
        
        public ConstantBuffer(GraphicsDevice device,
                              string name,
                              int[] parameterIndexes,
                              int[] parameterOffsets,
                              int sizeInBytes,
                              ShaderProfileType profile)
        {
            _strategy = device.Strategy.MainContext.Strategy.CreateConstantBufferStrategy(name, parameterIndexes, parameterOffsets, sizeInBytes, profile);

            SetGraphicsDevice(device);
        }

        public ConstantBuffer(ConstantBuffer cloneSource)
        {
            SetGraphicsDevice(cloneSource.GraphicsDevice);

            _strategy = (ConstantBufferStrategy)cloneSource._strategy.Clone();
        }


        internal void Apply(GraphicsContextStrategy contextStrategy, ShaderStage stage, int slot)
        {
            _strategy.PlatformApply(contextStrategy, stage, slot);
        }

        internal void Clear()
        {
            _strategy.PlatformClear();
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
            if (_strategy.StateKey > EffectParameter.NextStateKey)
                _strategy.StateKey = 0;
            
            for (var p = 0; p < _strategy.Parameters.Length; p++)
            {
                var index = _strategy.Parameters[p];
                var param = parameters[index];

                if (param.StateKey < _strategy.StateKey)
                    continue;

                var offset = _strategy.Offsets[p];
                _strategy.Dirty = true;

                SetParameter(param, offset);
            }

            _strategy.StateKey = EffectParameter.NextStateKey;
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
                    Buffer.BlockCopy(source, 0, _strategy.Buffer, offset, elementSize);
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
                Buffer.BlockCopy(source, 0, _strategy.Buffer, offset, rows * stride);
            }
            // Take care of Matrix3x3 and Matrix4x3. (unroll loop)
            else if (rows == 3 && (columns == 3 || columns == 4))
            {
                Array source = data as Array;
                int stride = (columns*elementSize);
                Buffer.BlockCopy(source, stride*0, _strategy.Buffer, offset + (rowSize*0), stride);
                Buffer.BlockCopy(source, stride*1, _strategy.Buffer, offset + (rowSize*1), stride);
                Buffer.BlockCopy(source, stride*2, _strategy.Buffer, offset + (rowSize*2), stride);
            }
            else
            {
                Array source = data as Array;
                int stride = (columns*elementSize);
                for (int y = 0; y < rows; y++)
                {
                    Buffer.BlockCopy(source, stride * y, _strategy.Buffer, offset + (rowSize * y), stride);
                }
            }
        }


        protected override void Dispose(bool disposing)
        {
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
