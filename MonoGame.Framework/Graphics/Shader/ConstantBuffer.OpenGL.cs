// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;


namespace Microsoft.Xna.Framework.Graphics
{
    internal partial class ConstantBuffer
    {
        private void PlatformInitialize()
        {
        }

        private void PlatformClear()
        {
        }

        internal unsafe void PlatformApply(ShaderStage stage, int slot)
        {
        }

        private void PlatformDispose(bool disposing)
        {
            if (disposing)
            {
            }

        }
    }
}
