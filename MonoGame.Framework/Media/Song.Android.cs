// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Platform.Media;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Song : SongStrategy
    {
        static Android.Media.MediaPlayer _androidPlayer;
        static Song _playingSong;

        private Album _album;
        private Artist _artist;
        private Genre _genre;
        private string _name2;
        private TimeSpan _duration2;
        private TimeSpan _position;
        private Android.Net.Uri _assetUri;

        [CLSCompliant(false)]
        public Android.Net.Uri AssetUri
        {
            get { return this._assetUri; }
        }

        static Song()
        {
            // TODO: Move _androidPlayer to MediaPlayer
            _androidPlayer = new Android.Media.MediaPlayer();
            _androidPlayer.Completion += AndroidPlayer_Completion;
        }

        internal Song(Album album, Artist artist, Genre genre, string name, TimeSpan duration, Android.Net.Uri assetUri)
        {
            _strategy = this;
            this._album = album;
            this._artist = artist;
            this._genre = genre;
            this._name2 = name;
            this._duration2 = duration;
            this._assetUri = assetUri;
        }

        internal override void PlatformInitialize(string fileName)
        {
            // Nothing to do here
        }

        static void AndroidPlayer_Completion(object sender, EventArgs e)
        {
            var playingSong = _playingSong;
            _playingSong = null;

            if (playingSong != null && playingSong.DonePlaying != null)
                playingSong.DonePlaying(sender, e);
        }

        /// <summary>
        /// Set the event handler for "Finished Playing". Done this way to prevent multiple bindings.
        /// </summary>
        internal void SetEventHandler(FinishedPlayingHandler handler)
        {
            if (DonePlaying == null)
                DonePlaying += handler;
        }

        internal override void PlatformDispose(bool disposing)
        {
            // Appears to be a noOp on Android

            if (disposing)
            {
            }
        }

        internal void Play()
        {
            // Prepare the player
            _androidPlayer.Reset();

            if (_assetUri != null)
            {
                _androidPlayer.SetDataSource(MediaLibrary.Context, this._assetUri);
            }
            else
            {
                var afd = Game.Activity.Assets.OpenFd(_name);
                if (afd == null)
                    return;

                _androidPlayer.SetDataSource(afd.FileDescriptor, afd.StartOffset, afd.Length);
            }


            _androidPlayer.Prepare();
            _androidPlayer.Looping = MediaPlayer.IsRepeating;
            _playingSong = this;

            _androidPlayer.Start();
            _playCount++;
        }

        internal void Resume()
        {
            _androidPlayer.Start();
        }

        internal void Pause()
        {
            _androidPlayer.Pause();
        }

        internal void Stop()
        {
            _androidPlayer.Stop();
            _playingSong = null;
            _playCount = 0;
            _position = TimeSpan.Zero;
        }

        internal float Volume
        {
            get
            {
                return 0.0f;
            }

            set
            {
                _androidPlayer.SetVolume(value, value);
            }
        }

        public TimeSpan Position
        {
            get
            {
                if (_playingSong == this && _androidPlayer.IsPlaying)
                    _position = TimeSpan.FromMilliseconds(_androidPlayer.CurrentPosition);

                return _position;
            }
            set
            {
                _androidPlayer.SeekTo((int)value.TotalMilliseconds);   
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
            return this._assetUri != null ? this._duration2 : _duration;
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
            return this._assetUri != null ? this._name2 : Path.GetFileNameWithoutExtension(_name);
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

