// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;


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
        Released = 0,

        /// <summary>
        /// The button is touched.
        /// </summary>
        Touched = 1,

        /// <summary>
        /// The button is pressed.
        /// </summary>
        Pressed = 2,
    }
}
