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

        protected ConstantBufferStrategy(GraphicsDevice graphicsDevice)
        {
            this.GraphicsDevice = graphicsDevice;
        }

        protected ConstantBufferStrategy(ConstantBufferStrategy source)
        {
            this.GraphicsDevice = source.GraphicsDevice;
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
            }

        }
        #endregion
    }
}
