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
    public sealed class ConcreteGamePad : GamePadStrategy
    {

        // Default & SDL Xbox Controller dead zones
        // Based on the XInput constants
        public override float LeftThumbDeadZone { get { return 0.24f; } }
        public override float RightThumbDeadZone { get { return 0.265f; } }

        public override int PlatformGetMaxNumberOfGamePads()
        {
            return 4;
        }

        private bool IndexIsUsed(GCControllerPlayerIndex index)
        {
            foreach (GCController ctrl in GCController.Controllers)
                if ((long)ctrl.PlayerIndex == (long)index) return true;

            return false;
        }

        private void AssingIndex(GCControllerPlayerIndex index)
        {
            if (IndexIsUsed(index))
                return;

            foreach (GCController controller in GCController.Controllers)
            {
                if ((long)controller.PlayerIndex == (long)index)
                    break;
                if ((long)controller.PlayerIndex == (long)GCControllerPlayerIndex.Unset)
                {
#if XAMARINIOS
                    controller.PlayerIndex = (System.nint)(long)index;
#else
                    controller.PlayerIndex = index;
#endif
                    break;
                }
            }
        }

        public override GamePadCapabilities PlatformGetCapabilities(int index)
        {
            GCControllerPlayerIndex ind = (GCControllerPlayerIndex)index;

            AssingIndex(ind);

            foreach (GCController controller in GCController.Controllers)
            {
                if (controller == null)
                    continue;
                if ((long)controller.PlayerIndex == (long)ind)
                    return GetCapabilities(controller);
            }
            return new GamePadCapabilities { IsConnected = false };
        }

        private GamePadCapabilities GetCapabilities(GCController controller)
        {
            //All iOS controllers have these basics
            GamePadCapabilities capabilities = new GamePadCapabilities();
            capabilities.IsConnected = false;
            capabilities.GamePadType = GamePadType.GamePad;

            if (controller.ExtendedGamepad != null)
            {
                capabilities.HasAButton = true;
                capabilities.HasBButton = true;
                capabilities.HasXButton = true;
                capabilities.HasYButton = true;
                capabilities.HasBackButton = true;
                capabilities.HasStartButton = true;
                capabilities.HasDPadUpButton = true;
                capabilities.HasDPadDownButton = true;
                capabilities.HasDPadLeftButton = true;
                capabilities.HasDPadRightButton = true;
                capabilities.HasLeftShoulderButton = true;
                capabilities.HasRightShoulderButton = true;
                capabilities.HasLeftTrigger = true;
                capabilities.HasRightTrigger = true;
                capabilities.HasLeftXThumbStick = true;
                capabilities.HasLeftYThumbStick = true;
                capabilities.HasRightXThumbStick = true;
                capabilities.HasRightYThumbStick = true;
            }
            else if (controller.Gamepad != null)
            {
                capabilities.HasAButton = true;
                capabilities.HasBButton = true;
                capabilities.HasXButton = true;
                capabilities.HasYButton = true;
                capabilities.HasDPadUpButton = true;
                capabilities.HasDPadDownButton = true;
                capabilities.HasDPadLeftButton = true;
                capabilities.HasDPadRightButton = true;
                capabilities.HasLeftShoulderButton = true;
                capabilities.HasRightShoulderButton = true;
            }
            return capabilities;
        }

        public override GamePadState PlatformGetState(int index, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
        {
            GCControllerPlayerIndex ind = (GCControllerPlayerIndex)index;


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

            AssingIndex(ind);

            foreach (GCController controller in GCController.Controllers)
            {

                if (controller == null)
                    continue;

                if ((long)controller.PlayerIndex != (long)ind)
                    continue;

                connected = true;

                if (controller.ExtendedGamepad != null)
                {
                    if (controller.ExtendedGamepad.ButtonA.IsPressed)
                        buttons |= Buttons.A;
                    if (controller.ExtendedGamepad.ButtonB.IsPressed)
                        buttons |= Buttons.B;
                    if (controller.ExtendedGamepad.ButtonX.IsPressed)
                        buttons |= Buttons.X;
                    if (controller.ExtendedGamepad.ButtonY.IsPressed)
                        buttons |= Buttons.Y;

                    if (controller.ExtendedGamepad.LeftShoulder.IsPressed)
                        buttons |= Buttons.LeftShoulder;
                    if (controller.ExtendedGamepad.RightShoulder.IsPressed)
                        buttons |= Buttons.RightShoulder;

                    if (controller.ExtendedGamepad.LeftTrigger.IsPressed)
                        buttons |= Buttons.LeftTrigger;
                    if (controller.ExtendedGamepad.RightTrigger.IsPressed)
                        buttons |= Buttons.RightTrigger;

                    if (controller.ExtendedGamepad.ButtonMenu.IsPressed)
                        buttons |= Buttons.Start;
                    if (controller.ExtendedGamepad.ButtonOptions != null &&
                        controller.ExtendedGamepad.ButtonOptions.IsPressed)
                        buttons |= Buttons.Back;

                    if (controller.ExtendedGamepad.DPad.Up.IsPressed)
                    {
                        Up = ButtonState.Pressed;
                        buttons |= Buttons.DPadUp;
                    }
                    if (controller.ExtendedGamepad.DPad.Down.IsPressed)
                    {
                        Down = ButtonState.Pressed;
                        buttons |= Buttons.DPadDown;
                    }
                    if (controller.ExtendedGamepad.DPad.Left.IsPressed)
                    {
                        Left = ButtonState.Pressed;
                        buttons |= Buttons.DPadLeft;
                    }
                    if (controller.ExtendedGamepad.DPad.Right.IsPressed)
                    {
                        Right = ButtonState.Pressed;
                        buttons |= Buttons.DPadRight;
                    }

                    leftThumbStickPosition.X = controller.ExtendedGamepad.LeftThumbstick.XAxis.Value;
                    leftThumbStickPosition.Y = controller.ExtendedGamepad.LeftThumbstick.YAxis.Value;
                    rightThumbStickPosition.X = controller.ExtendedGamepad.RightThumbstick.XAxis.Value;
                    rightThumbStickPosition.Y = controller.ExtendedGamepad.RightThumbstick.YAxis.Value;
                    leftTriggerValue = controller.ExtendedGamepad.LeftTrigger.Value;
                    rightTriggerValue = controller.ExtendedGamepad.RightTrigger.Value;
                }
                else if (controller.Gamepad != null)
                {
                    if (controller.Gamepad.ButtonA.IsPressed)
                        buttons |= Buttons.A;
                    if (controller.Gamepad.ButtonB.IsPressed)
                        buttons |= Buttons.B;
                    if (controller.Gamepad.ButtonX.IsPressed)
                        buttons |= Buttons.X;
                    if (controller.Gamepad.ButtonY.IsPressed)
                        buttons |= Buttons.Y;

                    if (controller.Gamepad.DPad.Up.IsPressed)
                    {
                        Up = ButtonState.Pressed;
                        buttons |= Buttons.DPadUp;
                    }
                    if (controller.Gamepad.DPad.Down.IsPressed)
                    {
                        Down = ButtonState.Pressed;
                        buttons |= Buttons.DPadDown;
                    }
                    if (controller.Gamepad.DPad.Left.IsPressed)
                    {
                        Left = ButtonState.Pressed;
                        buttons |= Buttons.DPadLeft;
                    }
                    if (controller.Gamepad.DPad.Right.IsPressed)
                    {
                        Right = ButtonState.Pressed;
                        buttons |= Buttons.DPadRight;
                    }
                }
            }
            GamePadState state = new GamePadState(
                new GamePadThumbSticks(leftThumbStickPosition, rightThumbStickPosition, leftDeadZoneMode, rightDeadZoneMode),
                new GamePadTriggers(leftTriggerValue, rightTriggerValue),
                new GamePadButtons(buttons),
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
