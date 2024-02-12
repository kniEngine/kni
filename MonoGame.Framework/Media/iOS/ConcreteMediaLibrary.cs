// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Media;
using Foundation;
using MediaPlayer;


namespace Microsoft.Xna.Platform.Media
{
    internal class ConcreteMediaLibraryStrategy : MediaLibraryStrategy
    {
        private static AlbumCollection _albumCollection;
        private static SongCollection _songCollection;

        //private static readonly NSString MPMediaPlaylistPropertyName = new NSString(MPMediaPlaylistProperty.Name);


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

            foreach (MPMediaItemCollection collection in MPMediaQuery.AlbumsQuery.Collections)
            {
                NSObject nsAlbumArtist = collection.RepresentativeItem.ValueForProperty(MPMediaItem.AlbumArtistProperty);
                NSObject nsAlbumName = collection.RepresentativeItem.ValueForProperty(MPMediaItem.AlbumTitleProperty);
                NSObject nsAlbumGenre = collection.RepresentativeItem.ValueForProperty(MPMediaItem.GenreProperty);
                string albumArtist = nsAlbumArtist == null ? "Unknown Artist" : nsAlbumArtist.ToString();
                string albumName = nsAlbumName == null ? "Unknown Album" : nsAlbumName.ToString();
                string albumGenre = nsAlbumGenre == null ? "Unknown Genre" : nsAlbumGenre.ToString();
                MPMediaItemArtwork thumbnail = collection.RepresentativeItem.ValueForProperty(MPMediaItem.ArtworkProperty) as MPMediaItemArtwork;

                List<Song> albumSongs = new List<Song>((int)collection.Count);
                AlbumStrategy albumStrategy = new ConcreteAlbumStrategy(albumName, new Artist(albumArtist), new Genre(albumGenre), base.CreateSongCollection(albumSongs), thumbnail);
                Album album = base.CreateAlbum(albumStrategy);
                albumList.Add(album);

                foreach (MPMediaItem item in collection.Items)
                {
                    NSObject nsArtist = item.ValueForProperty(MPMediaItem.ArtistProperty);
                    NSObject nsTitle = item.ValueForProperty(MPMediaItem.TitleProperty);
                    NSObject nsGenre = item.ValueForProperty(MPMediaItem.GenreProperty);
                    NSUrl assetUrl = item.ValueForProperty(MPMediaItem.AssetURLProperty) as NSUrl;

                    if (nsTitle == null || assetUrl == null) // The Asset URL check will exclude iTunes match items from the Media Library that are not downloaded, but show up in the music app
                        continue;

                    string artist = nsArtist == null ? "Unknown Artist" : nsArtist.ToString();
                    string title = nsTitle.ToString();
                    string genre = nsGenre == null ? "Unknown Genre" : nsGenre.ToString();
                    TimeSpan duration = TimeSpan.FromSeconds(((NSNumber)item.ValueForProperty(MPMediaItem.PlaybackDurationProperty)).FloatValue);

                    ConcreteSongStrategy songStrategy = new ConcreteSongStrategy();
                    songStrategy.Album = album;
                    songStrategy.Artist = new Artist(artist);
                    songStrategy.Genre = new Genre(genre);
                    songStrategy.Name = title;
                    songStrategy.Duration = duration;
#if TVOS
                    ((ConcreteSongStrategy)songStrategy)._assetUrl = assetUrl;
#endif
                    ((ConcreteSongStrategy)songStrategy)._mediaItem = item;
                    Song song = base.CreateSong(songStrategy);

                    albumSongs.Add(song);
                    songList.Add(song);
                }
            }

            _albumCollection = base.CreateAlbumCollection(albumList);
            _songCollection = base.CreateSongCollection(songList);

            /*_playLists = new PlaylistCollection();
                    
            MPMediaQuery playlists = new MPMediaQuery();
            playlists.GroupingType = MPMediaGrouping.Playlist;
            for (int i = 0; i < playlists.Collections.Length; i++)
            {
                MPMediaItemCollection item = playlists.Collections[i];
                Playlist list = new Playlist();
                list.Name = playlists.Items[i].ValueForProperty(MPMediaPlaylistPropertyName).ToString();
                for (int k = 0; k < item.Items.Length; k++)
                {
                    TimeSpan time = TimeSpan.Parse(item.Items[k].ValueForProperty(MPMediaItem.PlaybackDurationProperty).ToString());
                    list.Duration += time;
                }
                _playLists.Add(list);
            }*/
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
