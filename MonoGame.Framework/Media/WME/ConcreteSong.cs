// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Windows.Storage;
using Microsoft.Xna.Framework.Media;


namespace Microsoft.Xna.Platform.Media
{
    internal sealed class ConcreteSongStrategy : SongStrategy
    {
        private Uri _streamSource;
        internal MusicProperties _musicProperties;

        internal Uri StreamSource { get { return _streamSource; } }

        [CLSCompliant(false)]
        public StorageFile StorageFile
        {
            get { return this._musicProperties.File; }
        }

        internal ConcreteSongStrategy()
            : base()
        {
        }

        internal ConcreteSongStrategy(string name, Uri streamSource)
            : base()
        {
            this.Name = name;
            this._streamSource = streamSource;
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

        public override string Filename
        {
            get
            {
                if (this._musicProperties != null)
                    return this._musicProperties.Title;

                return StreamSource.OriginalString;
            }
        }

        public override string Name
        {
            get
            {
                if (this._musicProperties != null)
                    return this._musicProperties.Title;

                return base.Name;
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

