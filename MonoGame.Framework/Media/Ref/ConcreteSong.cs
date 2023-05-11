// Copyright (C)2022 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Platform.Media;


namespace Microsoft.Xna.Framework.Media
{
    public sealed class ConcreteSongStrategy : SongStrategy
    {

        public ConcreteSongStrategy(string name, Uri streamSource)
        {
            throw new PlatformNotSupportedException();
        }

		internal float Volume
		{
			get { throw new PlatformNotSupportedException(); }
			set { throw new PlatformNotSupportedException(); }			
		}

        internal TimeSpan Position
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public override Album Album
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

        public override bool IsProtected
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public override bool IsRated
        {
            get { throw new PlatformNotSupportedException(); }
        }

        internal override string Filename
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public override string Name
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public override int PlayCount
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public override int Rating
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public override int TrackNumber
        {
            get { throw new PlatformNotSupportedException(); }
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

