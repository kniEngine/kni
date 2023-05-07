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
        private MPMoviePlayerViewController _movieView;

        internal MPMoviePlayerViewController MovieView { get { return _movieView; } }


        internal ConcreteVideoStrategy(GraphicsDevice graphicsDevice, string fileName, TimeSpan duration)
            : base(graphicsDevice, fileName, duration)
        {
            NSUrl url = NSUrl.FromFilename(Path.GetFullPath(FileName));

            _movieView = new MPMoviePlayerViewController(url);
            MovieView.MoviePlayer.ScalingMode = MPMovieScalingMode.AspectFill;
            MovieView.MoviePlayer.ControlStyle = MPMovieControlStyle.None;
            MovieView.MoviePlayer.PrepareToPlay();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_movieView != null)
                {
                    _movieView.Dispose();
                    _movieView = null;
                }

            }

            base.Dispose(disposing);
        }
    }
}
