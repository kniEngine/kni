// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Media;


namespace Microsoft.Xna.Framework.Media
{
    public sealed class MediaPlayer : IMediaPlayer
    {
        private static MediaPlayer _current;

        /// <summary>
        /// Returns the current FrameworkDispatcher instance.
        /// </summary> 
        internal static IMediaPlayer Current
        {
            get
            {
                lock (typeof(MediaPlayer))
                {
                    if (_current == null)
                    {
                        _current = new MediaPlayer();
                    }

                    return _current;
                }
            }
        }


        public static event EventHandler<EventArgs> ActiveSongChanged;
        public static event EventHandler<EventArgs> MediaStateChanged;


        #region Properties

        public static MediaQueue Queue
        {
            get { return Current.Queue; }
        }
        
        public static bool IsMuted
        {
            get { return Current.IsMuted; }
            set { Current.IsMuted = value; }
        }

        public static bool IsRepeating 
        {
            get { return Current.IsRepeating; }
            set { Current.IsRepeating = value; }
        }

        public static bool IsShuffled
        {
            get { return Current.IsShuffled; }
            set { Current.IsShuffled = value; }
        }

        public static bool IsVisualizationEnabled
        {
            get { return Current.IsVisualizationEnabled; }
            set { Current.IsVisualizationEnabled = value; }
        }

        public static TimeSpan PlayPosition
        {
            get { return Current.PlayPosition; }
        }

        public static MediaState State
        {
            get { return Current.State; }
        }
        
        public static bool GameHasControl
        {
            get { return Current.GameHasControl; }
        }
        
        public static float Volume
        {
            get { return Current.Volume; }
            set { Current.Volume = value; }
        }

        #endregion

        /// <summary>
        /// Play clears the current playback queue, and then queues up the specified song for playback. 
        /// Playback starts immediately at the beginning of the song.
        /// </summary>
        public static void Play(Song song)
        {
            MediaPlayer.Current.Play(song);
        }

        public static void Play(SongCollection collection)
        {
            MediaPlayer.Current.Play(collection);
        }

        public static void Play(SongCollection collection, int index)
        {
            MediaPlayer.Current.Play(collection, index);
        }

        public static void Pause()
        {
            MediaPlayer.Current.Pause();
        }

        public static void Resume()
        {
            MediaPlayer.Current.Resume();
        }

        public static void Stop()
        {
            MediaPlayer.Current.Stop();
        }

        public static void MoveNext()
        {
            MediaPlayer.Current.MoveNext();
        }

        public static void MovePrevious()
        {
            MediaPlayer.Current.MovePrevious();
        }

        private void OnActiveSongChanged(EventArgs args)
        {
            var handler = _activeSongChanged;
            if (handler != null)
                handler(this, args);

            var staticHandler = MediaPlayer.ActiveSongChanged;
            if (staticHandler != null)
                staticHandler(null, args);
        }

        private void OnMediaStateChanged(EventArgs args)
        {
            var handler = _mediaStateChanged;
            if (handler != null)
                handler(this, args);

            var staticHandler = MediaPlayer.MediaStateChanged;
            if (staticHandler != null)
                staticHandler(null, args);
        }


        private MediaPlayerStrategy _strategy;

        private event EventHandler<EventArgs> _activeSongChanged;
        private event EventHandler<EventArgs> _mediaStateChanged;


        internal MediaPlayerStrategy Strategy { get { return _strategy; } }


        event EventHandler<EventArgs> IMediaPlayer.ActiveSongChanged
        {
            add { _activeSongChanged += value; }
            remove { ActiveSongChanged -= value; }
        }

        event EventHandler<EventArgs> IMediaPlayer.MediaStateChanged
        {
            add { _mediaStateChanged += value; }
            remove { _mediaStateChanged -= value; }
        }


        MediaQueue IMediaPlayer.Queue
        {
            get { return Strategy.Queue; }
        }

        bool IMediaPlayer.IsMuted
        {
            get { return Strategy.PlatformIsMuted; }
            set { Strategy.PlatformIsMuted = value; }
        }

        bool IMediaPlayer.IsRepeating
        {
            get { return Strategy.PlatformIsRepeating; }
            set { Strategy.PlatformIsRepeating = value; }
        }

        bool IMediaPlayer.IsShuffled
        {
            get { return Strategy.PlatformIsShuffled; }
            set { Strategy.PlatformIsShuffled=value; }
        }

        bool IMediaPlayer.IsVisualizationEnabled
        {
            get { return Strategy.PlatformIsVisualizationEnabled; }
            set { Strategy.PlatformIsVisualizationEnabled = value; }
        }

