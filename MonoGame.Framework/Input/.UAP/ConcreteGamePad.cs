// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Windows.ApplicationModel;
using WGI = Windows.Gaming.Input;

namespace Microsoft.Xna.Platform.Input
{
    // TODO: move GamePadDevice to Framework.Input library
    public abstract class GamePadDevice
    {
        public GamePadCapabilities Capabilities;

        public GamePadDevice()
        {

        }
    }

    public sealed class ConcreteGamePad : GamePadStrategy
    {
        // Attempts to mimic SharpDX.XInput.Gamepad which defines the trigger threshold as 30 with a range of 0 to 255. 
        // The trigger here has a range of 0.0 to 1.0. So, 30 / 255 = 0.11765.
        private const double TriggerThreshold = 0.11765;

        const int MaxNumberOfGamePads = 16;

        internal bool Back;

        private WGI.Gamepad[] _gamepads;
        private int _initGamepadsCount;

        // Default & SDL Xbox Controller dead zones
        // Based on the XInput constants
        public override float LeftThumbDeadZone { get { return 0.24f; } }
        public override float RightThumbDeadZone { get { return 0.265f; } }

        public ConcreteGamePad()
        {
            _gamepads = new WGI.Gamepad[MaxNumberOfGamePads];
            IReadOnlyList<WGI.Gamepad> gamepadsTmp = WGI.Gamepad.Gamepads;
            _initGamepadsCount = gamepadsTmp.Count; // workaround UAP bug. first call to 'WGI.Gamepad.Gamepads' returns an empty instance.
            IReadOnlyList<WGI.Gamepad> gamepads = WGI.Gamepad.Gamepads;
            for (int i = 0; i < _gamepads.Length && i < gamepads.Count; i++)
                _gamepads[i] = gamepads[i];

            WGI.Gamepad.GamepadAdded += WGIGamepad_GamepadAdded;
            WGI.Gamepad.GamepadRemoved += WGIGamepad_GamepadRemoved;
        }

        private void WGIGamepad_GamepadAdded(object sender, WGI.Gamepad device)
        {
            for (int i = 0; i < _gamepads.Length; i++)
            {
                if (_gamepads[i] == null)
                {
                    _gamepads[i] = device;
                    break;
                }
            }
        }

        private void WGIGamepad_GamepadRemoved(object sender, WGI.Gamepad device)
        {
            for (int i = 0; i < _gamepads.Length; i++)
            {
                if (_gamepads[i] == device)
                {
                    _gamepads[i] = null;
                    break;
                }
            }
        }

        public override int PlatformGetMaxNumberOfGamePads()
        {
            return MaxNumberOfGamePads;
        }

        private GamePadCapabilities GetDefaultCapabilities()
        {
            return base.CreateGamePadCapabilities(
                    gamePadType: GamePadType.Unknown,
                    displayName: null,
                    identifier: null,
                    isConnected: false,
                    buttons: (Buttons)0,
                    hasLeftVibrationMotor: false,
                    hasRightVibrationMotor: false,
                    hasVoiceSupport: false
                );
        }

        public override GamePadCapabilities PlatformGetCapabilities(int index)
        {
            WGI.Gamepad gamepad = _gamepads[index];
            if (gamepad == null)
                return GetDefaultCapabilities();

            //--
            GamePadType gamePadType = GamePadType.Unknown;
            string displayName = String.Empty;
            string identifier = String.Empty;
            bool isConnected;
            Buttons buttons = (Buttons)0;
            bool hasLeftVibrationMotor = false;
            bool hasRightVibrationMotor = false;
            bool hasVoiceSupport = false;
            //--

            // we can't check gamepad capabilities for most stuff with Windows.Gaming.Input.Gamepad
            {
                isConnected = true;
                gamePadType = GamePadType.GamePad;
                buttons |= Buttons.A;
                buttons |= Buttons.B;
                buttons |= Buttons.X;
                buttons |= Buttons.Y;
                buttons |= Buttons.Back;
                buttons |= Buttons.Start;
                buttons |= Buttons.DPadDown;
                buttons |= Buttons.DPadLeft;
                buttons |= Buttons.DPadRight;
                buttons |= Buttons.DPadUp;
                buttons |= Buttons.LeftShoulder;
                buttons |= Buttons.RightShoulder;
                buttons |= Buttons.LeftStick;
                buttons |= Buttons.RightStick;
                buttons |= Buttons.LeftTrigger;
                buttons |= Buttons.RightTrigger;
                buttons |= Buttons.LeftThumbstickLeft | Buttons.LeftThumbstickRight;
                buttons |= Buttons.LeftThumbstickDown | Buttons.LeftThumbstickUp;
                buttons |= Buttons.RightThumbstickLeft | Buttons.RightThumbstickRight;
                buttons |= Buttons.RightThumbstickDown | Buttons.RightThumbstickUp;
                hasLeftVibrationMotor = true;
                hasRightVibrationMotor = true;
                hasVoiceSupport = (gamepad.Headset != null && !string.IsNullOrEmpty(gamepad.Headset.CaptureDeviceId));
            };

            return base.CreateGamePadCapabilities(
                    gamePadType: gamePadType,
                    displayName: displayName,
                    identifier: identifier,
                    isConnected: isConnected,
                    buttons: buttons,
                    hasLeftVibrationMotor: hasLeftVibrationMotor,
                    hasRightVibrationMotor: hasRightVibrationMotor,
                    hasVoiceSupport: hasVoiceSupport
                );
        }

