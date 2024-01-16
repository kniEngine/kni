// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content
{
    internal class ContentBufferPool : IComparer<byte[]>
    {
        public const int MinimumBufferSize = 2 * 1024 * 1024;

        private static readonly ContentBufferPool _current = new ContentBufferPool();

        internal static ContentBufferPool Current
        {
            get { return _current; }
        }

        private SortedSet<byte[]> _bufferSet;

        private ContentBufferPool()
        {
            _bufferSet = new SortedSet<byte[]>(this);
        }

        public byte[] Get(int size)
        {
            lock (_bufferSet)
            {
                foreach (byte[] buffer in _bufferSet)
                {
                    if (buffer.Length >= size)
                    {
                        _bufferSet.Remove(buffer);
                        return buffer;
                    }
                }

                if (_bufferSet.Count >= 1)
                    _bufferSet.Remove(_bufferSet.Max);

                int dataSize = Math.Max(MinimumBufferSize, size);
                return new byte[dataSize];
            }
        }

        public void Return(byte[] buffer)
        {
            lock (_bufferSet)
            {
                _bufferSet.Add(buffer);
            }
        }

        int IComparer<byte[]>.Compare(byte[] x, byte[] y)
        {
            return (x.Length - y.Length);
        }
    }
}
