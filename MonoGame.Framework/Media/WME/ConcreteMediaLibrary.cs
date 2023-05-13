﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using WinFileProperties = Windows.Storage.FileProperties;
using Microsoft.Xna.Framework.Media;

namespace Microsoft.Xna.Platform.Media
{
    internal class ConcreteMediaLibraryStrategy : MediaLibraryStrategy
    {
        private static AlbumCollection _albumCollection;
        private static SongCollection _songCollection;

        private const string CacheFile = "MediaLibrary.cache";

        private static StorageFolder _musicFolder;

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


        public ConcreteMediaLibraryStrategy()
            : base()
        {
        }

        public ConcreteMediaLibraryStrategy(MediaSource mediaSource)
            : base(mediaSource)
        {
            throw new NotSupportedException("Initializing from MediaSource is not supported");
        }

        public override void Load(Action<int> progressCallback = null)
        {
            Task.Run(async () =>
            {
                if (_musicFolder == null)
                {
                    try
                    {
                        _musicFolder = KnownFolders.MusicLibrary;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Failed to access Music Library: " + e.Message);
                        _albumCollection = new AlbumCollection(new List<Album>());
                        _songCollection = new SongCollection(new List<Song>());
                        return;
                    }
                }


                var files = new List<StorageFile>();
                await this.GetAllFiles(_musicFolder, files);

                var songList = new List<Song>();
                var albumList = new List<Album>();

                var artists = new Dictionary<string, Artist>();
                var albums = new Dictionary<string, Album>();
                var genres = new Dictionary<string, Genre>();

                var cache = new Dictionary<string, MusicProperties>();

                // Read cache
                var cacheFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(CacheFile, CreationCollisionOption.OpenIfExists);
                using (var baseStream = await cacheFile.OpenStreamForReadAsync())
                using (var stream = new BinaryReader(baseStream))
                    try
                    {
                        for (; baseStream.Position < baseStream.Length;)
                        {
                            var entry = MusicProperties.Deserialize(stream);
                            cache.Add(entry.Path, entry);
                        }
                    }
                    catch { }

                // Write cache
                cacheFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(CacheFile, CreationCollisionOption.ReplaceExisting);
                using (var stream = new BinaryWriter(await cacheFile.OpenStreamForWriteAsync()))
                {
                    int prevProgress = 0;

                    for (int i = 0; i < files.Count; i++)
                    {
                        var file = files[i];
                        try
                        {
                            MusicProperties properties;
                            if (!(cache.TryGetValue(file.Path, out properties) && properties.TryMatch(file)))
                                properties = new MusicProperties(file);
                            properties.Serialize(stream);

                            if (string.IsNullOrWhiteSpace(properties.Title))
                                continue;

                            Artist artist;
                            if (!artists.TryGetValue(properties.Artist, out artist))
                            {
                                artist = new Artist(properties.Artist);
                                artists.Add(artist.Name, artist);
                            }

                            Artist albumArtist;
                            if (!artists.TryGetValue(properties.AlbumArtist, out albumArtist))
                            {
                                albumArtist = new Artist(properties.AlbumArtist);
                                artists.Add(albumArtist.Name, albumArtist);
                            }

                            Genre genre;
                            if (!genres.TryGetValue(properties.Genre, out genre))
                            {
                                genre = new Genre(properties.Genre);
                                genres.Add(genre.Name, genre);
                            }

                            Album album;
                            if (!albums.TryGetValue(properties.Album, out album))
                            {
                                var thumbnail = Task.Run(async () => await properties.File.GetThumbnailAsync(WinFileProperties.ThumbnailMode.MusicView, 300, WinFileProperties.ThumbnailOptions.ResizeThumbnail)).Result;
                                AlbumStrategy albumStrategy = new ConcreteAlbumStrategy(properties.Album, albumArtist, genre, new SongCollection(), thumbnail.Type == WinFileProperties.ThumbnailType.Image ? thumbnail : null);
                                album = new Album(albumStrategy);
                                albums.Add(album.Name, album);
                                albumList.Add(album);
                            }

                            var songStrategy = new ConcreteSongStrategy();
                            songStrategy.Album = album;
                            songStrategy.Artist = artist;
                            songStrategy.Genre = genre;
                            songStrategy._musicProperties = properties;
                            Song song = new Song(songStrategy);

                            song.Album.Songs.Add(song);
                            songList.Add(song);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("MediaLibrary exception: " + e.Message);
                        }

                        int progress = 100 * i / files.Count;
                        if (progress > prevProgress)
                        {
                            prevProgress = progress;
                            if (progressCallback != null)
                                progressCallback.Invoke(progress);
                        }
                    }
                }

                if (progressCallback != null)
                    progressCallback.Invoke(100);

                _albumCollection = new AlbumCollection(albumList);
                _songCollection = new SongCollection(songList);
            }).Wait();
        }

        public override void SavePicture(string name, byte[] imageBuffer)
        {
            throw new NotImplementedException();
        }

        public override void SavePicture(string name, Stream source)
        {
            throw new NotImplementedException();
        }


        private async Task GetAllFiles(StorageFolder storageFolder, List<StorageFile> musicFiles)
        {
            foreach (var item in await storageFolder.GetItemsAsync())
                if (item is StorageFile)
                {
                    var file = item as StorageFile;
                    if (file.ContentType.StartsWith("audio") && !file.ContentType.EndsWith("url"))
                        musicFiles.Add(file);
                }
                else
                {
                    var folder = item as StorageFolder;
                    await this.GetAllFiles(folder, musicFiles);
                }
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
