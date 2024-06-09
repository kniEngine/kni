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
        const int DeviceNotConnectedHResult = unchecked((int)0x8007048f);
        const int MaxNumberOfGamePads = 4;

        internal bool Back;

        private static readonly XInput.Controller[] _controllers = new[]
        {
            new XInput.Controller(XInput.UserIndex.One),
            new XInput.Controller(XInput.UserIndex.Two),
            new XInput.Controller(XInput.UserIndex.Three),
            new XInput.Controller(XInput.UserIndex.Four),
        };

        private readonly bool[] _isConnected = new bool[MaxNumberOfGamePads];
        private readonly DateTime[] _timeout = new DateTime[MaxNumberOfGamePads];
        private readonly TimeSpan TimeoutTicks = TimeSpan.FromSeconds(1);

        // XInput Xbox Controller dead zones
        // Dead zones are slightly different between left and right sticks, this may come from Microsoft usability tests
        public override float LeftThumbDeadZone { get { return XInput.Gamepad.LeftThumbDeadZone / (float)short.MaxValue; } }
        public override float RightThumbDeadZone { get { return XInput.Gamepad.RightThumbDeadZone / (float)short.MaxValue; } }

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
            DateTime utcNow = DateTime.UtcNow;

            // If the device was disconnected then wait for
            // the timeout to elapsed before we test it again.
            if (!_isConnected[index])
            {
                if (_timeout[index] > utcNow)
                {
                    return GetDefaultCapabilities();
                }
            }

            // Check to see if the device is connected.
            _isConnected[index] = _controllers[index].IsConnected;

            // If the device is disconnected retry it after the
            // timeout period has elapsed to avoid the overhead.
            if (!_isConnected[index])
            {
                _timeout[index] = utcNow + TimeoutTicks;
                return GetDefaultCapabilities();
            }

            XInput.Capabilities xCapabilities;
            try
            {
                xCapabilities = _controllers[index].GetCapabilities(XInput.DeviceQueryType.Any);
            }
            catch (SharpDX.SharpDXException ex)
            {
                if (ex.ResultCode.Code == DeviceNotConnectedHResult)
                {
                    _isConnected[index] = false;
                    _timeout[index] = utcNow + TimeoutTicks;
                    return GetDefaultCapabilities();
                }
                throw;
            }

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

            gamePadType = XInputToXnaGamePadType(xCapabilities.SubType);

            XInput.Gamepad xgamepad = xCapabilities.Gamepad;

            // digital buttons
            XInput.GamepadButtonFlags xbuttons = xgamepad.Buttons;
            buttons |= ((xbuttons & GPBF.A) == GPBF.A) ? Buttons.A : (Buttons)0;
            buttons |= ((xbuttons & GPBF.Back) == GPBF.Back) ? Buttons.Back : (Buttons)0;
            buttons |= ((xbuttons & GPBF.B) == GPBF.B) ? Buttons.B : (Buttons)0;
            buttons |= (false) ? Buttons.BigButton : (Buttons)0; // TODO: what IS this? Is it related to GamePadType.BigGamePad?
            buttons |= ((xbuttons & GPBF.DPadDown) == GPBF.DPadDown) ? Buttons.DPadDown : (Buttons)0;
            buttons |= ((xbuttons & GPBF.DPadLeft) == GPBF.DPadLeft) ? Buttons.DPadLeft : (Buttons)0;
            buttons |= ((xbuttons & GPBF.DPadRight) == GPBF.DPadRight) ? Buttons.DPadRight : (Buttons)0;
            buttons |= ((xbuttons & GPBF.DPadUp) == GPBF.DPadUp) ? Buttons.DPadUp : (Buttons)0;
            buttons |= ((xbuttons & GPBF.LeftShoulder) == GPBF.LeftShoulder) ? Buttons.LeftShoulder : (Buttons)0;
            buttons |= ((xbuttons & GPBF.LeftThumb) == GPBF.LeftThumb) ? Buttons.LeftStick : (Buttons)0;
            buttons |= ((xbuttons & GPBF.RightShoulder) == GPBF.RightShoulder) ? Buttons.RightShoulder : (Buttons)0;
            buttons |= ((xbuttons & GPBF.RightThumb) == GPBF.RightThumb) ? Buttons.RightStick : (Buttons)0;
            buttons |= ((xbuttons & GPBF.Start) == GPBF.Start) ? Buttons.Start : (Buttons)0;
            buttons |= ((xbuttons & GPBF.X) == GPBF.X) ? Buttons.X : (Buttons)0;
            buttons |= ((xbuttons & GPBF.Y) == GPBF.Y) ? Buttons.Y : (Buttons)0;

            // analog controls
            buttons |= (xgamepad.LeftTrigger > 0) ? Buttons.LeftTrigger : (Buttons)0;
            buttons |= (xgamepad.LeftThumbX != 0) ? Buttons.LeftThumbstickLeft | Buttons.LeftThumbstickRight : (Buttons)0;
            buttons |= (xgamepad.LeftThumbY != 0) ? Buttons.LeftThumbstickDown | Buttons.LeftThumbstickUp : (Buttons)0;
            buttons |= (xgamepad.RightTrigger > 0) ? Buttons.RightTrigger : (Buttons)0;
            buttons |= (xgamepad.RightThumbX != 0) ? Buttons.RightThumbstickLeft | Buttons.RightThumbstickRight : (Buttons)0;
            buttons |= (xgamepad.RightThumbY != 0) ? Buttons.RightThumbstickDown | Buttons.RightThumbstickUp : (Buttons)0;

            // vibration
