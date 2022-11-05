// Copyright (C)2022 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public sealed partial class SamplerStateCollection
    {

        private void PlatformSetSamplerState(int index)
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformClear()
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformDirty()
        {
            throw new PlatformNotSupportedException();
        }

        internal void PlatformApply()
        {
            throw new PlatformNotSupportedException();
        }

	}
}
