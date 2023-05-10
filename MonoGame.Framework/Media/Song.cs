// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Platform.Media;


namespace Microsoft.Xna.Framework.Media
{
    public sealed class Song : IEquatable<Song>, IDisposable
    {
        private SongStrategy _strategy;
        bool _isDisposed;

        public SongStrategy Strategy { get { return _strategy; } }

        public bool IsDisposed { get { return _isDisposed; } }

        /// <summary>
        /// Gets the Album on which the Song appears.
        /// </summary>
        public Album Album { get { return _strategy.Album; } }

        /// <summary>
        /// Gets the Artist of the Song.
        /// </summary>
        public Artist Artist { get { return _strategy.Artist; } }

        /// <summary>
        /// Gets the Genre of the Song.
        /// </summary>
        public Genre Genre { get { return _strategy.Genre; } }
		
        public TimeSpan Duration { get { return _strategy.Duration; } }

        public bool IsProtected { get { return _strategy.IsProtected; } }

        public bool IsRated { get { return _strategy.IsRated; } }

        public string Name { get { return _strategy.Name; } }

        public int PlayCount { get { return _strategy.PlayCount; } }

        public int Rating { get { return _strategy.Rating; } }

        public int TrackNumber { get { return _strategy.TrackNumber; } }

        internal Song(string filename, int durationMS)
        {
            _strategy = new ConcreteSongStrategy();
			_strategy.Name = filename;
            _strategy.PlatformInitialize(filename);

            _strategy.Duration = TimeSpan.FromMilliseconds(durationMS);
        }

		internal Song(string name, Uri uri)
		{
            string filename = uri.OriginalString;

            _strategy = new ConcreteSongStrategy();
			_strategy.Name = filename;
            _strategy.PlatformInitialize(filename);

            _strategy.Name = name;
        }

        internal Song(SongStrategy strategy)
        {
            _strategy = strategy;
        }

        ~Song()
        {
            Dispose(false);
        }


        /// <summary>
        /// Returns a song that can be played via <see cref="MediaPlayer"/>.
        /// </summary>
        /// <param name="name">The name for the song. See <see cref="Song.Name"/>.</param>
        /// <param name="uri">The path to the song file.</param>
        /// <returns></returns>
        public static Song FromUri(string name, Uri uri)
        {
            Song song = new Song(name, uri);
            return song;
        }


        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool Equals(Song other)
        {
#if DIRECTX
            return (other != null && _strategy.Name == other._strategy.Name);
#else
            return ((object)other != null) && (Name == other.Name);
#endif
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            return Equals(obj as Song);
        }

        public static bool operator ==(Song left, Song right)
        {
            if ((object)left == null)
            {
                return (object)right == null;
            }

            return left.Equals(right);
        }

        public static bool operator !=(Song left, Song right)
        {
            return !(left == right);
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
                if (disposing)
                    _strategy.Dispose();

                _isDisposed = true;
            }
        }
    }
}

