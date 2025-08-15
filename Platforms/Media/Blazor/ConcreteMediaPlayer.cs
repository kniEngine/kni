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
        bool _isFirstLoop;
        TimeSpan _prevCurrentTime;

        internal ConcreteMediaPlayerStrategy()
        {
            _webPlayer = new WasmDom.Audio();
            _webPlayer.OnEnded += WebPlayer_OnEnded;
            _webPlayer.OnPlaying += WebPlayer_OnPlaying;
            _webPlayer.OnTimeUpdate += _webPlayer_OnTimeUpdate;
        }

        #region Properties

        public override bool PlatformIsMuted
        {
            get { return base.PlatformIsMuted; }
            set
            {
                base.PlatformIsMuted = value;
                _webPlayer.Muted = value;
            }
        }

        public override bool PlatformIsRepeating
        {
            get { return base.PlatformIsRepeating; }
            set
            {
                base.PlatformIsRepeating = value;
                _webPlayer.Loop = value;
            }
        }

        public override bool PlatformIsShuffled
        {
            get { return base.PlatformIsShuffled; }
            set { base.PlatformIsShuffled = value; }
        }

        public override TimeSpan PlatformPlayPosition
        {
            get { return _webPlayer.CurrentTime; }
        }

        public override float PlatformVolume
        {
            get { return base.PlatformVolume; }
            set
            {
                base.PlatformVolume = value;
                _webPlayer.Volume = value;
            }
        }

        public override bool PlatformGameHasControl
        {
            get { return true; }
        }

        protected override bool PlatformUpdateState(ref MediaState state)
        {
            return false;
        }

        #endregion

        public override void PlatformPlaySong(Song song)
        {
            if (base.Queue.ActiveSong != null)
            {
                _webPlayer.Src = ((IPlatformSong)song).Strategy.ToConcrete<ConcreteSongStrategy>().StreamSource.OriginalString;
                _webPlayer.Load();

                _webPlayer.Volume = this.PlatformVolume;
                _webPlayer.Muted = this.PlatformIsMuted;
                _webPlayer.Loop = this.PlatformIsRepeating;
                _playingSong = song;

                _isFirstLoop = true;
                _prevCurrentTime = TimeSpan.Zero;
                _webPlayer.Play();
                ((IPlatformSong)song).Strategy.PlayCount++;
            }
        }

        public override void PlatformPause()
        {
            Song activeSong = base.Queue.ActiveSong;
            if (activeSong != null)
            {
                _webPlayer.Pause();
            }
        }

        public override void PlatformResume()
        {
            Song activeSong = base.Queue.ActiveSong;
            if (activeSong != null)
            {
                _webPlayer.Play();
            }
        }

        public override void PlatformStop()
        {
            for (int i = 0; i < base.Queue.Count; i++)
            {
                Song queuedSong = base.Queue[i];

                Song activeSong = base.Queue.ActiveSong;
                _webPlayer.Pause();
                _webPlayer.Src = "";
                _playingSong = null;
                ((IPlatformSong)activeSong).Strategy.PlayCount = 0;
            }
        }

        private void WebPlayer_OnPlaying(object sender, EventArgs e)
        {
            WasmDom.Audio webPlayer = (WasmDom.Audio)sender;

            if (_isFirstLoop)
            {
                // ignore first OnPlaying event.
                _isFirstLoop = false;
                return;
            }

            // detect looping.
            if (this.PlatformIsRepeating)
            {
                if (this.State == MediaState.Playing)
                {
                    // ignore OnPlaying event due to streaming pause.
                    if (_prevCurrentTime != TimeSpan.Zero)
                        return;

                    Song activeSong = this.Queue.ActiveSong;

                    ((IPlatformSong)_playingSong).Strategy.PlayCount++;

                    base.OnPlatformMediaStateChanged();
                    // check if user changed the state during the MediaStateChanged event.
                    if (this.State != MediaState.Playing
                    ||  this.Queue.ActiveSong != activeSong)
                    {
                        return;
                    }

                    base.OnPlatformActiveSongChanged();
                }
            }
        }

        private void WebPlayer_OnEnded(object sender, EventArgs e)
        {
            if (_playingSong != null)
            {
                _playingSong = null;
                base.OnSongFinishedPlaying();
            }
        }

        private void _webPlayer_OnTimeUpdate(object sender, EventArgs e)
        {
            WasmDom.Audio webPlayer = (WasmDom.Audio)sender;
            TimeSpan currentTime = webPlayer.CurrentTime;

            _prevCurrentTime = currentTime;
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _webPlayer.OnPlaying -= WebPlayer_OnPlaying;
                _webPlayer.OnEnded -= WebPlayer_OnEnded;
            }

            base.Dispose(disposing);
        }
    }
}
