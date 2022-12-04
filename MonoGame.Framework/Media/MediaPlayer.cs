// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Media;


namespace Microsoft.Xna.Framework.Media
{
    public static class MediaPlayer
    {
        private volatile static MediaPlayerStrategy _strategy;
        internal readonly static object SyncHandle = new object();

        internal static MediaPlayerStrategy Strategy
        {
            get
            {
                var strategy = _strategy;
                if (strategy != null)
                    return strategy;

                // Create instance
                lock (SyncHandle)
                {
                    if (_strategy == null)
                    {
                        _strategy = new ConcreteMediaPlayerStrategy();
                        _strategy.PlatformActiveSongChanged += (sender, args) => OnActiveSongChanged(args);
                        _strategy.PlatformMediaStateChanged += (sender, args) => OnMediaStateChanged(args);
                    }

                    return _strategy;
                }
            }
        }

        public static event EventHandler<EventArgs> ActiveSongChanged;
        public static event EventHandler<EventArgs> MediaStateChanged;


        #region Properties

        public static MediaQueue Queue
        {
            get { return Strategy.Queue; }
        }
        
        public static bool IsMuted
        {
            get { return Strategy.PlatformGetIsMuted(); }
            set { Strategy.PlatformSetIsMuted(value); }
        }

        public static bool IsRepeating 
        {
            get { return Strategy.PlatformGetIsRepeating(); }
            set { Strategy.PlatformSetIsRepeating(value); }
        }

        public static bool IsShuffled
        {
            get { return Strategy.PlatformGetIsShuffled(); }
            set { Strategy.PlatformSetIsShuffled(value); }
        }

        public static bool IsVisualizationEnabled
        {
            get { return Strategy.PlatformGetIsVisualizationEnabled(); }
            set { Strategy.PlatformSetIsVisualizationEnabled(value); }
        }

        public static TimeSpan PlayPosition
        {
            get { return Strategy.PlatformGetPlayPosition(); }
        }

        public static MediaState State
        {
            get { return Strategy.State; }
        }
        
        public static bool GameHasControl
        {
            get { return Strategy.PlatformGetGameHasControl(); }
        }
        

        public static float Volume
        {
            get { return Strategy.PlatformGetVolume(); }
            set
            {
                var volume = MathHelper.Clamp(value, 0, 1);
                Strategy.PlatformSetVolume(volume);
            }
        }

        #endregion
        
        /// <summary>
        /// Play clears the current playback queue, and then queues up the specified song for playback. 
        /// Playback starts immediately at the beginning of the song.
        /// </summary>
        public static void Play(Song song)
        {
            Strategy.Play(song);
        }        

        public static void Play(SongCollection collection)
        {
            Strategy.Play(collection);
        }
        
        public static void Play(SongCollection collection, int index)
        {
            Strategy.Play(collection, index);
        }
        
        public static void Pause()
        {
            Strategy.Pause();
        }

        public static void Resume()
        {
            Strategy.Resume();
        }

        public static void Stop()
        {
            Strategy.Stop();
        }

        public static void MoveNext()
        {
            Strategy.MoveNext();
        }

        public static void MovePrevious()
        {
            Strategy.MovePrevious();
        }

        private static void OnActiveSongChanged(EventArgs args)
        {
            var handler = ActiveSongChanged;
            if (handler != null)
                handler(null, args);
        }

        private static void OnMediaStateChanged(EventArgs args)
        {
            var handler = MediaStateChanged;
            if (handler != null)
                handler(null, args);
        }

        
    }
}
