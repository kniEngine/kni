// Copyright (C)2022 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Media
{
    public partial class MediaLibrary
    {
        private void PlatformLoad(Action<int> progressCallback)
        {
            throw new PlatformNotSupportedException();
        }

        private AlbumCollection PlatformGetAlbums()
        {
            throw new PlatformNotSupportedException();
        }

        private SongCollection PlatformGetSongs()
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformDispose()
        {

        }
    }
}
