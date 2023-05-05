// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.IO;
using Foundation;
using AVFoundation;
using MediaPlayer;
using CoreMedia;
using Microsoft.Xna.Platform.Media;


namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Song
    {

        #if TVOS
        internal Song(Album album, Artist artist, Genre genre, string title, TimeSpan duration, NSUrl assetUrl, object mediaItem)
        #else
        internal Song(Album album, Artist artist, Genre genre, string title, TimeSpan duration, NSUrl assetUrl, MPMediaItem mediaItem)
        #endif
        {
            _strategy = new ConcreteSongStrategy();

            _strategy.Album = album;
            _strategy.Artist = artist;
            _strategy.Genre = genre;
            ((ConcreteSongStrategy)_strategy)._name2 = title;
            ((ConcreteSongStrategy)_strategy)._duration2 = duration;

            #if TVOS
            ((ConcreteSongStrategy)_strategy)._assetUrl = assetUrl;
            #endif
            ((ConcreteSongStrategy)_strategy)._mediaItem = mediaItem;
        }
    }

    public sealed class ConcreteSongStrategy : SongStrategy
    {
        internal string _name2;
        internal TimeSpan _duration2;

        #if !TVOS
        internal MPMediaItem _mediaItem;
        #endif
        internal NSUrl _assetUrl;

        private AVPlayerItem _sound;
        private AVPlayer _player; // TODO: Move _player to MediaPlayer
        private NSObject _playToEndObserver;

        [CLSCompliant(false)]
        public NSUrl AssetUrl
        {
            get { return this._assetUrl; }
        }


        internal override void PlatformInitialize(string fileName)
        {
            this.PlatformInitialize(NSUrl.FromFilename(fileName));
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
                    this.PlatformInitialize(_assetUrl);
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
            get
            {
                #if !TVOS
                if (this._mediaItem != null)
                    return this._duration2;
                #endif

                return base.Duration;
            }
        }

        public override bool IsProtected
        {
            get { return base.IsProtected; }
        }

        public override bool IsRated
        {
            get { return base.IsRated; }
        }

        public override string Name
        {
            get
            {
                if (this._name2 != null)
                    return this._name2;

                return Path.GetFileNameWithoutExtension(base.Name);
            }
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

