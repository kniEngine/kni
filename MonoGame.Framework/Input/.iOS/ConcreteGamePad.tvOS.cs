// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using GameController;
using UIKit;

namespace Microsoft.Xna.Platform.Input
{
    public sealed class ConcreteGamePad : GamePadStrategy
    {
        internal bool MenuPressed = false;

        public override int PlatformGetMaxNumberOfGamePads()
        {
            return 4;
        }

        private bool IndexIsUsed(GCControllerPlayerIndex index)
        {
            foreach (var ctrl in GCController.Controllers)
                if (ctrl.PlayerIndex==(int)index) return true;

            return false;
        }

        private void AssingIndex(GCControllerPlayerIndex index)
        {
            if (IndexIsUsed(index))
                return;

            foreach (var controller in GCController.Controllers)
            {
                if (controller.PlayerIndex == (int)index)
                    break;
                if (controller.PlayerIndex == (int)GCControllerPlayerIndex.Unset)
                {
                    controller.PlayerIndex = (int)index;
                    break;
                }
            }
        }

        public override GamePadCapabilities PlatformGetCapabilities(int index)
        {
            GCControllerPlayerIndex ind = (GCControllerPlayerIndex)index;

            AssingIndex(ind);

            foreach (var controller in GCController.Controllers)
            {
                if (controller == null)
                    continue;
                if (controller.PlayerIndex == (int)ind)
                    return GetCapabilities(controller);
            }

            return new GamePadCapabilities(
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
                capabilities.HasBackButton = true;
                buttons |= Buttons.DPadUp;
                buttons |= Buttons.DPadDown;
                buttons |= Buttons.DPadLeft;
                buttons |= Buttons.DPadRight;
                buttons |= Buttons.LeftShoulder;
                buttons |= Buttons.RightShoulder;
            }
            else if (controller.MicroGamepad != null)
            {
                buttons |= Buttons.A;
                buttons |= Buttons.X;
                capabilities.HasBackButton = true;
                buttons |= Buttons.DPadUp;
                buttons |= Buttons.DPadDown;
                buttons |= Buttons.DPadLeft;
                buttons |= Buttons.DPadRight;
            }

            return new GamePadCapabilities(
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
            GCControllerPlayerIndex ind = (GCControllerPlayerIndex)index;


            List<Buttons> buttons = new List<Buttons>();
            bool connected = false;
            ButtonState Up = ButtonState.Released;
            ButtonState Down = ButtonState.Released;
            ButtonState Left = ButtonState.Released;
            ButtonState Right = ButtonState.Released;

            Vector2 leftThumbStickPosition = Vector2.Zero;
            Vector2 rightThumbStickPosition = Vector2.Zero;

            float leftTriggerValue = 0;
            float rightTriggerValue = 0;

            AssingIndex(ind);

            foreach (var controller in GCController.Controllers)
            {
                if (controller == null)
                    continue;

                if (controller.PlayerIndex != (int)ind)
                    continue;

                connected = true;
                if (MenuPressed)
                {
                    buttons.Add(Buttons.Back);
                    MenuPressed = false;
                }

                if (controller.ExtendedGamepad != null)
                {
                    if (controller.ExtendedGamepad.ButtonA.IsPressed == true && !buttons.Contains(Buttons.A))
                        buttons.Add(Buttons.A);
                    if (controller.ExtendedGamepad.ButtonB.IsPressed == true && !buttons.Contains(Buttons.B))
                        buttons.Add(Buttons.B);
                    if (controller.ExtendedGamepad.ButtonX.IsPressed == true && !buttons.Contains(Buttons.X))
                        buttons.Add(Buttons.X);
                    if (controller.ExtendedGamepad.ButtonY.IsPressed == true && !buttons.Contains(Buttons.Y))
                        buttons.Add(Buttons.Y);

                    if (controller.ExtendedGamepad.LeftShoulder.IsPressed == true && !buttons.Contains(Buttons.LeftShoulder))
                        buttons.Add(Buttons.LeftShoulder);
                    if (controller.ExtendedGamepad.RightShoulder.IsPressed == true && !buttons.Contains(Buttons.RightShoulder))
                        buttons.Add(Buttons.RightShoulder);

                    Up = controller.ExtendedGamepad.DPad.Up.IsPressed ? ButtonState.Pressed : ButtonState.Released;
                    Down = controller.ExtendedGamepad.DPad.Down.IsPressed ? ButtonState.Pressed : ButtonState.Released;
                    Left = controller.ExtendedGamepad.DPad.Left.IsPressed ? ButtonState.Pressed : ButtonState.Released;
                    Right = controller.ExtendedGamepad.DPad.Right.IsPressed ? ButtonState.Pressed : ButtonState.Released;

                    leftThumbStickPosition.X = controller.ExtendedGamepad.LeftThumbstick.XAxis.Value;
                    leftThumbStickPosition.Y = controller.ExtendedGamepad.LeftThumbstick.YAxis.Value;
                    rightThumbStickPosition.X = controller.ExtendedGamepad.RightThumbstick.XAxis.Value;
                    rightThumbStickPosition.Y = controller.ExtendedGamepad.RightThumbstick.YAxis.Value;
                    leftTriggerValue = controller.ExtendedGamepad.LeftTrigger.Value;
                    rightTriggerValue = controller.ExtendedGamepad.RightTrigger.Value;
                }
                else if (controller.Gamepad != null)
                {
                    if (controller.Gamepad.ButtonA.IsPressed == true && !buttons.Contains(Buttons.A))
                        buttons.Add(Buttons.A);
                    if (controller.Gamepad.ButtonB.IsPressed == true && !buttons.Contains(Buttons.B))
                        buttons.Add(Buttons.B);
                    if (controller.Gamepad.ButtonX.IsPressed == true && !buttons.Contains(Buttons.X))
                        buttons.Add(Buttons.X);
                    if (controller.Gamepad.ButtonY.IsPressed == true && !buttons.Contains(Buttons.Y))
                        buttons.Add(Buttons.Y);
                    Up = controller.Gamepad.DPad.Up.IsPressed ? ButtonState.Pressed : ButtonState.Released;
                    Down = controller.Gamepad.DPad.Down.IsPressed ? ButtonState.Pressed : ButtonState.Released;
                    Left = controller.Gamepad.DPad.Left.IsPressed ? ButtonState.Pressed : ButtonState.Released;
                    Right = controller.Gamepad.DPad.Right.IsPressed ? ButtonState.Pressed : ButtonState.Released;

                }
                else if (controller.MicroGamepad != null)
                {
                    if (controller.MicroGamepad.ButtonA.IsPressed == true && !buttons.Contains(Buttons.A))
                        buttons.Add(Buttons.A);
                    if (controller.MicroGamepad.ButtonX.IsPressed == true && !buttons.Contains(Buttons.X))
                        buttons.Add(Buttons.X);
                    Up = controller.MicroGamepad.Dpad.Up.IsPressed ? ButtonState.Pressed : ButtonState.Released;
                    Down = controller.MicroGamepad.Dpad.Down.IsPressed ? ButtonState.Pressed : ButtonState.Released;
                    Left = controller.MicroGamepad.Dpad.Left.IsPressed ? ButtonState.Pressed : ButtonState.Released;
                    Right = controller.MicroGamepad.Dpad.Right.IsPressed ? ButtonState.Pressed : ButtonState.Released;
                }
            }
            GamePadState state = new GamePadState(
                new GamePadThumbSticks(leftThumbStickPosition, rightThumbStickPosition, leftDeadZoneMode, rightDeadZoneMode),
                new GamePadTriggers(leftTriggerValue, rightTriggerValue),
                new GamePadButtons(buttons.ToArray()),
                new GamePadDPad(Up, Down, Left, Right));
            state.IsConnected = connected;
            return state;
        }

        public override bool PlatformSetVibration(int index, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger)
        {
            return false;
        }
    }
}
