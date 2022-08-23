// Copyright (C)2022 Nick Kastellanos

using System;


namespace Microsoft.Xna.Framework.Graphics
{
    internal partial class ConstantBuffer
    {
        private void PlatformInitialize()
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformClear()
        {
            throw new PlatformNotSupportedException();
        }

        internal unsafe void PlatformApply(ShaderStage stage, int slot)
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformDispose(bool disposing)
        {
            if (disposing)
            {
            }

        }
    }
}
