﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Media;

namespace Microsoft.Xna.Platform.Media
{
    internal class ConcreteMediaLibraryStrategy : MediaLibraryStrategy
    {
        public override MediaSource MediaSource
        {
            get { return base.MediaSource; }
        }

        public override AlbumCollection Albums
        {
            get { return null; }
        }

        public override SongCollection Songs
        {
            get { return null; }
        }

        public override PlaylistCollection Playlists
        {
            get { return null; }
        }

        //public override ArtistCollection Artists
        //{
        //    get { return base.Artists; }
        //}

        //public override GenreCollection Genres
        //{
        //    get { return base.Genres; }
        //}


        public ConcreteMediaLibraryStrategy()
            : base()
        {
        }

        public ConcreteMediaLibraryStrategy(MediaSource mediaSource)
            : base(mediaSource)
        {
            throw new NotSupportedException("Initializing from MediaSource is not supported");
        }

        public override void Load(Action<int> progressCallback = null)
        {
            
        }

        public override void SavePicture(string name, byte[] imageBuffer)
        {
            throw new NotImplementedException();
        }

        public override void SavePicture(string name, Stream source)
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
