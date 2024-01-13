// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using MediaPlayer;
using Foundation;
using UIKit;

namespace Microsoft.Xna.Platform.Media
{
    internal sealed class ConcreteVideoPlayerStrategy : VideoPlayerStrategy
    {
        private NSObject _playbackDidFinishObserver;

        public override MediaState State
        {
            get { return base.State; }
            protected set { base.State = value; }
        }

        public override bool IsLooped
        {
            get { return base.IsLooped; }
            set { base.IsLooped = value; }
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

        public override TimeSpan PlayPosition
        {
            get { throw new NotImplementedException(); }
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

        internal ConcreteVideoPlayerStrategy()
        {

        }

        public override Texture2D PlatformGetTexture()
        {
            throw new NotImplementedException();
        }

        protected override void PlatformUpdateState(ref MediaState state)
        {
        }

        public override void PlatformPlay(Video video)
        {
            base.Video = video;

            ConcreteGame concreteGame = ConcreteGame.ConcreteGameInstance;
            if (concreteGame == null)
                throw new InvalidOperationException("No iOS GameStrategy instance was available");

            _playbackDidFinishObserver = NSNotificationCenter.DefaultCenter.AddObserver(
                MPMoviePlayerController.PlaybackDidFinishNotification, OnStop);

            VideoPlatformStream _videoPlatformStream = ((IPlatformVideo)base.Video).Strategy.ToConcrete<ConcreteVideoStrategy>().GetVideoPlatformStream();
            _videoPlatformStream.MovieView.MoviePlayer.RepeatMode = IsLooped ? MPMovieRepeatMode.One : MPMovieRepeatMode.None;

            concreteGame.ViewController.PresentViewController(_videoPlatformStream.MovieView, false, null);
            _videoPlatformStream.MovieView.MoviePlayer.Play();

            State = MediaState.Playing;
        }
        
        public override void PlatformPause()
        {
            throw new NotImplementedException();
        }

        public override void PlatformResume()
        {
            VideoPlatformStream _videoPlatformStream = ((IPlatformVideo)base.Video).Strategy.ToConcrete<ConcreteVideoStrategy>().GetVideoPlatformStream();
            _videoPlatformStream.MovieView.MoviePlayer.Play();
            State = MediaState.Playing;
        }

        public override void PlatformStop()
        {
            ConcreteGame concreteGame = ConcreteGame.ConcreteGameInstance;
            if (concreteGame == null)
                throw new InvalidOperationException("No iOS GameStrategy instance was available");

            if (_playbackDidFinishObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(_playbackDidFinishObserver);
                _playbackDidFinishObserver = null;
            }

            VideoPlatformStream _videoPlatformStream = ((IPlatformVideo)base.Video).Strategy.ToConcrete<ConcreteVideoStrategy>().GetVideoPlatformStream();
            _videoPlatformStream.MovieView.MoviePlayer.Stop();
            concreteGame.ViewController.DismissViewController(false, null);

            State = MediaState.Stopped;
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


    internal sealed class VideoPlatformStream : IDisposable
    {
        private MPMoviePlayerViewController _movieView;

        internal MPMoviePlayerViewController MovieView { get { return _movieView; } }


        internal VideoPlatformStream(string filename)
        {
            NSUrl url = NSUrl.FromFilename(Path.GetFullPath(filename));
            _movieView = CreateMovieView(url);
        }

        private MPMoviePlayerViewController CreateMovieView(NSUrl url)
        {
            var movieView = new MPMoviePlayerViewController(url);
            MovieView.MoviePlayer.ScalingMode = MPMovieScalingMode.AspectFill;
            MovieView.MoviePlayer.ControlStyle = MPMovieControlStyle.None;
            MovieView.MoviePlayer.PrepareToPlay();
            return movieView;
        }


        #region IDisposable
        ~VideoPlatformStream()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_movieView != null)
                {
                    _movieView.Dispose();
                    _movieView = null;
                }

            }

            //base.Dispose(disposing);
        }
        #endregion
    }
}
