// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;


namespace Microsoft.Xna.Framework.Graphics
{

    internal sealed class ConcreteConstantBufferStrategy : ConstantBufferStrategy
    {
        public ConcreteConstantBufferStrategy(GraphicsDevice graphicsDevice)
            : base(graphicsDevice)
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
