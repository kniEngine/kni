// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Foundation;
using AVFoundation;
using MediaPlayer;
using CoreMedia;
using Microsoft.Xna.Platform.Media;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Song : SongStrategy
    {
        private Album _album;
        private Artist _artist;
        private Genre _genre;
        private string _title;
        private TimeSpan duration;
        #if !TVOS
        private MPMediaItem _mediaItem;
        #endif
        private AVPlayerItem _sound;
        private AVPlayer _player; // TODO: Move _player to MediaPlayer
        private NSUrl _assetUrl;
        private NSObject _playToEndObserver;

        [CLSCompliant(false)]
        public NSUrl AssetUrl
        {
            get { return this._assetUrl; }
        }

        #if !TVOS
        internal Song(Album album, Artist artist, Genre genre, string title, TimeSpan duration, MPMediaItem mediaItem, NSUrl assetUrl)
        #else
        internal Song(Album album, Artist artist, Genre genre, string title, TimeSpan duration, object mediaItem, NSUrl assetUrl)
        #endif
        {
            _strategy = this;
            this._album = album;
            this._artist = artist;
            this._genre = genre;
            this._title = title;
            this.duration = duration;
            #if !TVOS
            this._mediaItem = mediaItem;
            #endif
            this._assetUrl = assetUrl;
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

        internal override void PlatformDispose(bool disposing)
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
        }

        internal void OnFinishedPlaying (object sender, NSNotificationEventArgs args)
		{
			if (DonePlaying != null)
			    DonePlaying(sender, args);
		}

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

            PlatformPlay();

            _playCount++;
        }

        private void PlatformPlay()
        {
            
            _player.Seek(CMTime.Zero); // Seek to start to ensure playback at the start.
            
            _player.Play();
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

            PlatformResume();
		}

        private void PlatformResume()
        {
			_player.Play();
        }
		
		internal void Stop()
		{
            if (_player == null)
				return;
			
            _player.Pause();
			_playCount = 0;
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

        internal override Album PlatformGetAlbum()
        {
            return this._album;
        }

        internal override void PlatformSetAlbum(Album album)
        {
            this._album = album;
        }

        internal override Artist PlatformGetArtist()
        {
            return this._artist;
        }

        internal override Genre PlatformGetGenre()
        {
            return this._genre;
        }

        internal override TimeSpan PlatformGetDuration()
        {
            #if !TVOS
            if (this._mediaItem != null)
                return this.duration;
            #endif
            return _duration;
        }

        internal override bool PlatformIsProtected()
        {
            return false;
        }

        internal override bool PlatformIsRated()
        {
            return false;
        }

        internal override string PlatformGetName()
        {
            return this._title ?? Path.GetFileNameWithoutExtension(_name);
        }

        internal override int PlatformGetPlayCount()
        {
            return _playCount;
        }

        internal override int PlatformGetRating()
        {
            return 0;
        }

        internal override int PlatformGetTrackNumber()
        {
            return 0;
        }
    }
}

