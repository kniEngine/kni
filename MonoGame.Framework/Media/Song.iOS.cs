// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.IO;
using Foundation;
using AVFoundation;
using MediaPlayer;
using CoreMedia;
using Microsoft.Xna.Platform.Media;


namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Song
    {

        #if TVOS
        internal Song(Album album, Artist artist, Genre genre, string title, TimeSpan duration, NSUrl assetUrl, object mediaItem)
        #else
        internal Song(Album album, Artist artist, Genre genre, string title, TimeSpan duration, NSUrl assetUrl, MPMediaItem mediaItem)
        #endif
        {
            _strategy = new ConcreteSongStrategy();

            _strategy.Album = album;
            _strategy.Artist = artist;
            _strategy.Genre = genre;
            ((ConcreteSongStrategy)_strategy)._name2 = title;
            ((ConcreteSongStrategy)_strategy)._duration2 = duration;

            #if TVOS
            ((ConcreteSongStrategy)_strategy)._assetUrl = assetUrl;
            #endif
            ((ConcreteSongStrategy)_strategy)._mediaItem = mediaItem;
        }
    }
}

