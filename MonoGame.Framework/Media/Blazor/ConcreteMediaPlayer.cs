// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Media;
using WasmDom = nkast.Wasm.Dom;


namespace Microsoft.Xna.Platform.Media
{
    internal sealed class ConcreteMediaPlayerStrategy : MediaPlayerStrategy
    {
        WasmDom.Audio _webPlayer;
        private Song _playingSong;

        internal ConcreteMediaPlayerStrategy()
        {
            _webPlayer = new WasmDom.Audio();
            _webPlayer.OnEnded += Player_OnEnded;
        }

        #region Properties

        internal override bool PlatformIsMuted
        {
            get { return base.PlatformIsMuted; }
            set
            {
                base.PlatformIsMuted = value;
                _webPlayer.Muted = value;
            }
        }

        internal override bool PlatformIsRepeating
        {
            get { return base.PlatformIsRepeating; }
            set
            {
                base.PlatformIsRepeating = value;
                _webPlayer.Loop = value;
            }
        }

        internal override bool PlatformIsShuffled
        {
            get { return base.PlatformIsShuffled; }
            set { base.PlatformIsShuffled = value; }
        }

        internal override TimeSpan PlatformPlayPosition
        {
            get { throw new NotImplementedException(); }
        }

        protected override bool PlatformUpdateState(ref MediaState state)
        {
            return false;
        }

        internal override float PlatformVolume
        {
            get { return base.PlatformVolume; }
            set
            {
                base.PlatformVolume = value;
                _webPlayer.Volume = value;
            }
        }

        internal override bool PlatformGameHasControl
        {
            get { return true; }
        }

        #endregion

        internal override void PlatformPlaySong(Song song)
        {
            if (Queue.ActiveSong != null)
            {
                ConcreteSongStrategy songStrategy = (ConcreteSongStrategy)song.Strategy;

                _webPlayer.Src = songStrategy.StreamSource.OriginalString;
                _webPlayer.Load();

                _webPlayer.Volume = this.PlatformVolume;
                _webPlayer.Muted = this.PlatformIsMuted;
                _webPlayer.Loop = this.PlatformIsShuffled;
                _playingSong = song;

                _webPlayer.Play();
                song.Strategy.PlayCount++;
            }
        }

        internal override void PlatformPause()
        {
            Song activeSong = Queue.ActiveSong;
            if (activeSong != null)
            {
                _webPlayer.Pause();
            }
        }

        internal override void PlatformResume()
        {
            Song activeSong = Queue.ActiveSong;
            if (activeSong != null)
            {
                _webPlayer.Play();
            }
        }

        internal override void PlatformStop()
        {
            foreach (Song queuedSong in Queue.Songs)
            {
                Song activeSong = Queue.ActiveSong;
                _webPlayer.Pause();
                _webPlayer.Src = "";
                _playingSong = null;
                activeSong.Strategy.PlayCount = 0;
            }
        }


        private void Player_OnEnded(object sender, EventArgs e)
        {
            if (_playingSong != null)
            {
                _playingSong = null;
                base.OnSongFinishedPlaying();
            }
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _webPlayer.OnEnded -= Player_OnEnded;
            }

            base.Dispose(disposing);
        }
    }
}
