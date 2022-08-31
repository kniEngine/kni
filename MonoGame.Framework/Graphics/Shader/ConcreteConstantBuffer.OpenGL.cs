// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;


namespace Microsoft.Xna.Framework.Graphics
{

    internal sealed class ConcreteConstantBufferStrategy : ConstantBufferStrategy
    {
        public ConcreteConstantBufferStrategy(GraphicsDevice graphicsDevice, string name, int[] parameters, int[] offsets, int sizeInBytes)
            : base(graphicsDevice, name, parameters, offsets, sizeInBytes)
        {
        }

        private ConcreteConstantBufferStrategy(ConcreteConstantBufferStrategy source)
            : base(source)
        {
        }

        public override object Clone()
        {
            return new ConcreteConstantBufferStrategy(this);
        }

        internal override void PlatformInitialize()
        {
        }

        internal override void PlatformClear()
        {
        }

        internal unsafe override void PlatformApply(ShaderStage stage, int slot)
        {
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
