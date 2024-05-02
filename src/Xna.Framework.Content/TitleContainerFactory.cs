// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Platform
{
    public abstract class TitleContainerFactory
    {
        private volatile static TitleContainerFactory _current;

        internal static TitleContainerFactory Current
        {
            get
            {
                TitleContainerFactory current = _current;
                if (current != null)
                    return current;

                lock (typeof(TitleContainerFactory))
                {
                    if (_current != null)
                        return _current;

                    Console.WriteLine("TitleContainerFactory not found.");
                    Console.WriteLine("Initialize title with 'TitleContainerFactory.RegisterTitleContainerFactory(new ConcreteTitleContainerFactory());'.");
                    TitleContainerFactory titleContainerFactory = CreateTitleContainerFactory();
                    TitleContainerFactory.RegisterTitleContainerFactory(titleContainerFactory);
                }

                return _current;
            }
        }

        private static TitleContainerFactory CreateTitleContainerFactory()
        {
            Console.WriteLine("Registering Concrete TitleContainerFactoryStrategy through reflection.");

            Type type = Type.GetType("Microsoft.Xna.Platform.ConcreteTitleContainerFactory, MonoGame.Framework", false);
            if (type != null)
                if (type.IsSubclassOf(typeof(TitleContainerFactory)) && !type.IsAbstract)
                    return (TitleContainerFactory)Activator.CreateInstance(type);

            return null;
        }

        public static void RegisterTitleContainerFactory(TitleContainerFactory titleContainerFactory)
        {
            if (titleContainerFactory == null)
                throw new NullReferenceException("titleContainerFactory");

            lock (typeof(TitleContainerFactory))
            {
                if (_current == null)
                    _current = titleContainerFactory;
                else
                    throw new InvalidOperationException("TitleContainerFactory allready registered.");
            }
        }

        public abstract TitleContainerStrategy CreateTitleContainerStrategy();
    }

}

