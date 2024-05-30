// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Android.Views;

namespace Microsoft.Xna.Platform.Input
{
    public class GamePadDevice
    {
        public GamePadCapabilities Capabilities;

        public GamePadDevice()
        {

        }
    }

    internal class AndroidGamePadDevice : GamePadDevice
    {
        public InputDevice _device;
        public int _deviceId;
        public string _descriptor;
        public bool _isConnected;
        public bool DPadButtons;

        public Buttons _buttons;
        public float _leftTrigger, _rightTrigger;
        public Vector2 _leftStick, _rightStick;

        public AndroidGamePadDevice(InputDevice device, GamePadCapabilities caps)
            : base()
        {
            _device = device;
            _deviceId = device.Id;
            _descriptor = device.Descriptor;
            _isConnected = true;
            Capabilities = caps;
        }

    }

    public sealed class ConcreteGamePad : GamePadStrategy
    {
        // we will support up to 4 local controllers
        private readonly AndroidGamePadDevice[] GamePads = new AndroidGamePadDevice[4];
        // support the back button when we don't have a gamepad connected
        internal bool Back;

        // Default & SDL Xbox Controller dead zones
        // Based on the XInput constants
        public override float LeftThumbDeadZone { get { return 0.24f; } }
        public override float RightThumbDeadZone { get { return 0.265f; } }

        public override int PlatformGetMaxNumberOfGamePads()
        {
            return 4;
        }

        public override GamePadCapabilities PlatformGetCapabilities(int index)
        {
            AndroidGamePadDevice gamePad = GamePads[index];
            if (gamePad != null)
                return gamePad.Capabilities;

            // we need to add the default "no gamepad connected but the user hit back"
            // behaviour here
            return base.CreateGamePadCapabilities(
                    gamePadType: GamePadType.Unknown,
                    displayName: null,
                    identifier: null,
                    isConnected: false,
                    buttons: Buttons.Back,
                    hasLeftVibrationMotor: false,
                    hasRightVibrationMotor: false,
                    hasVoiceSupport: false
                );
        }

        public override GamePadState PlatformGetState(int index, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
        {
            AndroidGamePadDevice gamePad = GamePads[index];
            GamePadState state = GamePadState.Default;
            if (gamePad != null && gamePad._isConnected)
            {
                // Check if the device was disconnected
                InputDevice dvc = InputDevice.GetDevice(gamePad._deviceId);
                if (dvc == null)
                {
                    Android.Util.Log.Debug("MonoGame", "Detected controller disconnect [" + index + "] ");
                    gamePad._isConnected = false;
                    return state;
                }

                GamePadThumbSticks thumbSticks = base.CreateGamePadThumbSticks(gamePad._leftStick, gamePad._rightStick, leftDeadZoneMode, rightDeadZoneMode);

                state = base.CreateGamePadState(
                    thumbSticks,
                    new GamePadTriggers(gamePad._leftTrigger, gamePad._rightTrigger),
                    new GamePadButtons(gamePad._buttons),
                     base.CreateGamePadDPad(gamePad._buttons));
            }
            // we need to add the default "no gamepad connected but the user hit back"
            // behaviour here
            else
            {
                if (index == 0 && Back)
                {
                    // Consume state
                    state = base.CreateGamePadState(new GamePadThumbSticks(), new GamePadTriggers(), new GamePadButtons(Buttons.Back), new GamePadDPad(),
                        isConnected: false);
                }
                else
                    state = new GamePadState();
            }

            return state;
        }

        public override bool PlatformSetVibration(int index, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger)
        {
            AndroidGamePadDevice gamePad = GamePads[index];
            if (gamePad == null)
                return false;

            Android.OS.Vibrator vibrator = gamePad._device.Vibrator;
            if (!vibrator.HasVibrator)
                return false;
            vibrator.Vibrate(500);
            return true;
        }

        internal AndroidGamePadDevice GetGamePad(InputDevice device)
        {
            if (device == null || (device.Sources & InputSourceType.Gamepad) != InputSourceType.Gamepad)
                return null;

            int firstDisconnectedPadId = -1;
            for (int i = 0; i < GamePads.Length; i++)
            {
                AndroidGamePadDevice pad = GamePads[i];
                if (pad != null && pad._isConnected && pad._deviceId == device.Id)
                {
                    return pad;
                }
                else if (pad != null && !pad._isConnected && pad._descriptor == device.Descriptor)
                {
                    Android.Util.Log.Debug("MonoGame", "Found previous controller [" + i + "] " + device.Name);
                    pad._deviceId = device.Id;
                    pad._isConnected = true;
                    return pad;
                }
                else if (pad == null)
                {
                    Android.Util.Log.Debug("MonoGame", "Found new controller [" + i + "] " + device.Name);
                    GamePadCapabilities caps = CapabilitiesOfDevice(device);
                    pad = new AndroidGamePadDevice(device, caps);
                    GamePads[i] = pad;
                    return pad;
                }
                else if (!pad._isConnected && firstDisconnectedPadId < 0)
                {
                    firstDisconnectedPadId = i;
                }
            }

            // If we get here, we failed to find a game pad or an empty slot to create one.
            // If we're holding onto a disconnected pad, overwrite it with this one
            if (firstDisconnectedPadId >= 0)
            {
                Android.Util.Log.Debug("MonoGame", "Found new controller in place of disconnected controller [" + firstDisconnectedPadId + "] " + device.Name);
                GamePadCapabilities caps = CapabilitiesOfDevice(device);
                AndroidGamePadDevice pad = new AndroidGamePadDevice(device, caps);
                GamePads[firstDisconnectedPadId] = pad;
                return pad;
            }

            // All pad slots are taken so ignore further devices.
            return null;
        }

        internal bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            AndroidGamePadDevice gamePad = GetGamePad(e.Device);
            if (gamePad == null)
                return false;

            gamePad.DPadButtons |= e.KeyCode == Keycode.DpadLeft ||
                                   e.KeyCode == Keycode.DpadUp || 
                                   e.KeyCode == Keycode.DpadRight || 
                                   e.KeyCode == Keycode.DpadDown;
            gamePad._buttons |= ButtonForKeyCode(keyCode);

            return true;
        }

