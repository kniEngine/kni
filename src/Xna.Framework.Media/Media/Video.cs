﻿// Copyright (C)2023 Nick Kastellanos

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
        , IPlatformVideo
    {
        private VideoStrategy _strategy;
        private bool _isDisposed;

        VideoStrategy IPlatformVideo.Strategy { get { return _strategy; } }

        #region Public API

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
            MediaLibrary.PreserveMediaContentTypeReaders();

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
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _strategy.Dispose();
                }

                _isDisposed = true;
            }
        }

        #endregion
    }
}
