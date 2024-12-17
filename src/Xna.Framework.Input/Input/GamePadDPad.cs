// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Input
{
    public struct GamePadDPad
    {
        internal Buttons _buttons;

        /// <summary>
        /// Gets a value indicating whether up is pressed on the directional pad.
        /// </summary>
        /// <value><see cref="ButtonState.Pressed"/> if the up button is pressed; otherwise, <see cref="ButtonState.Released"/>.</value>
        public ButtonState Up
        {
            get { return ((_buttons & Buttons.DPadUp) == Buttons.DPadUp) ? ButtonState.Pressed : ButtonState.Released; }
            private set
            {
                if (value == ButtonState.Pressed)
                    _buttons |= Buttons.DPadUp;
                else
                    _buttons &= ~Buttons.DPadUp;
            }
        }

        /// <summary>
        /// Gets a value indicating whether down is pressed on the directional pad.
        /// </summary>
        /// <value><see cref="ButtonState.Pressed"/> if the down button is pressed; otherwise, <see cref="ButtonState.Released"/>.</value>
        public ButtonState Down
        {
            get { return ((_buttons & Buttons.DPadDown) == Buttons.DPadDown) ? ButtonState.Pressed : ButtonState.Released; }
            private set
            {
                if (value == ButtonState.Pressed)
                    _buttons |= Buttons.DPadDown;
                else
                    _buttons &= ~Buttons.DPadDown;
            }
        }

        /// <summary>
        /// Gets a value indicating whether left is pressed on the directional pad.
        /// </summary>
        /// <value><see cref="ButtonState.Pressed"/> if the left button is pressed; otherwise, <see cref="ButtonState.Released"/>.</value>
        public ButtonState Left
        {
            get { return ((_buttons & Buttons.DPadLeft) == Buttons.DPadLeft) ? ButtonState.Pressed : ButtonState.Released; }
            private set
            {
                if (value == ButtonState.Pressed)
                    _buttons |= Buttons.DPadLeft;
                else
                    _buttons &= ~Buttons.DPadLeft;
            }
        }

        /// <summary>
        /// Gets a value indicating whether right is pressed on the directional pad.
        /// </summary>
        /// <value><see cref="ButtonState.Pressed"/> if the right button is pressed; otherwise, <see cref="ButtonState.Released"/>.</value>
        public ButtonState Right
        {
            get { return ((_buttons & Buttons.DPadRight) == Buttons.DPadRight) ? ButtonState.Pressed : ButtonState.Released; }
            private set
            {
                if (value == ButtonState.Pressed)
                    _buttons |= Buttons.DPadRight;
                else
                    _buttons &= ~Buttons.DPadRight;
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Xna.Framework.Input.GamePadDPad"/> struct.
        /// </summary>
        /// <param name="upValue">Current state of directional pad up.</param>
        /// <param name="downValue">Current state of directional pad down.</param>
        /// <param name="leftValue">Current state of directional pad left.</param>
        /// <param name="rightValue">Current state of directional pad right.</param>
        public GamePadDPad(ButtonState upValue, ButtonState downValue, ButtonState leftValue, ButtonState rightValue) : this()
        {
            if (downValue == ButtonState.Pressed)
                this.Up = ButtonState.Pressed;
            if (downValue == ButtonState.Pressed)
                this.Down = ButtonState.Pressed;
            if (leftValue == ButtonState.Pressed)
                this.Left = ButtonState.Pressed;
            if (rightValue == ButtonState.Pressed)
                this.Right = ButtonState.Pressed;
        }

        internal GamePadDPad(params Buttons[] buttons) : this()
        {
            foreach (Buttons button in buttons)
            {
                if ((button & Buttons.DPadUp) == Buttons.DPadUp)
                    this.Up = ButtonState.Pressed;
                if ((button & Buttons.DPadDown) == Buttons.DPadDown)
                    this.Down = ButtonState.Pressed;
                if ((button & Buttons.DPadLeft) == Buttons.DPadLeft)
                    this.Left = ButtonState.Pressed;
                if ((button & Buttons.DPadRight) == Buttons.DPadRight)
                    this.Right = ButtonState.Pressed;
            }
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="GamePadDPad"/> are equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>true if <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, false.</returns>
        public static bool operator ==(GamePadDPad left, GamePadDPad right)
        {
            return (left._buttons == right._buttons);
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="GamePadDPad"/> are not equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>true if <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, false.</returns>
        public static bool operator !=(GamePadDPad left, GamePadDPad right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare to this instance.</param>
        /// <returns>true if <paramref name="obj"/> is a <see cref="GamePadDPad"/> and has the same value as this instance; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return (obj is GamePadDPad) && (this == (GamePadDPad)obj);
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="T:Microsoft.Xna.Framework.Input.GamePadDPad"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            return (int)_buttons;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadDPad"/>
        /// in a format of 0000 where each number represents a boolean value of each respecting object property: Left, Up, Right, Down.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadDPad"/>.</returns>
        public override string ToString()
        {
            return "" + (int)Left + (int)Up + (int)Right + (int)Down;
        }
    }
}
