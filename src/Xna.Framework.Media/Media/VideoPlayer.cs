// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Media;


namespace Microsoft.Xna.Framework.Media
{
    public sealed class VideoPlayer : IDisposable
    {
        private VideoPlayerStrategy _strategy;


        #region Properties

        private VideoPlayerStrategy Strategy
        {
            get { return _strategy; }
        }


        /// <summary>
        /// Gets a value that indicates whether the object is disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets a value that indicates whether the player is playing video in a loop.
        /// </summary>
        public bool IsLooped
        {
            get { return Strategy.IsLooped; }
            set { Strategy.IsLooped = value; }
        }

        /// <summary>
        /// Gets or sets the muted setting for the video player.
        /// </summary>
        public bool IsMuted
        {
            get { return Strategy.IsMuted; }
            set
            {
                if (Strategy.IsMuted != value)
                    Strategy.IsMuted = value;
            }
        }

        /// <summary>
        /// Gets the play position within the currently playing video.
        /// </summary>
        public TimeSpan PlayPosition
        {
            get
            {
                if (Strategy.Video == null)
                    return TimeSpan.Zero;

                if (State == MediaState.Stopped)
                    return TimeSpan.Zero;

                return Strategy.PlayPosition;
            }
        }

        /// <summary>
        /// Gets the media playback state, MediaState.
        /// </summary>
        public MediaState State
        { 
            get { return Strategy.State; }
        }

        /// <summary>
        /// Gets the Video that is currently playing.
        /// </summary>
        public Video Video
        {
            get { return Strategy.Video; }
        }

        /// <summary>
        /// Video player volume, from 0.0f (silence) to 1.0f (full volume relative to the current device volume).
        /// </summary>
        public float Volume
        {
            get { return Strategy.Volume; }            
            set { Strategy.Volume = value; }
        }

        #endregion

        #region Public API

        public VideoPlayer()
        {
            _strategy = MediaFactory.Current.CreateVideoPlayerStrategy();
        }

        /// <summary>
        /// Retrieves a Texture2D containing the current frame of video being played.
        /// </summary>
        /// <returns>The current frame of video.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no video is set on the player</exception>
        /// <exception cref="InvalidOperationException">Thrown if the platform was unable to get a texture in a reasonable amount of time. Often the platform specific media code is running
        /// in a different thread or process. Note: This may be a change from XNA behaviour</exception>
        public Texture2D GetTexture()
        {
            if (Strategy.Video == null)
                throw new InvalidOperationException("Operation is not valid due to the current state of the object");

            Texture2D texture = Strategy.PlatformGetTexture();
            System.Diagnostics.Debug.Assert(texture != null);

            return texture;
        }

        /// <summary>
        /// Pauses the currently playing video.
        /// </summary>
        public void Pause()
        {
            if (Strategy.Video != null)
                Strategy.PlatformPause();
        }

        /// <summary>
        /// Plays a Video.
        /// </summary>
        /// <param name="video">Video to play.</param>
        public void Play(Video video)
        {
            if (video == null)
                throw new ArgumentNullException("video is null.");

            if (Strategy.Video == video)
            {
                MediaState state = State;
                switch (state)
                {
                    case MediaState.Stopped:
                        Strategy.PlatformPlay(video);
                        return;
                    case MediaState.Playing:
                        return;
                    case MediaState.Paused:
                        Strategy.PlatformResume();
                        return;
                }
            }

            Strategy.PlatformPlay(video);
        }

        /// <summary>
        /// Resumes a paused video.
        /// </summary>
        public void Resume()
        {
            if (Strategy.Video == null)
                return;

            MediaState state = State;
            switch (state)
            {
                case MediaState.Stopped:
                    Strategy.PlatformPlay(Strategy.Video);
                    return;
                case MediaState.Playing:
                    return;
                case MediaState.Paused:
                    Strategy.PlatformResume();
                    break;
            }

            return;
        }

        /// <summary>
        /// Stops playing a video.
        /// </summary>
        public void Stop()
        {
            if (Strategy.Video != null)
                Strategy.PlatformStop();
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Immediately releases the unmanaged resources used by this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    Strategy.Dispose();
                }

                IsDisposed = true;
            }
        }

        #endregion

    }
}
