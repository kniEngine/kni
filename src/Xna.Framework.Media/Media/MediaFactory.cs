// Copyright (C)2024 Nick Kastellanos

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
                MediaFactory current = _current;
                if (current != null)
                    return current;

                lock (typeof(MediaFactory))
                {
                    if (_current != null)
                        return _current;

                    Console.WriteLine("MediaFactory not found.");
                    Console.WriteLine("Initialize media with 'MediaFactory.RegisterMediaFactory(new ConcreteMediaFactory());'.");
                    MediaFactory mediaFactory = CreateMediaFactory();
                    MediaFactory.RegisterMediaFactory(mediaFactory);
                }

                return _current;
            }
        }

        private static MediaFactory CreateMediaFactory()
        {
            Console.WriteLine("Registering Concrete MediaFactoryStrategy through reflection.");

            Type type = Type.GetType("Microsoft.Xna.Platform.Media.ConcreteMediaFactory, MonoGame.Framework", false);
            if (type != null)
                if (type.IsSubclassOf(typeof(MediaFactory)) && !type.IsAbstract)
                    return (MediaFactory)Activator.CreateInstance(type);

            return null;
        }

        public static void RegisterMediaFactory(MediaFactory mediaFactory)
        {
            if (mediaFactory == null)
                throw new NullReferenceException("mediaFactory");

            lock (typeof(MediaFactory))
            {
                if (_current == null)
                    _current = mediaFactory;
                else
                    throw new InvalidOperationException("MediaFactory allready registered.");
            }
        }

        public abstract MediaLibraryStrategy CreateMediaLibraryStrategy();
        public abstract MediaLibraryStrategy CreateMediaLibraryStrategy(MediaSource mediaSource);
        public abstract MediaPlayerStrategy CreateMediaPlayerStrategy();
        public abstract SongStrategy CreateSongStrategy(string name, Uri streamSource);
        public abstract VideoPlayerStrategy CreateVideoPlayerStrategy();
        public abstract VideoStrategy CreateVideoStrategy(GraphicsDevice graphicsDevice, string fileName, TimeSpan timeSpan);

        public abstract IList<MediaSource> GetAvailableMediaSources();

        protected MediaSource CreateMediaSource(string name, MediaSourceType type)
        {
            return new MediaSource(name, type);
        }
    }
}
