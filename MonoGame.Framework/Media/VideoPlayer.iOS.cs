// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform;
using MediaPlayer;
using Foundation;
using UIKit;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class VideoPlayer : IDisposable
    {
        private NSObject _playbackDidFinishObserver;

        private void PlatformInitialize()
        {

        }

        private Texture2D PlatformGetTexture()
        {
            throw new NotImplementedException();
        }

        private void PlatformGetState(ref MediaState result)
        {
        }

        private void PlatformPause()
        {
            throw new NotImplementedException();
        }

        private void PlatformResume()
        {
            _currentVideo.MovieView.MoviePlayer.Play();
        }

        private void PlatformPlay()
        {
            ConcreteGame concreteGame = (ConcreteGame)Game.Instance.Strategy;
            if (concreteGame == null)
                throw new InvalidOperationException("No iOS GameStrategy instance was available");

            concreteGame.IsPlayingVideo = true;

            _playbackDidFinishObserver = NSNotificationCenter.DefaultCenter.AddObserver(
                MPMoviePlayerController.PlaybackDidFinishNotification, OnStop);

            _currentVideo.MovieView.MoviePlayer.RepeatMode = IsLooped ? MPMovieRepeatMode.One : MPMovieRepeatMode.None;

            concreteGame.ViewController.PresentViewController(_currentVideo.MovieView, false, null);
            _currentVideo.MovieView.MoviePlayer.Play();
        }

        private void PlatformStop()
        {
            ConcreteGame concreteGame = (ConcreteGame)Game.Instance.Strategy;
            if (concreteGame == null)
                throw new InvalidOperationException("No iOS GameStrategy instance was available");

            if (_playbackDidFinishObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(_playbackDidFinishObserver);
                _playbackDidFinishObserver = null;
            }

            _currentVideo.MovieView.MoviePlayer.Stop();
            concreteGame.IsPlayingVideo = false;
            concreteGame.ViewController.DismissViewController(false, null);
        }

        private void OnStop(NSNotification e)
        {
            Stop();
        }

        private TimeSpan PlatformGetPlayPosition()
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

        private TimeSpan PlatformSetVolume()
        {
            throw new NotImplementedException();
        }

        private void PlatformDispose(bool disposing)
        {
        }
    }
}
