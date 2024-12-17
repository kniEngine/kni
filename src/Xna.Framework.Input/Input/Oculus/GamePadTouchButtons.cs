// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022-2024 Nick Kastellanos

using System;
using System.Collections.Generic;


namespace Microsoft.Xna.Framework.Input.Oculus
{
    /// <summary>
    /// A struct that represents the current button states for the controller.
    /// </summary>
    public struct GamePadTouchButtons
    {
        internal readonly Buttons _buttons;
        internal readonly Buttons _touches;

        /// <summary>
        /// Gets a value indicating if the button A is pressed or Touched.
        /// </summary>
        public TouchButtonState A
        {
            get
            {
                if ((_buttons & Buttons.A) == Buttons.A) return TouchButtonState.Pressed;
                if ((_touches & Buttons.A) == Buttons.A) return TouchButtonState.Touched;
                return TouchButtonState.Released;
            }
        }

        /// <summary>
        /// Gets a value indicating if the button B is pressed or Touched.
        /// </summary>
        public TouchButtonState B
        {
            get
            {
                if ((_buttons & Buttons.B) == Buttons.B) return TouchButtonState.Pressed;
                if ((_touches & Buttons.B) == Buttons.B) return TouchButtonState.Touched;
                return TouchButtonState.Released;
            }
        }

        /// <summary>
        /// Gets a value indicating if the button Back is pressed or Touched.
        /// </summary>
        public TouchButtonState Back
        {
            get
            {
                if ((_buttons & Buttons.Back) == Buttons.Back) return TouchButtonState.Pressed;
                if ((_touches & Buttons.Back) == Buttons.Back) return TouchButtonState.Touched;
                return TouchButtonState.Released;
            }
        }

        /// <summary>
        /// Gets a value indicating if the button X is pressed or Touched.
        /// </summary>
        public TouchButtonState X
        {
            get
            {
                if ((_buttons & Buttons.X) == Buttons.X) return TouchButtonState.Pressed;
                if ((_touches & Buttons.X) == Buttons.X) return TouchButtonState.Touched;
                return TouchButtonState.Released;
            }
        }

        /// <summary>
        /// Gets a value indicating if the button Y is pressed or Touched.
        /// </summary>
        public TouchButtonState Y
        {
            get
            {
                if ((_buttons & Buttons.Y) == Buttons.Y) return TouchButtonState.Pressed;
                if ((_touches & Buttons.Y) == Buttons.Y) return TouchButtonState.Touched;
                return TouchButtonState.Released;
            }
        }

        /// <summary>
        /// Gets a value indicating if the button Start is pressed or Touched.
        /// </summary>
        public TouchButtonState Start
        {
            get
            {
                if ((_buttons & Buttons.Start) == Buttons.Start) return TouchButtonState.Pressed;
                if ((_touches & Buttons.Start) == Buttons.Start) return TouchButtonState.Touched;
                return TouchButtonState.Released;
            }
        }

        /// <summary>
        /// Gets a value indicating if the left shoulder button is pressed or Touched.
        /// </summary>
        public TouchButtonState LeftShoulder
        {
            get
            {
                if ((_buttons & Buttons.LeftShoulder) == Buttons.LeftShoulder) return TouchButtonState.Pressed;
                if ((_touches & Buttons.LeftShoulder) == Buttons.LeftShoulder) return TouchButtonState.Touched;
                return TouchButtonState.Released;
            }
        }

        /// <summary>
        /// Gets a value indicating if the left grip button is pressed or Touched.
        /// </summary>
        public TouchButtonState LeftGrip
        {
            get
            {
                if ((_buttons & Buttons.LeftGrip) == Buttons.LeftGrip) return TouchButtonState.Pressed;
                if ((_touches & Buttons.LeftGrip) == Buttons.LeftGrip) return TouchButtonState.Touched;
                return TouchButtonState.Released;
            }
        }

        /// <summary>
        /// Gets a value indicating if the left stick button is pressed or Touched.
        /// </summary>
        public TouchButtonState LeftStick
        {
            get
            {
                if ((_buttons & Buttons.LeftStick) == Buttons.LeftStick) return TouchButtonState.Pressed;
                if ((_touches & Buttons.LeftStick) == Buttons.LeftStick) return TouchButtonState.Touched;
                return TouchButtonState.Released;
            }
        }

