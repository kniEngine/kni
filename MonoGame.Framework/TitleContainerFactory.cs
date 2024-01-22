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

                TitleContainerFactory titleContainerFactory = CreateTitleContainerFactory();
                TitleContainerFactory.RegisterTitleContainerFactory(titleContainerFactory);

                return _current;
            }
        }

        private static TitleContainerFactory CreateTitleContainerFactory()
        {
            return new ConcreteTitleContainerFactory();
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

