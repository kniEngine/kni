// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;


namespace Microsoft.Xna.Framework.Media
{
    public sealed class MediaQueue
    {
        private List<Song> _songs = new List<Song>();
        private int _activeSongIndex = -1;
        private Random _random = new Random();


        public MediaQueue()
        {
            
        }
        
        public Song ActiveSong
        {
            get
            {
                if (_songs.Count == 0 || _activeSongIndex < 0)
                    return null;
                
                return _songs[_activeSongIndex];
            }
        }
        
        public int ActiveSongIndex
        {
            get { return _activeSongIndex; }
            set { _activeSongIndex = value; }
        }

        public int Count
        {
            get { return _songs.Count; }
        }

        public Song this[int index]
        {
            get { return _songs[index]; }
        }

        internal void Add(Song song)
        {
            _songs.Add(song);
        }

        internal void Remove(Song song)
        {
            _songs.Remove(song);
        }

        internal Song GetNextSong(int direction, bool shuffle)
        {
            if (shuffle)
                _activeSongIndex = _random.Next(_songs.Count);
            else			
                _activeSongIndex = (int)MathHelper.Clamp(_activeSongIndex + direction, 0, _songs.Count - 1);
            
            return _songs[_activeSongIndex];
        }

    }
}

