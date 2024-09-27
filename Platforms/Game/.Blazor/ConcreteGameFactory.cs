// Copyright (C)2024 Nick Kastellanos

using Microsoft.Xna.Framework;
using Microsoft.Xna.Platform.Input;

namespace Microsoft.Xna.Platform
{
    public sealed class ConcreteGameFactory : GameFactory
    {

        public override GameStrategy CreateGameStrategy(Game game)
        {
            return new ConcreteGame(game);
        }

        public override GraphicsDeviceManagerStrategy CreateGraphicsDeviceManagerStrategy(Game game)
        {
            return new ConcreteGraphicsDeviceManager(game);
        }

    }
}