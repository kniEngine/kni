// Copyright (C)2022-24 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Oculus;
using Microsoft.Xna.Framework.XR;
using nkast.LibOVR;

namespace Microsoft.Xna.Platform.Input.Oculus
{
    public sealed class ConcreteTouchControllerStrategy : IOculusInput
    {
        private XRDevice _xrDevice;

        void IOculusInput.GetCapabilities(TouchControllerType controllerType,
            ref GamePadType gamePadType, ref string displayName, ref string identifier, ref bool isConnected,
            ref Buttons buttons,
            ref bool hasLeftVibrationMotor, ref bool hasRightVibrationMotor,
            ref bool hasVoiceSupport
            )
        {
            var session = _xrDevice.Session;
            if (session == null)
            {
                gamePadType = GamePadType.Unknown;
                displayName = String.Empty;
                identifier = String.Empty;
                isConnected = false;
                buttons = (Buttons)0;
                hasLeftVibrationMotor = false;
                hasRightVibrationMotor = false;
                hasVoiceSupport = false;
            }

            OvrControllerType ovrControllerType = (OvrControllerType)controllerType;
            OvrControllerType connectedControllerTypes = session.GetConnectedControllerTypes();

            isConnected = ((int)connectedControllerTypes & (int)ovrControllerType) != 0;

            connectedControllerTypes =(OvrControllerType)((int)connectedControllerTypes & (int)ovrControllerType);

            if (((int)connectedControllerTypes & (int)OvrControllerType.LTouch) == (int)OvrControllerType.LTouch)
            {
                buttons |= Buttons.X;
                buttons |= Buttons.Y;
                buttons |= Buttons.LeftThumbstickLeft & Buttons.LeftThumbstickRight;
                buttons |= Buttons.LeftThumbstickDown & Buttons.LeftThumbstickUp;
                //buttons |= Buttons.LeftStick;
                //buttons |= Buttons.Start;
            }

            if (((int)connectedControllerTypes & (int)OvrControllerType.RTouch) == (int)OvrControllerType.RTouch)
            {
                buttons |= Buttons.A;
                buttons |= Buttons.B;
                buttons |= Buttons.RightThumbstickLeft & Buttons.RightThumbstickRight;
                buttons |= Buttons.RightThumbstickDown & Buttons.RightThumbstickUp;
                //buttons |= Buttons.RightStick;
                //buttons |= Buttons.Back;
            }

        }

        Buttons _virtualButtons;

        public ConcreteTouchControllerStrategy(XRDevice xrDevice)
        {
            this._xrDevice = xrDevice;
        }

