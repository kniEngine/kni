// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using GameController;

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

        // Default & SDL Xbox Controller dead zones
        // Based on the XInput constants
        public override float LeftThumbDeadZone { get { return 0.24f; } }
        public override float RightThumbDeadZone { get { return 0.265f; } }

        public override int PlatformGetMaxNumberOfGamePads()
        {
            return MaxNumberOfGamePads;
        }

        private void AssingIndex(GCController[] controllers, GCControllerPlayerIndex gcindex)
        {
            // index is used ?
            foreach (GCController controller in controllers)
            {
                if ((long)controller.PlayerIndex == (long)gcindex)
                    return;
            }

            foreach (GCController controller in controllers)
            {
                if ((long)controller.PlayerIndex == (long)GCControllerPlayerIndex.Unset)
                {
#if XAMARINIOS
                    controller.PlayerIndex = (System.nint)(long)index;
#else
                    controller.PlayerIndex = gcindex;
#endif
                    return;
                }
            }
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
            GCControllerPlayerIndex gcindex = (GCControllerPlayerIndex)index;

            GCController[] controllers = GCController.Controllers;

            AssingIndex(controllers, gcindex);

            GCController gccontroller = null;
            foreach (GCController controller in controllers)
            {
                if (controller == null)
                    continue;

                if ((long)controller.PlayerIndex == (long)gcindex)
                {
                    gccontroller = controller;
                    break;
                }
            }

            if (gccontroller == null)
                return GetDefaultCapabilities();

            return GetCapabilities(gccontroller);
        }

        private GamePadCapabilities GetCapabilities(GCController controller)
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

            //All iOS controllers have these basics
            isConnected = false;
            gamePadType = GamePadType.GamePad;

            if (controller.ExtendedGamepad != null)
            {
                buttons |= Buttons.A;
                buttons |= Buttons.B;
                buttons |= Buttons.X;
                buttons |= Buttons.Y;
                buttons |= Buttons.Back;
                buttons |= Buttons.Start;
                buttons |= Buttons.DPadUp;
                buttons |= Buttons.DPadDown;
                buttons |= Buttons.DPadLeft;
                buttons |= Buttons.DPadRight;
                buttons |= Buttons.LeftShoulder;
                buttons |= Buttons.RightShoulder;
                buttons |= Buttons.LeftTrigger;
                buttons |= Buttons.RightTrigger;
                buttons |= Buttons.LeftThumbstickLeft | Buttons.LeftThumbstickRight;       
                buttons |= Buttons.LeftThumbstickDown | Buttons.LeftThumbstickUp;
                buttons |= Buttons.RightThumbstickLeft | Buttons.RightThumbstickRight;
                buttons |= Buttons.RightThumbstickDown | Buttons.RightThumbstickUp;
            }
            else if (controller.Gamepad != null)
            {
                buttons |= Buttons.A;
                buttons |= Buttons.B;
                buttons |= Buttons.X;
                buttons |= Buttons.Y;
                buttons |= Buttons.DPadUp;
                buttons |= Buttons.DPadDown;
                buttons |= Buttons.DPadLeft;
                buttons |= Buttons.DPadRight;
                buttons |= Buttons.LeftShoulder;
                buttons |= Buttons.RightShoulder;
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

        public override GamePadState PlatformGetState(int index, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
        {
            GCControllerPlayerIndex gcindex = (GCControllerPlayerIndex)index;

            Buttons buttons = 0;
            bool connected = false;
            ButtonState Up = ButtonState.Released;
            ButtonState Down = ButtonState.Released;
            ButtonState Left = ButtonState.Released;
            ButtonState Right = ButtonState.Released;

            Vector2 leftThumbStickPosition = Vector2.Zero;
            Vector2 rightThumbStickPosition = Vector2.Zero;

            float leftTriggerValue = 0;
            float rightTriggerValue = 0;

            GCController[] controllers = GCController.Controllers;

            AssingIndex(controllers, gcindex);

            GCController gccontroller = null;
            foreach (GCController controller in controllers)
            {
                if (controller == null)
                    continue;

                if ((long)controller.PlayerIndex == (long)gcindex)
                {
                    gccontroller = controller;
                    break;
                }
            }

            if (gccontroller != null)
            {
                connected = true;

                if (gccontroller.ExtendedGamepad != null)
                {
                    if (gccontroller.ExtendedGamepad.ButtonA.IsPressed)
                        buttons |= Buttons.A;
                    if (gccontroller.ExtendedGamepad.ButtonB.IsPressed)
                        buttons |= Buttons.B;
                    if (gccontroller.ExtendedGamepad.ButtonX.IsPressed)
                        buttons |= Buttons.X;
                    if (gccontroller.ExtendedGamepad.ButtonY.IsPressed)
                        buttons |= Buttons.Y;

                    if (gccontroller.ExtendedGamepad.LeftShoulder.IsPressed)
                        buttons |= Buttons.LeftShoulder;
                    if (gccontroller.ExtendedGamepad.RightShoulder.IsPressed)
                        buttons |= Buttons.RightShoulder;

                    if (gccontroller.ExtendedGamepad.LeftTrigger.IsPressed)
                        buttons |= Buttons.LeftTrigger;
                    if (gccontroller.ExtendedGamepad.RightTrigger.IsPressed)
                        buttons |= Buttons.RightTrigger;

                    if (gccontroller.ExtendedGamepad.ButtonMenu.IsPressed)
                        buttons |= Buttons.Start;
                    if (gccontroller.ExtendedGamepad.ButtonOptions != null &&
                        gccontroller.ExtendedGamepad.ButtonOptions.IsPressed)
                        buttons |= Buttons.Back;

                    if (gccontroller.ExtendedGamepad.DPad.Up.IsPressed)
                    {
                        Up = ButtonState.Pressed;
                        buttons |= Buttons.DPadUp;
                    }
                    if (gccontroller.ExtendedGamepad.DPad.Down.IsPressed)
                    {
                        Down = ButtonState.Pressed;
                        buttons |= Buttons.DPadDown;
                    }
                    if (gccontroller.ExtendedGamepad.DPad.Left.IsPressed)
                    {
                        Left = ButtonState.Pressed;
                        buttons |= Buttons.DPadLeft;
                    }
                    if (gccontroller.ExtendedGamepad.DPad.Right.IsPressed)
                    {
                        Right = ButtonState.Pressed;
                        buttons |= Buttons.DPadRight;
                    }

                    leftThumbStickPosition.X = gccontroller.ExtendedGamepad.LeftThumbstick.XAxis.Value;
                    leftThumbStickPosition.Y = gccontroller.ExtendedGamepad.LeftThumbstick.YAxis.Value;
                    rightThumbStickPosition.X = gccontroller.ExtendedGamepad.RightThumbstick.XAxis.Value;
                    rightThumbStickPosition.Y = gccontroller.ExtendedGamepad.RightThumbstick.YAxis.Value;
                    leftTriggerValue = gccontroller.ExtendedGamepad.LeftTrigger.Value;
                    rightTriggerValue = gccontroller.ExtendedGamepad.RightTrigger.Value;
                }
                else if (gccontroller.Gamepad != null)
                {
                    if (gccontroller.Gamepad.ButtonA.IsPressed)
                        buttons |= Buttons.A;
                    if (gccontroller.Gamepad.ButtonB.IsPressed)
                        buttons |= Buttons.B;
                    if (gccontroller.Gamepad.ButtonX.IsPressed)
                        buttons |= Buttons.X;
                    if (gccontroller.Gamepad.ButtonY.IsPressed)
                        buttons |= Buttons.Y;

                    if (gccontroller.Gamepad.DPad.Up.IsPressed)
                    {
                        Up = ButtonState.Pressed;
                        buttons |= Buttons.DPadUp;
                    }
                    if (gccontroller.Gamepad.DPad.Down.IsPressed)
                    {
                        Down = ButtonState.Pressed;
                        buttons |= Buttons.DPadDown;
                    }
                    if (gccontroller.Gamepad.DPad.Left.IsPressed)
                    {
                        Left = ButtonState.Pressed;
                        buttons |= Buttons.DPadLeft;
                    }
                    if (gccontroller.Gamepad.DPad.Right.IsPressed)
                    {
                        Right = ButtonState.Pressed;
                        buttons |= Buttons.DPadRight;
                    }
                }
            }
            GamePadState state = base.CreateGamePadState(
                base.CreateGamePadThumbSticks(leftThumbStickPosition, rightThumbStickPosition, leftDeadZoneMode, rightDeadZoneMode),
                new GamePadTriggers(leftTriggerValue, rightTriggerValue),
                new GamePadButtons(buttons),
                new GamePadDPad(Up, Down, Left, Right),
                isConnected: connected);
            return state;
        }

        public override bool PlatformSetVibration(int index, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger)
        {
            return false;
        }
    }
}
