// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;


namespace Microsoft.Xna.Platform.Media
{
    internal sealed class ConcreteMediaPlayerStrategy : MediaPlayerStrategy
    {
        private Android.Media.MediaPlayer _androidPlayer;
        private Song _playingSong;

        internal ConcreteMediaPlayerStrategy()
        {
            this._androidPlayer = new Android.Media.MediaPlayer();
            this._androidPlayer.Completion += AndroidPlayer_Completion;
        }

        #region Properties

        public override bool PlatformIsMuted
        {
            set
            {
                base.PlatformIsMuted = value;

                if (base.Queue.Count > 0)
                    SetChannelVolumes();
            }
        }

        public override TimeSpan PlatformPlayPosition
        {
            get
            {
                Song activeSong = base.Queue.ActiveSong;
                if (activeSong == null)
                    return TimeSpan.Zero;

                if (_playingSong == activeSong && _androidPlayer.IsPlaying)
                    ((IPlatformSong)activeSong).Strategy.ToConcrete<ConcreteSongStrategy>()._position = TimeSpan.FromMilliseconds(_androidPlayer.CurrentPosition);

                return ((IPlatformSong)activeSong).Strategy.ToConcrete<ConcreteSongStrategy>()._position;
            }
        }

        protected override bool PlatformUpdateState(ref MediaState state)
        {
            return false;
        }

        public override float PlatformVolume
        {
            set
            {
                base.PlatformVolume = value;

                if (base.Queue.ActiveSong != null)
                    SetChannelVolumes();
            }
        }

        public override bool PlatformGameHasControl
        {
            get { return true; }
        }

        #endregion

        private void SetChannelVolumes()
        {
            float innerVolume = base.PlatformIsMuted ? 0.0f : base.PlatformVolume;

            for (int i = 0; i < base.Queue.Count; i++)
            {
                Song queuedSong = base.Queue[i];

                _androidPlayer.SetVolume(innerVolume, innerVolume);
            }
        }
        
        public override void PlatformPlaySong(Song song)
        {
            if (base.Queue.ActiveSong != null)
            {
                float innerVolume = base.PlatformIsMuted ? 0.0f : base.PlatformVolume;

                _androidPlayer.SetVolume(innerVolume, innerVolume);

                // Prepare the player
                _androidPlayer.Reset();

                if (((IPlatformSong)song).Strategy.ToConcrete<ConcreteSongStrategy>().AssetUri != null)
                {
                    _androidPlayer.SetDataSource(Android.App.Application.Context,
                                                 ((IPlatformSong)song).Strategy.ToConcrete<ConcreteSongStrategy>().AssetUri);
                }
                else
                {
                    Android.Content.Res.AssetFileDescriptor afd = Android.App.Application.Context.Assets.OpenFd(
                        ((IPlatformSong)song).Strategy.ToConcrete<ConcreteSongStrategy>().StreamSource.OriginalString);
                    if (afd == null)
                        return;

                    _androidPlayer.SetDataSource(afd.FileDescriptor, afd.StartOffset, afd.Length);
                    afd.Close();
                }

                _androidPlayer.Prepare();
                _androidPlayer.Looping = MediaPlayer.IsRepeating;
                _playingSong = song;

                _androidPlayer.Start();
                ((IPlatformSong)song).Strategy.PlayCount++;
            }
        }

        public override void PlatformPause()
        {
            Song activeSong = base.Queue.ActiveSong;
            if (activeSong != null)
            {
                _androidPlayer.Pause();
            }
        }

        public override void PlatformResume()
        {
            Song activeSong = base.Queue.ActiveSong;
            if (activeSong != null)
            {
                _androidPlayer.Start();
            }
        }

        public override void PlatformStop()
        {
            for (int i = 0; i < base.Queue.Count; i++)
            {
                Song queuedSong = base.Queue[i];

                Song activeSong = base.Queue.ActiveSong;
                _androidPlayer.Stop();
                _playingSong = null;
                ((IPlatformSong)activeSong).Strategy.PlayCount = 0;
                ((IPlatformSong)activeSong).Strategy.ToConcrete<ConcreteSongStrategy>()._position = TimeSpan.Zero;


            }
        }

        protected internal override void PlatformClearQueue()
        {
            while (base.Queue.Count > 0)
            {
                Song song = base.Queue[0];

                _androidPlayer.Stop();
                _playingSong = null;
                ((IPlatformSong)song).Strategy.PlayCount = 0;
                ((IPlatformSong)song).Strategy.ToConcrete<ConcreteSongStrategy>()._position = TimeSpan.Zero;

                base.RemoveQueuedSong(song);
            }

            _numSongsInQueuePlayed = 0;
            //base.ClearQueue();
        }

        private void AndroidPlayer_Completion(object sender, EventArgs e)
        {
            if (_playingSong != null)
            {
                 _playingSong = null;
                base.OnSongFinishedPlaying();
            }
        }
    }
}
