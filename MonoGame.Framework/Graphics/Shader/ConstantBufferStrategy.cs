// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    public abstract class ConstantBufferStrategy : ICloneable, IDisposable
    {
        public GraphicsDevice GraphicsDevice { get; private set; }

        private string _name;
        private int[] _parameters;
        private int[] _offsets;
        private byte[] _buffer;
        private ShaderProfileType _profile;
        private bool _dirty;
        private ulong _stateKey;

        public string Name { get { return _name; } }
        public int[] Parameters { get { return _parameters; } }
        public int[] Offsets { get { return _offsets; } }
        public byte[] Buffer { get { return _buffer; } }
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

        protected ConstantBufferStrategy(GraphicsDevice graphicsDevice, string name, int[] parameters, int[] offsets, int sizeInBytes, ShaderProfileType profile)
        {
            this.GraphicsDevice = graphicsDevice;

            this._name = name;
            this._parameters = parameters;
            this._offsets = offsets;
            this._buffer = new byte[sizeInBytes];
            this._profile = profile;
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
            this._profile = source._profile;
        }

        public abstract object Clone();
        internal abstract void PlatformApply(GraphicsContextStrategy contextStrategy, ShaderStage stage, int slot);
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
