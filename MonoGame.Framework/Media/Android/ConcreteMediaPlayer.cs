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
            if (activeSong == null)
                return TimeSpan.Zero;
            
            if (ConcreteSongStrategy._playingSong == activeSong.Strategy && ConcreteSongStrategy._androidPlayer.IsPlaying)
                ((ConcreteSongStrategy)activeSong.Strategy)._position = TimeSpan.FromMilliseconds(ConcreteSongStrategy._androidPlayer.CurrentPosition);

            return ((ConcreteSongStrategy)activeSong.Strategy)._position;
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
            return true;
        }

        #endregion

        private void SetChannelVolumes()
        {
            float innerVolume = base.PlatformGetIsMuted() ? 0.0f : base.PlatformGetVolume();

            foreach (Song queuedSong in Queue.Songs)
            {
                ConcreteSongStrategy._androidPlayer.SetVolume(innerVolume, innerVolume);
            }
        }
        
        protected override void PlatformPlaySong(Song song)
        {
            if (Queue.ActiveSong != null)
           {
                ((ConcreteSongStrategy)song.Strategy).SetEventHandler(OnSongFinishedPlaying);

                float innerVolume = base.PlatformGetIsMuted() ? 0.0f : base.PlatformGetVolume();

                ConcreteSongStrategy._androidPlayer.SetVolume(innerVolume, innerVolume);
                                             
                // Prepare the player
                ConcreteSongStrategy._androidPlayer.Reset();

                if (((ConcreteSongStrategy)song.Strategy).AssetUri != null)
                {
                    ConcreteSongStrategy._androidPlayer.SetDataSource(ConcreteMediaLibraryStrategy.Context,
                                                                      ((ConcreteSongStrategy)song.Strategy).AssetUri);
                }
                else
                {
                    var afd = AndroidGameWindow.Activity.Assets.OpenFd(
                        ((ConcreteSongStrategy)song.Strategy).StreamSource.OriginalString);
                    if (afd == null)
                        return;

                    ConcreteSongStrategy._androidPlayer.SetDataSource(afd.FileDescriptor, afd.StartOffset, afd.Length);
                }

                ConcreteSongStrategy._androidPlayer.Prepare();
                ConcreteSongStrategy._androidPlayer.Looping = MediaPlayer.IsRepeating;
                ConcreteSongStrategy._playingSong = ((ConcreteSongStrategy)song.Strategy);

                ConcreteSongStrategy._androidPlayer.Start();
                song.Strategy.PlayCount++;
            }
        }

        protected override void PlatformPause()
        {
            Song activeSong = Queue.ActiveSong;
            if (activeSong != null)
            {
                ConcreteSongStrategy._androidPlayer.Pause();
            }
        }

        protected override void PlatformResume()
        {
            Song activeSong = Queue.ActiveSong;
            if (activeSong != null)
            {
                ConcreteSongStrategy._androidPlayer.Start();
            }
        }

        protected override void PlatformStop()
        {
            foreach (Song queuedSong in Queue.Songs)
            {
                var activeSong = Queue.ActiveSong;
                ConcreteSongStrategy._androidPlayer.Stop();
                ConcreteSongStrategy._playingSong = null;
                activeSong.Strategy.PlayCount = 0;
                ((ConcreteSongStrategy)activeSong.Strategy)._position = TimeSpan.Zero;


            }
        }

        protected override void PlatformClearQueue()
        {
            while (Queue.Count > 0)
            {
                Song song = Queue[0];
                ConcreteSongStrategy._androidPlayer.Stop();
                ConcreteSongStrategy._playingSong = null;
                song.Strategy.PlayCount = 0;
                ((ConcreteSongStrategy)song.Strategy)._position = TimeSpan.Zero;
                
                Queue.Remove(song);
            }

            //base.ClearQueue();
        }

    }
}

