// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Platform.Media;


namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Song
    {
        internal Song(Album album, Artist artist, Genre genre, string name, TimeSpan duration, Android.Net.Uri assetUri)
        {
            _strategy = new ConcreteSongStrategy();

            _strategy.Album = album;
            _strategy.Artist = artist;
            _strategy.Genre = genre;
            ((ConcreteSongStrategy)_strategy)._name2 = name;
            ((ConcreteSongStrategy)_strategy)._duration2 = duration;
            ((ConcreteSongStrategy)_strategy)._assetUri = assetUri;
        }
    }
}

