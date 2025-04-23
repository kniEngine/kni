// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    public interface IPlatformConstantBuffer
    {
        ConstantBufferStrategy Strategy { get; }
    }

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

        public T ToConcrete<T>() where T : ConstantBufferStrategy
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
                                return (param.ColumnCount * RowSize);
                            }
                            else
                            {
                                SetData(offset, param.RowCount, param.ColumnCount, param.Data);
                                return (param.RowCount * RowSize);
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

        // Shader registers are always 4 bytes and all the
        // incoming data objects should be 4 bytes per element.
        const int ElementSize = 4;
        const int RowSize = (ElementSize * 4);

        private void SetData(int offset, int rows, int columns, object data)
        {
            Array src = (Array)data;

            // Take care of a single element.
            if (rows == 1 && columns == 1)
            {
                Buffer.BlockCopy(src, 0, this.BufferData, offset, ElementSize);
            }
            // Take care of the single copy case!
            else if (rows == 1 || (rows == 4 && columns == 4))
            {
                int srcStride = (columns * ElementSize);
                Buffer.BlockCopy(src, 0, this.BufferData, offset, srcStride * rows);
            }
            // Take care of Matrix3x3 and Matrix4x3. (unroll loop)
            else if (rows == 3 && (columns == 3 || columns == 4))
            {
                int srcStride = (columns * ElementSize);
                Buffer.BlockCopy(src, srcStride * 0, this.BufferData, offset + (RowSize * 0), srcStride);
                Buffer.BlockCopy(src, srcStride * 1, this.BufferData, offset + (RowSize * 1), srcStride);
                Buffer.BlockCopy(src, srcStride * 2, this.BufferData, offset + (RowSize * 2), srcStride);
            }
            else
            {
                int srcStride = (columns * ElementSize);
                for (int r = 0; r < rows; r++)
                    Buffer.BlockCopy(src, srcStride * r, this.BufferData, offset + (RowSize * r), srcStride);
            }
        }


        public abstract object Clone();
        public abstract void PlatformContextLost();


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
