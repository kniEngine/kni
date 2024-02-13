// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;


namespace Microsoft.Xna.Framework.Media
{

    public sealed class PlaylistCollection : ICollection<Playlist>, IEnumerable<Playlist>, IEnumerable, IDisposable
    {
        private bool _isReadOnly = false;
        private List<Playlist> _innerList = new List<Playlist>();
        
        public void Dispose()
        {
        }

        public IEnumerator<Playlist> GetEnumerator()
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

        public Playlist this[int index]
        {
            get { return this._innerList[index]; }
        }
        
        public void Add(Playlist item)
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
                if (item.Duration < this._innerList[i].Duration)
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
        
        public PlaylistCollection Clone()
        {
            PlaylistCollection plc = new PlaylistCollection();
            foreach (Playlist playlist in this._innerList)
                plc.Add(playlist);
            return plc;
        }
        
        public bool Contains(Playlist item)
        {
            return _innerList.Contains(item);
        }
        
        public void CopyTo(Playlist[] array, int arrayIndex)
        {
            _innerList.CopyTo(array, arrayIndex);
        }
        
        public int IndexOf(Playlist item)
        {
            return _innerList.IndexOf(item);
        }
        
        public bool Remove(Playlist item)
        {
            return _innerList.Remove(item);
        }
    }
}

