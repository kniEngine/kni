// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Media;


namespace Microsoft.Xna.Platform.Media
{
    abstract public class SongStrategy // : IDisposable
    {
        internal SongStrategy() { }

        internal abstract void PlatformInitialize(string fileName);

        internal abstract Album PlatformGetAlbum();
        internal abstract void PlatformSetAlbum(Album album);
        internal abstract Artist PlatformGetArtist();
        internal abstract Genre PlatformGetGenre();

        internal abstract TimeSpan PlatformGetDuration();
        internal abstract bool PlatformIsProtected();
        internal abstract bool PlatformIsRated();
        
        internal abstract string PlatformGetName();
        internal abstract int PlatformGetPlayCount();
        internal abstract int PlatformGetRating();
        internal abstract int PlatformGetTrackNumber();
        
        internal abstract void PlatformDispose(bool disposing);

        #region IDisposable
        //~SongStrategy()
        //{
        //    Dispose(false);
        //}

        //public void Dispose()
        //{
        //    Dispose(true);
        //    GC.SuppressFinalize(this);
        //}

        //protected abstract void Dispose(bool disposing);
        #endregion
    }
}
