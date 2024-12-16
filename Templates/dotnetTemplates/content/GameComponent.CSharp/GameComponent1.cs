using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace KniGameComponent
{
    internal class GameComponent1 : GameComponent
    {
        public GameComponent1(Game game) : base(game)
        {
        }

        /// <summary>Initializes the component. Used to load non-graphical resources.</summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>Update the component.</summary>
        /// <param name="gameTime">GameTime of the Game.</param>
        public override void Update(GameTime gameTime)
        {
        }

    }
}
