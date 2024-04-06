// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Platform
{
    public abstract class GameFactory
    {
        private volatile static GameFactory _current;

        internal static GameFactory Current
        {
            get
            {
                GameFactory current = _current;
                if (current != null)
                    return current;

                GameFactory gameFactory = CreateGameFactory();
                GameFactory.RegisterGameFactory(gameFactory);

                return _current;
            }
        }

        private static GameFactory CreateGameFactory()
        {
            return new ConcreteGameFactory();
        }

        public static void RegisterGameFactory(GameFactory gameFactory)
        {
            if (gameFactory == null)
                throw new NullReferenceException("gameFactory");

            lock (typeof(GameFactory))
            {
                if (_current == null)
                    _current = gameFactory;
                else
                    throw new InvalidOperationException("inputFactory allready registered.");
            }
        }

        public abstract GameStrategy CreateGameStrategy(Game game);
        public abstract GraphicsDeviceManagerStrategy CreateGraphicsDeviceManagerStrategy(Game game);

    }

}
