// Copyright (C)2023 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Framework.Media;


namespace Microsoft.Xna.Platform.Media
{
    internal class ConcreteAlbumStrategy : AlbumStrategy
    {
        private string _name;
        private Artist _artist;
        private Genre _genre;
        private SongCollection _songs;


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
            get { throw new NotImplementedException(); }
        }

        public override SongCollection Songs
        {
            get { return this._songs; }
        }


        internal ConcreteAlbumStrategy(string name, Artist artist, Genre genre, SongCollection songCollection)
        {
            this._name = name;
            this._artist = artist;
            this._genre = genre;
            this._songs = songCollection;
        }


        public override Stream GetAlbumArt()
        {
            throw new NotImplementedException();
        }

        public override Stream GetThumbnail()
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }

            //base.Dispose(disposing);
        }
    }
}
