// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform;
using Android.Widget;


namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class VideoPlayer : IDisposable
    {

        private void PlatformInitialize()
        {
            
        }

        private Texture2D PlatformGetTexture()
        {
            throw new NotImplementedException();
        }

        private MediaState PlatformUpdateState(MediaState currentState)
        {
            return currentState;
        }

        private void PlatformPause()
        {
            Strategy.Video.Player.Pause();
        }

        private void PlatformResume()
        {
            Strategy.Video.Player.Start();
        }

        private void PlatformPlay(Video video)
        {
            Strategy.Video.Player.SetDisplay(((AndroidGameWindow)Game.Instance.Window).GameView.Holder);
            Strategy.Video.Player.Start();

            ConcreteGame.IsPlayingVideo = true;
        }

        private void PlatformStop()
        {
            Strategy.Video.Player.Stop();

            ConcreteGame.IsPlayingVideo = false;
            Strategy.Video.Player.SetDisplay(null);
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
        }
    }
}
