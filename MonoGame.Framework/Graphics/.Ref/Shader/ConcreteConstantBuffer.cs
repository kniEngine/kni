// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{

    internal sealed class ConcreteConstantBuffer : ConstantBufferStrategy
    {
        public ConcreteConstantBuffer(GraphicsContextStrategy contextStrategy, string name, int[] parameters, int[] offsets, int sizeInBytes)
            : base(contextStrategy.Context.DeviceStrategy.Device, name, parameters, offsets, sizeInBytes)
        {
            throw new PlatformNotSupportedException();
        }

        private ConcreteConstantBuffer(ConcreteConstantBuffer source)
            : base(source)
        {
            throw new PlatformNotSupportedException();
        }

        public override object Clone()
        {
            throw new PlatformNotSupportedException();
        }

        internal override void PlatformClear()
        {
            throw new PlatformNotSupportedException();
        }

        internal unsafe override void PlatformApply(GraphicsContextStrategy contextStrategy, ShaderStage stage, int slot)
        {
            throw new PlatformNotSupportedException();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            base.Dispose(disposing);
        }
    }
}
