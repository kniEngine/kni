// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XInput = SharpDX.XInput;
using GPBF = SharpDX.XInput.GamepadButtonFlags;

namespace Microsoft.Xna.Platform.Input
{
    public sealed class ConcreteGamePad : GamePadStrategy
    {
        const int DeviceNotConnectedHResult = unchecked((int)0x8007048f);

        internal bool Back;

        private static readonly XInput.Controller[] _controllers = new[]
        {
            new XInput.Controller(XInput.UserIndex.One),
            new XInput.Controller(XInput.UserIndex.Two),
            new XInput.Controller(XInput.UserIndex.Three),
            new XInput.Controller(XInput.UserIndex.Four),
        };

        private readonly bool[] _connected = new bool[4];
        private readonly long[] _timeout = new long[4];
        private readonly long TimeoutTicks = TimeSpan.FromSeconds(1).Ticks;

        // XInput Xbox Controller dead zones
        // Dead zones are slightly different between left and right sticks, this may come from Microsoft usability tests
        public override float LeftThumbDeadZone { get { return XInput.Gamepad.LeftThumbDeadZone / (float)short.MaxValue; } }
        public override float RightThumbDeadZone { get { return XInput.Gamepad.RightThumbDeadZone / (float)short.MaxValue; } }

        public override int PlatformGetMaxNumberOfGamePads()
        {
            return 4;
        }

        public override GamePadCapabilities PlatformGetCapabilities(int index)
        {
            // If the device was disconneced then wait for 
            // the timeout to elapsed before we test it again.
            if (!_connected[index] && !HasDisconnectedTimeoutElapsed(index))
                return new GamePadCapabilities();

            // Check to see if the device is connected.
            XInput.Controller controller = _controllers[index];
            _connected[index] = controller.IsConnected;

            // If the device is disconnected retry it after the
            // timeout period has elapsed to avoid the overhead.
            if (!_connected[index])
            {
                SetDisconnectedTimeout(index);
                return new GamePadCapabilities();
            }

            XInput.Capabilities capabilities;
            try
            {
                capabilities = controller.GetCapabilities(XInput.DeviceQueryType.Any);
            }
            catch (SharpDX.SharpDXException ex)
            {
                if (ex.ResultCode.Code == DeviceNotConnectedHResult)
                {
                    _connected[index] = false;
                    SetDisconnectedTimeout(index);
                    return new GamePadCapabilities();
                }
                throw;
            }

            GamePadCapabilities ret = new GamePadCapabilities();
            switch (capabilities.SubType)
            {
#if DIRECTX11_1
                case XInput.DeviceSubType.ArcadePad:
                    Debug.WriteLine("XInput's DeviceSubType.ArcadePad is not supported in XNA");
                    ret.GamePadType = Input.GamePadType.Unknown; // TODO: Should this be BigButtonPad?
                    break;
                case XInput.DeviceSubType.FlightStick:
                    ret.GamePadType = Input.GamePadType.FlightStick;
                    break;
                case XInput.DeviceSubType.GuitarAlternate:
                    ret.GamePadType = Input.GamePadType.AlternateGuitar;
                    break;
                case XInput.DeviceSubType.GuitarBass:
                    // Note: XNA doesn't distinguish between Guitar and GuitarBass, but 
                    // GuitarBass is identical to Guitar in XInput, distinguished only
                    // to help setup for those controllers. 
                    ret.GamePadType = Input.GamePadType.Guitar;
                    break;
                case XInput.DeviceSubType.Unknown:
                    ret.GamePadType = Input.GamePadType.Unknown;
                    break;
#endif
                case XInput.DeviceSubType.ArcadeStick:
                    ret.GamePadType = GamePadType.ArcadeStick;
                    break;
                case XInput.DeviceSubType.DancePad:
                    ret.GamePadType = GamePadType.DancePad;
                    break;
                case XInput.DeviceSubType.DrumKit:
                    ret.GamePadType = GamePadType.DrumKit;
                    break;

                case XInput.DeviceSubType.Gamepad:
                    ret.GamePadType = GamePadType.GamePad;
                    break;
                case XInput.DeviceSubType.Guitar:
                    ret.GamePadType = GamePadType.Guitar;
                    break;
                case XInput.DeviceSubType.Wheel:
                    ret.GamePadType = GamePadType.Wheel;
                    break;
                default:
                    Debug.WriteLine("unexpected XInput DeviceSubType: {0}", capabilities.SubType.ToString());
                    ret.GamePadType = GamePadType.Unknown;
                    break;
            }

            XInput.Gamepad gamepad = capabilities.Gamepad;

            // digital buttons
            XInput.GamepadButtonFlags buttons = gamepad.Buttons;
            ret.HasAButton = (buttons & GPBF.A) == GPBF.A;
            ret.HasBackButton = (buttons & GPBF.Back) == GPBF.Back;
            ret.HasBButton = (buttons & GPBF.B) == GPBF.B;
            ret.HasBigButton = false; // TODO: what IS this? Is it related to GamePadType.BigGamePad?
            ret.HasDPadDownButton = (buttons & GPBF.DPadDown) == GPBF.DPadDown;
            ret.HasDPadLeftButton = (buttons & GPBF.DPadLeft) == GPBF.DPadLeft;
            ret.HasDPadRightButton = (buttons & GPBF.DPadRight) == GPBF.DPadRight;
            ret.HasDPadUpButton = (buttons & GPBF.DPadUp) == GPBF.DPadUp;
            ret.HasLeftShoulderButton = (buttons & GPBF.LeftShoulder) == GPBF.LeftShoulder;
            ret.HasLeftStickButton = (buttons & GPBF.LeftThumb) == GPBF.LeftThumb;
            ret.HasRightShoulderButton = (buttons & GPBF.RightShoulder) == GPBF.RightShoulder;
            ret.HasRightStickButton = (buttons & GPBF.RightThumb) == GPBF.RightThumb;
            ret.HasStartButton = (buttons & GPBF.Start) == GPBF.Start;
            ret.HasXButton = (buttons & GPBF.X) == GPBF.X;
            ret.HasYButton = (buttons & GPBF.Y) == GPBF.Y;

            // analog controls
            ret.HasRightTrigger = gamepad.RightTrigger > 0;
            ret.HasRightXThumbStick = gamepad.RightThumbX != 0;
            ret.HasRightYThumbStick = gamepad.RightThumbY != 0;
            ret.HasLeftTrigger = gamepad.LeftTrigger > 0;
            ret.HasLeftXThumbStick = gamepad.LeftThumbX != 0;
            ret.HasLeftYThumbStick = gamepad.LeftThumbY != 0;

            // vibration
#if DIRECTX11_1
            bool hasForceFeedback = (capabilities.Flags & XInput.CapabilityFlags.FfbSupported) == XInput.CapabilityFlags.FfbSupported;
            ret.HasLeftVibrationMotor = hasForceFeedback && capabilities.Vibration.LeftMotorSpeed > 0;
            ret.HasRightVibrationMotor = hasForceFeedback && capabilities.Vibration.RightMotorSpeed > 0;
#else
            ret.HasLeftVibrationMotor = (capabilities.Vibration.LeftMotorSpeed > 0);
            ret.HasRightVibrationMotor = (capabilities.Vibration.RightMotorSpeed > 0);
#endif

            // other
            ret.IsConnected = controller.IsConnected;
            ret.HasVoiceSupport = (capabilities.Flags & XInput.CapabilityFlags.VoiceSupported) == XInput.CapabilityFlags.VoiceSupported;

            return ret;
        }

