// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Platform.Media;

#if IOS || TVOS
using System.Drawing;
using CoreGraphics;
using MediaPlayer;
using UIKit;
#endif


namespace Microsoft.Xna.Framework.Media
{
    public sealed class Album : IDisposable
    {
        private AlbumStrategy _strategy;

        public AlbumStrategy Strategy { get { return _strategy; } }


#if IOS
        private MPMediaItemArtwork _thumbnail;
#endif

        public Artist Artist
        {
            get { return _strategy.Artist; }
        }

        /// <summary>
        /// Gets the duration of the Album.
        /// </summary>
        public TimeSpan Duration
        {
            get { return _strategy.Duration; }
        }

        /// <summary>
        /// Gets the Genre of the Album.
        /// </summary>
        public Genre Genre
        {
            get { return _strategy.Genre; }
        }

        /// <summary>
        /// Gets a value indicating whether the Album has associated album art.
        /// </summary>
        public bool HasArt
        {
            get
            {
#if IOS
                // If album art is missing the bounds will be: Infinity, Infinity, 0, 0
                return this._thumbnail != null && this._thumbnail.Bounds.Width != 0;
#else
                return _strategy.HasArt;
#endif
            }
        }

        /// <summary>
        /// Gets a value indicating whether the object is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the name of the Album.
        /// </summary>
        public string Name
        {
            get { return _strategy.Name; }
        }

        /// <summary>
        /// Gets a SongCollection that contains the songs on the album.
        /// </summary>
        public SongCollection Songs
        {
            get { return _strategy.Songs; }
        }

        internal Album(AlbumStrategy strategy)
        {
            _strategy = strategy;
        }

#if IOS
        internal Album(string name, Artist artist, Genre genre, SongCollection songCollection, MPMediaItemArtwork thumbnail)
        {
            _strategy = new ConcreteAlbumStrategy(name, artist, genre, songCollection);
            this._thumbnail = thumbnail;
        }
#endif

        /// <summary>
        /// Immediately releases the unmanaged resources used by this object.
        /// </summary>
        public void Dispose()
        {
            _strategy.Dispose();
        }

#if IOS
        [CLSCompliant(false)]
        public UIImage Platform_GetAlbumArt(int width = 0, int height = 0)
        {
            if (width == 0)
                width = (int)this._thumbnail.Bounds.Width;
            if (height == 0)
                height = (int)this._thumbnail.Bounds.Height;

			return this._thumbnail.ImageWithSize(new CGSize(width, height));
        }
#endif

        /// <summary>
        /// Returns the stream that contains the album art image data.
        /// </summary>
        public Stream GetAlbumArt()
        {
            return _strategy.GetAlbumArt();
        }

#if IOS
        [CLSCompliant(false)]
        public UIImage Platform_GetThumbnail()
        {
            return this.Platform_GetAlbumArt(220, 220);
        }
#endif

        /// <summary>
        /// Returns the stream that contains the album thumbnail image data.
        /// </summary>
        public Stream GetThumbnail()
        {
            return _strategy.GetThumbnail();
        }

        /// <summary>
        /// Returns a String representation of this Album.
        /// </summary>
        public override string ToString()
        {
            return _strategy.Name;
        }

        /// <summary>
        /// Gets the hash code for this instance.
        /// </summary>
        public override int GetHashCode()
        {
            return _strategy.Name.GetHashCode();
        }
    }
}