        private GamePadState GetDefaultState()
        {
            GamePadButtons buttons = new GamePadButtons((Back) ? Buttons.Back : 0);
            return base.CreateGamePadState(default(GamePadThumbSticks), default(GamePadTriggers), buttons, default(GamePadDPad));
        }

        public override GamePadState PlatformGetState(int index, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
        {
            WGI.Gamepad gamepad = _gamepads[index];
            if (gamepad == null)
                return (index == 0 ? GetDefaultState() : new GamePadState());
            
            WGI.GamepadReading state = gamepad.GetCurrentReading();

            GamePadThumbSticks sticks = base.CreateGamePadThumbSticks(
                    new Vector2((float)state.LeftThumbstickX, (float)state.LeftThumbstickY),
                    new Vector2((float)state.RightThumbstickX, (float)state.RightThumbstickY),
                    leftDeadZoneMode,
                    rightDeadZoneMode
                );

            GamePadTriggers triggers = new GamePadTriggers(
                    (float)state.LeftTrigger,
                    (float)state.RightTrigger
                );

            Buttons buttonStates =
                (state.Buttons.HasFlag(WGI.GamepadButtons.A) ? Buttons.A : 0) |
                (state.Buttons.HasFlag(WGI.GamepadButtons.B) ? Buttons.B : 0) |
                ((state.Buttons.HasFlag(WGI.GamepadButtons.View) || Back) ? Buttons.Back : 0) |
                0 | //BigButton is unavailable by Windows.Gaming.Input.Gamepad
                (state.Buttons.HasFlag(WGI.GamepadButtons.LeftShoulder) ? Buttons.LeftShoulder : 0) |
                (state.Buttons.HasFlag(WGI.GamepadButtons.LeftThumbstick) ? Buttons.LeftStick : 0) |
                (state.Buttons.HasFlag(WGI.GamepadButtons.RightShoulder) ? Buttons.RightShoulder : 0) |
                (state.Buttons.HasFlag(WGI.GamepadButtons.RightThumbstick) ? Buttons.RightStick : 0) |
                (state.Buttons.HasFlag(WGI.GamepadButtons.Menu) ? Buttons.Start : 0) |
                (state.Buttons.HasFlag(WGI.GamepadButtons.X) ? Buttons.X : 0) |
                (state.Buttons.HasFlag(WGI.GamepadButtons.Y) ? Buttons.Y : 0) |
                0;

            // Check triggers
            if (triggers.Left > TriggerThreshold)
                buttonStates |= Buttons.LeftTrigger;
            if (triggers.Right > TriggerThreshold)
                buttonStates |= Buttons.RightTrigger;

            GamePadButtons buttons = new GamePadButtons(buttonStates);

            GamePadDPad dpad = new GamePadDPad(
                    state.Buttons.HasFlag(WGI.GamepadButtons.DPadUp)   ? ButtonState.Pressed : ButtonState.Released,
                    state.Buttons.HasFlag(WGI.GamepadButtons.DPadDown) ? ButtonState.Pressed : ButtonState.Released,
                    state.Buttons.HasFlag(WGI.GamepadButtons.DPadLeft)  ? ButtonState.Pressed : ButtonState.Released,
                    state.Buttons.HasFlag(WGI.GamepadButtons.DPadRight) ? ButtonState.Pressed : ButtonState.Released
                );

            return base.CreateGamePadState(sticks, triggers, buttons, dpad,
                                           packetNumber: (int)state.Timestamp);
        }

        public override bool PlatformSetVibration(int index, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger)
        {
            WGI.Gamepad gamepad = _gamepads[index];
            if (gamepad == null)
                return false;

            gamepad.Vibration = new WGI.GamepadVibration
            {
                LeftMotor = leftMotor,
                LeftTrigger = leftTrigger,
                RightMotor = rightMotor,
                RightTrigger = rightTrigger
            };

            return true;
        }
    }
}
