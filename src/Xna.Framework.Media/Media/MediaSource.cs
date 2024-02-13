// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Platform.Media;


namespace Microsoft.Xna.Framework.Media
{
    public sealed class MediaSource
    {
        private string _name;
        private MediaSourceType _type;

        public string Name { get { return _name; } }

        public MediaSourceType MediaSourceType { get { return _type; } }

        internal MediaSource(string name, MediaSourceType type)
        {
            _name = name;
            _type = type;
        }

        public static IList<MediaSource> GetAvailableMediaSources()
        {
            return MediaFactory.Current.GetAvailableMediaSources();
        }
    }
}
