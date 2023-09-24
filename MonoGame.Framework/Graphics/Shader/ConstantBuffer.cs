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
        
        internal ConstantBufferStrategy Strategy { get { return _strategy; } }

        public ConstantBuffer(GraphicsDevice device,
                              string name,
                              int[] parameterIndexes,
                              int[] parameterOffsets,
                              int sizeInBytes,
                              ShaderProfileType profile)
            : base(true)
        {
            _strategy = device.Strategy.MainContext.Strategy.CreateConstantBufferStrategy(name, parameterIndexes, parameterOffsets, sizeInBytes, profile);
            SetResourceStrategy((IGraphicsResourceStrategy)_strategy);
        }

        public ConstantBuffer(ConstantBuffer cloneSource)
            : base(true)
        {
            _strategy = (ConstantBufferStrategy)cloneSource._strategy.Clone();
            SetResourceStrategy((IGraphicsResourceStrategy)_strategy);
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
                            Strategy.SetData(offset, param.ColumnCount, param.RowCount, param.Data);
                            return (param.ColumnCount * rowSize);
                        }
                        else
                        {
                            Strategy.SetData(offset, param.RowCount, param.ColumnCount, param.Data);
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