        TouchControllerState IOculusInput.GetState(TouchControllerType controllerType)
        {
            var session = _xrDevice.Session;
            if (session == null)
                return new TouchControllerState();

            OvrControllerType ovrControllerType = (OvrControllerType)controllerType;

            OvrControllerType connectedControllerTypes = session.GetConnectedControllerTypes();
            var isConnected = ((int)connectedControllerTypes & (int)ovrControllerType) != 0;
            if (!isConnected)
                return new TouchControllerState();

            OvrInputState ovrInputState;
            var ovrResult = session.GetInputState(ovrControllerType, out ovrInputState);

            GamePadThumbSticks thumbSticks = default(GamePadThumbSticks);
            GamePadTriggers triggers = default(GamePadTriggers);
            GamePadTriggers grips = default(GamePadTriggers);
            Buttons buttons = default(Buttons);
            Buttons touches = default(Buttons);

            thumbSticks = new GamePadThumbSticks(
                leftPosition:  new Vector2(ovrInputState.Thumbstick[0].X, ovrInputState.Thumbstick[0].Y),
                rightPosition: new Vector2(ovrInputState.Thumbstick[1].X, ovrInputState.Thumbstick[1].Y));

            triggers = new GamePadTriggers(
                    leftTrigger: ovrInputState.IndexTrigger[0],
                    rightTrigger: ovrInputState.IndexTrigger[1]);

            grips = new GamePadTriggers(
                    leftTrigger: ovrInputState.HandTrigger[0],
                    rightTrigger: ovrInputState.HandTrigger[1]);

            // left buttons
            if (((int)ovrInputState.Buttons & (int)OvrButton.X) != 0)
                buttons |= Buttons.X;
            if (((int)ovrInputState.Buttons & (int)OvrButton.Y) != 0)
                buttons |= Buttons.Y;
            if (((int)ovrInputState.Buttons & (int)OvrButton.LThumb) != 0)
                buttons |= Buttons.LeftStick;
            if (((int)ovrInputState.Buttons & (int)OvrButton.Enter) != 0) // Menu
                buttons |= Buttons.Start;

            // right buttons
            if (((int)ovrInputState.Buttons & (int)OvrButton.A) != 0)
                buttons |= Buttons.A;
            if (((int)ovrInputState.Buttons & (int)OvrButton.B) != 0)
                buttons |= Buttons.B;
            if (((int)ovrInputState.Buttons & (int)OvrButton.RThumb) != 0)
                buttons |= Buttons.RightStick;
            if (((int)ovrInputState.Buttons & (int)OvrButton.Home) != 0) // Oculus button
                buttons |= Buttons.Back;

            float TriggerThresholdOn = 0.6f;
            float TriggerThresholdOff = 0.7f;
            float ThumbstickThresholdOn = 0.5f;
            float ThumbstickThresholdOff = 0.4f;
            // virtual trigger buttons
            if (ovrInputState.IndexTrigger[0] > TriggerThresholdOn && (_virtualButtons & Buttons.LeftTrigger) == 0)
                _virtualButtons |= Buttons.LeftTrigger;
            else if (ovrInputState.IndexTrigger[0] < TriggerThresholdOff && (_virtualButtons & Buttons.LeftTrigger) != 0)
                _virtualButtons &= ~Buttons.LeftTrigger;
            if (ovrInputState.IndexTrigger[1] > TriggerThresholdOn && (_virtualButtons & Buttons.RightTrigger) == 0)
                _virtualButtons |= Buttons.RightTrigger;
            else if (ovrInputState.IndexTrigger[1] < TriggerThresholdOff && (_virtualButtons & Buttons.RightTrigger) != 0)
                _virtualButtons &= ~Buttons.RightTrigger;
            // virtual grip buttons
            if (ovrInputState.HandTriggerRaw[0] > TriggerThresholdOn && (_virtualButtons & Buttons.LeftGrip) == 0)
                _virtualButtons |= Buttons.LeftGrip;
            else if (ovrInputState.HandTriggerRaw[0] < TriggerThresholdOff && (_virtualButtons & Buttons.LeftGrip) != 0)
                _virtualButtons &= ~Buttons.LeftGrip;
            if (ovrInputState.HandTriggerRaw[1] > TriggerThresholdOn && (_virtualButtons & Buttons.RightGrip) == 0)
                _virtualButtons |= Buttons.RightGrip;
            else if (ovrInputState.HandTriggerRaw[1] < TriggerThresholdOff && (_virtualButtons & Buttons.RightGrip) != 0)
                _virtualButtons &= ~Buttons.RightGrip;
            // virtual thumbstick buttons
            if (ovrInputState.Thumbstick[0].X < -ThumbstickThresholdOn && (_virtualButtons & Buttons.LeftThumbstickLeft) == 0)
                _virtualButtons |= Buttons.LeftThumbstickLeft;
            else if (ovrInputState.Thumbstick[0].X > -ThumbstickThresholdOff && (_virtualButtons & Buttons.LeftThumbstickLeft) != 0)
                _virtualButtons &= ~Buttons.LeftThumbstickLeft;
            if (ovrInputState.Thumbstick[1].X < -ThumbstickThresholdOn && (_virtualButtons & Buttons.RightThumbstickLeft) == 0)
                _virtualButtons |= Buttons.RightThumbstickLeft;
            else if (ovrInputState.Thumbstick[1].X > -ThumbstickThresholdOff && (_virtualButtons & Buttons.RightThumbstickLeft) != 0)
                _virtualButtons &= ~Buttons.RightThumbstickLeft;
            if (ovrInputState.Thumbstick[0].X > ThumbstickThresholdOn && (_virtualButtons & Buttons.LeftThumbstickRight) == 0)
                _virtualButtons |= Buttons.LeftThumbstickRight;
            else if (ovrInputState.Thumbstick[0].X < ThumbstickThresholdOff && (_virtualButtons & Buttons.LeftThumbstickRight) != 0)
                _virtualButtons &= ~Buttons.LeftThumbstickRight;
            if (ovrInputState.Thumbstick[1].X > ThumbstickThresholdOn && (_virtualButtons & Buttons.RightThumbstickRight) == 0)
                _virtualButtons |= Buttons.RightThumbstickRight;
            else if (ovrInputState.Thumbstick[1].X < ThumbstickThresholdOff && (_virtualButtons & Buttons.RightThumbstickRight) != 0)
                _virtualButtons &= ~Buttons.RightThumbstickRight;
            if (ovrInputState.Thumbstick[0].Y < -ThumbstickThresholdOn && (_virtualButtons & Buttons.LeftThumbstickDown) == 0)
                _virtualButtons |= Buttons.LeftThumbstickDown;
            else if (ovrInputState.Thumbstick[0].Y > -ThumbstickThresholdOff && (_virtualButtons & Buttons.LeftThumbstickDown) != 0)
                _virtualButtons &= ~Buttons.LeftThumbstickDown;
            if (ovrInputState.Thumbstick[1].Y < -ThumbstickThresholdOn && (_virtualButtons & Buttons.RightThumbstickDown) == 0)
                _virtualButtons |= Buttons.RightThumbstickDown;
            else if (ovrInputState.Thumbstick[1].Y > -ThumbstickThresholdOff && (_virtualButtons & Buttons.RightThumbstickDown) != 0)
                _virtualButtons &= ~Buttons.RightThumbstickDown;
            if (ovrInputState.Thumbstick[0].Y > ThumbstickThresholdOn && (_virtualButtons & Buttons.LeftThumbstickUp) == 0)
                _virtualButtons |= Buttons.LeftThumbstickUp;
            else if (ovrInputState.Thumbstick[0].Y < ThumbstickThresholdOff && (_virtualButtons & Buttons.LeftThumbstickUp) != 0)
                _virtualButtons &= ~Buttons.LeftThumbstickUp;
            if (ovrInputState.Thumbstick[1].Y > ThumbstickThresholdOn && (_virtualButtons & Buttons.RightThumbstickUp) == 0)
                _virtualButtons |= Buttons.RightThumbstickUp;
            else if (ovrInputState.Thumbstick[1].Y < ThumbstickThresholdOff && (_virtualButtons & Buttons.RightThumbstickUp) != 0)
                _virtualButtons &= ~Buttons.RightThumbstickUp;

            buttons |= _virtualButtons;

            // left touches
            if (((int)ovrInputState.Touches & (int)OvrTouch.A) != 0)
                touches |= Buttons.A;
            if (((int)ovrInputState.Touches & (int)OvrTouch.B) != 0)
                touches |= Buttons.B;
            if (((int)ovrInputState.Touches & (int)OvrTouch.LThumb) != 0)
                touches |= Buttons.LeftStick;
            if (((int)ovrInputState.Touches & (int)OvrTouch.LIndexTrigger) != 0)
                touches |= Buttons.LeftTrigger;

            // right touches
            if (((int)ovrInputState.Touches & (int)OvrTouch.X) != 0)
                touches |= Buttons.X;
            if (((int)ovrInputState.Touches & (int)OvrTouch.Y) != 0)
                touches |= Buttons.Y;
            if (((int)ovrInputState.Touches & (int)OvrTouch.RThumb) != 0)
                touches |= Buttons.RightStick;
            if (((int)ovrInputState.Touches & (int)OvrTouch.RIndexTrigger) != 0)
                touches |= Buttons.RightTrigger;


            var state = new TouchControllerState(
                thumbSticks: thumbSticks,
                triggers: triggers,
                grips: grips,
                touchButtons: new TouchButtons(buttons, touches)
                );

            return state;
        }

        bool IOculusInput.SetVibration(TouchControllerType controllerType, float amplitude)
        {
            var session = _xrDevice.Session;
            if (session == null)
                return false;

            int result = session.SetControllerVibration((OvrControllerType)controllerType, 0.5f, amplitude);
            if (result < 0)
                return false;
            
            return true;
        }
    }
}
