// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.IO;
using Windows.Storage;
using Microsoft.Xna.Platform.Media;


namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Song
    {
		internal Song(Album album, Artist artist, Genre genre, MusicProperties musicProperties)
		{
            _strategy = new ConcreteSongStrategy();

            _strategy.Album = album;
            _strategy.Artist = artist;
            _strategy.Genre = genre;
            ((ConcreteSongStrategy)_strategy)._musicProperties = musicProperties;
        }
    }
}

