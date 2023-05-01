// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class VideoPlayer : IDisposable
    {

        private void PlatformInitialize()
        {
            throw new NotImplementedException();
        }

        private Texture2D PlatformGetTexture()
        {
            throw new NotImplementedException();
        }

        private MediaState PlatformUpdateState(MediaState currentState)
        {
            throw new NotImplementedException();
        }

        private void PlatformPause()
        {
            throw new NotImplementedException();
        }

        private void PlatformResume()
        {
            throw new NotImplementedException();
        }

        private void PlatformPlay(Video video)
        {
            throw new NotImplementedException();
        }

        private void PlatformStop()
        {
            throw new NotImplementedException();
        }

        private void PlatformSetIsLooped()
        {
            throw new NotImplementedException();
        }

        private void PlatformSetIsMuted()
        {
            throw new NotImplementedException();
        }

        private TimeSpan PlatformGetPlayPosition()
        {
            throw new NotImplementedException();
        }

        private TimeSpan PlatformSetVolume()
        {
            throw new NotImplementedException();
        }

        private void PlatformDispose(bool disposing)
        {
            if (disposing)
            {
            }

        }
    }
}
