// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Media;


namespace Microsoft.Xna.Platform.Media
{
    public interface IPlatformAlbum
    {
        AlbumStrategy Strategy { get; }
    }

    abstract public class AlbumStrategy : IDisposable
    {
        abstract public string Name { get; }
        abstract public Artist Artist { get; }
        abstract public Genre Genre { get; }
        abstract public TimeSpan Duration { get; }
        abstract public bool HasArt { get; }
        abstract public SongCollection Songs { get; }


        public AlbumStrategy()
        {
        }

        abstract public Stream GetAlbumArt();
        abstract public Stream GetThumbnail();
        

        #region IDisposable
        ~AlbumStrategy()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        abstract protected void Dispose(bool disposing);
        #endregion
    }

}
