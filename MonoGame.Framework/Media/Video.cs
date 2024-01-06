// Copyright (C)2023 Nick Kastellanos

// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Media;

namespace Microsoft.Xna.Framework.Media
{
    /// <summary>
    /// Represents a video.
    /// </summary>
    public sealed class Video : IDisposable
    {
        private bool _disposed;
        private VideoStrategy _strategy;

        internal VideoStrategy Strategy { get { return _strategy; } }
        internal GraphicsDevice GraphicsDevice { get { return _strategy.GraphicsDevice; } }

        #region Public API

        /// <summary>
        /// I actually think this is a file PATH...
        /// </summary>
        public string FileName { get { return _strategy.FileName; } }

		/// <summary>
		/// Gets the duration of the Video.
        /// </summary>
        public TimeSpan Duration { get { return _strategy.Duration; } }

        /// <summary>
        /// Gets the frame rate of this video.
        /// </summary>
        public float FramesPerSecond
        {
            get { return _strategy.FramesPerSecond; }
            internal set { _strategy.FramesPerSecond = value; }
        }

        /// <summary>
        /// Gets the height of this video, in pixels.
        /// </summary>
        public int Height
        {
            get { return _strategy.Height; }
            internal set { _strategy.Height = value; }
        }

        /// <summary>
        /// Gets the VideoSoundtrackType for this video.
        /// </summary>
        public VideoSoundtrackType VideoSoundtrackType
        {
            get { return _strategy.VideoSoundtrackType; }
            internal set { _strategy.VideoSoundtrackType = value; }
        }

        /// <summary>
        /// Gets the width of this video, in pixels.
        /// </summary>
        public int Width
        {
            get { return _strategy.Width; }
            internal set { _strategy.Width = value; }
}

        #endregion

        #region Internal API

        internal Video(GraphicsDevice graphicsDevice, string fileName, float durationMS)
        {
            _strategy = MediaFactory.Current.CreateVideoStrategy(graphicsDevice, fileName, TimeSpan.FromMilliseconds(durationMS));

        }

        ~Video()
        {
            Dispose(false);
        }

        #endregion

        #region IDisposable Implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _strategy.Dispose();
                }

                _disposed = true;
            }
        }

        #endregion
    }
}
