// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// A stuct that represents the controller capabilities.
    /// </summary>
    public struct GamePadCapabilities
    {
        private const Buttons ButtonLeftXThumbStick = (Buttons.LeftThumbstickLeft & Buttons.LeftThumbstickRight);
        private const Buttons ButtonLeftYThumbStick = (Buttons.LeftThumbstickDown & Buttons.LeftThumbstickUp);
        private const Buttons ButtonRightXThumbStick = (Buttons.RightThumbstickLeft & Buttons.RightThumbstickRight);
        private const Buttons ButtonRightYThumbStick = (Buttons.RightThumbstickDown & Buttons.RightThumbstickUp);

        private const uint CapsConnected = (1 << 0);
        private const uint CapsLeftVibrationMotor = (1 << 1);
        private const uint CapsRightVibrationMotor = (1 << 2);
        private const uint CapsVoiceSupport = (1 << 3);

        private Buttons _hasButtons;
        private uint _hasCaps;
        private GamePadType _gamePadType;

        [Obsolete]
        public GamePadCapabilities(
            GamePadType gamePadType, string displayName, string identifier, bool isConnected, 
            bool hasAButton, bool hasBButton, bool hasXButton, bool hasYButton,            
            bool hasStartButton, bool hasBackButton,  bool hasBigButton, 
            bool hasDPadDownButton, bool hasDPadLeftButton, bool hasDPadRightButton, bool hasDPadUpButton, 
            bool hasLeftShoulderButton, bool hasLeftStickButton, 
            bool hasRightShoulderButton, bool hasRightStickButton,            
            bool hasLeftXThumbStick, bool hasLeftYThumbStick, 
            bool hasRightXThumbStick, bool hasRightYThumbStick, 
            bool hasLeftTrigger, bool hasRightTrigger, 
            bool hasLeftVibrationMotor, bool hasRightVibrationMotor, 
            bool hasVoiceSupport) : this()
        {
            _gamePadType = gamePadType;
            DisplayName = displayName;
            Identifier = identifier;
            IsConnected = isConnected;
            HasAButton = hasAButton;
            HasBButton = hasBButton;
            HasXButton = hasXButton;
            HasYButton = hasYButton;
            HasStartButton = hasStartButton;
            HasBackButton = hasBackButton;
            HasBigButton = hasBigButton;
            HasDPadDownButton = hasDPadDownButton;
            HasDPadLeftButton = hasDPadLeftButton;
            HasDPadRightButton = hasDPadRightButton;
            HasDPadUpButton = hasDPadUpButton;
            HasLeftShoulderButton = hasLeftShoulderButton;
            HasLeftStickButton = hasLeftStickButton;
            HasRightShoulderButton = hasRightShoulderButton;
            HasRightStickButton = hasRightStickButton;
            HasLeftXThumbStick = hasLeftXThumbStick;
            HasLeftYThumbStick = hasLeftYThumbStick;
            HasRightXThumbStick = hasRightXThumbStick;
            HasRightYThumbStick = hasRightYThumbStick;
            HasLeftTrigger = hasLeftTrigger;
            HasRightTrigger=hasRightTrigger;
            HasLeftVibrationMotor = hasLeftVibrationMotor;
            HasRightVibrationMotor=hasRightVibrationMotor;
            HasVoiceSupport = hasVoiceSupport;
        }

        internal GamePadCapabilities(
            GamePadType gamePadType, string displayName, string identifier, bool isConnected,
            Buttons buttons,
            bool hasLeftVibrationMotor, bool hasRightVibrationMotor,
            bool hasVoiceSupport) : this()
        {
            bool hasLeftThumbstickLeft = (_hasButtons & Buttons.LeftThumbstickLeft) == Buttons.LeftThumbstickLeft;
            bool hasLeftThumbstickRight = (_hasButtons & Buttons.LeftThumbstickRight) == Buttons.LeftThumbstickRight;
            bool hasLeftThumbstickDown = (_hasButtons & Buttons.LeftThumbstickDown) == Buttons.LeftThumbstickDown;
            bool hasLeftThumbstickUp = (_hasButtons & Buttons.LeftThumbstickUp) == Buttons.LeftThumbstickUp;
            bool hasRightThumbstickLeft = (_hasButtons & Buttons.RightThumbstickLeft) == Buttons.RightThumbstickLeft;
            bool hasRightThumbstickRight = (_hasButtons & Buttons.RightThumbstickRight) == Buttons.RightThumbstickRight;
            bool hasRightThumbstickDown = (_hasButtons & Buttons.RightThumbstickDown) == Buttons.RightThumbstickDown;
            bool hasRightThumbstickUp = (_hasButtons & Buttons.RightThumbstickUp) == Buttons.RightThumbstickUp;
            Debug.Assert(hasLeftThumbstickLeft == hasLeftThumbstickRight); // ButtonLeftXThumbStick
            Debug.Assert(hasLeftThumbstickDown == hasLeftThumbstickUp); // ButtonLeftYThumbStick
            Debug.Assert(hasRightThumbstickLeft == hasRightThumbstickRight); // ButtonRightXThumbStick
            Debug.Assert(hasRightThumbstickDown == hasRightThumbstickUp); // ButtonRightYThumbStick

            this._gamePadType = gamePadType;
            this.DisplayName = displayName;
            this.Identifier = identifier;
            this.IsConnected = isConnected;
            this._hasButtons = buttons;
            this.HasLeftVibrationMotor = hasLeftVibrationMotor;
            this.HasRightVibrationMotor = hasRightVibrationMotor;
            this.HasVoiceSupport = hasVoiceSupport;
        }

        /// <summary>
        /// Gets a value indicating if the controller is connected.
        /// </summary>
        /// <value><c>true</c> if it is connected; otherwise, <c>false</c>.</value>
        public bool IsConnected
        {
            get { return (_hasCaps & CapsConnected) == CapsConnected; }
            internal set
            {
                _hasCaps = (value)
                         ? (_hasCaps | CapsConnected)
                         : (_hasCaps & ~CapsConnected);
            }
        }

        /// <summary>
        /// Gets the gamepad display name.
        /// 
        /// This property is not available in XNA.
        /// </summary>
        /// <value>String representing the display name of the gamepad.</value>
        public string DisplayName { get; internal set; }

        /// <summary>
        /// Gets the unique identifier of the gamepad.
        /// 
        /// This property is not available in XNA.
        /// </summary>
        /// <value>String representing the unique identifier of the gamepad.</value>
        public string Identifier { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the controller has the button A.
        /// </summary>
        /// <value><c>true</c> if it has the button A; otherwise, <c>false</c>.</value>
        public bool HasAButton 
        {
            get { return (_hasButtons & Buttons.A) == Buttons.A; }
            internal set 
            {
                _hasButtons = (value)
                            ? (_hasButtons |  Buttons.A)
                            : (_hasButtons & ~Buttons.A);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the controller has the button Back.
        /// </summary>
        /// <value><c>true</c> if it has the button Back; otherwise, <c>false</c>.</value>
        public bool HasBackButton
        {
            get { return (_hasButtons & Buttons.Back) == Buttons.Back; }
            internal set
            {
                _hasButtons = (value)
                            ? (_hasButtons | Buttons.Back)
                            : (_hasButtons & ~Buttons.Back);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the controller has the button B.
        /// </summary>
        /// <value><c>true</c> if it has the button B; otherwise, <c>false</c>.</value>
        public bool HasBButton
        {
            get { return (_hasButtons & Buttons.B) == Buttons.B; }
            internal set
            {
                _hasButtons = (value)
                            ? (_hasButtons | Buttons.B)
                            : (_hasButtons & ~Buttons.B);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the controller has the directional pad down button.
        /// </summary>
        /// <value><c>true</c> if it has the directional pad down button; otherwise, <c>false</c>.</value>
        public bool HasDPadDownButton
        {
            get { return (_hasButtons & Buttons.DPadDown) == Buttons.DPadDown; }
            internal set
            {
                _hasButtons = (value)
                            ? (_hasButtons | Buttons.DPadDown)
                            : (_hasButtons & ~Buttons.DPadDown);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the controller has the directional pad left button.
        /// </summary>
        /// <value><c>true</c> if it has the directional pad left button; otherwise, <c>false</c>.</value>
        public bool HasDPadLeftButton
        {
            get { return (_hasButtons & Buttons.DPadLeft) == Buttons.DPadLeft; }
            internal set
            {
                _hasButtons = (value)
                            ? (_hasButtons | Buttons.DPadLeft)
                            : (_hasButtons & ~Buttons.DPadLeft);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the controller has the directional pad right button.
        /// </summary>
        /// <value><c>true</c> if it has the directional pad right button; otherwise, <c>false</c>.</value>
        public bool HasDPadRightButton
        {
            get { return (_hasButtons & Buttons.DPadRight) == Buttons.DPadRight; }
            internal set
            {
                _hasButtons = (value)
                            ? (_hasButtons | Buttons.DPadRight)
                            : (_hasButtons & ~Buttons.DPadRight);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the controller has the directional pad up button.
        /// </summary>
        /// <value><c>true</c> if it has the directional pad up button; otherwise, <c>false</c>.</value>
        public bool HasDPadUpButton
        {
            get { return (_hasButtons & Buttons.DPadUp) == Buttons.DPadUp; }
            internal set
            {
                _hasButtons = (value)
                            ? (_hasButtons | Buttons.DPadUp)
                            : (_hasButtons & ~Buttons.DPadUp);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the controller has the left shoulder button.
        /// </summary>
        /// <value><c>true</c> if it has the left shoulder button; otherwise, <c>false</c>.</value>
        public bool HasLeftShoulderButton
        {
            get { return (_hasButtons & Buttons.LeftShoulder) == Buttons.LeftShoulder; }
            internal set
            {
                _hasButtons = (value)
                            ? (_hasButtons | Buttons.LeftShoulder)
                            : (_hasButtons & ~Buttons.LeftShoulder);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the controller has the left stick button.
        /// </summary>
        /// <value><c>true</c> if it has the left stick button; otherwise, <c>false</c>.</value>
        public bool HasLeftStickButton
        {
            get { return (_hasButtons & Buttons.LeftStick) == Buttons.LeftStick; }
            internal set
            {
                _hasButtons = (value)
                            ? (_hasButtons | Buttons.LeftStick)
                            : (_hasButtons & ~Buttons.LeftStick);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the controller has the right shoulder button.
        /// </summary>
        /// <value><c>true</c> if it has the right shoulder button; otherwise, <c>false</c>.</value>
        public bool HasRightShoulderButton
        {
            get { return (_hasButtons & Buttons.RightShoulder) == Buttons.RightShoulder; }
            internal set
            {
                _hasButtons = (value)
                            ? (_hasButtons | Buttons.RightShoulder)
                            : (_hasButtons & ~Buttons.RightShoulder);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the controller has the right stick button.
        /// </summary>
        /// <value><c>true</c> if it has the right stick button; otherwise, <c>false</c>.</value>
        public bool HasRightStickButton
        {
            get { return (_hasButtons & Buttons.RightStick) == Buttons.RightStick; }
            internal set
            {
                _hasButtons = (value)
                            ? (_hasButtons | Buttons.RightStick)
                            : (_hasButtons & ~Buttons.RightStick);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the controller has the button Start.
        /// </summary>
        /// <value><c>true</c> if it has the button Start; otherwise, <c>false</c>.</value>
        public bool HasStartButton
        {
            get { return (_hasButtons & Buttons.Start) == Buttons.Start; }
            internal set
            {
                _hasButtons = (value)
                            ? (_hasButtons | Buttons.Start)
                            : (_hasButtons & ~Buttons.Start);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the controller has the button X.
        /// </summary>
        /// <value><c>true</c> if it has the button X; otherwise, <c>false</c>.</value>
        public bool HasXButton
        {
            get { return (_hasButtons & Buttons.X) == Buttons.X; }
            internal set
            {
                _hasButtons = (value)
                            ? (_hasButtons | Buttons.X)
                            : (_hasButtons & ~Buttons.X);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the controller has the button Y.
        /// </summary>
        /// <value><c>true</c> if it has the button Y; otherwise, <c>false</c>.</value>
        public bool HasYButton
        {
            get { return (_hasButtons & Buttons.Y) == Buttons.Y; }
            internal set
            {
                _hasButtons = (value)
                            ? (_hasButtons | Buttons.Y)
                            : (_hasButtons & ~Buttons.Y);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the controller has the guide button.
        /// </summary>
        /// <value><c>true</c> if it has the guide button; otherwise, <c>false</c>.</value>
        public bool HasBigButton
        {
            get { return (_hasButtons & Buttons.BigButton) == Buttons.BigButton; }
            internal set
            {
                _hasButtons = (value)
                            ? (_hasButtons | Buttons.BigButton)
                            : (_hasButtons & ~Buttons.BigButton);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the controller has X axis for the left stick (thumbstick) button.
        /// </summary>
        /// <value><c>true</c> if it has X axis for the left stick (thumbstick) button; otherwise, <c>false</c>.</value>
        public bool HasLeftXThumbStick
        {
            get { return (_hasButtons & ButtonLeftXThumbStick) == ButtonLeftXThumbStick; }
            internal set
            {
                _hasButtons = (value)
                            ? (_hasButtons | ButtonLeftXThumbStick)
                            : (_hasButtons & ~ButtonLeftXThumbStick);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the controller has Y axis for the left stick (thumbstick) button.
        /// </summary>
        /// <value><c>true</c> if it has Y axis for the left stick (thumbstick) button; otherwise, <c>false</c>.</value>
        public bool HasLeftYThumbStick
        {
            get { return (_hasButtons & ButtonLeftYThumbStick) == ButtonLeftYThumbStick; }
            internal set
            {
                _hasButtons = (value)
                            ? (_hasButtons | ButtonLeftYThumbStick)
                            : (_hasButtons & ~ButtonLeftYThumbStick);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the controller has X axis for the right stick (thumbstick) button.
        /// </summary>
        /// <value><c>true</c> if it has X axis for the right stick (thumbstick) button; otherwise, <c>false</c>.</value>
        public bool HasRightXThumbStick
        {
            get { return (_hasButtons & ButtonRightXThumbStick) == ButtonRightXThumbStick; }
            internal set
            {
                _hasButtons = (value)
                            ? (_hasButtons | ButtonRightXThumbStick)
                            : (_hasButtons & ~ButtonRightXThumbStick);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the controller has Y axis for the right stick (thumbstick) button.
        /// </summary>
        /// <value><c>true</c> if it has Y axis for the right stick (thumbstick) button; otherwise, <c>false</c>.</value>
        public bool HasRightYThumbStick
        {
            get { return (_hasButtons & ButtonRightYThumbStick) == ButtonRightYThumbStick; }
            internal set
            {
                _hasButtons = (value)
                            ? (_hasButtons | ButtonRightYThumbStick)
                            : (_hasButtons & ~ButtonRightYThumbStick);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the controller has the left trigger button.
        /// </summary>
        /// <value><c>true</c> if it has the left trigger button; otherwise, <c>false</c>.</value>
        public bool HasLeftTrigger
        {
            get { return (_hasButtons & Buttons.LeftTrigger) == Buttons.LeftTrigger; }
            internal set
            {
                _hasButtons = (value)
                            ? (_hasButtons | Buttons.LeftTrigger)
                            : (_hasButtons & ~Buttons.LeftTrigger);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the controller has the right trigger button.
        /// </summary>
        /// <value><c>true</c> if it has the right trigger button; otherwise, <c>false</c>.</value>
        public bool HasRightTrigger
        {
            get { return (_hasButtons & Buttons.RightTrigger) == Buttons.RightTrigger; }
            internal set
            {
                _hasButtons = (value)
                            ? (_hasButtons | Buttons.RightTrigger)
                            : (_hasButtons & ~Buttons.RightTrigger);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the controller has the left vibration motor.
        /// </summary>
        /// <value><c>true</c> if it has the left vibration motor; otherwise, <c>false</c>.</value>
        public bool HasLeftVibrationMotor
        {
            get { return (_hasCaps & CapsLeftVibrationMotor) == CapsLeftVibrationMotor; }
            internal set
            {
                _hasCaps = (value)
                         ? (_hasCaps | CapsLeftVibrationMotor)
                         : (_hasCaps & ~CapsLeftVibrationMotor);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the controller has the right vibration motor.
        /// </summary>
        /// <value><c>true</c> if it has the right vibration motor; otherwise, <c>false</c>.</value>
        public bool HasRightVibrationMotor
        {
            get { return (_hasCaps & CapsRightVibrationMotor) == CapsRightVibrationMotor; }
            internal set
            {
                _hasCaps = (value)
                         ? (_hasCaps | CapsRightVibrationMotor)
                         : (_hasCaps & ~CapsRightVibrationMotor);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the controller has a microphone.
        /// </summary>
        /// <value><c>true</c> if it has a microphone; otherwise, <c>false</c>.</value>
        public bool HasVoiceSupport
        {
            get { return (_hasCaps & CapsVoiceSupport) == CapsVoiceSupport; }
            internal set
            {
                _hasCaps = (value)
                         ? (_hasCaps | CapsVoiceSupport)
                         : (_hasCaps & ~CapsVoiceSupport);
            }
        }

        /// <summary>
        /// Gets the type of the controller.
        /// </summary>
        /// <value>A <see cref="GamePadType"/> representing the controller type..</value>
        public GamePadType GamePadType
        {
            get { return _gamePadType; }
            internal set { _gamePadType = value; }
        }

        /// <summary>
        /// Determines whether a specified instance of <see cref="Microsoft.Xna.Framework.Input.GamePadCapabilities"/>
        /// is equal to another specified <see cref="Microsoft.Xna.Framework.Input.GamePadCapabilities"/>.
        /// </summary>
        /// <param name="left">The first <see cref="Microsoft.Xna.Framework.Input.GamePadCapabilities"/> to compare.</param>
        /// <param name="right">The second <see cref="Microsoft.Xna.Framework.Input.GamePadCapabilities"/> to compare.</param>
        /// <returns><c>true</c> if <c>left</c> and <c>right</c> are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(GamePadCapabilities left, GamePadCapabilities right)
        {
            bool eq = true;

            eq &= (left.DisplayName == right.DisplayName);
            eq &= (left.Identifier == right.Identifier);
            eq &= (left._hasButtons == right._hasButtons);
            eq &= (left._hasCaps == right._hasCaps);
            eq &= (left._gamePadType == right._gamePadType);

            return eq;
        }

        /// <summary>
        /// Determines whether a specified instance of <see cref="Microsoft.Xna.Framework.Input.GamePadCapabilities"/>
        /// is not equal to another specified <see cref="Microsoft.Xna.Framework.Input.GamePadCapabilities"/>.
        /// </summary>
        /// <param name="left">The first <see cref="Microsoft.Xna.Framework.Input.GamePadCapabilities"/> to compare.</param>
        /// <param name="right">The second <see cref="Microsoft.Xna.Framework.Input.GamePadCapabilities"/> to compare.</param>
        /// <returns><c>true</c> if <c>left</c> and <c>right</c> are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(GamePadCapabilities left, GamePadCapabilities right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadCapabilities"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadCapabilities"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="object"/> is equal to the current
        /// <see cref="T:Microsoft.Xna.Framework.Input.GamePadCapabilities"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return (obj is GamePadCapabilities) && (this == (GamePadCapabilities)obj);
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="T:Microsoft.Xna.Framework.Input.GamePadCapabilities"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            return Identifier.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadCapabilities"/>.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadCapabilities"/>.</returns>
        public override string ToString()
        {
            return "[GamePadCapabilities: IsConnected=" + IsConnected +
                ", DisplayName=" + DisplayName +
                ", Identifier=" + Identifier +
                ", HasAButton=" + HasAButton +
                ", HasBackButton=" + HasBackButton +
                ", HasBButton=" + HasBButton +
                ", HasDPadDownButton=" + HasDPadDownButton +
                ", HasDPadLeftButton=" + HasDPadLeftButton +
                ", HasDPadRightButton=" + HasDPadRightButton +
                ", HasDPadUpButton=" + HasDPadUpButton +
                ", HasLeftShoulderButton=" + HasLeftShoulderButton +
                ", HasLeftStickButton=" + HasLeftStickButton +
                ", HasRightShoulderButton=" + HasRightShoulderButton +
                ", HasRightStickButton=" + HasRightStickButton +
                ", HasStartButton=" + HasStartButton +
                ", HasXButton=" + HasXButton +
                ", HasYButton=" + HasYButton +
                ", HasBigButton=" + HasBigButton +
                ", HasLeftXThumbStick=" + HasLeftXThumbStick +
                ", HasLeftYThumbStick=" + HasLeftYThumbStick +
                ", HasRightXThumbStick=" + HasRightXThumbStick +
                ", HasRightYThumbStick=" + HasRightYThumbStick +
                ", HasLeftTrigger=" + HasLeftTrigger +
                ", HasRightTrigger=" + HasRightTrigger +
                ", HasLeftVibrationMotor=" + HasLeftVibrationMotor +
                ", HasRightVibrationMotor=" + HasRightVibrationMotor +
                ", HasVoiceSupport=" + HasVoiceSupport +
                ", GamePadType=" + GamePadType +
                "]";
        }
    }
}
