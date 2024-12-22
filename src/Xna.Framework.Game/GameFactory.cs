// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
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

                lock (typeof(GameFactory))
                {
                    if (_current != null)
                        return _current;

                    Console.WriteLine("GameFactory not found.");
                    Console.WriteLine("Initialize input with 'GameFactory.RegisterGameFactory(new ConcreteGameFactory());'.");
                    GameFactory gameFactory = CreateGameFactory();
                    GameFactory.RegisterGameFactory(gameFactory);
                }

                return _current;
            }
        }

        private static GameFactory CreateGameFactory()
        {
            Console.WriteLine("Registering ConcreteGameFactoryStrategy through reflection.");

            Type type = Type.GetType("Microsoft.Xna.Platform.ConcreteGameFactory, Xna.Platform", false);
            if (type != null)
                if (type.IsSubclassOf(typeof(GameFactory)) && !type.IsAbstract)
                    return (GameFactory)Activator.CreateInstance(type);

            return null;
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
                    throw new InvalidOperationException("gameFactory allready registered.");
            }
        }

        public abstract GameStrategy CreateGameStrategy(Game game);
        public abstract GraphicsDeviceManagerStrategy CreateGraphicsDeviceManagerStrategy(Game game);

    }

}
