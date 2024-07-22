// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Framework.Media;
using WinFileProperties = Windows.Storage.FileProperties;


namespace Microsoft.Xna.Platform.Media
{
    internal class ConcreteAlbumStrategy : AlbumStrategy
    {
        private string _name;
        private Artist _artist;
        private Genre _genre;
        private SongCollection _songs;

        private WinFileProperties.StorageItemThumbnail _thumbnail;


        public override string Name
        {
            get { return this._name; }
        }

        public override Artist Artist
        {
            get { return this._artist; }
        }

        public override Genre Genre
        {
            get { return this._genre; }
        }

        public override TimeSpan Duration
        {
            get { throw new NotImplementedException(); }
        }

        public override bool HasArt
        {
            get { return this._thumbnail != null; }
        }

        public override SongCollection Songs
        {
            get { return this._songs; }
        }


        internal ConcreteAlbumStrategy(string name, Artist artist, Genre genre, SongCollection songCollection,
                                     WinFileProperties.StorageItemThumbnail thumbnail)
        {
            this._name = name;
            this._artist = artist;
            this._genre = genre;
            this._songs = songCollection;

            this._thumbnail = thumbnail;
        }


        public override Stream GetAlbumArt()
        {
            if (this.HasArt)
                return this._thumbnail.AsStream();
            return null;
        }

        public override Stream GetThumbnail()
        {
            if (this.HasArt)
                return this._thumbnail.AsStream();

            return null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this._thumbnail != null)
                    this._thumbnail.Dispose();
            }

            //base.Dispose(disposing);
        }
    }
}
