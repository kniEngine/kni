// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Input.Oculus
{
    /// <summary>
    /// Defines a button state for buttons of mouse, gamepad or joystick.
    /// </summary>
    public enum TouchButtonState
    {
        /// <summary>
        /// The button is released.
        /// </summary>
        Released = 1,

        /// <summary>
        /// The button is touched.
        /// </summary>
        Touched = 2,

        /// <summary>
        /// The button is pressed.
        /// </summary>
        Pressed = 4,
    }
}
