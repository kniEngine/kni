// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Media;
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

namespace Microsoft.Xna.Platform.Media
{
    abstract public class MediaPlayerStrategy : IDisposable
    {
        private float _volume = 1.0f;
        private bool _isMuted;
        private bool _isRepeating;
        private bool _isShuffled;
        private bool _isVisualizationEnabled;

        private MediaState _state = MediaState.Stopped;

        private readonly MediaQueue _queue = new MediaQueue();
        // Need to hold onto this to keep track of how many songs
        // have played when in shuffle mode
        private int _numSongsInQueuePlayed = 0;
        

        internal event EventHandler<EventArgs> PlatformActiveSongChanged;
        internal event EventHandler<EventArgs> PlatformMediaStateChanged;
        
        ~MediaPlayerStrategy()
        {
            Dispose(false);
        }
        
        internal MediaState State
        {
            get
            {
                PlatformUpdateState(ref _state);
                return _state;
            }
        }

        internal MediaQueue Queue { get {return _queue; }}

        internal virtual float PlatformGetVolume()
        {
            return _volume;
        }
        internal virtual void  PlatformSetVolume(float volume)
        {
            _volume = volume;
        }
        internal virtual bool PlatformGetIsMuted()
        {
            return _isMuted;
        }
        internal virtual void PlatformSetIsMuted(bool muted)
        {
            _isMuted = muted;
        }
        internal virtual bool PlatformGetIsRepeating()
        {
            return _isRepeating;
        }
        internal virtual void PlatformSetIsRepeating(bool repeating)
        {
            _isRepeating = repeating;
        }
        internal virtual bool PlatformGetIsShuffled()
        {
            return _isShuffled;
        }
        internal virtual void PlatformSetIsShuffled(bool shuffled)
        {
            _isShuffled = shuffled;
        }
        internal virtual bool PlatformGetIsVisualizationEnabled()
        {
            return _isVisualizationEnabled;
        }
        internal virtual void PlatformSetIsVisualizationEnabled(bool enabled)
        {
            _isVisualizationEnabled = enabled;
        }

        internal abstract bool PlatformGetGameHasControl();
        internal abstract TimeSpan PlatformGetPlayPosition();

        protected abstract bool PlatformUpdateState(ref MediaState state);
        protected abstract void PlatformPlaySong(Song song);
        protected abstract void PlatformPause();
        protected abstract void PlatformResume();
        protected abstract void PlatformStop();


        protected virtual void OnPlatformActiveSongChanged(EventArgs args)
        {
            var handler = PlatformActiveSongChanged;
            if (handler != null)
                handler(null, args);
        }

        protected virtual void OnPlatformMediaStateChanged(EventArgs args)
        {
            var handler = PlatformMediaStateChanged;
            if (handler != null)
                handler(null, args);
        }

        protected virtual void PlatformOnSongRepeat()
        {

        }


        internal void Play(Song song)
        {
            if (song == null)
                throw new ArgumentNullException("song", "This method does not accept null for this parameter.");

            var previousSong = _queue.Count > 0 ? _queue[0] : null;
            _queue.Clear();
            _numSongsInQueuePlayed = 0;
            _queue.Add(song);
            _queue.ActiveSongIndex = 0;

            PlaySong(song);

            if (previousSong != song)
                OnPlatformActiveSongChanged(EventArgs.Empty);
        }

        internal void Play(SongCollection collection)
        {
            Play(collection, 0);
        }

        internal void Play(SongCollection collection, int index)
        {
            if (collection == null)
                throw new ArgumentNullException("collection", "This method does not accept null for this parameter.");

            _queue.Clear();
            _numSongsInQueuePlayed = 0;

            foreach (var song in collection)
                _queue.Add(song);

            _queue.ActiveSongIndex = index;

            PlaySong(_queue.ActiveSong);
        }
        
        internal void PlaySong(Song song)
        {
            if (song != null && song.IsDisposed)
                throw new ObjectDisposedException("song");

            PlatformPlaySong(song);
            _state = MediaState.Playing;
            OnPlatformMediaStateChanged(EventArgs.Empty);
        }

        internal void Pause()
        {
            switch (State)
            {
                case MediaState.Playing:
                    if (_queue.ActiveSong != null)
                    {
                        PlatformPause();
                        _state =  MediaState.Paused;
                        OnPlatformMediaStateChanged(EventArgs.Empty);
                    }
                    break;
            }
        }

        internal void Resume()
        {
            switch (State)
            {
                case MediaState.Paused:
                    {
                        PlatformResume();
                        _state = MediaState.Playing;
                        OnPlatformMediaStateChanged(EventArgs.Empty);
                    }
                    break;
            }
        }

        internal void Stop()
        {
            switch (State)
            {
                case MediaState.Playing:
                case MediaState.Paused:
                    {
                        PlatformStop();
                        _state = MediaState.Stopped;
                        OnPlatformMediaStateChanged(EventArgs.Empty);
                    }
                    break;
            }
        }

        internal void MoveNext()
        {
            NextSong(1);
        }

        internal void MovePrevious()
        {
            NextSong(-1);
        }

        private void NextSong(int direction)
        {
            Stop();

            if (PlatformGetIsRepeating() && _queue.ActiveSongIndex >= _queue.Count - 1)
            {
                _queue.ActiveSongIndex = 0;
                
                // Setting direction to 0 will force the first song
                // in the queue to be played.
                // if we're on "shuffle", then it'll pick a random one
                // anyway, regardless of the "direction".
                direction = 0;
            }

            var nextSong = _queue.GetNextSong(direction, PlatformGetIsShuffled());

            if (nextSong != null)
                PlaySong(nextSong);

            OnPlatformActiveSongChanged(EventArgs.Empty);
        }

        internal void OnSongFinishedPlaying(object sender, EventArgs args)
        {
            // TODO: Check args to see if song sucessfully played
            _numSongsInQueuePlayed++;
            
            if (_numSongsInQueuePlayed >= _queue.Count)
            {
                _numSongsInQueuePlayed = 0;
                if (!PlatformGetIsRepeating())
                {
                    Stop();
                    OnPlatformActiveSongChanged(EventArgs.Empty);
                    return;
                }
            }

            if (PlatformGetIsRepeating())
                PlatformOnSongRepeat();

            MoveNext();
        }


        #region IDisposable Members
        
        /// <summary>Releases the resources held by this <see cref="Microsoft.Xna.Framework.Audio.SoundEffect"/>.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {               
                
            }
            
        }

        #endregion
    }

    abstract public class SongStrategy // : IDisposable
    {
        internal SongStrategy() { }

        internal abstract void PlatformInitialize(string fileName);

        internal abstract Album PlatformGetAlbum();
        internal abstract void PlatformSetAlbum(Album album);
        internal abstract Artist PlatformGetArtist();
        internal abstract Genre PlatformGetGenre();

        internal abstract TimeSpan PlatformGetDuration();
        internal abstract bool PlatformIsProtected();
        internal abstract bool PlatformIsRated();
        
        internal abstract string PlatformGetName();
        internal abstract int PlatformGetPlayCount();
        internal abstract int PlatformGetRating();
        internal abstract int PlatformGetTrackNumber();
        
        internal abstract void PlatformDispose(bool disposing);

        #region IDisposable
        //~SongStrategy()
        //{
        //    Dispose(false);
        //}

        //public void Dispose()
        //{
        //    Dispose(true);
        //    GC.SuppressFinalize(this);
        //}

        //protected abstract void Dispose(bool disposing);
        #endregion
    }
}
