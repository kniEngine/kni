﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework.Media;
using Android.Content;
using Android.Provider;
using Uri = Android.Net.Uri;

namespace Microsoft.Xna.Platform.Media
{
    internal class ConcreteMediaLibraryStrategy : MediaLibraryStrategy
    {
        private static AlbumCollection _albumCollection;
        private static SongCollection _songCollection;

        private static readonly TimeSpan MinimumSongDuration = TimeSpan.FromSeconds(3);
        internal static Context Context { get; set; }


        public override MediaSource MediaSource
        {
            get { return base.MediaSource; }
        }

        public override AlbumCollection Albums
        {
            get { return _albumCollection; }
        }

        public override SongCollection Songs
        {
            get { return _songCollection; }
        }

        public override PlaylistCollection Playlists
        {
            get { throw new NotImplementedException(); }
        }

        //public override ArtistCollection Artists
        //{
        //    get { return base.Artists; }
        //}

        //public override GenreCollection Genres
        //{
        //    get { return base.Genres; }
        //}


        internal ConcreteMediaLibraryStrategy()
            : base()
        {
        }

        internal ConcreteMediaLibraryStrategy(MediaSource mediaSource)
            : base(mediaSource)
        {
            throw new NotSupportedException("Initializing from MediaSource is not supported");
        }

        public override void Load(Action<int> progressCallback = null)
        {


            List<Song> songList = new List<Song>();
            List<Album> albumList = new List<Album>();

            using (Android.Database.ICursor musicCursor = Context.ContentResolver.Query(MediaStore.Audio.Media.ExternalContentUri, null, null, null, null))
            {
                if (musicCursor != null)
                {
                    Dictionary<string, Artist> artists = new Dictionary<string, Artist>();
                    Dictionary<string, Album> albums = new Dictionary<string, Album>();
                    Dictionary<string, Genre> genres = new Dictionary<string, Genre>();

                    // Note: Grabbing album art using MediaStore.Audio.AlbumColumns.AlbumArt and
                    // MediaStore.Audio.AudioColumns.AlbumArt is broken
                    // See: https://code.google.com/p/android/issues/detail?id=1630
                    // Workaround: http://stackoverflow.com/questions/1954434/cover-art-on-android

                    int albumNameColumn = musicCursor.GetColumnIndex(MediaStore.Audio.AlbumColumns.Album);
                    int albumArtistColumn = musicCursor.GetColumnIndex(MediaStore.Audio.AlbumColumns.Artist);
                    int albumIdColumn = musicCursor.GetColumnIndex(MediaStore.Audio.AlbumColumns.AlbumId);
                    int genreColumn = musicCursor.GetColumnIndex(MediaStore.Audio.GenresColumns.Name); // Also broken :(

                    int artistColumn = musicCursor.GetColumnIndex(MediaStore.Audio.AudioColumns.Artist);
                    int titleColumn = musicCursor.GetColumnIndex(MediaStore.Audio.AudioColumns.Title);
                    int durationColumn = musicCursor.GetColumnIndex(MediaStore.Audio.AudioColumns.Duration);
                    int assetIdColumn = musicCursor.GetColumnIndex(MediaStore.Audio.AudioColumns.Id);

                    if (titleColumn == -1 || durationColumn == -1 || assetIdColumn == -1)
                    {
                        Debug.WriteLine("Missing essential properties from music library. Returning empty library.");
                        _albumCollection = base.CreateAlbumCollection(albumList);
                        _songCollection = base.CreateSongCollection(songList);
                        return;
                    }

                    for (musicCursor.MoveToFirst(); !musicCursor.IsAfterLast; musicCursor.MoveToNext())
                        try
                        {
                            long durationProperty = musicCursor.GetLong(durationColumn);
                            TimeSpan duration = TimeSpan.FromMilliseconds(durationProperty);

                            // Exclude sound effects
                            if (duration < MinimumSongDuration)
                                continue;

                            string albumNameProperty = (albumNameColumn > -1 ? musicCursor.GetString(albumNameColumn) : null) ?? "Unknown Album";
                            string albumArtistProperty = (albumArtistColumn > -1 ? musicCursor.GetString(albumArtistColumn) : null) ?? "Unknown Artist";
                            string genreProperty = (genreColumn > -1 ? musicCursor.GetString(genreColumn) : null) ?? "Unknown Genre";
                            string artistProperty = (artistColumn > -1 ? musicCursor.GetString(artistColumn) : null) ?? "Unknown Artist";
                            string titleProperty = musicCursor.GetString(titleColumn);

                            long assetId = musicCursor.GetLong(assetIdColumn);
                            Uri assetUri = ContentUris.WithAppendedId(MediaStore.Audio.Media.ExternalContentUri, assetId);
                            long albumId = albumIdColumn > -1 ? musicCursor.GetInt(albumIdColumn) : -1;
                            Uri albumArtUri = albumId > -1 ? ContentUris.WithAppendedId(Uri.Parse("content://media/external/audio/albumart"), albumId) : null;

                            Artist artist;
                            if (!artists.TryGetValue(artistProperty, out artist))
                            {
                                artist = new Artist(artistProperty);
                                artists.Add(artist.Name, artist);
                            }

                            Artist albumArtist;
                            if (!artists.TryGetValue(albumArtistProperty, out albumArtist))
                            {
                                albumArtist = new Artist(albumArtistProperty);
                                artists.Add(albumArtist.Name, albumArtist);
                            }

                            Genre genre;
                            if (!genres.TryGetValue(genreProperty, out genre))
                            {
                                genre = new Genre(genreProperty);
                                genres.Add(genre.Name, genre);
                            }

                            Album album;
                            if (!albums.TryGetValue(albumNameProperty, out album))
                            {
                                AlbumStrategy albumStrategy = new ConcreteAlbumStrategy(albumNameProperty, albumArtist, genre, base.CreateSongCollection(new List<Song>()), albumArtUri);
                                album = base.CreateAlbum(albumStrategy);
                                albums.Add(album.Name, album);
                                albumList.Add(album);
                            }

                            ConcreteSongStrategy songStrategy = new ConcreteSongStrategy();
                            songStrategy.Album = album;
                            songStrategy.Artist = artist;
                            songStrategy.Genre = genre;
                            songStrategy.Name = titleProperty;
                            songStrategy.Duration = duration;
                            songStrategy._assetUri = assetUri;
                            Song song = base.CreateSong(songStrategy);

                            song.Album.Songs.Add(song);
                            songList.Add(song);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("MediaLibrary exception: " + e.Message);
                        }
                }

                musicCursor.Close();
            }

            _albumCollection = base.CreateAlbumCollection(albumList);
            _songCollection = base.CreateSongCollection(songList);
        }

        public override void SavePicture(string name, byte[] imageBuffer)
        {
            throw new NotImplementedException();
        }

        public override void SavePicture(string name, Stream source)
        {
            throw new NotImplementedException();
        }

        

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }

            //base.Dispose(disposing);
        }
    }
}
