// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Platform.Media;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Song : SongStrategy
    {
        internal override void PlatformInitialize(string fileName)
        {

        }

        internal override void PlatformDispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        private void PlatformPlay()
        {
            throw new NotImplementedException();
        }

		internal void Resume()
		{
            throw new NotImplementedException();
        }

		
		internal void Pause()
		{
            throw new NotImplementedException();
        }
		
		internal void Stop()
		{
            throw new NotImplementedException();
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

        internal override Album PlatformGetAlbum()
        {
            throw new NotImplementedException();
        }

        internal override void PlatformSetAlbum(Album album)
        {
            throw new NotImplementedException();
        }

        internal override Artist PlatformGetArtist()
        {
            throw new NotImplementedException();
        }

        internal override Genre PlatformGetGenre()
        {
            throw new NotImplementedException();
        }

        internal override TimeSpan PlatformGetDuration()
        {
            throw new NotImplementedException();
        }

        internal override bool PlatformIsProtected()
        {
            throw new NotImplementedException();
        }

        internal override bool PlatformIsRated()
        {
            throw new NotImplementedException();
        }

        internal override string PlatformGetName()
        {
            throw new NotImplementedException();
        }

        internal override int PlatformGetPlayCount()
        {
            return _playCount;
        }

        internal override int PlatformGetRating()
        {
            return 0;
        }

        internal override int PlatformGetTrackNumber()
        {
            return 0;
        }
    }
}

