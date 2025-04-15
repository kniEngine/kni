// Copyright (C)2025 Nick Kastellanos

using System.Collections.Generic;

namespace Content.Pipeline.Editor
{
    internal class ContentItemPathComparer : IComparer<ContentItem>
    {
        private readonly PathComparer _pathComparer = new PathComparer();

        public int Compare(ContentItem x, ContentItem y)
        {
            return _pathComparer.Compare(x.OriginalPath, y.OriginalPath);
        }
    }
}