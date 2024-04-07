// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022-2024 Nick Kastellanos

using System;


namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// Defines the buttons on gamepad.
    /// </summary>
    [Flags]
    public enum Buttons
    {
        /// <summary>
        /// Directional pad up.
        /// </summary>
        DPadUp = 1 << 0,

        /// <summary>
        /// Directional pad down.
        /// </summary>
        DPadDown = 1 << 1,

        /// <summary>
        /// Directional pad left.
        /// </summary>
        DPadLeft = 1 << 2,

        /// <summary>
        /// Directional pad right.
        /// </summary>
        DPadRight = 1 << 3,

        /// <summary>
        /// START button.
        /// </summary>
        Start = 1 << 4,
      
        /// <summary>
        /// BACK button.
        /// </summary>
        Back = 1 << 5,

        /// <summary>
        /// Left stick button (pressing the left stick).
        /// </summary>
        LeftStick = 1 << 6,

        /// <summary>
        /// Right stick button (pressing the right stick).
        /// </summary>
        RightStick = 1 << 7,

        /// <summary>
        /// Left bumper (shoulder) button.
        /// </summary>
        LeftShoulder = 1 << 8,

        /// <summary>
        /// Right bumper (shoulder) button.
        /// </summary>
        RightShoulder = 1 << 9,

        /// <summary>
        /// Big button.
        /// </summary>    
        BigButton = 1 << 11,
       
        /// <summary>
        /// A button.
        /// </summary>
        A = 1 << 12,

        /// <summary>
        /// B button.
        /// </summary>
        B = 1 << 13,

        /// <summary>
        /// X button.
        /// </summary>
        X = 1 << 14,

        /// <summary>
        /// Y button.
        /// </summary>
        Y = 1 << 15,    

        /// <summary>
        /// Left grip.
        /// </summary>
        LeftGrip = 1 << 19,

        /// <summary>
        /// Right grip.
        /// </summary>
        RightGrip = 1 << 20,

        /// <summary>
        /// Left stick is towards the left.
        /// </summary>
        LeftThumbstickLeft = 1 << 21,

        /// <summary>
        /// Right trigger.
        /// </summary>
        RightTrigger = 1 << 22,

        /// <summary>
        /// Left trigger.
        /// </summary>
        LeftTrigger = 1 << 23,

        /// <summary>
        /// Right stick is towards up.
        /// </summary>   
        RightThumbstickUp = 1 << 24,

        /// <summary>
        /// Right stick is towards down.
        /// </summary>   
        RightThumbstickDown = 1 << 25,

        /// <summary>
        /// Right stick is towards the right.
        /// </summary>
        RightThumbstickRight = 1 << 26,

        /// <summary>
        /// Right stick is towards the left.
        /// </summary>
        RightThumbstickLeft = 1 << 27,

        /// <summary>
        /// Left stick is towards up.
        /// </summary>  
        LeftThumbstickUp = 1 << 28,

        /// <summary>
        /// Left stick is towards down.
        /// </summary>  
        LeftThumbstickDown = 1 << 29,

        /// <summary>
        /// Left stick is towards the right.
        /// </summary>
        LeftThumbstickRight = 1 << 30
    }
}
