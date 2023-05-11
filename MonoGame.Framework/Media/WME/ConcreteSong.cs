// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.IO;
using Windows.Storage;
using Microsoft.Xna.Platform.Media;


namespace Microsoft.Xna.Framework.Media
{
    public sealed class ConcreteSongStrategy : SongStrategy
    {
        internal MusicProperties _musicProperties;

        [CLSCompliant(false)]
        public StorageFile StorageFile
        {
            get { return this._musicProperties.File; }
        }

        public ConcreteSongStrategy()
        {
        }

        public ConcreteSongStrategy(string name, Uri uri)
        {
            string filename = uri.OriginalString;
            this.Name = filename;
            this.PlatformInitialize(filename);
            this.Name = name;
        }

        public ConcreteSongStrategy(string filename)
        {
            this.Name = filename;
            this.PlatformInitialize(filename);
        }

        private void PlatformInitialize(string fileName)
        {

        }

        public override Album Album
        {
            get { return base.Album; }
        }

        public override Artist Artist
        {
            get { return base.Artist; }
        }

        public override Genre Genre
        {
            get { return base.Genre; }
        }

        public override TimeSpan Duration
        {
            get
            {
                if (this._musicProperties != null)
                    return this._musicProperties.Duration;

                return base.Duration;
            }
        }

        public override bool IsProtected
        {
            get
            {
                if (this._musicProperties != null)
                    return this._musicProperties.IsProtected;

                return base.IsProtected;
            }
        }

        public override bool IsRated
        {
            get
            {
                if (this._musicProperties != null)
                    return this._musicProperties.Rating != 0;

                return base.IsRated;
            }
        }

        public override string Name
        {
            get
            {
                if (this._musicProperties != null)
                    return this._musicProperties.Title;

                return Path.GetFileNameWithoutExtension(base.Name);
            }
        }

        public override int PlayCount
        {
            get { return base.PlayCount; }
        }

        public override int Rating
        {
            get
            {
                if (this._musicProperties != null)
                    return this._musicProperties.Rating;

                return base.Rating;
            }
        }

        public override int TrackNumber
        {
            get
            {
                if (this._musicProperties != null)
                    return this._musicProperties.TrackNumber;

                return base.TrackNumber;
            }
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

