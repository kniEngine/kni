// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Windows.Storage;
using Microsoft.Xna.Platform.Media;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Song : SongStrategy
    {
        private Album _album;
        private Artist _artist;
        private Genre _genre;
        
		private MusicProperties _musicProperties;

        [CLSCompliant(false)]
        public StorageFile StorageFile
        {
            get { return this._musicProperties.File; }
        }
        
		internal Song(Album album, Artist artist, Genre genre, MusicProperties musicProperties)
		{
            _strategy = this;
            this._album = album;
            this._artist = artist;
            this._genre = genre;
            this._musicProperties = musicProperties;
		}

        internal override void PlatformInitialize(string fileName)
        {

        }

        internal override void PlatformDispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        internal override Album PlatformGetAlbum()
        {
            return this._album;
        }

        internal override void PlatformSetAlbum(Album album)
        {
            this._album = album;
        }

        internal override Artist PlatformGetArtist()
        {
            return this._artist;
        }

        internal override Genre PlatformGetGenre()
        {
            return this._genre;
        }

        internal override TimeSpan PlatformGetDuration()
        {
            if (this._musicProperties != null)
                return this._musicProperties.Duration;

            return _duration;
        }

        internal override bool PlatformIsProtected()
        {
            if (this._musicProperties != null)
                return this._musicProperties.IsProtected;

            return false;
        }

        internal override bool PlatformIsRated()
        {
            if (this._musicProperties != null)
                return this._musicProperties.Rating != 0;

            return false;
        }

        internal override string PlatformGetName()
        {
            if (this._musicProperties != null)
                return this._musicProperties.Title;

            return Path.GetFileNameWithoutExtension(_name);
        }

        internal override int PlatformGetPlayCount()
        {
            return _playCount;
        }

        internal override int PlatformGetRating()
        {
            if (this._musicProperties != null)
                return this._musicProperties.Rating;

            return 0;
        }

        internal override int PlatformGetTrackNumber()
        {
            if (this._musicProperties != null)
                return this._musicProperties.TrackNumber;

            return 0;
        }
    }
}
