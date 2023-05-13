// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Framework.Media;
using Foundation;
using AVFoundation;
using MediaPlayer;
using CoreMedia;


namespace Microsoft.Xna.Platform.Media
{
    public sealed class ConcreteSongStrategy : SongStrategy
    {
        private Uri _streamSource;

        #if !TVOS
        internal MPMediaItem _mediaItem;
        #endif
        internal NSUrl _assetUrl;

        private AVPlayerItem _sound;
        private AVPlayer _player; // TODO: Move _player to MediaPlayer
        private NSObject _playToEndObserver;

        internal Uri StreamSource { get { return _streamSource; } }

        [CLSCompliant(false)]
        public NSUrl AssetUrl
        {
            get { return this._assetUrl; }
        }

        public ConcreteSongStrategy()
        {
        }

        public ConcreteSongStrategy(string name, Uri streamSource)
        {
            this.Name = name;
            this._streamSource = streamSource;
            this.PlatformInitialize(streamSource);
        }

        private void PlatformInitialize(Uri streamSource)
        {
            NSUrl nsUrl = NSUrl.FromFilename(streamSource.OriginalString);
            this.PlatformInitialize(nsUrl);
        }

        private void PlatformLazyInitialize(NSUrl url)
        {
            this.PlatformInitialize(url);
        }

        private void PlatformInitialize(NSUrl url)
        {
            _sound = AVPlayerItem.FromUrl(url);
            _player = AVPlayer.FromPlayerItem(_sound);
            _playToEndObserver = AVPlayerItem.Notifications.ObserveDidPlayToEndTime(OnFinishedPlaying);
        }

        private void OnFinishedPlaying(object sender, NSNotificationEventArgs args)
		{
            var handler = DonePlaying;
            if (handler != null)
                handler(this, EventArgs.Empty);
		}

        internal delegate void FinishedPlayingHandler(object sender, EventArgs args);
        event FinishedPlayingHandler DonePlaying;

		/// <summary>
		/// Set the event handler for "Finished Playing". Done this way to prevent multiple bindings.
		/// </summary>
		internal void SetEventHandler(FinishedPlayingHandler handler)
		{
			if (DonePlaying == null)
			    DonePlaying += handler;
		}

        internal void Play()
        {
            if (_player == null)
            {
                // MediaLibrary items are lazy loaded
                if (_assetUrl != null)
                    this.PlatformLazyInitialize(_assetUrl);
                else
                    return;
            }

            _player.Seek(CMTime.Zero); // Seek to start to ensure playback at the start.
            _player.Play();

            PlayCount++;
        }
		
		internal void Pause()
		{			            
            if (_player == null)
				return;
			
            _player.Pause();
        }

		internal void Resume()
		{
            if (_player == null)
				return;

            _player.Play();
		}
		
		internal void Stop()
		{
            if (_player == null)
				return;
			
            _player.Pause();
            PlayCount = 0;
		}

		internal float Volume
		{
			get
			{
                if (_player != null)
                    return _player.Volume;
				else
					return 0.0f;
			}
			
			set
			{
                if ( _player != null && _player.Volume != value )
                    _player.Volume = value;
			}			
		}

		internal TimeSpan Position
        {
            get
            {
                return TimeSpan.FromSeconds(_player.CurrentTime.Seconds);		
            }
            set
            {
                _player.Seek(CMTime.FromSeconds(value.TotalSeconds, 1000));
            }
        }

        public override Album Album
        {
            get { return base.Album; }
        }

        public override Artist Artist
        {
            get { return base.Artist; }
        }

        public override Genre Genre
        {
            get { return base.Genre; }
        }

        public override TimeSpan Duration
        {
            get { return base.Duration; }
        }

        public override bool IsProtected
        {
            get { return base.IsProtected; }
        }

        public override bool IsRated
        {
            get { return base.IsRated; }
        }

        internal override string Filename
        {
            get
            {
                if (this.StreamSource == null)
                    return this.Name;

                return StreamSource.OriginalString;
            }
        }

        public override string Name
        {
            get { return base.Name; }
        }

        public override int PlayCount
        {
            get { return base.PlayCount; }
        }

        public override int Rating
        {
            get { return base.Rating; }
        }

        public override int TrackNumber
        {
            get { return base.TrackNumber; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_sound != null)
                {
                    _playToEndObserver.Dispose();
                    _sound.Dispose();
                    _player.Dispose();
                }

                _playToEndObserver = null;
                _sound = null;
                _player = null;
            }

            //base.Dispose(disposing);
        }
    }
}

