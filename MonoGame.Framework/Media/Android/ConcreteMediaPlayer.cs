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

        internal ConcreteMediaPlayerStrategy()
        {
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

                if (MediaPlatformStream._playingSong == activeSong.Strategy && MediaPlatformStream._androidPlayer.IsPlaying)
                    ((ConcreteSongStrategy)activeSong.Strategy)._position = TimeSpan.FromMilliseconds(MediaPlatformStream._androidPlayer.CurrentPosition);

                return ((ConcreteSongStrategy)activeSong.Strategy)._position;
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
                MediaPlatformStream._androidPlayer.SetVolume(innerVolume, innerVolume);
            }
        }
        
        internal override void PlatformPlaySong(Song song)
        {
            if (Queue.ActiveSong != null)
           {
                MediaPlatformStream mediaPlatformStream = ((ConcreteSongStrategy)song.Strategy).GetMediaPlatformStream();
                mediaPlatformStream.SetEventHandler(OnSongFinishedPlaying);

                float innerVolume = base.PlatformIsMuted ? 0.0f : base.PlatformVolume;

                MediaPlatformStream._androidPlayer.SetVolume(innerVolume, innerVolume);

                // Prepare the player
                MediaPlatformStream._androidPlayer.Reset();

                if (((ConcreteSongStrategy)song.Strategy).AssetUri != null)
                {
                    MediaPlatformStream._androidPlayer.SetDataSource(ConcreteMediaLibraryStrategy.Context,
                                                                      ((ConcreteSongStrategy)song.Strategy).AssetUri);
                }
                else
                {
                    var afd = AndroidGameWindow.Activity.Assets.OpenFd(
                        ((ConcreteSongStrategy)song.Strategy).StreamSource.OriginalString);
                    if (afd == null)
                        return;

                    MediaPlatformStream._androidPlayer.SetDataSource(afd.FileDescriptor, afd.StartOffset, afd.Length);
                }

                MediaPlatformStream._androidPlayer.Prepare();
                MediaPlatformStream._androidPlayer.Looping = MediaPlayer.IsRepeating;
                MediaPlatformStream._playingSong = ((ConcreteSongStrategy)song.Strategy);

                MediaPlatformStream._androidPlayer.Start();
                song.Strategy.PlayCount++;
            }
        }

        internal override void PlatformPause()
        {
            Song activeSong = Queue.ActiveSong;
            if (activeSong != null)
            {
                MediaPlatformStream._androidPlayer.Pause();
            }
        }

        internal override void PlatformResume()
        {
            Song activeSong = Queue.ActiveSong;
            if (activeSong != null)
            {
                MediaPlatformStream._androidPlayer.Start();
            }
        }

        internal override void PlatformStop()
        {
            foreach (Song queuedSong in Queue.Songs)
            {
                var activeSong = Queue.ActiveSong;
                MediaPlatformStream._androidPlayer.Stop();
                MediaPlatformStream._playingSong = null;
                activeSong.Strategy.PlayCount = 0;
                ((ConcreteSongStrategy)activeSong.Strategy)._position = TimeSpan.Zero;


            }
        }

        internal override void PlatformClearQueue()
        {
            while (Queue.Count > 0)
            {
                Song song = Queue[0];

                MediaPlatformStream._androidPlayer.Stop();
                MediaPlatformStream._playingSong = null;
                song.Strategy.PlayCount = 0;
                ((ConcreteSongStrategy)song.Strategy)._position = TimeSpan.Zero;
                
                Queue.Remove(song);
            }

            _numSongsInQueuePlayed = 0;
            //base.ClearQueue();
        }

    }
    
    
    internal sealed class MediaPlatformStream : IDisposable
    {
        static internal Android.Media.MediaPlayer _androidPlayer;
        static internal ConcreteSongStrategy _playingSong;


        static MediaPlatformStream()
        {
            MediaPlatformStream._androidPlayer = new Android.Media.MediaPlayer();
            MediaPlatformStream._androidPlayer.Completion += AndroidPlayer_Completion;
        }

        internal MediaPlatformStream(Uri streamSource)
        {
        }
        
        static void AndroidPlayer_Completion(object sender, EventArgs e)
        {
            ConcreteSongStrategy playingSong = _playingSong;
            MediaPlatformStream._playingSong = null;

            MediaPlatformStream mediaPlatformStream = playingSong.GetMediaPlatformStream();

            if (playingSong != null)
            {
                FinishedPlayingHandler handler = mediaPlatformStream.DonePlaying;
                if (handler != null)
                    handler();
            }
        }

        internal delegate void FinishedPlayingHandler();
        event FinishedPlayingHandler DonePlaying;

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

            }

            //base.Dispose(disposing);

        }
        #endregion
    }
}
