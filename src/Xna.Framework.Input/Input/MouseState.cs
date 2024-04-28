// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// Represents a mouse state with cursor position and button press information.
    /// </summary>
    public struct MouseState
    {
        private const int LeftButtonBit = 0;
        private const int MiddleButtonBit = 1;
        private const int RightButtonBit = 2;
        private const int XButton1Bit = 3;
        private const int XButton2Bit = 4;

        private int _x;
        private int _y;
        private int _scrollWheelValue;
        private int _horizontalScrollWheelValue;
        private int _rawX;
        private int _rawY;
        private int _buttons;

        /// <summary>
        /// Initializes a new instance of the MouseState.
        /// </summary>
        /// <param name="x">Horizontal position of the mouse in relation to the window.</param>
        /// <param name="y">Vertical position of the mouse in relation to the window.</param>
        /// <param name="scrollWheel">Mouse scroll wheel's value.</param>
        /// <param name="leftButton">Left mouse button's state.</param>
        /// <param name="middleButton">Middle mouse button's state.</param>
        /// <param name="rightButton">Right mouse button's state.</param>
        /// <param name="xButton1">XBUTTON1's state.</param>
        /// <param name="xButton2">XBUTTON2's state.</param>
        /// <remarks>Normally <see cref="Mouse.GetState()"/> should be used to get mouse current state. The constructor is provided for simulating mouse input.</remarks>
        public MouseState(
            int x,
            int y,
            int scrollWheel,
            ButtonState leftButton,
            ButtonState middleButton,
            ButtonState rightButton,
            ButtonState xButton1,
            ButtonState xButton2)
        {
            _x = x;
            _y = y;
            _scrollWheelValue = scrollWheel;
            _horizontalScrollWheelValue = 0;
            _rawX = 0;
            _rawY = 0;
            _buttons = (int)leftButton << LeftButtonBit |
                       (int)middleButton << MiddleButtonBit |
                       (int)rightButton << RightButtonBit |
                       (int)xButton1 << XButton1Bit |
                       (int)xButton2 << XButton2Bit; ;
        }

        /// <summary>
        /// Initializes a new instance of the MouseState.
        /// </summary>
        /// <param name="x">Horizontal position of the mouse in relation to the window.</param>
        /// <param name="y">Vertical position of the mouse in relation to the window.</param>
        /// <param name="scrollWheel">Mouse scroll wheel's value.</param>
        /// <param name="horizontalScrollWheel">Mouse horizontal scroll wheel's value.</param>
        /// <param name="rawX">Mouse rawX value.</param>
        /// <param name="rawY">Mouse rawY value.</param>
        /// <param name="leftButton">Left mouse button's state.</param>
        /// <param name="middleButton">Middle mouse button's state.</param>
        /// <param name="rightButton">Right mouse button's state.</param>
        /// <param name="xButton1">XBUTTON1's state.</param>
        /// <param name="xButton2">XBUTTON2's state.</param>
        /// <remarks>Normally <see cref="Mouse.GetState()"/> should be used to get mouse current state. The constructor is provided for simulating mouse input.</remarks>
        public MouseState(
            int x,
            int y,
            int scrollWheel,
            int horizontalScrollWheel,
            int rawX, int rawY,
            ButtonState leftButton,
            ButtonState middleButton,
            ButtonState rightButton,
            ButtonState xButton1,
            ButtonState xButton2)
        {
            _x = x;
            _y = y;
            _scrollWheelValue = scrollWheel;
            _horizontalScrollWheelValue = horizontalScrollWheel;
            _rawX = rawX;
            _rawY = rawY;
            _buttons = (int)leftButton << LeftButtonBit |
                       (int)middleButton << MiddleButtonBit |
                       (int)rightButton << RightButtonBit |
                       (int)xButton1 << XButton1Bit |
                       (int)xButton2 << XButton2Bit;
        }

        /// <summary>
        /// Compares whether two MouseState instances are equal.
        /// </summary>
        /// <param name="left">MouseState instance on the left of the equal sign.</param>
        /// <param name="right">MouseState instance  on the right of the equal sign.</param>
        /// <returns>true if the instances are equal; false otherwise.</returns>
        public static bool operator ==(MouseState left, MouseState right)
        {
            return left._x == right._x &&
                   left._y == right._y &&
                   left._scrollWheelValue == right._scrollWheelValue &&
                   left._horizontalScrollWheelValue == right._horizontalScrollWheelValue &&
                   left.RawX == right.RawX &&
                   left.RawY == right.RawY &&
                   left._buttons == right._buttons;
        }

        /// <summary>
        /// Compares whether two MouseState instances are not equal.
        /// </summary>
        /// <param name="left">MouseState instance on the left of the equal sign.</param>
        /// <param name="right">MouseState instance  on the right of the equal sign.</param>
        /// <returns>true if the objects are not equal; false otherwise.</returns>
        public static bool operator !=(MouseState left, MouseState right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Compares whether current instance is equal to specified object.
        /// </summary>
        /// <param name="obj">The MouseState to compare.</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is MouseState)
                return this == (MouseState)obj;
            return false;
        }

        /// <summary>
        /// Gets the hash code for MouseState instance.
        /// </summary>
        /// <returns>Hash code of the object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _x;
                hashCode = (hashCode * 397) ^ _y;
                hashCode = (hashCode * 397) ^ _scrollWheelValue;
                hashCode = (hashCode * 397) ^ _horizontalScrollWheelValue;
                hashCode = (hashCode * 397) ^ RawX.GetHashCode();
                hashCode = (hashCode * 397) ^ RawY.GetHashCode();
                hashCode = (hashCode * 397) ^ _buttons;
                return hashCode;
            }
        }

        /// <summary>
        /// Returns a string describing the mouse state.
        /// </summary>
        public override string ToString()
        {
            string buttons;
            if (_buttons == 0)
                buttons = "None";
            else
            {
                buttons = string.Empty;
                if (LeftButton == ButtonState.Pressed)
                {
                    if (buttons.Length > 0)
                        buttons += " Left";
                    else
                        buttons += "Left";
                }
                if (RightButton == ButtonState.Pressed)
                {
                    if (buttons.Length > 0)
                        buttons += " Right";
                    else
                        buttons += "Right";
                }
                if (MiddleButton == ButtonState.Pressed)
                {
                    if (buttons.Length > 0)
                        buttons += " Middle";
                    else
                        buttons += "Middle";
                }
                if (XButton1 == ButtonState.Pressed)
                {
                    if (buttons.Length > 0)
                        buttons += " XButton1";
                    else
                        buttons += "XButton1";
                }
                if (XButton2 == ButtonState.Pressed)
                {
                    if (buttons.Length > 0)
                        buttons += " XButton2";
                    else
                        buttons += "XButton2";
                }
            }

            return  "[MouseState X=" + _x +
                    ", Y=" + _y +
                    ", Buttons=" + buttons +
                    ", Wheel=" + _scrollWheelValue +
                    ", HWheel=" + _horizontalScrollWheelValue +
                    "]";
        }

        /// <summary>
        /// Gets horizontal position of the cursor in relation to the window.
        /// </summary>
        public int X
        {
            get { return _x; }
            internal set { _x = value; }
        }

        /// <summary>
        /// Gets vertical position of the cursor in relation to the window.
        /// </summary>
        public int Y
        {
            get { return _y; }
            internal set { _y = value; }
        }

        /// <summary>
        /// Gets cursor position.
        /// </summary>
        public Point Position
        {
            get { return new Point(_x, _y); }
        }

        /// <summary>
        /// Returns cumulative scroll wheel value since the game start.
        /// </summary>
        public int ScrollWheelValue
        {
            get { return _scrollWheelValue; }
            internal set { _scrollWheelValue = value; }
        }

        /// <summary>
        /// Returns the cumulative horizontal scroll wheel value since the game start
        /// </summary>
        public int HorizontalScrollWheelValue
        {
            get { return _horizontalScrollWheelValue; }
            internal set { _horizontalScrollWheelValue = value; }
        }
        
        /// <summary>
        /// Gets cursor raw X input.
        /// </summary>        
        public int RawX
        {
            get { return _rawX; }
            internal set { _rawX = value; }
        }

        /// <summary>
        /// Gets cursor raw X input.
        /// </summary>        
        public int RawY
        {
            get { return _rawY; }
            internal set { _rawY = value; }
        }

        /// <summary>
        /// Gets state of the left mouse button.
        /// </summary>
        public ButtonState LeftButton
        {
            get { return (ButtonState)((_buttons >> LeftButtonBit) & 1); }
            internal set { _buttons = _buttons & (~(1 << LeftButtonBit)) | (int)value << LeftButtonBit; }
        }

        /// <summary>
        /// Gets state of the middle mouse button.
        /// </summary>
        public ButtonState MiddleButton
        {
            get { return (ButtonState)((_buttons >> MiddleButtonBit) & 1); }
            internal set { _buttons = _buttons & (~(1 << MiddleButtonBit)) | (int)value << MiddleButtonBit; }
        }

        /// <summary>
        /// Gets state of the right mouse button.
        /// </summary>
        public ButtonState RightButton
        {
            get { return (ButtonState)((_buttons >> RightButtonBit) & 1); }
            internal set { _buttons = _buttons & (~(1 << RightButtonBit)) | (int)value << RightButtonBit; }
        }

        /// <summary>
        /// Gets state of the XButton1.
        /// </summary>
        public ButtonState XButton1
        {
            get { return (ButtonState)((_buttons >> XButton1Bit) & 1); }
            internal set { _buttons = _buttons & (~(1 << XButton1Bit)) | (int)value << XButton1Bit; }
        }

        /// <summary>
        /// Gets state of the XButton2.
        /// </summary>
        public ButtonState XButton2
        {
            get { return (ButtonState)((_buttons >> XButton2Bit) & 1); }
            internal set { _buttons = _buttons & (~(1 << XButton2Bit)) | (int)value << XButton2Bit; }
        }

    }
}
