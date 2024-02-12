// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;


namespace Microsoft.Xna.Framework.Media
{
	public class SongCollection : ICollection<Song>, IEnumerable<Song>, IEnumerable, IDisposable
	{
		private bool _isReadOnly = false;
		private List<Song> _innerList;


        internal SongCollection(List<Song> songs)
        {
            this._innerList = songs;
        }

		public void Dispose()
        {
        }
		
		public IEnumerator<Song> GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }
		
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        public int Count
        {
            get { return _innerList.Count; }
        }
		
		public bool IsReadOnly
        {
		    get { return this._isReadOnly; }
        }

        public Song this[int index]
        {
            get { return this._innerList[index]; }
        }
		
		public void Add(Song item)
        {

            if (item == null)
                throw new ArgumentNullException();

            if (_innerList.Count == 0)
            {
                this._innerList.Add(item);
                return;
            }

            for (int i = 0; i < this._innerList.Count; i++)
            {
                if (item.TrackNumber < this._innerList[i].TrackNumber)
                {
                    this._innerList.Insert(i, item);
                    return;
                }
            }

            this._innerList.Add(item);
        }
		
		public void Clear()
        {
            _innerList.Clear();
        }
        
        public SongCollection Clone()
        {
            List<Song> songs = new List<Song>();
            SongCollection sc = new SongCollection(songs);

            foreach (Song song in this._innerList)
                sc.Add(song);

            return sc;
        }
        
        public bool Contains(Song item)
        {
            return _innerList.Contains(item);
        }
        
        public void CopyTo(Song[] array, int arrayIndex)
        {
            _innerList.CopyTo(array, arrayIndex);
        }
		
		public int IndexOf(Song item)
        {
            return _innerList.IndexOf(item);
        }
        
        public bool Remove(Song item)
        {
            return _innerList.Remove(item);
        }
	}
}

