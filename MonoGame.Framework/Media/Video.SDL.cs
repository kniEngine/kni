// Copyright (C)2022 Nick Kastellanos

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Media
{
    /// <summary>
    /// Represents a video.
    /// </summary>
    public sealed partial class Video : IDisposable
    {
        private void PlatformInitialize()
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