        private GamePadState GetDefaultState()
        {
            GamePadState state = new GamePadState();
            state.Buttons = new GamePadButtons(Back ? Buttons.Back : 0);
            return state;
        }

        public override GamePadState PlatformGetState(int index, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
        {
            // If the device was disconneced then wait for 
            // the timeout to elapsed before we test it again.
            if (!_connected[index] && !HasDisconnectedTimeoutElapsed(index))
                return GetDefaultState();

            int packetNumber = 0;

            // Try to get the controller state.
            XInput.Gamepad gamepad = new XInput.Gamepad();
            try
            {
                XInput.State xistate;
                XInput.Controller controller = _controllers[index];
                _connected[index] = controller.GetState(out xistate);
                packetNumber = xistate.PacketNumber;
                gamepad = xistate.Gamepad;
            }
            catch (Exception)
            {
            }

            // If the device is disconnected retry it after the
            // timeout period has elapsed to avoid the overhead.
            if (!_connected[index])
            {
                SetDisconnectedTimeout(index);
                return GetDefaultState();
            }

            GamePadThumbSticks thumbSticks = new GamePadThumbSticks(
                leftPosition: new Vector2(gamepad.LeftThumbX, gamepad.LeftThumbY) / (float)short.MaxValue,
                rightPosition: new Vector2(gamepad.RightThumbX, gamepad.RightThumbY) / (float)short.MaxValue,
                    leftDeadZoneMode: leftDeadZoneMode,
                    rightDeadZoneMode: rightDeadZoneMode);

            GamePadTriggers triggers = new GamePadTriggers(
                    leftTrigger: gamepad.LeftTrigger / (float)byte.MaxValue,
                    rightTrigger: gamepad.RightTrigger / (float)byte.MaxValue);

            GamePadDPad dpadState = new GamePadDPad(
                upValue: ConvertToButtonState(gamepad.Buttons, XInput.GamepadButtonFlags.DPadUp),
                downValue: ConvertToButtonState(gamepad.Buttons, XInput.GamepadButtonFlags.DPadDown),
                leftValue: ConvertToButtonState(gamepad.Buttons, XInput.GamepadButtonFlags.DPadLeft),
                rightValue: ConvertToButtonState(gamepad.Buttons, XInput.GamepadButtonFlags.DPadRight));

            GamePadButtons buttons = ConvertToButtons(
                buttonFlags: gamepad.Buttons,
                leftTrigger: gamepad.LeftTrigger,
                rightTrigger: gamepad.RightTrigger);

            GamePadState state = new GamePadState(
                thumbSticks: thumbSticks,
                triggers: triggers,
                buttons: buttons,
                dPad: dpadState);

            state.PacketNumber = packetNumber;

            return state;
        }

        private ButtonState ConvertToButtonState(
            XInput.GamepadButtonFlags buttonFlags,
            XInput.GamepadButtonFlags desiredButton)
        {
            return ((buttonFlags & desiredButton) == desiredButton) ? ButtonState.Pressed : ButtonState.Released;
        }

        private Buttons AddButtonIfPressed(
            XInput.GamepadButtonFlags buttonFlags,
            XInput.GamepadButtonFlags xInputButton,
            Buttons xnaButton)
        {
            ButtonState buttonState = ((buttonFlags & xInputButton) == xInputButton) ? ButtonState.Pressed : ButtonState.Released;
            return buttonState == ButtonState.Pressed ? xnaButton : 0;
        }

        private GamePadButtons ConvertToButtons(XInput.GamepadButtonFlags buttonFlags,
            byte leftTrigger,
            byte rightTrigger)
        {
            Buttons ret = (Buttons)0;
            ret |= AddButtonIfPressed(buttonFlags, GPBF.A, Buttons.A);
            ret |= AddButtonIfPressed(buttonFlags, GPBF.B, Buttons.B);
            ret |= AddButtonIfPressed(buttonFlags, GPBF.Back, Buttons.Back);
            ret |= AddButtonIfPressed(buttonFlags, GPBF.DPadDown, Buttons.DPadDown);
            ret |= AddButtonIfPressed(buttonFlags, GPBF.DPadLeft, Buttons.DPadLeft);
            ret |= AddButtonIfPressed(buttonFlags, GPBF.DPadRight, Buttons.DPadRight);
            ret |= AddButtonIfPressed(buttonFlags, GPBF.DPadUp, Buttons.DPadUp);
            ret |= AddButtonIfPressed(buttonFlags, GPBF.LeftShoulder, Buttons.LeftShoulder);
            ret |= AddButtonIfPressed(buttonFlags, GPBF.RightShoulder, Buttons.RightShoulder);
            ret |= AddButtonIfPressed(buttonFlags, GPBF.LeftThumb, Buttons.LeftStick);
            ret |= AddButtonIfPressed(buttonFlags, GPBF.RightThumb, Buttons.RightStick);
            ret |= AddButtonIfPressed(buttonFlags, GPBF.Start, Buttons.Start);
            ret |= AddButtonIfPressed(buttonFlags, GPBF.X, Buttons.X);
            ret |= AddButtonIfPressed(buttonFlags, GPBF.Y, Buttons.Y);

            if (leftTrigger >= XInput.Gamepad.TriggerThreshold)
                ret |= Buttons.LeftTrigger;

            if (rightTrigger >= XInput.Gamepad.TriggerThreshold)
                ret |= Buttons.RightTrigger;

            // Check for the hardware back button.
            if (Back)
                ret |= Buttons.Back;

            return new GamePadButtons(ret);
        }

        public override bool PlatformSetVibration(int index, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger)
        {
            if (!_connected[index])
            {
                if (!HasDisconnectedTimeoutElapsed(index))
                    return false;
                if (!_controllers[index].IsConnected)
                {
                    SetDisconnectedTimeout(index);
                    return false;
                }
                _connected[index] = true;
            }

            SharpDX.Result result;
            try
            {
                XInput.Vibration vibration = new XInput.Vibration();
                vibration.LeftMotorSpeed = (ushort)(leftMotor * ushort.MaxValue);
                vibration.RightMotorSpeed = (ushort)(rightMotor * ushort.MaxValue);
                result = _controllers[index].SetVibration(vibration);
            }
            catch (SharpDX.SharpDXException ex)
            {
                if (ex.ResultCode.Code == DeviceNotConnectedHResult)
                {
                    _connected[index] = false;
                    SetDisconnectedTimeout(index);
                    return false;
                }
                throw;
            }

            return result == SharpDX.Result.Ok;
        }

        private bool HasDisconnectedTimeoutElapsed(int index)
        {
            return _timeout[index] <= DateTime.UtcNow.Ticks;
        }

        private void SetDisconnectedTimeout(int index)
        {
            _timeout[index] = DateTime.UtcNow.Ticks + TimeoutTicks;
        }
    }
}
