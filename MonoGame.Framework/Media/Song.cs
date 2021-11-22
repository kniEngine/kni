// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Platform.Media;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Song : IEquatable<Song>, IDisposable
    {
        internal SongStrategy _strategy;
        private string _name;
		private int _playCount = 0;
        private TimeSpan _duration = TimeSpan.Zero;
        bool _isDisposed;

        /// <summary>
        /// Gets the Album on which the Song appears.
        /// </summary>
        public Album Album
        {
            get { return _strategy.PlatformGetAlbum(); }
#if WINDOWS_UAP
            internal set { _strategy.PlatformSetAlbum(value); }
#endif
        }

        /// <summary>
        /// Gets the Artist of the Song.
        /// </summary>
        public Artist Artist
        {
            get { return _strategy.PlatformGetArtist(); }
        }

        /// <summary>
        /// Gets the Genre of the Song.
        /// </summary>
        public Genre Genre
        {
            get { return _strategy.PlatformGetGenre(); }
        }
        
        public bool IsDisposed
        {
            get { return _isDisposed; }
        }

#if ANDROID || OPENAL || WEB || IOS
        internal delegate void FinishedPlayingHandler(object sender, EventArgs args);
#if !DESKTOPGL
        event FinishedPlayingHandler DonePlaying;
#endif
#endif
        internal Song(string fileName, int durationMS)
            : this(fileName)
        {
            _strategy = this;
            _duration = TimeSpan.FromMilliseconds(durationMS);
        }

		internal Song(string fileName)
		{
            _strategy = this;
			_name = fileName;

            _strategy.PlatformInitialize(fileName);
        }

        ~Song()
        {
            Dispose(false);
        }

        internal string FilePath
		{
			get { return _name; }
		}

        /// <summary>
        /// Returns a song that can be played via <see cref="MediaPlayer"/>.
        /// </summary>
        /// <param name="name">The name for the song. See <see cref="Song.Name"/>.</param>
        /// <param name="uri">The path to the song file.</param>
        /// <returns></returns>
        public static Song FromUri(string name, Uri uri)
        {
            var song = new Song(uri.OriginalString);
            song._name = name;
            return song;
        }
		
		public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                _strategy.PlatformDispose(disposing);

                _isDisposed = true;
            }
        }

        public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

        public bool Equals(Song song)
        {
#if DIRECTX
            return song != null && song.FilePath == FilePath;
#else
			return ((object)song != null) && (Name == song.Name);
#endif
		}
		
		
		public override bool Equals(Object obj)
		{
			if(obj == null)
			{
				return false;
			}
			
			return Equals(obj as Song);  
		}
		
		public static bool operator ==(Song song1, Song song2)
		{
			if((object)song1 == null)
			{
				return (object)song2 == null;
			}

			return song1.Equals(song2);
		}
		
		public static bool operator !=(Song song1, Song song2)
		{
		  return ! (song1 == song2);
		}

        public TimeSpan Duration
        {
            get { return _strategy.PlatformGetDuration(); }
        }	

        public bool IsProtected
        {
            get { return _strategy.PlatformIsProtected(); }
        }

        public bool IsRated
        {
            get { return _strategy.PlatformIsRated(); }
        }

        public string Name
        {
            get { return _strategy.PlatformGetName(); }
        }

        public int PlayCount
        {
            get { return _strategy.PlatformGetPlayCount(); }
        }

        public int Rating
        {
            get { return _strategy.PlatformGetRating(); }
        }

        public int TrackNumber
        {
            get { return _strategy.PlatformGetTrackNumber(); }
        }
    }
}

