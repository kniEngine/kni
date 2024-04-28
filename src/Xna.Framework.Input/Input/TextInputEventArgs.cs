// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// This class is used in the <see cref="GameWindow.TextInput"/> event as <see cref="EventArgs"/>.
    /// </summary>
    public class TextInputEventArgs : InputKeyEventArgs
    {
        /// <summary>
        /// The character for the key that was pressed.
        /// </summary>
        public readonly char Character;
        
        public TextInputEventArgs(Keys key, char character)
            : base(key)
        {
            Character = character;
        }
    }
}
