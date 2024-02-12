// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;


namespace Microsoft.Xna.Framework.Media
{
    public sealed class AlbumCollection : IDisposable
    {
        private List<Album> _innerList;

        /// <summary>
        /// Gets a value indicating whether the object is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the number of Album objects in the AlbumCollection.
        /// </summary>
        public int Count
        {
            get { return this._innerList.Count; }
        }


        internal AlbumCollection(List<Album> albums)
        {
            this._innerList = albums;
        }

        /// <summary>
        /// Gets the Album at the specified index in the AlbumCollection.
        /// </summary>
        /// <param name="index">Index of the Album to get.</param>
        public Album this[int index]
        {
            get { return this._innerList[index]; }
        }

        /// <summary>
        /// Immediately releases the unmanaged resources used by this object.
        /// </summary>
        public void Dispose()
        {
            foreach (Album album in this._innerList)
                album.Dispose();
        }
    }
}
