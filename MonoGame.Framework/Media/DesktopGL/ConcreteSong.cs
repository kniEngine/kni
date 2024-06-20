// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Media;


namespace Microsoft.Xna.Platform.Media
{
    internal sealed class ConcreteSongStrategy : SongStrategy
    {
        internal MediaPlatformStream _mediaPlatformStream;

        private Uri _streamSource;


        internal Uri StreamSource { get { return _streamSource; } }


        public override Album Album
        {
            get { return null; }
        }

        public override Artist Artist
        {
            get { return null; }
        }

        public override Genre Genre
        {
            get { return null; }
        }

        public override TimeSpan Duration
        {
            get { return base.Duration; }
        }

        public override bool IsProtected
        {
            get { return base.IsProtected; }
        }

        public override bool IsRated
        {
            get { return base.IsRated; }
        }

        public override string Filename
        {
            get { return StreamSource.OriginalString; }
        }

        public override string Name
        {
            get { return base.Name; }
        }

        public override int PlayCount
        {
            get { return base.PlayCount; }
        }

        public override int Rating
        {
            get { return base.Rating; }
        }

        public override int TrackNumber
        {
            get { return base.TrackNumber; }
        }

        internal ConcreteSongStrategy(string name, Uri streamSource)
            : base()
        {
            this.Name = name;
            this._streamSource = streamSource;

            this._mediaPlatformStream = new MediaPlatformStream();
        }

        internal MediaPlatformStream GetMediaPlatformStream()
        {
            return _mediaPlatformStream;
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_mediaPlatformStream != null)
                {
                    _mediaPlatformStream.Dispose();
                    _mediaPlatformStream = null;
                }

            }

            //base.Dispose(disposing);
        }
    }
}

