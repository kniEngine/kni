// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Platform.Media;


namespace Microsoft.Xna.Framework.Media
{
    public sealed class Album : IDisposable
        , IPlatformAlbum
    {
        private AlbumStrategy _strategy;

        AlbumStrategy IPlatformAlbum.Strategy { get { return _strategy; } }


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
            get { return _strategy.HasArt; }
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


        /// <summary>
        /// Immediately releases the unmanaged resources used by this object.
        /// </summary>
        public void Dispose()
        {
            _strategy.Dispose();
        }


        /// <summary>
        /// Returns the stream that contains the album art image data.
        /// </summary>
        public Stream GetAlbumArt()
        {
            return _strategy.GetAlbumArt();
        }

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
