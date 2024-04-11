// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// Describes joystick hat state.
    /// </summary>
    public struct JoystickHat
    {
        private readonly Buttons _dPadButtons;

        internal JoystickHat(Buttons dPadButtons)
        {
            this._dPadButtons = dPadButtons;
        }


        /// <summary>
        /// Gets if joysticks hat "down" is pressed.
        /// </summary>
        /// <value><see cref="ButtonState.Pressed"/> if the button is pressed otherwise, <see cref="ButtonState.Released"/>.</value>
        public ButtonState Down { get { return (ButtonState)((int)(_dPadButtons & Buttons.DPadDown) >> 1); } }

        /// <summary>
        /// Gets if joysticks hat "left" is pressed.
        /// </summary>
        /// <value><see cref="ButtonState.Pressed"/> if the button is pressed otherwise, <see cref="ButtonState.Released"/>.</value>
        public ButtonState Left { get { return (ButtonState)((int)(_dPadButtons & Buttons.DPadLeft) >> 2); } }

        /// <summary>
        /// Gets if joysticks hat "right" is pressed.
        /// </summary>
        /// <value><see cref="ButtonState.Pressed"/> if the button is pressed otherwise, <see cref="ButtonState.Released"/>.</value>
        public ButtonState Right { get { return (ButtonState)((int)(_dPadButtons & Buttons.DPadRight) >> 3); } }

        /// <summary>
        /// Gets if joysticks hat "up" is pressed.
        /// </summary>
        /// <value><see cref="ButtonState.Pressed"/> if the button is pressed otherwise, <see cref="ButtonState.Released"/>.</value>
        public ButtonState Up { get { return (ButtonState)((int)(_dPadButtons & Buttons.DPadUp) >> 0); } }

        /// <summary>
        /// Determines whether a specified instance of <see cref="Microsoft.Xna.Framework.Input.JoystickHat"/> is equal
        /// to another specified <see cref="Microsoft.Xna.Framework.Input.JoystickHat"/>.
        /// </summary>
        /// <param name="left">The first <see cref="Microsoft.Xna.Framework.Input.JoystickHat"/> to compare.</param>
        /// <param name="right">The second <see cref="Microsoft.Xna.Framework.Input.JoystickHat"/> to compare.</param>
        /// <returns><c>true</c> if <c>left</c> and <c>right</c> are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(JoystickHat left, JoystickHat right)
        {
            return (left._dPadButtons == right._dPadButtons);
        }

        /// <summary>
        /// Determines whether a specified instance of <see cref="Microsoft.Xna.Framework.Input.JoystickHat"/> is not
        /// equal to another specified <see cref="Microsoft.Xna.Framework.Input.JoystickHat"/>.
        /// </summary>
        /// <param name="left">The first <see cref="Microsoft.Xna.Framework.Input.JoystickHat"/> to compare.</param>
        /// <param name="right">The second <see cref="Microsoft.Xna.Framework.Input.JoystickHat"/> to compare.</param>
        /// <returns><c>true</c> if <c>left</c> and <c>right</c> are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(JoystickHat left, JoystickHat right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="T:Microsoft.Xna.Framework.Input.JoystickHat"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="T:Microsoft.Xna.Framework.Input.JoystickHat"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="object"/> is equal to the current
        /// <see cref="T:Microsoft.Xna.Framework.Input.JoystickHat"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return (obj is JoystickHat) && (this == (JoystickHat)obj);
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="T:Microsoft.Xna.Framework.Input.JoystickHat"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            int hash = 0;

            hash |= (int)_dPadButtons;

            return hash;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.JoystickHat"/> in a format of 0000 where each number represents a boolean value of each respecting object property: Left, Up, Right, Down.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.JoystickHat"/>.</returns>
        public override string ToString()
        {
            return string.Format("{{ Left: {0}, Up: {1}, Right: {2}, Down: {3} }}",
                (int)Left, (int)Up, (int)Right, (int)Down);
        }
    }
}

