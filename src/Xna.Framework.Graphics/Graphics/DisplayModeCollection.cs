// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022-2024 Nick Kastellanos

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
    public class DisplayModeCollection : IEnumerable<DisplayMode>
    {
        private readonly List<DisplayMode> _modes;

        public IEnumerable<DisplayMode> this[SurfaceFormat format]
        {
            get 
            {
                List<DisplayMode> list = new List<DisplayMode>();
                foreach (DisplayMode mode in _modes)
                {
                    if (mode.Format == format)
                        list.Add(mode);
                }
                return list;
            }
        }

        public IEnumerator<DisplayMode> GetEnumerator()
        {
            return _modes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _modes.GetEnumerator();
        }
        
        internal DisplayModeCollection(List<DisplayMode> modes)
        {
            // Sort the modes in a consistent way that happens
            // to match XNA behavior on some graphics devices.
            modes.Sort(DisplayModeComparison);

            _modes = modes;
        }

        private static int DisplayModeComparison(DisplayMode a, DisplayMode b)
        {
            if (a == b)
                return 0;

            int formatComparison = a.Format.CompareTo(b.Format);
            if (formatComparison != 0)
                return formatComparison;
            int widthComparison = a.Width.CompareTo(b.Width);
            if (widthComparison != 0)
                return widthComparison;
            int heightComparison = a.Height.CompareTo(b.Height);
            if (heightComparison != 0)
                return heightComparison;

            return 0;
        }
    }
}