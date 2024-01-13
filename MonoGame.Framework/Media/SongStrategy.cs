// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Media;


namespace Microsoft.Xna.Platform.Media
{
    public interface IPlatformSong
    {
        SongStrategy Strategy { get; }
    }

    abstract public class SongStrategy : IDisposable
    {
        private string _name;
        private TimeSpan _duration = TimeSpan.Zero;
        private int _playCount = 0;

        private Album _album;
        private Artist _artist;
        private Genre _genre;
        private bool _isProtected;
        private bool _isRated;
        private int _rating;
        private int _trackNumber;

        abstract internal string Filename { get; }

        public virtual string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public virtual TimeSpan Duration
        {
            get { return _duration; }
            set { _duration = value; }
        }

        public virtual int PlayCount
        {
            get { return _playCount; }
            set { _playCount = value; }
        }

        public virtual Album Album
        {
            get { return _album; }
            set { _album = value; }
        }

        public virtual Genre Genre
        {
            get { return _genre; }
            set { _genre = value; }
        }

        public virtual Artist Artist
        {
            get { return _artist; }
            set { _artist = value; }
        }

        public virtual bool IsProtected
        {
            get { return _isProtected; }
            set { _isProtected = value; }
        }

        public virtual bool IsRated
        {
            get { return _isRated; }
            set { _isRated = value; }
        }

        public virtual int Rating
        {
            get { return _rating; }
            set { _rating = value; }
        }

        public virtual int TrackNumber
        {
            get { return _trackNumber; }
            set { _trackNumber = value; }
        }

        internal SongStrategy()
        {
        }

        public T ToConcrete<T>() where T : SongStrategy
        {
            return (T)this;
        }


        #region IDisposable
        ~SongStrategy()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);
        #endregion
    }
}
