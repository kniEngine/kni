// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Platform.Input;

namespace Microsoft.Xna.Platform.Input
{
    public interface IKeyboard
    {
        KeyboardState GetState();
    }
}

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// Allows getting keystrokes from keyboard.
    /// </summary>
    public sealed partial class Keyboard : IKeyboard
    {
        private static Keyboard _current;

        /// <summary>
        /// Returns the current Keyboard instance.
        /// </summary> 
        public static Keyboard Current
        {
            get
            {
                if (_current != null)
                    return _current;

                lock (typeof(Keyboard))
                {
                    if (_current == null)
                        _current = new Keyboard();

                    return _current;
                }
            }
        }

        /// <summary>
        /// Returns the current keyboard state.
        /// </summary>
        /// <returns>Current keyboard state.</returns>
        public static KeyboardState GetState()
        {
            return ((IKeyboard)Keyboard.Current).GetState();
        }
        
        /// <summary>
        /// Returns the current keyboard state for a given player.
        /// </summary>
        /// <param name="playerIndex">Player index of the keyboard.</param>
        /// <returns>Current keyboard state.</returns>
        [Obsolete("Use GetState() instead. In future versions this method can be removed.")]
        public static KeyboardState GetState(PlayerIndex playerIndex)
        {
            return ((IKeyboard)Keyboard.Current).GetState();
        }


        //private Keyboard()
        //{
        //}


        #region IKeyboard

        KeyboardState IKeyboard.GetState()
        {
            return PlatformGetState();
        }

        #endregion IKeyboard
    }
}
