// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class VideoPlayer : IDisposable
    {

        private void PlatformInitialize()
        {
            throw new PlatformNotSupportedException();
        }

        private Texture2D PlatformGetTexture()
        {
            throw new PlatformNotSupportedException();
        }

        private MediaState PlatformUpdateState(MediaState currentState)
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformPause()
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformResume()
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformPlay(Video video)
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformStop()
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformSetIsLooped()
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformSetIsMuted()
        {
            throw new PlatformNotSupportedException();
        }

        private TimeSpan PlatformGetPlayPosition()
        {
            throw new PlatformNotSupportedException();
        }

        private TimeSpan PlatformSetVolume()
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