        internal bool OnKeyUp(Keycode keyCode, KeyEvent e)
        {
            AndroidGamePadDevice gamePad = GetGamePad(e.Device);
            if (gamePad == null)
                return false;

            gamePad._buttons &= ~ButtonForKeyCode(keyCode);
            return true;
        }

        internal bool OnGenericMotionEvent(MotionEvent e)
        {
            AndroidGamePadDevice gamePad = GetGamePad(e.Device);
            if (gamePad == null)
                return false;

            if (e.Action != MotionEventActions.Move)
                return false;

            gamePad._leftStick = new Vector2(e.GetAxisValue(Axis.X), -e.GetAxisValue(Axis.Y));
            gamePad._rightStick = new Vector2(e.GetAxisValue(Axis.Z), -e.GetAxisValue(Axis.Rz));
            gamePad._leftTrigger = e.GetAxisValue(Axis.Ltrigger);
            gamePad._rightTrigger = e.GetAxisValue(Axis.Rtrigger);
            if (gamePad._leftTrigger == 0)
                gamePad._leftTrigger = e.GetAxisValue(Axis.Brake);
            if (gamePad._rightTrigger == 0)
                gamePad._rightTrigger = e.GetAxisValue(Axis.Gas);

            if(!gamePad.DPadButtons)
            {
                if(e.GetAxisValue(Axis.HatX) < 0)
                {
                    gamePad._buttons |= Buttons.DPadLeft;
                    gamePad._buttons &= ~Buttons.DPadRight;
                }
                else if(e.GetAxisValue(Axis.HatX) > 0)
                {
                    gamePad._buttons &= ~Buttons.DPadLeft;
                    gamePad._buttons |= Buttons.DPadRight;
                }
                else
                {
                    gamePad._buttons &= ~Buttons.DPadLeft;
                    gamePad._buttons &= ~Buttons.DPadRight;
                }

                if(e.GetAxisValue(Axis.HatY) < 0)
                {
                    gamePad._buttons |= Buttons.DPadUp;
                    gamePad._buttons &= ~Buttons.DPadDown;
                }
                else if(e.GetAxisValue(Axis.HatY) > 0)
                {
                    gamePad._buttons &= ~Buttons.DPadUp;
                    gamePad._buttons |= Buttons.DPadDown;
                }
                else
                {
                    gamePad._buttons &= ~Buttons.DPadUp;
                    gamePad._buttons &= ~Buttons.DPadDown;
                }
            }

            return true;
        }

        private Buttons ButtonForKeyCode(Keycode keyCode)
        {
            switch (keyCode)
            {
                case Keycode.ButtonA:
                    return Buttons.A;
                case Keycode.ButtonX:
                    return Buttons.X;
                case Keycode.ButtonY:
                    return Buttons.Y;
                case Keycode.ButtonB:
                    return Buttons.B;

                case Keycode.ButtonL1:
                    return Buttons.LeftShoulder;
                case Keycode.ButtonL2:
                    return Buttons.LeftTrigger;
                case Keycode.ButtonR1:
                    return Buttons.RightShoulder;
                case Keycode.ButtonR2:
                    return Buttons.RightTrigger;

                case Keycode.ButtonThumbl:
                    return Buttons.LeftStick;
                case Keycode.ButtonThumbr:
                    return Buttons.RightStick;

                case Keycode.DpadUp:
                    return Buttons.DPadUp;
                case Keycode.DpadDown:
                    return Buttons.DPadDown;
                case Keycode.DpadLeft:
                    return Buttons.DPadLeft;
                case Keycode.DpadRight:
                    return Buttons.DPadRight;

                case Keycode.ButtonStart:
                    return Buttons.Start;
                case Keycode.Back:
                    return Buttons.Back;

                default:
                    return 0;
            }
        }

