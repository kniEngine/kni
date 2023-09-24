// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{

    internal sealed class ConcreteConstantBuffer : ConstantBufferStrategy
    {
        public ConcreteConstantBuffer(GraphicsContextStrategy contextStrategy, string name, int[] parameters, int[] offsets, int sizeInBytes, ShaderProfileType profile)
            : base(contextStrategy, name, parameters, offsets, sizeInBytes, profile)
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

        internal unsafe override void PlatformApply(GraphicsContextStrategy contextStrategy, int slot, ShaderStage stage)
        {
            throw new PlatformNotSupportedException();
        }

        internal override void PlatformDeviceResetting()
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
