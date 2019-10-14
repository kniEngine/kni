// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

extern alias MicrosoftXnaFramework;
using System;
using System.IO;
using Microsoft.Xna.Platform.Media;
using MsSong = MicrosoftXnaFramework::Microsoft.Xna.Framework.Media.Song;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Song : SongStrategy
    {
        internal MsSong InternalSong { get; private set; }

        internal Song(MsSong song)
        {
            _strategy = this;
            this.InternalSong = song;
        }

        internal override void PlatformInitialize(string fileName)
        {

        }

        internal override void PlatformDispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        internal override Album PlatformGetAlbum()
        {
            if (this.InternalSong != null)
                return (Album)this.InternalSong.Album;

            return null;
        }

        internal override void PlatformSetAlbum(Album album)
        {
            
        }

        internal override Artist PlatformGetArtist()
        {
            if (this.InternalSong != null)
                return this.InternalSong.Artist;

            return null;
        }

        internal override Genre PlatformGetGenre()
        {
            if (this.InternalSong != null)
                return this.InternalSong.Genre;

            return null;
        }

        internal override TimeSpan PlatformGetDuration()
        {
            if (this.InternalSong != null)
                return this.InternalSong.Duration;

            return _duration;
        }

        internal override bool PlatformIsProtected()
        {
            if (this.InternalSong != null)
                return this.InternalSong.IsProtected;

            return false;
        }

        internal override bool PlatformIsRated()
        {
            if (this.InternalSong != null)
                return this.InternalSong.IsRated;

            return false;
        }

        internal override string PlatformGetName()
        {
            if (this.InternalSong != null)
                return this.InternalSong.Name;

            return Path.GetFileNameWithoutExtension(_name);
        }

        internal override int PlatformGetPlayCount()
        {
            if (this.InternalSong != null)
                return this.InternalSong.PlayCount;

            return _playCount;
        }

        internal override int PlatformGetRating()
        {
            if (this.InternalSong != null)
                return this.InternalSong.Rating;

            return 0;
        }

        internal override int PlatformGetTrackNumber()
        {
            if (this.InternalSong != null)
                return this.InternalSong.TrackNumber;

            return 0;
        }
    }
}

