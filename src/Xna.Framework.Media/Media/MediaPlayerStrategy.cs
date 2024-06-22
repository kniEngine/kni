// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Media;
using FrameworkMedia = Microsoft.Xna.Framework.Media;


namespace Microsoft.Xna.Platform.Media
{
    public interface IPlatformMediaPlayer
    {
        MediaPlayerStrategy Strategy { get; }
    }

    abstract public class MediaPlayerStrategy : IDisposable
    {
        private float _volume = 1.0f;
        private bool _isMuted;
        private bool _isRepeating;
        private bool _isShuffled;
        private bool _isVisualizationEnabled;

        internal MediaState _state = MediaState.Stopped;

        private readonly MediaQueue _queue = new MediaQueue();
        // Need to hold onto this to keep track of how many songs
        // have played when in shuffle mode
        protected int _numSongsInQueuePlayed = 0;

        private readonly WeakReference _mediaPlayerRef = new WeakReference(null);
        internal FrameworkMedia.MediaPlayer MediaPlayer
        {
            get { return _mediaPlayerRef.Target as FrameworkMedia.MediaPlayer; }
            set { _mediaPlayerRef.Target = value; }
        }
        
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

        public MediaQueue Queue { get {return _queue; } }

        public virtual float PlatformVolume
        {
            get { return _volume; }
            set { _volume = value; }
        }
        public virtual bool PlatformIsMuted
        {
            get { return _isMuted; }
            set { _isMuted = value; }
        }
        public virtual bool PlatformIsRepeating
        {
            get { return _isRepeating; }
            set { _isRepeating = value; }
        }

        public virtual bool PlatformIsShuffled
        {
            get { return _isShuffled; }
            set { _isShuffled = value; }
        }
        internal virtual bool PlatformIsVisualizationEnabled
        {
            get { return _isVisualizationEnabled; }
            set { _isVisualizationEnabled = value; }
        }

        public abstract bool PlatformGameHasControl { get; }
        public abstract TimeSpan PlatformPlayPosition { get; }

        protected abstract bool PlatformUpdateState(ref MediaState state);
        public abstract void PlatformPlaySong(Song song);
        public abstract void PlatformPause();
        public abstract void PlatformResume();
        public abstract void PlatformStop();


        internal protected void OnPlatformActiveSongChanged()
        {
            if (_mediaPlayerRef.Target != null)
            {
                FrameworkMedia.MediaPlayer mediaPlayer = (FrameworkMedia.MediaPlayer)_mediaPlayerRef.Target;
                mediaPlayer.OnActiveSongChanged(EventArgs.Empty);
            }
        }

        internal protected void OnPlatformMediaStateChanged()
        {
            if (_mediaPlayerRef.Target != null)
            {
                FrameworkMedia.MediaPlayer mediaPlayer = (FrameworkMedia.MediaPlayer)_mediaPlayerRef.Target;
                mediaPlayer.OnMediaStateChanged(EventArgs.Empty);
            }
        }

        protected internal virtual void PlatformClearQueue()
        {
            while (_queue.Count > 0)
            {
                Song song = _queue[0];
                _queue.Remove(song);
            }

            _numSongsInQueuePlayed = 0;
        }

        protected void RemoveQueuedSong(Song song)
        {
            this.Queue.Remove(song);
        }

        internal void PlatformMoveNext()
        {
            NextSong(1);
        }

        internal void PlatformMovePrevious()
        {
            NextSong(-1);
        }

        private void NextSong(int direction)
        {
            if (PlatformIsRepeating && _queue.ActiveSongIndex >= _queue.Count - 1)
            {
                _queue.ActiveSongIndex = 0;
                
                // Setting direction to 0 will force the first song
                // in the queue to be played.
                // if we're on "shuffle", then it'll pick a random one
                // anyway, regardless of the "direction".
                direction = 0;
            }

            Song nextSong = _queue.GetNextSong(direction, PlatformIsShuffled);
            if (nextSong != null)
            {
                if (nextSong.IsDisposed)
                    throw new ObjectDisposedException("nextSong");

                PlatformPlaySong(nextSong);
                _state = MediaState.Playing;
                OnPlatformMediaStateChanged();
            }

            OnPlatformActiveSongChanged();
        }

        protected void OnSongFinishedPlaying()
        {
            Song activeSong = this.Queue.ActiveSong;

            _numSongsInQueuePlayed++;
            
            if (_numSongsInQueuePlayed >= _queue.Count)
            {
                _numSongsInQueuePlayed = 0;
                if (!PlatformIsRepeating)
                {
                    // Stop
                    MediaState state = State;
                    switch (state)
                    {
                        case MediaState.Playing:
                        case MediaState.Paused:
                            {
                                PlatformStop();
                                _state = MediaState.Stopped;
                                OnPlatformMediaStateChanged();
                            }
                            break;
                    }

                    OnPlatformActiveSongChanged();
                    return;
                }
            }

            // Stop
            {
                MediaState state = State;
                switch (state)
                {
                    case MediaState.Playing:
                    case MediaState.Paused:
                        {
                            PlatformStop();
                            _state = MediaState.Stopped;
                            OnPlatformMediaStateChanged();
                        }
                        break;
                }
            }

            NextSong(1);
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
}
