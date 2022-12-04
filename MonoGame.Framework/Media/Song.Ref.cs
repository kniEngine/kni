// Copyright (C)2022 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Platform.Media;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Song : SongStrategy
    {
        internal override void PlatformInitialize(string fileName)
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

        internal override Album PlatformGetAlbum()
        {
            throw new PlatformNotSupportedException();
        }

        internal override void PlatformSetAlbum(Album album)
        {
            throw new PlatformNotSupportedException();
        }

        internal override Artist PlatformGetArtist()
        {
            throw new PlatformNotSupportedException();
        }

        internal override Genre PlatformGetGenre()
        {
            throw new PlatformNotSupportedException();
        }

        internal override TimeSpan PlatformGetDuration()
        {
            throw new PlatformNotSupportedException();
        }

        internal override bool PlatformIsProtected()
        {
            throw new PlatformNotSupportedException();
        }

        internal override bool PlatformIsRated()
        {
            throw new PlatformNotSupportedException();
        }

        internal override string PlatformGetName()
        {
            throw new PlatformNotSupportedException();
        }

        internal override int PlatformGetPlayCount()
        {
            throw new PlatformNotSupportedException();
        }

        internal override int PlatformGetRating()
        {
            throw new PlatformNotSupportedException();
        }

        internal override int PlatformGetTrackNumber()
        {
            throw new PlatformNotSupportedException();
        }

        internal override void PlatformDispose(bool disposing)
        {
            if (disposing)
            {
            }

        }

    }
}

