// Copyright (C)2025 Nick Kastellanos

using System;
using System.Collections.Generic;

namespace Content.Pipeline.Editor
{
    internal class PathComparer : IComparer<string>
    {
        private readonly StringComparer _stringComparer = StringComparer.Ordinal;

        public int Compare(string x, string y)
        {
            var sx = x.Split('/', '\\');
            var sy = y.Split('/', '\\');

            int minLength = Math.Min(sx.Length, sy.Length);
            for (int i = 0; i < minLength-1; i++)
            {
                int cmp = _stringComparer.Compare(sx[i], sy[i]);
                if (cmp != 0)
                    return cmp;
            }

            if (sx.Length > sy.Length)
                    return -1;
            if (sy.Length > sx.Length)
                    return 1;

            return _stringComparer.Compare(sx[minLength - 1], sy[minLength - 1]);
        }
    }
}