// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using MediaPlayer;
using Foundation;


namespace Microsoft.Xna.Framework.Media
{
    /// <summary>
    /// Represents a video.
    /// </summary>
    public sealed partial class Video : IDisposable
    {
        internal MPMoviePlayerViewController MovieView { get; private set; }


        private void PlatformInitialize()
        {
            NSUrl url = NSUrl.FromFilename(Path.GetFullPath(FileName));

            MovieView = new MPMoviePlayerViewController(url);
            MovieView.MoviePlayer.ScalingMode = MPMovieScalingMode.AspectFill;
            MovieView.MoviePlayer.ControlStyle = MPMovieControlStyle.None;
            MovieView.MoviePlayer.PrepareToPlay();
        }

        private void PlatformDispose(bool disposing)
        {
            if (MovieView == null)
                return;
            
            MovieView.Dispose();
            MovieView = null;
        }
    }
}
