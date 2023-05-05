// Copyright (C)2023 Nick Kastellanos

// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using MediaPlayer;
using Foundation;


namespace Microsoft.Xna.Platform.Media
{
    public sealed class ConcreteVideoStrategy : VideoStrategy
    {
        internal MPMoviePlayerViewController MovieView { get; private set; }


        internal ConcreteVideoStrategy(GraphicsDevice graphicsDevice, string fileName, TimeSpan duration)
            : base(graphicsDevice, fileName, duration)
        {
            NSUrl url = NSUrl.FromFilename(Path.GetFullPath(FileName));

            MovieView = new MPMoviePlayerViewController(url);
            MovieView.MoviePlayer.ScalingMode = MPMovieScalingMode.AspectFill;
            MovieView.MoviePlayer.ControlStyle = MPMovieControlStyle.None;
            MovieView.MoviePlayer.PrepareToPlay();
        }

        private void PlatformDispose(bool disposing)
        {
        }

        protected override void Dispose(bool disposing)
        {
            /* PlatformDispose(...) disabled in https://github.com/MonoGame/MonoGame/pull/2406
            if (MovieView != null)
            {
                MovieView.Dispose();
                MovieView = null;
            }
            */
            
            if (disposing)
            {

            }

            base.Dispose(disposing);
        }
    }
}
