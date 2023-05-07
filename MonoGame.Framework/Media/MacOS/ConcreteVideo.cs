// Copyright (C)2023 Nick Kastellanos

// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using ObjCRuntime;
using Foundation;
using AVFoundation;


namespace Microsoft.Xna.Platform.Media
{
    public sealed class ConcreteVideoStrategy : VideoStrategy
    {
        private AVPlayer _player;
        private AVPlayerItem _movie;

        internal AVPlayer Player { get { return _player; } }

        internal float Volume
        {
            get { return Player.Volume; }
            set
            {
                // TODO When Xamarain fix the set Volume mMovie.Volume = value;
            }
        }

        internal TimeSpan CurrentPosition
        {
            get { return new TimeSpan(_movie.CurrentTime.Value); }
        }

        internal ConcreteVideoStrategy(GraphicsDevice graphicsDevice, string fileName, TimeSpan duration)
            : base(graphicsDevice, fileName, duration)
        {
            NSError err = new NSError();

            _movie = AVPlayerItem.FromUrl(NSUrl.FromFilename(FileName));
            _player = new AVPlayer(_movie);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_player != null)
                {
                    _player.Dispose();
                    _player = null;
                }

                if (_movie != null)
                {
                    _movie.Dispose();
                    _movie = null;
                }

            }

            base.Dispose(disposing);
        }
    }
}
