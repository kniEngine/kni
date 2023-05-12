// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Platform.Media;

#if WINDOWS_UAP
using Windows.Storage.FileProperties;
#endif
#if IOS || TVOS
using System.Drawing;
using CoreGraphics;
using MediaPlayer;
using UIKit;
#endif
#if ANDROID
using Android.Graphics;
using Android.Provider;
#endif


namespace Microsoft.Xna.Framework.Media
{
    public sealed class Album : IDisposable
    {
        private AlbumStrategy _strategy;

        private string _name;
        private Artist _artist;
        private Genre _genre;
        private SongCollection _songCollection;


        public AlbumStrategy Strategy { get { return _strategy; } }


#if WINDOWS_UAP
        private StorageItemThumbnail _thumbnail;
#endif
#if IOS
        private MPMediaItemArtwork _thumbnail;
#endif
#if ANDROID
        private Android.Net.Uri _thumbnail;
#endif

        public Artist Artist
        {
            get { return this._artist; }
        }

        /// <summary>
        /// Gets the duration of the Album.
        /// </summary>
        public TimeSpan Duration
        {
            get
            {
                return TimeSpan.Zero; // Not implemented
            }
        }

        /// <summary>
        /// Gets the Genre of the Album.
        /// </summary>
        public Genre Genre
        {
            get { return this._genre; }
        }

        /// <summary>
        /// Gets a value indicating whether the Album has associated album art.
        /// </summary>
        public bool HasArt
        {
            get
            {
#if WINDOWS_UAP
                return this._thumbnail != null;
#elif IOS
                // If album art is missing the bounds will be: Infinity, Infinity, 0, 0
                return this._thumbnail != null && this._thumbnail.Bounds.Width != 0;
#elif ANDROID
                return this._thumbnail != null;
#else
                return false;
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
            get { return this._name; }
        }

        /// <summary>
        /// Gets a SongCollection that contains the songs on the album.
        /// </summary>
        public SongCollection Songs
        {
            get { return this._songCollection; }
        }

        internal Album(AlbumStrategy strategy)
        {
            _strategy = strategy;
        }

        private Album(string name, Artist artist, Genre genre, SongCollection songCollection)
        {
            //_strategy = new ConcreteAlbumStrategy();
            this._name = name;
            this._artist = artist;
            this._genre = genre;
            this._songCollection = songCollection;
        }

#if WINDOWS_UAP
        internal Album(string name, Artist artist, Genre genre, SongCollection songCollection, StorageItemThumbnail thumbnail)
        {
            this._name = name;
            this._artist = artist;
            this._genre = genre;
            this._songCollection = songCollection;
            this._thumbnail = thumbnail;
        }
#endif
#if IOS
        internal Album(string name, Artist artist, Genre genre, SongCollection songCollection, MPMediaItemArtwork thumbnail)
        {
            this._name = name;
            this._artist = artist;
            this._genre = genre;
            this._songCollection = songCollection;
            this._thumbnail = thumbnail;
        }
#endif
#if ANDROID
        internal Album(string name, Artist artist, Genre genre, SongCollection songCollection, Android.Net.Uri thumbnail)
        {
            this._name = name;
            this._artist = artist;
            this._genre = genre;
            this._songCollection = songCollection;
            this._thumbnail = thumbnail;
        }
#endif

        /// <summary>
        /// Immediately releases the unmanaged resources used by this object.
        /// </summary>
        public void Dispose()
        {
#if WINDOWS_UAP
            if (this._thumbnail != null)
                this._thumbnail.Dispose();
#endif
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
#if ANDROID
        [CLSCompliant(false)]
        public Bitmap Platform_GetAlbumArt(int width = 0, int height = 0)
        {
            var albumArt = MediaStore.Images.Media.GetBitmap(ConcreteMediaLibraryStrategy.Context.ContentResolver, this._thumbnail);
            if (width == 0 || height == 0)
                return albumArt;

            var scaledAlbumArt = Bitmap.CreateScaledBitmap(albumArt, width, height, true);
            albumArt.Dispose();
            return scaledAlbumArt;
        }
#endif

        /// <summary>
        /// Returns the stream that contains the album art image data.
        /// </summary>
        public Stream GetAlbumArt()
        {
#if WINDOWS_UAP
            if (this.HasArt)
                return this._thumbnail.AsStream();
            return null;
#else
            throw new NotImplementedException();
#endif
        }

#if IOS
        [CLSCompliant(false)]
        public UIImage Platform_GetThumbnail()
        {
            return this.Platform_GetAlbumArt(220, 220);
        }
#endif
#if ANDROID
        [CLSCompliant(false)]
        public Bitmap Platform_GetThumbnail()
        {
            return this.Platform_GetAlbumArt(220, 220);
        }
#endif

        /// <summary>
        /// Returns the stream that contains the album thumbnail image data.
        /// </summary>
        public Stream GetThumbnail()
        {
#if WINDOWS_UAP
            if (this.HasArt)
                return this._thumbnail.AsStream();

            return null;
#else
            throw new NotImplementedException();
#endif
        }

        /// <summary>
        /// Returns a String representation of this Album.
        /// </summary>
        public override string ToString()
        {
            return this._name;
        }

        /// <summary>
        /// Gets the hash code for this instance.
        /// </summary>
        public override int GetHashCode()
        {
            return this._name.GetHashCode();
        }
    }
}
