using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Input.Cardboard
{
    public class Headset
    {
        internal static GameWindow GameWindow;

        public static HeadsetState GetState()
		{
            HeadsetState state;

            var window = GameWindow as AndroidGameWindow;
            window.UpdateHeadsetState(out state);

            return state;
		}
    }
}