        TimeSpan IMediaPlayer.PlayPosition
        {
            get { return Strategy.PlatformPlayPosition; }
        }

        MediaState IMediaPlayer.State
        {
            get { return Strategy.State; }
        }

        bool IMediaPlayer.GameHasControl
        {
            get { return Strategy.PlatformGameHasControl; }
        }

        float IMediaPlayer.Volume
        {
            get { return Strategy.PlatformVolume; }
            set
            {
                var volume = MathHelper.Clamp(value, 0, 1);
                Strategy.PlatformVolume = volume;
            }
        }


        private MediaPlayer()
        {
            _strategy = MediaFactory.Current.CreateMediaPlayerStrategy();
            _strategy.MediaPlayer = this;
        }


        internal void Strategy_PlatformActiveSongChanged()
        {
            OnActiveSongChanged(EventArgs.Empty);
        }

        internal void Strategy_PlatformMediaStateChanged()
        {
            OnMediaStateChanged(EventArgs.Empty);
        }

        void IMediaPlayer.Play(Song song)
        {
            if (song == null)
                throw new ArgumentNullException("song", "This method does not accept null for this parameter.");

            var previousSong = Strategy.Queue.Count > 0
                             ? Strategy.Queue[0]
                             : null;

            Strategy.PlatformClearQueue();
            Strategy.Queue.Add(song);
            Strategy.Queue.ActiveSongIndex = 0;

            if (song.IsDisposed)
                throw new ObjectDisposedException("song");

            Strategy.PlatformPlaySong(song);
            Strategy._state = MediaState.Playing;
            Strategy.OnPlatformMediaStateChanged();

            if (previousSong != song)
                Strategy.OnPlatformActiveSongChanged();
        }

        void IMediaPlayer.Play(SongCollection collection)
        {
            ((IMediaPlayer)this).Play(collection, 0);
        }

        void IMediaPlayer.Play(SongCollection collection, int index)
        {
            if (collection == null)
                throw new ArgumentNullException("collection", "This method does not accept null for this parameter.");

            Strategy.PlatformClearQueue();

            foreach (var song in collection)
                Strategy.Queue.Add(song);

            Strategy.Queue.ActiveSongIndex = index;

            Song activeSong = Strategy.Queue.ActiveSong;
            if (activeSong.IsDisposed)
                throw new ObjectDisposedException("activeSong");

            Strategy.PlatformPlaySong(activeSong);
            Strategy._state = MediaState.Playing;
            Strategy.OnPlatformMediaStateChanged();
        }

        void IMediaPlayer.Pause()
        {
            MediaState state = State;
            switch (state)
            {
                case MediaState.Playing:
                    if (Strategy.Queue.ActiveSong != null)
                    {
                        Strategy.PlatformPause();
                        Strategy._state =  MediaState.Paused;
                        Strategy.OnPlatformMediaStateChanged();
                    }
                    break;
            }
        }

        void IMediaPlayer.Resume()
        {
            MediaState state = State;
            switch (state)
            {
                case MediaState.Paused:
                    {
                        Strategy.PlatformResume();
                        Strategy._state = MediaState.Playing;
                        Strategy.OnPlatformMediaStateChanged();
                    }
                    break;
            }
        }

        void IMediaPlayer.Stop()
        {
            MediaState state = State;
            switch (state)
            {
                case MediaState.Playing:
                case MediaState.Paused:
                    {
                        Strategy.PlatformStop();
                        Strategy._state = MediaState.Stopped;
                        Strategy.OnPlatformMediaStateChanged();
                    }
                    break;
            }
        }

        void IMediaPlayer.MoveNext()
        {
            Stop();
            Strategy.PlatformMoveNext();
        }

        void IMediaPlayer.MovePrevious()
        {
            Stop();
            Strategy.PlatformMovePrevious();
        }
                
    }

    internal interface IMediaPlayer
    {
        MediaQueue Queue { get; }
        bool IsMuted { get; set; }
        bool IsRepeating { get; set; }
        bool IsShuffled { get; set; }
        bool IsVisualizationEnabled { get; set; }
        TimeSpan PlayPosition { get; }
        MediaState State { get; }
        bool GameHasControl { get; }
        float Volume { get; set; }

        event EventHandler<EventArgs> ActiveSongChanged;
        event EventHandler<EventArgs> MediaStateChanged;

        void Play(Song song);
        void Play(SongCollection collection);
        void Play(SongCollection collection, int index);
        void Pause();
        void Resume();
        void Stop();
        void MoveNext();
        void MovePrevious();
    }
}
