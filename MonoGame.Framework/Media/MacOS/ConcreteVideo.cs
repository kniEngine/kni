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
        AVPlayerItem movie;

        internal AVPlayer Player { get; private set; }

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
            get { return new TimeSpan(movie.CurrentTime.Value); }
        }

        internal ConcreteVideoStrategy(GraphicsDevice graphicsDevice, string fileName, TimeSpan duration)
            : base(graphicsDevice, fileName, duration)
        {
            NSError err = new NSError();

            movie = AVPlayerItem.FromUrl(NSUrl.FromFilename(FileName));
            Player = new AVPlayer(movie);
        }

        protected override void Dispose(bool disposing)
        {
            /* PlatformDispose(...) disabled in https://github.com/MonoGame/MonoGame/pull/2406
            if (Player != null)
            {
                Player.Dispose();
                Player = null;
            }

            if (movie != null)
            {
                movie.Dispose();
                movie = null;
            }
            */

            if (disposing)
            {

            }

            base.Dispose(disposing);
        }
    }
}
