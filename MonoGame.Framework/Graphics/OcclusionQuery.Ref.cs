// Copyright (C)2022 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    partial class OcclusionQuery
    {

        private void PlatformConstruct()
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformBegin()
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformEnd()
        {
            throw new PlatformNotSupportedException();
        }

        private bool PlatformGetResult(out int pixelCount)
        {
            throw new PlatformNotSupportedException();
        }

    }
}

