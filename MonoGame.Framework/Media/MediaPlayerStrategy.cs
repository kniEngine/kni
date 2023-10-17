// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Media;


namespace Microsoft.Xna.Platform.Media
{
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
        internal int _numSongsInQueuePlayed = 0;

        private readonly WeakReference _mediaPlayerRef = new WeakReference(null);
        internal MediaPlayer MediaPlayer
        {
            get { return _mediaPlayerRef.Target as MediaPlayer; }
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

        internal MediaQueue Queue { get {return _queue; }}

        internal virtual float PlatformVolume
        {
            get { return _volume; }
            set { _volume = value; }
        }
        internal virtual bool PlatformIsMuted
        {
            get { return _isMuted; }
            set { _isMuted = value; }
        }
        internal virtual bool PlatformIsRepeating
        {
            get { return _isRepeating; }
            set { _isRepeating = value; }
        }

        internal virtual bool PlatformIsShuffled
        {
            get { return _isShuffled; }
            set { _isShuffled = value; }
        }
        internal virtual bool PlatformIsVisualizationEnabled
        {
            get { return _isVisualizationEnabled; }
            set { _isVisualizationEnabled = value; }
        }

        internal abstract bool PlatformGameHasControl { get; }
        internal abstract TimeSpan PlatformPlayPosition { get; }

        protected abstract bool PlatformUpdateState(ref MediaState state);
        internal abstract void PlatformPlaySong(Song song);
        internal abstract void PlatformPause();
        internal abstract void PlatformResume();
        internal abstract void PlatformStop();


        internal void OnPlatformActiveSongChanged()
        {
            if (_mediaPlayerRef.Target != null)
            {
                MediaPlayer mediaPlayer = (MediaPlayer)_mediaPlayerRef.Target;
                mediaPlayer.Strategy_PlatformActiveSongChanged();
            }
        }

        internal void OnPlatformMediaStateChanged()
        {
            if (_mediaPlayerRef.Target != null)
            {
                MediaPlayer mediaPlayer = (MediaPlayer)_mediaPlayerRef.Target;
                mediaPlayer.Strategy_PlatformMediaStateChanged();
            }
        }

        internal virtual void PlatformClearQueue()
        {
            while (_queue.Count > 0)
            {
                Song song = _queue[0];
                _queue.Remove(song);
            }

            _numSongsInQueuePlayed = 0;
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

            var nextSong = _queue.GetNextSong(direction, PlatformIsShuffled);
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

        internal void OnSongFinishedPlaying()
        {
            Song activeSong = Queue.ActiveSong;

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
