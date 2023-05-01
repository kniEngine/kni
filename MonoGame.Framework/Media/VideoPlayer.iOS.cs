// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform;
using Microsoft.Xna.Platform.Media;
using MediaPlayer;
using Foundation;
using UIKit;


namespace Microsoft.Xna.Framework.Media
{
    public sealed class ConcreteVideoPlayerStrategy : VideoPlayerStrategy
    {
        private NSObject _playbackDidFinishObserver;

        public override MediaState State
        {
            get { return base.State; }
            protected set { base.State = value; }
        }

        public override bool IsMuted
        {
            get { return base.IsMuted; }
            set
            {
                base.IsMuted = value;
                throw new NotImplementedException();
            }
        }

        public override bool IsLooped
        {
            get { return base.IsLooped; }
            set { base.IsLooped = value; }
        }

        public override float Volume
        {
            get { return base.Volume; }
            set
            {
                base.Volume = value;
                if (base.Video != null)
                    PlatformSetVolume();
            }
        }

        public ConcreteVideoPlayerStrategy()
        {

        }

        public override Texture2D PlatformGetTexture()
        {
            throw new NotImplementedException();
        }

        protected override void PlatformUpdateState(ref MediaState state)
        {
        }

        public override void PlatformPause()
        {
            throw new NotImplementedException();
        }

        public override void PlatformResume()
        {
            base.Video.MovieView.MoviePlayer.Play();
            State = MediaState.Playing;
        }

        public override void PlatformPlay(Video video)
        {
            base.Video = video;

            ConcreteGame concreteGame = (ConcreteGame)Game.Instance.Strategy;
            if (concreteGame == null)
                throw new InvalidOperationException("No iOS GameStrategy instance was available");

            concreteGame.IsPlayingVideo = true;

            _playbackDidFinishObserver = NSNotificationCenter.DefaultCenter.AddObserver(
                MPMoviePlayerController.PlaybackDidFinishNotification, OnStop);

            base.Video.MovieView.MoviePlayer.RepeatMode = IsLooped ? MPMovieRepeatMode.One : MPMovieRepeatMode.None;

            concreteGame.ViewController.PresentViewController(base.Video.MovieView, false, null);
            base.Video.MovieView.MoviePlayer.Play();

            State = MediaState.Playing;
        }

        public override void PlatformStop()
        {
            ConcreteGame concreteGame = (ConcreteGame)Game.Instance.Strategy;
            if (concreteGame == null)
                throw new InvalidOperationException("No iOS GameStrategy instance was available");

            if (_playbackDidFinishObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(_playbackDidFinishObserver);
                _playbackDidFinishObserver = null;
            }

            base.Video.MovieView.MoviePlayer.Stop();
            concreteGame.IsPlayingVideo = false;
            concreteGame.ViewController.DismissViewController(false, null);

            State = MediaState.Stopped;
        }

        public override TimeSpan PlatformGetPlayPosition()
        {
            throw new NotImplementedException();
        }

        private void PlatformSetVolume()
        {
            throw new NotImplementedException();
        }

        private void OnStop(NSNotification e)
        {
            if (base.Video != null)
                PlatformStop();
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
