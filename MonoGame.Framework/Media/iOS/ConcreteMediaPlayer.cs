// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Media;
using AudioToolbox;
using AVFoundation;
using CoreMedia;
using Foundation;
using MediaPlayer;


namespace Microsoft.Xna.Platform.Media
{
    internal sealed class ConcreteMediaPlayerStrategy : MediaPlayerStrategy
    {

        internal ConcreteMediaPlayerStrategy()
        {
        }

        #region Properties

        public override bool PlatformIsMuted
        {
            set
            {
                base.PlatformIsMuted = value;

                if (Queue.Count > 0)
                    SetChannelVolumes();
            }
        }

        public override TimeSpan PlatformPlayPosition
        {
            get
            {
                Song activeSong = Queue.ActiveSong;
                if (activeSong != null)
                {
                    MediaPlatformStream mediaPlatformStream = ((IPlatformSong)activeSong).Strategy.ToConcrete<ConcreteSongStrategy>().GetMediaPlatformStream();
                    AVPlayer player = mediaPlatformStream.Player;
                    return TimeSpan.FromSeconds(player.CurrentTime.Seconds);
                }

                return TimeSpan.Zero;
            }
        }

        internal void PlatformSetPlayPosition(TimeSpan value)
        {
            Song activeSong = Queue.ActiveSong;
            if (activeSong != null)
            {
                MediaPlatformStream mediaPlatformStream = ((IPlatformSong)activeSong).Strategy.ToConcrete<ConcreteSongStrategy>().GetMediaPlatformStream();
                AVPlayer player = mediaPlatformStream.Player;
                player.Seek(CMTime.FromSeconds(value.TotalSeconds, 1000));
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

                if (Queue.ActiveSong != null)
                    SetChannelVolumes();
            }
        }

        public override bool PlatformGameHasControl
        {
            get { return !AVAudioSession.SharedInstance().OtherAudioPlaying; }
        }

        #endregion

        private void SetChannelVolumes()
        {
            float innerVolume = base.PlatformIsMuted ? 0.0f : base.PlatformVolume;
            
            foreach (Song queuedSong in Queue.Songs)
            {
                MediaPlatformStream mediaPlatformStream = ((IPlatformSong)queuedSong).Strategy.ToConcrete<ConcreteSongStrategy>().GetMediaPlatformStream();
                if (mediaPlatformStream.Player != null)
                {
                    AVPlayer player = mediaPlatformStream.Player;
                    if (player.Volume != innerVolume)
                        player.Volume = innerVolume;
                }
            }
        }

        public override void PlatformPlaySong(Song song)
        {
            if (Queue.ActiveSong != null)
            {
                MediaPlatformStream mediaPlatformStream = ((IPlatformSong)song).Strategy.ToConcrete<ConcreteSongStrategy>().GetMediaPlatformStream();
                mediaPlatformStream.SetEventHandler(OnSongFinishedPlaying);

                float innerVolume = base.PlatformIsMuted ? 0.0f : base.PlatformVolume;

                if (mediaPlatformStream.Player != null)
                {
                    AVPlayer player = mediaPlatformStream.Player;
                    if (player.Volume != innerVolume)
                        player.Volume = innerVolume;
                }

                if (mediaPlatformStream.Player == null)
                {
                    // MediaLibrary items are lazy loaded
                    if (((IPlatformSong)song).Strategy.ToConcrete<ConcreteSongStrategy>().AssetUrl != null)
                        mediaPlatformStream.CreatePlayer(((IPlatformSong)song).Strategy.ToConcrete<ConcreteSongStrategy>().AssetUrl);
                    else
                        return;
                }

                AVPlayer player2 = mediaPlatformStream.Player;
                player2.Seek(CMTime.Zero); // Seek to start to ensure playback at the start.
                player2.Play();

                ((IPlatformSong)song).Strategy.PlayCount++;
            }
        }

        public override void PlatformPause()
        {
            Song activeSong = Queue.ActiveSong;
            if (activeSong != null)
            {
                MediaPlatformStream mediaPlatformStream = ((IPlatformSong)activeSong).Strategy.ToConcrete<ConcreteSongStrategy>().GetMediaPlatformStream();
                if (mediaPlatformStream.Player != null)
                {
                    AVPlayer player = mediaPlatformStream.Player;
                    player.Pause();
                }
            }
        }

        public override void PlatformResume()
        {
            Song activeSong = Queue.ActiveSong;
            if (activeSong != null)
            {
                MediaPlatformStream mediaPlatformStream = ((IPlatformSong)activeSong).Strategy.ToConcrete<ConcreteSongStrategy>().GetMediaPlatformStream();
                if (mediaPlatformStream.Player != null)
                {
                    AVPlayer player = mediaPlatformStream.Player;
                    player.Play();
                }
            }
        }

        public override void PlatformStop()
        {
            foreach (Song queuedSong in Queue.Songs)
            {
                Song activeSong = Queue.ActiveSong;

                MediaPlatformStream mediaPlatformStream = ((IPlatformSong)activeSong).Strategy.ToConcrete<ConcreteSongStrategy>().GetMediaPlatformStream();
                if (mediaPlatformStream.Player != null)
                {
                    AVPlayer player = mediaPlatformStream.Player;
                    player.Pause();
                    ((IPlatformSong)activeSong).Strategy.PlayCount = 0;
                }
            }
        }

        internal override void PlatformClearQueue()
        {
            while (Queue.Count > 0)
            {
                Song song = Queue[0];

                MediaPlatformStream mediaPlatformStream = ((IPlatformSong)song).Strategy.ToConcrete<ConcreteSongStrategy>().GetMediaPlatformStream();
                if (mediaPlatformStream.Player != null)
                {
                    AVPlayer player = mediaPlatformStream.Player;
                    player.Pause();
                    ((IPlatformSong)song).Strategy.PlayCount = 0;
                }

                Queue.Remove(song);
            }

            _numSongsInQueuePlayed = 0;
            //base.ClearQueue();
        }

    }

    internal sealed class MediaPlatformStream : IDisposable
    {
        private AVPlayer _player; // TODO: Move _player to MediaPlayer
        private NSObject _playToEndObserver;
        private AVPlayerItem _sound;

        internal AVPlayer Player { get { return _player; } }


        internal MediaPlatformStream(Uri streamSource)
        {
            NSUrl nsUrl = NSUrl.FromFilename(streamSource.OriginalString);
            this.CreatePlayer(nsUrl);
        }

        internal void CreatePlayer(NSUrl url)
        {
            _sound = AVPlayerItem.FromUrl(url);
            _player = AVPlayer.FromPlayerItem(_sound);
            _playToEndObserver = AVPlayerItem.Notifications.ObserveDidPlayToEndTime(OnFinishedPlaying);
        }

        internal delegate void FinishedPlayingHandler();
        event FinishedPlayingHandler DonePlaying;

        private void OnFinishedPlaying(object sender, NSNotificationEventArgs args)
		{
            FinishedPlayingHandler handler = DonePlaying;
            if (handler != null)
                handler();
		}

		/// <summary>
		/// Set the event handler for "Finished Playing". Done this way to prevent multiple bindings.
		/// </summary>
		internal void SetEventHandler(FinishedPlayingHandler handler)
		{
			if (DonePlaying == null)
			    DonePlaying += handler;
		}


        #region IDisposable
        ~MediaPlatformStream()
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
        #endregion
    }
}

