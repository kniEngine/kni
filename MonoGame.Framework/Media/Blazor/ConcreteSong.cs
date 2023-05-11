// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Platform.Media;


namespace Microsoft.Xna.Framework.Media
{
    public sealed class ConcreteSongStrategy : SongStrategy
    {
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

        protected void PlatformInitialize(string fileName)
        {

        }

		internal float Volume
		{
			get
			{
                throw new NotImplementedException();
            }
			
			set
			{
                throw new NotImplementedException();
            }			
		}

        internal TimeSpan Position
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override Album Album
        {
            get { throw new NotImplementedException(); }
        }

        public override Artist Artist
        {
            get { throw new NotImplementedException(); }
        }

        public override Genre Genre
        {
            get { throw new NotImplementedException(); }
        }

        public override TimeSpan Duration
        {
            get { throw new NotImplementedException(); }
        }

        public override bool IsProtected
        {
            get { throw new NotImplementedException(); }
        }

        public override bool IsRated
        {
            get { throw new NotImplementedException(); }
        }

        public override string Name
        {
            get { throw new NotImplementedException(); }
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
            }

            //base.Dispose(disposing);
        }
    }
}

