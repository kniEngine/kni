// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Framework.Media;
using CoreGraphics;
using MediaPlayer;
using UIKit;


namespace Microsoft.Xna.Platform.Media
{
    internal class ConcreteAlbumStrategy : AlbumStrategy
    {
        private string _name;
        private Artist _artist;
        private Genre _genre;
        private SongCollection _songs;

        private MPMediaItemArtwork _thumbnail;

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
            get
            {
                // If album art is missing the bounds will be: Infinity, Infinity, 0, 0
                return this._thumbnail != null && this._thumbnail.Bounds.Width != 0;
            }
        }

        public override SongCollection Songs
        {
            get { return this._songs; }
        }


        internal ConcreteAlbumStrategy(string name, Artist artist, Genre genre, SongCollection songCollection,
                                     MPMediaItemArtwork thumbnail)
        {
            this._name = name;
            this._artist = artist;
            this._genre = genre;
            this._songs = songCollection;

            this._thumbnail = thumbnail;
        }


        public override Stream GetAlbumArt()
        {
            throw new NotImplementedException();
        }

        public override Stream GetThumbnail()
        {
            throw new NotImplementedException();
        }


        [CLSCompliant(false)]
        public UIImage Platform_GetAlbumArt(int width = 0, int height = 0)
        {
            if (width == 0)
                width = (int)this._thumbnail.Bounds.Width;
            if (height == 0)
                height = (int)this._thumbnail.Bounds.Height;

            return this._thumbnail.ImageWithSize(new CGSize(width, height));
        }

        [CLSCompliant(false)]
        public UIImage Platform_GetThumbnail()
        {
            return this.Platform_GetAlbumArt(220, 220);
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