        /// <summary>
        /// Gets a value indicating if the left trigger button is pressed or Touched.
        /// </summary>
        public TouchButtonState LeftTrigger
        {
            get
            {
                if ((_buttons & Buttons.LeftTrigger) == Buttons.LeftTrigger) return TouchButtonState.Pressed;
                if ((_touches & Buttons.LeftTrigger) == Buttons.LeftTrigger) return TouchButtonState.Touched;
                return TouchButtonState.Released;
            }
        }

        /// <summary>
        /// Gets a value indicating if the right shoulder button is pressed or Touched.
        /// </summary>
        public TouchButtonState RightShoulder
        {
            get
            {
                if ((_buttons & Buttons.RightShoulder) == Buttons.RightShoulder) return TouchButtonState.Pressed;
                if ((_touches & Buttons.RightShoulder) == Buttons.RightShoulder) return TouchButtonState.Touched;
                return TouchButtonState.Released;
            }
        }

        /// <summary>
        /// Gets a value indicating if the right grip button is pressed or Touched.
        /// </summary>
        public TouchButtonState RightGrip
        {
            get
            {
                if ((_buttons & Buttons.RightGrip) == Buttons.RightGrip) return TouchButtonState.Pressed;
                if ((_touches & Buttons.RightGrip) == Buttons.RightGrip) return TouchButtonState.Touched;
                return TouchButtonState.Released;
            }
        }

        /// <summary>
        /// Gets a value indicating if the right stick button is pressed or Touched.
        /// </summary>
        public TouchButtonState RightStick
        {
            get
            {
                if ((_buttons & Buttons.RightStick) == Buttons.RightStick) return TouchButtonState.Pressed;
                if ((_touches & Buttons.RightStick) == Buttons.RightStick) return TouchButtonState.Touched;
                return TouchButtonState.Released;
            }
        }

        /// <summary>
        /// Gets a value indicating if the right trigger button is pressed or Touched.
        /// </summary>
        public TouchButtonState RightTrigger
        {
            get
            {
                if ((_buttons & Buttons.RightTrigger) == Buttons.RightTrigger) return TouchButtonState.Pressed;
                if ((_touches & Buttons.RightTrigger) == Buttons.RightTrigger) return TouchButtonState.Touched;
                return TouchButtonState.Released;
            }
        }

        /// <summary>
        /// Gets a value indicating if the guide button is pressed or Touched.
        /// </summary>
        public TouchButtonState BigButton
        {
            get
            {
                if ((_buttons & Buttons.BigButton) == Buttons.BigButton) return TouchButtonState.Pressed;
                if ((_touches & Buttons.BigButton) == Buttons.BigButton) return TouchButtonState.Touched;
                return TouchButtonState.Released;
            }
        }
        
        public GamePadTouchButtons(Buttons buttons, Buttons touches)
        {
            _buttons = buttons;
            _touches = touches;
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="GamePadButtons"/> are equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>true if <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, false.</returns>
        public static bool operator ==(GamePadTouchButtons left, GamePadTouchButtons right)
        {
            return left._buttons == right._buttons && left._touches == right._touches;
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="GamePadButtons"/> are not equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>true if <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, false.</returns>
        public static bool operator !=(GamePadTouchButtons left, GamePadTouchButtons right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare to this instance.</param>
        /// <returns>true if <paramref name="obj"/> is a <see cref="GamePadButtons"/> and has the same value as this instance; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return (obj is GamePadTouchButtons) && (this == (GamePadTouchButtons)obj);
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="T:Microsoft.Xna.Framework.Input.GamePadButtons"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            return (int)_buttons ^ (int)_touches;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadButtons"/>.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadButtons"/>.</returns>
        public override string ToString()
        {
            return "[GamePadButtons:" +
                " A=" + (int)A +
                ", B=" + (int)B +
                ", X=" + (int)X +
                ", Y=" + (int)Y +
                ", Start=" + (int)Start +
                ", Back=" + (int)Back +
                ", BigButton=" + (int)BigButton +
                ", LeftShoulder=" + (int)LeftShoulder +
                ", RightShoulder=" + (int)RightShoulder +
                ", LeftStick=" + (int)LeftStick +
                ", RightStick=" + (int)RightStick +
                "]";
        }
    }
}

