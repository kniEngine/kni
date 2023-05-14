// Copyright (C)2023 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Framework.Media;


namespace Microsoft.Xna.Platform.Media
{
    internal class ConcreteAlbumStrategy : AlbumStrategy
    {

        public override string Name
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public override Artist Artist
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public override Genre Genre
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public override TimeSpan Duration
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public override bool HasArt
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public override SongCollection Songs
        {
            get { throw new PlatformNotSupportedException(); }
        }


        internal ConcreteAlbumStrategy(string name, Artist artist, Genre genre, SongCollection songCollection)
        {
            throw new PlatformNotSupportedException();
        }


        public override Stream GetAlbumArt()
        {
            throw new PlatformNotSupportedException();
        }

        public override Stream GetThumbnail()
        {
            throw new PlatformNotSupportedException();
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
