﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;


namespace Microsoft.Xna.Framework.Input.Oculus
{
    /// <summary>
    /// Represents specific information about the state of the controller,
    /// including the current state of buttons and sticks.
    ///
    /// This is implemented as a partial struct to allow for individual platforms
    /// to offer additional data without separate state queries to GamePad.
    /// </summary>
    public struct TouchControllerState
    {
        /// <summary>
        /// The default initialized gamepad state.
        /// </summary>
        public static readonly GamePadState Default = new GamePadState();

        /// <summary>
        /// Gets a value indicating if the controller is connected.
        /// </summary>
        /// <value><c>true</c> if it is connected; otherwise, <c>false</c>.</value>
        public bool IsConnected { get; internal set; }

        /// <summary>
        /// Gets the packet number associated with this state.
        /// </summary>
        /// <value>The packet number.</value>
        public int PacketNumber { get; internal set; }

        /// <summary>
        /// Gets a structure that identifies what buttons on the controller are pressed.
        /// </summary>
        /// <value>The buttons structure.</value>
        public TouchButtons TouchButtons { get; internal set; }

        /// <summary>
        /// Gets a structure that indicates the position of the controller sticks (thumbsticks).
        /// </summary>
        /// <value>The thumbsticks position.</value>
        public GamePadThumbSticks ThumbSticks { get; internal set; }

        /// <summary>
        /// Gets a structure that identifies the position of triggers on the controller.
        /// </summary>
        /// <value>Positions of the triggers.</value>
        public GamePadTriggers Triggers { get; internal set; }


        public GamePadTriggers Grips { get; internal set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Xna.Framework.Input.GamePadState"/> struct
        /// using the specified GamePadThumbSticks, GamePadTriggers, GamePadButtons, and GamePadDPad.
        /// </summary>
        /// <param name="thumbSticks">Initial thumbstick state.</param>
        /// <param name="triggers">Initial trigger state.</param>
        /// <param name="grips">Initial directional pad state.</param>
        /// <param name="touchButtons">Initial button state.</param>
        public TouchControllerState(GamePadThumbSticks thumbSticks, GamePadTriggers triggers, GamePadTriggers grips, TouchButtons touchButtons) : this()
        {
            ThumbSticks = thumbSticks;
            Triggers = triggers;
            Grips = grips;
            TouchButtons = touchButtons;
            IsConnected = true;
        }

        /// <summary>
        /// Gets the button mask along with 'virtual buttons' like LeftThumbstickLeft.
        /// </summary>
        private Buttons GetVirtualButtons()
        {
            var result = TouchButtons._buttons;

            result |= ThumbSticks._virtualButtons;

            return result;
        }

        public bool IsButtonPressed(Buttons button)
        {
            return (GetVirtualButtons() & button) == button;
        }

        /// <summary>
        /// Determines whether a specified instance of <see cref="Microsoft.Xna.Framework.Input.GamePadState"/> is equal
        /// to another specified <see cref="Microsoft.Xna.Framework.Input.GamePadState"/>.
        /// </summary>
        /// <param name="left">The first <see cref="Microsoft.Xna.Framework.Input.GamePadState"/> to compare.</param>
        /// <param name="right">The second <see cref="Microsoft.Xna.Framework.Input.GamePadState"/> to compare.</param>
        /// <returns><c>true</c> if <c>left</c> and <c>right</c> are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(TouchControllerState left, TouchControllerState right)
        {
            return (left.IsConnected == right.IsConnected) &&
                (left.PacketNumber == right.PacketNumber) &&
                (left.TouchButtons == right.TouchButtons) &&
                (left.ThumbSticks == right.ThumbSticks) &&
                (left.Triggers == right.Triggers) &&
                (left.Grips == right.Grips);
        }

        /// <summary>
        /// Determines whether a specified instance of <see cref="Microsoft.Xna.Framework.Input.GamePadState"/> is not
        /// equal to another specified <see cref="Microsoft.Xna.Framework.Input.GamePadState"/>.
        /// </summary>
        /// <param name="left">The first <see cref="Microsoft.Xna.Framework.Input.GamePadState"/> to compare.</param>
        /// <param name="right">The second <see cref="Microsoft.Xna.Framework.Input.GamePadState"/> to compare.</param>
        /// <returns><c>true</c> if <c>left</c> and <c>right</c> are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(TouchControllerState left, TouchControllerState right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadState"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadState"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="object"/> is equal to the current
        /// <see cref="T:Microsoft.Xna.Framework.Input.GamePadState"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return (obj is TouchControllerState) && (this == (TouchControllerState)obj);
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="T:Microsoft.Xna.Framework.Input.GamePadState"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = PacketNumber;
                hash = (hash * 397) ^ TouchButtons.GetHashCode();
                hash = (hash * 397) ^ ThumbSticks.GetHashCode();
                hash = (hash * 397) ^ Triggers.GetHashCode();
                hash = (hash * 397) ^ Grips.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadState"/>.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadState"/>.</returns>
        public override string ToString()
        {
            if (!IsConnected)
                return "[GamePadState: IsConnected = 0]";

            return "[GamePadState: IsConnected=" + (IsConnected ? "1" : "0") +
                   ", PacketNumber=" + PacketNumber.ToString("00000") +
                   ", TouchButtons=" + TouchButtons +
                   ", ThumbSticks=" + ThumbSticks +
                   ", Triggers=" + Triggers +
                   ", Grips=" + Grips +
                   "]";
        }
    }
}