        internal void Initialize()
        {
            //Iterate and 'connect' any detected gamepads
            foreach (int deviceId in InputDevice.GetDeviceIds())
            {
                GetGamePad(InputDevice.GetDevice(deviceId));
            }
        }

        private GamePadCapabilities CapabilitiesOfDevice(InputDevice device)
        {
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

            isConnected = true;
            gamePadType = GamePadType.GamePad;
            hasLeftVibrationMotor = hasRightVibrationMotor = device.Vibrator.HasVibrator;

            // build out supported inputs from what the gamepad exposes
            int[] keyMap = new int[16];
            keyMap[0] = (int)Keycode.ButtonA;
            keyMap[1] = (int)Keycode.ButtonB;
            keyMap[2] = (int)Keycode.ButtonX;
            keyMap[3] = (int)Keycode.ButtonY;

            keyMap[4] = (int)Keycode.ButtonThumbl;
            keyMap[5] = (int)Keycode.ButtonThumbr;

            keyMap[6] = (int)Keycode.ButtonL1;
            keyMap[7] = (int)Keycode.ButtonR1;
            keyMap[8] = (int)Keycode.ButtonL2;
            keyMap[9] = (int)Keycode.ButtonR2;

            keyMap[10] = (int)Keycode.DpadDown;
            keyMap[11] = (int)Keycode.DpadLeft;
            keyMap[12] = (int)Keycode.DpadRight;
            keyMap[13] = (int)Keycode.DpadUp;

            keyMap[14] = (int)Keycode.ButtonStart;
            keyMap[15] = (int)Keycode.Back;

            // get a bool[] with indices matching the keyMap
            bool[] hasMap = new bool[16];
            // HasKeys() was defined in Kitkat / API19 / Android 4.4
            if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.Kitkat)
            {
                Keycode[] keyMap2 = new Keycode[keyMap.Length];
                for (int i = 0; i < keyMap.Length; i++)
                    keyMap2[i] = (Keycode)keyMap[i];
                hasMap = KeyCharacterMap.DeviceHasKeys(keyMap2);
            }
            else
            {
                hasMap = device.HasKeys(keyMap);
            }

            buttons |= hasMap[0] ? Buttons.A : (Buttons)0;
            buttons |= hasMap[1] ? Buttons.B : (Buttons)0;
            buttons |= hasMap[2] ? Buttons.X : (Buttons)0;
            buttons |= hasMap[3] ? Buttons.Y : (Buttons)0;

            // we only check for the thumb button to see if we have 2 thumbsticks
            // if ever a controller doesn't support buttons on the thumbsticks,
            // this will need fixing
            buttons |= hasMap[4] ? Buttons.LeftThumbstickLeft | Buttons.LeftThumbstickRight : (Buttons)0;
            buttons |= hasMap[4] ? Buttons.LeftThumbstickDown | Buttons.LeftThumbstickUp : (Buttons)0;
            buttons |= hasMap[5] ? Buttons.RightThumbstickLeft | Buttons.RightThumbstickRight : (Buttons)0;
            buttons |= hasMap[5] ? Buttons.RightThumbstickDown | Buttons.RightThumbstickUp : (Buttons)0;

            buttons |= hasMap[6] ? Buttons.LeftShoulder : (Buttons)0;
            buttons |= hasMap[7] ? Buttons.RightShoulder : (Buttons)0;
            buttons |= hasMap[8] ? Buttons.LeftTrigger : (Buttons)0;
            buttons |= hasMap[9] ? Buttons.RightTrigger : (Buttons)0;

            buttons |= hasMap[10] ? Buttons.DPadDown : (Buttons)0;
            buttons |= hasMap[11] ? Buttons.DPadLeft : (Buttons)0;
            buttons |= hasMap[12] ? Buttons.DPadRight : (Buttons)0;
            buttons |= hasMap[13] ? Buttons.DPadUp : (Buttons)0;

            buttons |= hasMap[14] ? Buttons.Start : (Buttons)0;
            buttons |= hasMap[15] ? Buttons.Back : (Buttons)0;

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
    }
}