#if DIRECTX11_1
            bool hasForceFeedback = (xCapabilities.Flags & XInput.CapabilityFlags.FfbSupported) == XInput.CapabilityFlags.FfbSupported;
            hasLeftVibrationMotor = hasForceFeedback && xCapabilities.Vibration.LeftMotorSpeed > 0;
            hasRightVibrationMotor = hasForceFeedback && xCapabilities.Vibration.RightMotorSpeed > 0;
#else
            hasLeftVibrationMotor = (xCapabilities.Vibration.LeftMotorSpeed > 0);
            hasRightVibrationMotor = (xCapabilities.Vibration.RightMotorSpeed > 0);
#endif

            // other
            isConnected = _controllers[index].IsConnected;
            hasVoiceSupport = (xCapabilities.Flags & XInput.CapabilityFlags.VoiceSupported) == XInput.CapabilityFlags.VoiceSupported;
            
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

        private GamePadType XInputToXnaGamePadType(XInput.DeviceSubType subType)
        {
            switch (subType)
            {
#if DIRECTX11_1
                case XInput.DeviceSubType.ArcadePad:
                    Debug.WriteLine("XInput's DeviceSubType.ArcadePad is not supported in XNA");
                    return GamePadType.Unknown; // TODO: Should this be BigButtonPad?
                case XInput.DeviceSubType.FlightStick:
                    return GamePadType.FlightStick;
                case XInput.DeviceSubType.GuitarAlternate:
                    return GamePadType.AlternateGuitar;
                case XInput.DeviceSubType.GuitarBass:
                    // Note: XNA doesn't distinguish between Guitar and GuitarBass, but 
                    // GuitarBass is identical to Guitar in XInput, distinguished only
                    // to help setup for those controllers. 
                    return GamePadType.Guitar;
                case XInput.DeviceSubType.Unknown:
                    return GamePadType.Unknown;
#endif

                case XInput.DeviceSubType.ArcadeStick:
                    return GamePadType.ArcadeStick;
                case XInput.DeviceSubType.DancePad:
                    return GamePadType.DancePad;
                case XInput.DeviceSubType.DrumKit:
                    return GamePadType.DrumKit;

                case XInput.DeviceSubType.Gamepad:
                    return GamePadType.GamePad;
                case XInput.DeviceSubType.Guitar:
                    return GamePadType.Guitar;
                case XInput.DeviceSubType.Wheel:
                    return GamePadType.Wheel;

                default:
                    Debug.WriteLine("unexpected XInput DeviceSubType: {0}", subType.ToString());
                    return GamePadType.Unknown;
            }
        }

        private GamePadState GetDefaultState()
        {
            GamePadButtons buttons = new GamePadButtons((Back) ? Buttons.Back : 0);
            return base.CreateGamePadState(default(GamePadThumbSticks), default(GamePadTriggers), buttons, default(GamePadDPad));
        }

        public override GamePadState PlatformGetState(int index, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
        {
            DateTime utcNow = DateTime.UtcNow;

            // If the device was disconnected then wait for 
            // the timeout to elapsed before we test it again.
            if (!_isConnected[index])
            {
                if (_timeout[index] > utcNow)
                {
                    return GetDefaultState();
                }
            }

            int packetNumber = 0;

            // Try to get the controller state.
            XInput.Gamepad gamepad = new XInput.Gamepad();
            try
            {
                XInput.State xistate;
                _isConnected[index] = _controllers[index].GetState(out xistate);
                packetNumber = xistate.PacketNumber;
                gamepad = xistate.Gamepad;
            }
            catch (Exception)
            {
            }

            // If the device is disconnected retry it after the
            // timeout period has elapsed to avoid the overhead.
            if (!_isConnected[index])
            {
                _timeout[index] = utcNow + TimeoutTicks;
                return GetDefaultState();
            }

            GamePadThumbSticks thumbSticks = base.CreateGamePadThumbSticks(
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

            return base.CreateGamePadState(
                thumbSticks: thumbSticks,
                triggers: triggers,
                buttons: buttons,
                dPad: dpadState,
                packetNumber: packetNumber);
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
            DateTime utcNow = DateTime.UtcNow;

            if (!_isConnected[index])
            {
                if (_timeout[index] > utcNow)
                    return false;
                if (!_controllers[index].IsConnected)
                {
                    _timeout[index] = utcNow + TimeoutTicks;
                    return false;
                }
                _isConnected[index] = true;
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
                    _isConnected[index] = false;
                    _timeout[index] = utcNow + TimeoutTicks;
                    return false;
                }
                throw;
            }

            return result == SharpDX.Result.Ok;
        }
    }
}
