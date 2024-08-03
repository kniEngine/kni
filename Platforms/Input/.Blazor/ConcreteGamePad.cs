// Copyright (C)2022-2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using nkast.Wasm.Dom;
using nkast.Wasm.Input;

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
        const int MaxNumberOfGamePads = 4;

        private readonly BlazorGamePadDevice[] _gamepads = new BlazorGamePadDevice[MaxNumberOfGamePads];

        // Default & SDL Xbox Controller dead zones
        // Based on the XInput constants
        public override float LeftThumbDeadZone { get { return 0.24f; } }
        public override float RightThumbDeadZone { get { return 0.265f; } }

        public ConcreteGamePad()
        {
            try
            {
                Gamepad[] gamepads = Window.Current.Navigator.GetGamepads();
                for (int deviceIndex = 0; deviceIndex < _gamepads.Length && deviceIndex < gamepads.Length; deviceIndex++)
                {
                    if (_gamepads[deviceIndex] != null)
                    {
                        Gamepad gamepad = gamepads[deviceIndex];
                        _gamepads[deviceIndex] = new BlazorGamePadDevice(deviceIndex);
                        _gamepads[deviceIndex].Capabilities = CreateCapabilities(gamepad);
                    }
                }

                Window.Current.OnGamepadConnected += Window_OnGamepadConnected;
                Window.Current.OnGamepadDisconnected += Window_OnGamepadDisconnected;
            }
            catch
            {
            }
        }

        internal void Window_OnGamepadConnected(object sender, int deviceIndex)
        {
            Gamepad[] gamepads = Window.Current.Navigator.GetGamepads();
            Gamepad gamepad = gamepads[deviceIndex];
            _gamepads[deviceIndex] = new BlazorGamePadDevice(deviceIndex);
            _gamepads[deviceIndex].Capabilities = CreateCapabilities(gamepad);
        }

        internal void Window_OnGamepadDisconnected(object sender, int deviceIndex)
        {
            BlazorGamePadDevice gamepad = _gamepads[deviceIndex];

            _gamepads[deviceIndex] = null;
        }


        public override int PlatformGetMaxNumberOfGamePads()
        {
            return MaxNumberOfGamePads;
        }

        public override GamePadCapabilities PlatformGetCapabilities(int index)
        {
            BlazorGamePadDevice gamepadDevice = _gamepads[index];

            if (gamepadDevice == null)
                return GetDefaultCapabilities();

            return gamepadDevice.Capabilities;
        }


        public override GamePadState PlatformGetState(int index, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
        {
            BlazorGamePadDevice gamepadDevice = _gamepads[index];
            GamePadState state = new GamePadState();

            if (gamepadDevice != null)
            {
                Gamepad[] gamepads = Window.Current.Navigator.GetGamepads();
                Gamepad gamepad = gamepads[gamepadDevice.DeviceIndex];

                if (gamepad != null && gamepad.Mapping == "standard")
                {
                    state = CreateGamePadState(gamepad, leftDeadZoneMode, rightDeadZoneMode);
                    return state;
                }
            }

            state = base.CreateGamePadState(new GamePadThumbSticks(), new GamePadTriggers(), new GamePadButtons(), new GamePadDPad(),
                    isConnected: false);

            return state;
        }

        public override bool PlatformSetVibration(int index, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger)
        {
            throw new NotImplementedException();
        }


        private GamePadState CreateGamePadState(Gamepad gamepad, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
        {
            GamePadState state;
            GamepadButton[] gpbuttons = gamepad.Buttons;
            float[] axes = gamepad.Axes;

            Vector2 leftStick  = new Vector2(axes[0], -axes[1]);
            Vector2 rightStick = new Vector2(axes[2], -axes[3]);

            GamePadThumbSticks thumbSticks = base.CreateGamePadThumbSticks(leftStick, rightStick, leftDeadZoneMode, rightDeadZoneMode);

            GamePadTriggers triggers = new GamePadTriggers(
                gpbuttons[6].Value, // LeftTrigger
                gpbuttons[7].Value  // RightTrigger
            );

            Buttons buttons = (Buttons)0;
            if (gpbuttons[0].Pressed)
                buttons |= Buttons.A;
            if (gpbuttons[1].Pressed)
                buttons |= Buttons.B;
            if (gpbuttons[2].Pressed)
                buttons |= Buttons.X;
            if (gpbuttons[3].Pressed)
                buttons |= Buttons.Y;
            if (gpbuttons[4].Pressed)
                buttons |= Buttons.LeftShoulder;
            if (gpbuttons[5].Pressed)
                buttons |= Buttons.RightShoulder;
            if (gpbuttons[6].Pressed)
                buttons |= Buttons.LeftTrigger;
            if (gpbuttons[7].Pressed)
                buttons |= Buttons.RightTrigger;
            if (gpbuttons[8].Pressed)
                buttons |= Buttons.Back;
            if (gpbuttons[9].Pressed)
                buttons |= Buttons.Start;
            if (gpbuttons[10].Pressed)
                buttons |= Buttons.LeftStick;
            if (gpbuttons[11].Pressed)
                buttons |= Buttons.RightStick;
            if (gpbuttons[16].Pressed)
                buttons |= Buttons.BigButton;

            GamePadDPad dPad = new GamePadDPad(
                (gpbuttons[12].Pressed) ? ButtonState.Pressed : ButtonState.Released, // DPadUp
                (gpbuttons[13].Pressed) ? ButtonState.Pressed : ButtonState.Released, // DpadDown
                (gpbuttons[14].Pressed) ? ButtonState.Pressed : ButtonState.Released, // DpadLeft
                (gpbuttons[15].Pressed) ? ButtonState.Pressed : ButtonState.Released  // DpadRight
            );

            int packetNumber = gamepad.Timestamp;


            state = base.CreateGamePadState(thumbSticks, triggers, new GamePadButtons(buttons), dPad,
                                                       packetNumber: packetNumber);
            return state;
        }

        private GamePadCapabilities CreateCapabilities(Gamepad gamepad)
        {
            //--
            GamePadType gamePadType = GamePadType.Unknown;
            string displayName = String.Empty;
            string identifier = String.Empty;
            bool isConnected = false;
            Buttons buttons = (Buttons)0;
            bool hasLeftVibrationMotor = false;
            bool hasRightVibrationMotor = false;
            bool hasVoiceSupport = false;
            //--

            {
                displayName = gamepad.Id;

                if (gamepad.Mapping == "standard")
                {
                    isConnected = true;
                    gamePadType = GamePadType.GamePad;
                    buttons |= Buttons.A;
                    buttons |= Buttons.B;
                    buttons |= Buttons.X;
                    buttons |= Buttons.Y;
                    buttons |= Buttons.LeftShoulder;
                    buttons |= Buttons.RightShoulder;
                    buttons |= Buttons.LeftTrigger;
                    buttons |= Buttons.RightTrigger;
                    buttons |= Buttons.Back;
                    buttons |= Buttons.Start;
                    buttons |= Buttons.LeftStick;
                    buttons |= Buttons.RightStick;
                    buttons |= Buttons.DPadUp;
                    buttons |= Buttons.DPadDown;
                    buttons |= Buttons.DPadLeft;
                    buttons |= Buttons.DPadRight;
                    buttons |= Buttons.BigButton;
                }
            }

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
    }
}
