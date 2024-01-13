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

        internal override bool PlatformIsMuted
        {
            set
            {
                base.PlatformIsMuted = value;

                if (Queue.Count > 0)
                    SetChannelVolumes();
            }
        }

        internal override TimeSpan PlatformPlayPosition
        {
            get
            {
                Song activeSong = Queue.ActiveSong;
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

        internal override float PlatformVolume
        {
            set
            {
                base.PlatformVolume = value;

                if (Queue.ActiveSong != null)
                    SetChannelVolumes();
            }
        }

        internal override bool PlatformGameHasControl
        {
            get { return true; }
        }

        #endregion

        private void SetChannelVolumes()
        {
            float innerVolume = base.PlatformIsMuted ? 0.0f : base.PlatformVolume;

            foreach (Song queuedSong in Queue.Songs)
            {
                _androidPlayer.SetVolume(innerVolume, innerVolume);
            }
        }
        
        internal override void PlatformPlaySong(Song song)
        {
            if (Queue.ActiveSong != null)
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
                    var afd = Android.App.Application.Context.Assets.OpenFd(
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

        internal override void PlatformPause()
        {
            Song activeSong = Queue.ActiveSong;
            if (activeSong != null)
            {
                _androidPlayer.Pause();
            }
        }

        internal override void PlatformResume()
        {
            Song activeSong = Queue.ActiveSong;
            if (activeSong != null)
            {
                _androidPlayer.Start();
            }
        }

        internal override void PlatformStop()
        {
            foreach (Song queuedSong in Queue.Songs)
            {
                Song activeSong = Queue.ActiveSong;
                _androidPlayer.Stop();
                _playingSong = null;
                ((IPlatformSong)activeSong).Strategy.PlayCount = 0;
                ((IPlatformSong)activeSong).Strategy.ToConcrete<ConcreteSongStrategy>()._position = TimeSpan.Zero;


            }
        }

        internal override void PlatformClearQueue()
        {
            while (Queue.Count > 0)
            {
                Song song = Queue[0];

                _androidPlayer.Stop();
                _playingSong = null;
                ((IPlatformSong)song).Strategy.PlayCount = 0;
                ((IPlatformSong)song).Strategy.ToConcrete<ConcreteSongStrategy>()._position = TimeSpan.Zero;
                
                Queue.Remove(song);
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
