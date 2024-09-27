// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Platform
{
    sealed class ConcreteGame : GameStrategy
    {
        public ConcreteGame(Game game) : base(game)
        {

        }

        protected override void RunGameLoop()
        {
            throw new PlatformNotSupportedException();
        }

        public override void TickExiting()
        {
            throw new PlatformNotSupportedException();
        }
    }
}
