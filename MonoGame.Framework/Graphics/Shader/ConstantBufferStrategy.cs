// Copyright (C)2022 Nick Kastellanos

using Microsoft.Xna.Framework.Graphics;
using System;

namespace Microsoft.Xna.Platform.Graphics
{
    public abstract class ConstantBufferStrategy : ICloneable, IDisposable
    {
        public GraphicsDevice GraphicsDevice { get; private set; }

        internal string _name;
        internal int[] _parameters;
        internal int[] _offsets;
        internal byte[] _buffer;
        internal bool _dirty;
        internal ulong _stateKey;

        protected ConstantBufferStrategy(GraphicsDevice graphicsDevice, string name, int[] parameters, int[] offsets, int sizeInBytes)
        {
            this.GraphicsDevice = graphicsDevice;

            this._name = name;
            this._parameters = parameters;
            this._offsets = offsets;
            this._buffer = new byte[sizeInBytes];
        }

        protected ConstantBufferStrategy(ConstantBufferStrategy source)
        {
            // shared
            this.GraphicsDevice = source.GraphicsDevice;
            this._name = source._name;
            this._parameters = source._parameters;
            this._offsets = source._offsets;

            // copies
            this._buffer = (byte[])source._buffer.Clone();
        }

        public abstract object Clone();
        internal abstract void PlatformInitialize();
        internal abstract void PlatformApply(ShaderStage stage, int slot);
        internal abstract void PlatformClear();


        #region IDisposable
        ~ConstantBufferStrategy()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {                
                GraphicsDevice = null;

                _name = null;
                _parameters = null;
                _offsets = null;
                _buffer = null;
                _dirty = true;
            }

        }
        #endregion
    }
}
