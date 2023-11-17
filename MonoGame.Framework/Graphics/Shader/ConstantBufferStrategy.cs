// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    public abstract class ConstantBufferStrategy : GraphicsResourceStrategy, ICloneable, IDisposable
    {
        private string _name;
        private int[] _parameters;
        private int[] _offsets;
        private byte[] _bufferData;
        private ShaderProfileType _profile;
        private bool _dirty;
        private ulong _stateKey;

        public string Name { get { return _name; } }
        public int[] Parameters { get { return _parameters; } }
        public int[] Offsets { get { return _offsets; } }
        public byte[] BufferData { get { return _bufferData; } }
        public virtual bool Dirty
        {
            get { return _dirty; }
            set { _dirty = value; }
        }
        public virtual ulong StateKey
        {
            get { return _stateKey; }
            set { _stateKey = value; }
        }
        protected ShaderProfileType Profile
        {
            get { return _profile; }
        }

        protected ConstantBufferStrategy(GraphicsContextStrategy contextStrategy, string name, int[] parameters, int[] offsets, int sizeInBytes, ShaderProfileType profile)
            : base(contextStrategy)
        {
            this._name = name;
            this._parameters = parameters;
            this._offsets = offsets;
            this._bufferData = new byte[sizeInBytes];
            this._profile = profile;
        }

        protected ConstantBufferStrategy(ConstantBufferStrategy source)
            : base(source)
        {
            // shared
            this._name = source._name;
            this._parameters = source._parameters;
            this._offsets = source._offsets;

            // copies
            this._bufferData = (byte[])source._bufferData.Clone();
            this._profile = source._profile;
        }

        internal T ToConcrete<T>() where T : ConstantBufferStrategy
        {
            return (T)this;
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
            if (this.StateKey > EffectParameter.NextStateKey)
                this.StateKey = 0;
            
            for (int p = 0; p < this.Parameters.Length; p++)
            {
                int index = this.Parameters[p];
                EffectParameter param = parameters[index];

                if (param.StateKey < this.StateKey)
                    continue;

                int offset = this.Offsets[p];
                this.Dirty = true;

                this.SetParameter(param, offset);
            }

            this.StateKey = EffectParameter.NextStateKey;
        }

        private int SetParameter(EffectParameter param, int offset)
        {
            const int elementSize = 4;
            const int rowSize = elementSize * 4;

            EffectParameterCollection elements = param.Elements;
            if (elements.Count <= 0)
            {
                if (param.Data != null)
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
            else // (elements.Count > 0)
            {
                int elementByteCount = 0;
                for (int i = 0; i < elements.Count; i++)
                {
                    int subElementByteCount = SetParameter(elements[i], offset + elementByteCount);
                    elementByteCount += subElementByteCount;
                }
                return elementByteCount;
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
                    Buffer.BlockCopy(source, 0, this.BufferData, offset, elementSize);
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
                Buffer.BlockCopy(source, 0, this.BufferData, offset, rows * stride);
            }
            // Take care of Matrix3x3 and Matrix4x3. (unroll loop)
            else if (rows == 3 && (columns == 3 || columns == 4))
            {
                Array source = data as Array;
                int stride = (columns * elementSize);
                Buffer.BlockCopy(source, stride * 0, this.BufferData, offset + (rowSize * 0), stride);
                Buffer.BlockCopy(source, stride * 1, this.BufferData, offset + (rowSize * 1), stride);
                Buffer.BlockCopy(source, stride * 2, this.BufferData, offset + (rowSize * 2), stride);
            }
            else
            {
                Array source = data as Array;
                int stride = (columns * elementSize);
                for (int y = 0; y < rows; y++)
                {
                    Buffer.BlockCopy(source, stride * y, this.BufferData, offset + (rowSize * y), stride);
                }
            }
        }


        public abstract object Clone();
        internal abstract void PlatformContextLost();


        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _name = null;
                _parameters = null;
                _offsets = null;
                _bufferData = null;
                _dirty = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
