// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Media;
using Foundation;
using MediaPlayer;

namespace Microsoft.Xna.Platform.Media
{
    internal sealed class ConcreteSongStrategy : SongStrategy
    {
        internal MediaPlatformStream _mediaPlatformStream;

        private Uri _streamSource;

        #if !TVOS
        internal MPMediaItem _mediaItem;
        #endif
        internal NSUrl _assetUrl;


        internal Uri StreamSource { get { return _streamSource; } }

        [CLSCompliant(false)]
        public NSUrl AssetUrl { get { return this._assetUrl; } }

        internal ConcreteSongStrategy()
            : base()
        {
        }

        internal ConcreteSongStrategy(string name, Uri streamSource)
            : base()
        {
            this.Name = name;
            this._streamSource = streamSource;

            this._mediaPlatformStream = new MediaPlatformStream(this._streamSource);

        }

        internal MediaPlatformStream GetMediaPlatformStream()
        {
            return _mediaPlatformStream;
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

        internal override string Filename
        {
            get
            {
                if (this.StreamSource == null)
                    return this.Name;

                return StreamSource.OriginalString;
            }
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

