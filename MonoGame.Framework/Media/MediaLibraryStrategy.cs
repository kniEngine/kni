// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Media;


namespace Microsoft.Xna.Platform.Media
{
    public interface IPlatformMediaLibrary
    {
        MediaLibraryStrategy Strategy { get; }
    }

    abstract public class MediaLibraryStrategy : IDisposable
    {
        public virtual MediaSource MediaSource { get; private set; }
        abstract public AlbumCollection Albums { get; }
        abstract public SongCollection Songs { get; }
        abstract public PlaylistCollection Playlists  { get; }
        //abstract public ArtistCollection Artists  { get; }
        //abstract public GenreCollection Genres  { get; }


        public MediaLibraryStrategy()
        {
            MediaSource = null;
        }

        public MediaLibraryStrategy(MediaSource mediaSource)
        {
            MediaSource = mediaSource;
        }

        abstract public void Load(Action<int> progressCallback = null);
        abstract public void SavePicture(string name, byte[] imageBuffer);
        abstract public void SavePicture(string name, Stream source);

        #region IDisposable
        ~MediaLibraryStrategy()
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
