// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;


namespace Microsoft.Xna.Platform.Media
{
    public abstract class MediaFactory
    {
        private volatile static MediaFactory _current;

        public static MediaFactory Current
        {
            get
            {
                var current = _current;
                if (current != null)
                    return current;

                MediaFactory audioFactory = CreateMediaFactory();
                MediaFactory.RegisterMediaFactory(audioFactory);

                return _current;
            }
        }

        private static MediaFactory CreateMediaFactory()
        {
            return new ConcreteMediaFactory();
        }

        public static void RegisterMediaFactory(MediaFactory mediaFactory)
        {
            // lock
            {
                if (_current == null)
                {
                    if (mediaFactory == null)
                        throw new NullReferenceException("mediaFactory");
                    _current = mediaFactory;
                }
                else
                {
                    throw new InvalidOperationException("MediaFactory allready registered.");
                }
            }
        }

        public abstract MediaLibraryStrategy CreateMediaLibraryStrategy();
        public abstract MediaLibraryStrategy CreateMediaLibraryStrategy(MediaSource mediaSource);
        public abstract MediaPlayerStrategy CreateMediaPlayerStrategy();
        public abstract SongStrategy CreateSongStrategy(string name, Uri streamSource);
        public abstract VideoPlayerStrategy CreateVideoPlayerStrategy();
        public abstract VideoStrategy CreateVideoStrategy(GraphicsDevice graphicsDevice, string fileName, TimeSpan timeSpan);

        public abstract IList<MediaSource> GetAvailableMediaSources();
    }
}
