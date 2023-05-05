// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Platform.Media;


namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Song
    {
        internal Song(Album album, Artist artist, Genre genre, string name, TimeSpan duration, Android.Net.Uri assetUri)
        {
            _strategy = new ConcreteSongStrategy();

            _strategy.Album = album;
            _strategy.Artist = artist;
            _strategy.Genre = genre;
            ((ConcreteSongStrategy)_strategy)._name2 = name;
            ((ConcreteSongStrategy)_strategy)._duration2 = duration;
            ((ConcreteSongStrategy)_strategy)._assetUri = assetUri;
        }
    }

    public sealed class ConcreteSongStrategy : SongStrategy
    {
        static Android.Media.MediaPlayer _androidPlayer;
        static ConcreteSongStrategy _playingSong;

        internal string _name2;
        internal TimeSpan _duration2;

        internal Android.Net.Uri _assetUri;
        private TimeSpan _position;

        [CLSCompliant(false)]
        public Android.Net.Uri AssetUri
        {
            get { return this._assetUri; }
        }

        static ConcreteSongStrategy()
        {
            // TODO: Move _androidPlayer to MediaPlayer
            _androidPlayer = new Android.Media.MediaPlayer();
            _androidPlayer.Completion += AndroidPlayer_Completion;
        }


        internal override void PlatformInitialize(string fileName)
        {
        }

        static void AndroidPlayer_Completion(object sender, EventArgs e)
        {
            var playingSong = _playingSong;
            _playingSong = null;

            if (playingSong != null)
            {
                var handler = ((ConcreteSongStrategy)playingSong).DonePlaying;
                if (handler != null)
                    handler(playingSong, EventArgs.Empty);
            }
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
            // Prepare the player
            _androidPlayer.Reset();

            if (_assetUri != null)
            {
                _androidPlayer.SetDataSource(MediaLibrary.Context, this._assetUri);
            }
            else
            {
                var afd = AndroidGameWindow.Activity.Assets.OpenFd(Name);
                if (afd == null)
                    return;

                _androidPlayer.SetDataSource(afd.FileDescriptor, afd.StartOffset, afd.Length);
            }


            _androidPlayer.Prepare();
            _androidPlayer.Looping = MediaPlayer.IsRepeating;
            _playingSong = this;

            _androidPlayer.Start();
            PlayCount++;
        }

        internal void Pause()
        {
            _androidPlayer.Pause();
        }

        internal void Resume()
        {
            _androidPlayer.Start();
        }

        internal void Stop()
        {
            _androidPlayer.Stop();
            _playingSong = null;
            PlayCount = 0;
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

        internal TimeSpan Position
        {
            get
            {
                if (_playingSong == this && _androidPlayer.IsPlaying)
                    _position = TimeSpan.FromMilliseconds(_androidPlayer.CurrentPosition);

                return _position;
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
                if (this._assetUri != null)
                    return this._duration2;

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
                if (this._assetUri != null)
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
            }

            //base.Dispose(disposing);
        }
    }
}

