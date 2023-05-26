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

        internal override void PlatformSetIsMuted(bool muted)
        {
            base.PlatformSetIsMuted(muted);

            if (Queue.Count == 0)
                return;

            SetChannelVolumes();
        }

        internal override TimeSpan PlatformGetPlayPosition()
        {
            Song activeSong = Queue.ActiveSong;
            if (activeSong != null)
            {
                MediaPlatformStream mediaPlatformStream = ((ConcreteSongStrategy)activeSong.Strategy).GetMediaPlatformStream();
                AVPlayer player = mediaPlatformStream.Player;
                return TimeSpan.FromSeconds(player.CurrentTime.Seconds);
            }

            return TimeSpan.Zero;
        }

        internal void PlatformSetPlayPosition(TimeSpan value)
        {
            Song activeSong = Queue.ActiveSong;
            if (activeSong != null)
            {
                MediaPlatformStream mediaPlatformStream = ((ConcreteSongStrategy)activeSong.Strategy).GetMediaPlatformStream();
                AVPlayer player = mediaPlatformStream.Player;
                player.Seek(CMTime.FromSeconds(value.TotalSeconds, 1000));
            }
        }

        protected override bool PlatformUpdateState(ref MediaState state)
        {
            return false;
        }

        internal override void PlatformSetVolume(float volume)
        {
            base.PlatformSetVolume(volume);

            if (Queue.ActiveSong != null)
                SetChannelVolumes();
        }

        internal override bool PlatformGetGameHasControl()
        {
            return !AVAudioSession.SharedInstance().OtherAudioPlaying;
        }

        #endregion

        private void SetChannelVolumes()
        {
            float innerVolume = base.PlatformGetIsMuted() ? 0.0f : base.PlatformGetVolume();
            
            foreach (Song queuedSong in Queue.Songs)
            {
                MediaPlatformStream mediaPlatformStream = ((ConcreteSongStrategy)queuedSong.Strategy).GetMediaPlatformStream();
                if (mediaPlatformStream.Player != null)
                {
                    AVPlayer player = mediaPlatformStream.Player;
                    if (player.Volume != innerVolume)
                        player.Volume = innerVolume;
                }
            }
        }

        internal override void PlatformPlaySong(Song song)
        {
            if (Queue.ActiveSong != null)
            {
                MediaPlatformStream mediaPlatformStream = ((ConcreteSongStrategy)song.Strategy).GetMediaPlatformStream();
                mediaPlatformStream.SetEventHandler(OnSongFinishedPlaying);

                float innerVolume = base.PlatformGetIsMuted() ? 0.0f : base.PlatformGetVolume();

                if (mediaPlatformStream.Player != null)
                {
                    AVPlayer player = mediaPlatformStream.Player;
                    if (player.Volume != innerVolume)
                        player.Volume = innerVolume;
                }

                if (mediaPlatformStream.Player == null)
                {
                    // MediaLibrary items are lazy loaded
                    if (((ConcreteSongStrategy)song.Strategy).AssetUrl != null)
                        mediaPlatformStream.CreatePlayer(((ConcreteSongStrategy)song.Strategy).AssetUrl);
                    else
                        return;
                }

                AVPlayer player2 = mediaPlatformStream.Player;
                player2.Seek(CMTime.Zero); // Seek to start to ensure playback at the start.
                player2.Play();

                song.Strategy.PlayCount++;
            }
        }

        internal override void PlatformPause()
        {
            Song activeSong = Queue.ActiveSong;
            if (activeSong != null)
            {
                MediaPlatformStream mediaPlatformStream = ((ConcreteSongStrategy)activeSong.Strategy).GetMediaPlatformStream();
                if (mediaPlatformStream.Player != null)
                {
                    AVPlayer player = mediaPlatformStream.Player;
                    player.Pause();
                }
            }
        }

        internal override void PlatformResume()
        {
            Song activeSong = Queue.ActiveSong;
            if (activeSong != null)
            {
                MediaPlatformStream mediaPlatformStream = ((ConcreteSongStrategy)activeSong.Strategy).GetMediaPlatformStream();
                if (mediaPlatformStream.Player != null)
                {
                    AVPlayer player = mediaPlatformStream.Player;
                    player.Play();
                }
            }
        }

        internal override void PlatformStop()
        {
            foreach (Song queuedSong in Queue.Songs)
            {
                var activeSong = Queue.ActiveSong;

                MediaPlatformStream mediaPlatformStream = ((ConcreteSongStrategy)activeSong.Strategy).GetMediaPlatformStream();
                if (mediaPlatformStream.Player != null)
                {
                    AVPlayer player = mediaPlatformStream.Player;
                    player.Pause();
                    activeSong.Strategy.PlayCount = 0;
                }
            }
        }

        internal override void PlatformClearQueue()
        {
            while (Queue.Count > 0)
            {
                Song song = Queue[0];

                MediaPlatformStream mediaPlatformStream = ((ConcreteSongStrategy)song.Strategy).GetMediaPlatformStream();
                if (mediaPlatformStream.Player != null)
                {
                    AVPlayer player = mediaPlatformStream.Player;
                    player.Pause();
                    song.Strategy.PlayCount = 0;
                }

                Queue.Remove(song);
            }

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

        internal delegate void FinishedPlayingHandler(object sender, EventArgs args);
        event FinishedPlayingHandler DonePlaying;

        private void OnFinishedPlaying(object sender, NSNotificationEventArgs args)
		{
            var handler = DonePlaying;
            if (handler != null)
                handler(this, EventArgs.Empty);
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

