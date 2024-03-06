// Copyright (C)2022-24 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Oculus;
using nkast.LibOVR;

namespace Microsoft.Xna.Platform.Input.Oculus
{
    public sealed class ConcreteTouchControllerStrategy : IOculusInput
    {
        private OvrDevice _ovrDevice;

        GamePadCapabilities IOculusInput.GetCapabilities(TouchControllerType controllerType)
        {
            var session = _ovrDevice.Session;
            if (session == null)
                return default(GamePadCapabilities);

            OvrControllerType ovrControllerType = (OvrControllerType)controllerType;
            OvrControllerType connectedControllerTypes = session.GetConnectedControllerTypes();

            //--
            GamePadType gamePadType = GamePadType.Unknown;
            string displayName = String.Empty;
            string identifier = String.Empty;
            bool isConnected;
            bool hasAButton = false;
            bool hasBButton = false;
            bool hasXButton = false;
            bool hasYButton = false;
            bool hasStartButton = false;
            bool hasBackButton = false;
            bool hasBigButton = false;
            bool hasDPadDownButton = false;
            bool hasDPadLeftButton = false;
            bool hasDPadRightButton = false;
            bool hasDPadUpButton = false;
            bool hasLeftShoulderButton = false;
            bool hasLeftStickButton = false;
            bool hasRightShoulderButton = false;
            bool hasRightStickButton = false;
            bool hasLeftXThumbStick = false;
            bool hasLeftYThumbStick = false;
            bool hasRightXThumbStick = false;
            bool hasRightYThumbStick = false;
            bool hasLeftTrigger = false;
            bool hasRightTrigger = false;
            bool hasLeftVibrationMotor = false;
            bool hasRightVibrationMotor = false;
            bool hasVoiceSupport = false;
            //--

            isConnected = ((int)connectedControllerTypes & (int)ovrControllerType) != 0;

            connectedControllerTypes =(OvrControllerType)((int)connectedControllerTypes & (int)ovrControllerType);

            if (((int)connectedControllerTypes & (int)OvrControllerType.LTouch) == (int)OvrControllerType.LTouch)
            {
                hasXButton = true;
                hasYButton = true;
                hasLeftXThumbStick = true;
                hasLeftYThumbStick = true;
                //capabilities.HasLeftStickButton = true;
                //capabilities.HasStartButton = true;
            }

            if (((int)connectedControllerTypes & (int)OvrControllerType.RTouch) == (int)OvrControllerType.RTouch)
            {
                hasAButton = true;
                hasBButton = true;
                hasRightXThumbStick = true;
                hasRightYThumbStick = true;
                //capabilities.HasRightStickButton = true;
                //capabilities.HasBackButton = true;
            }

            return new GamePadCapabilities(
                    gamePadType:gamePadType,
                    displayName:displayName,
                    identifier:identifier,
                    isConnected:isConnected,
                    hasAButton: hasAButton,
                    hasBButton: hasBButton,
                    hasXButton:hasXButton,
                    hasYButton:hasYButton,
                    hasStartButton:hasStartButton,
                    hasBackButton: hasBackButton,
                    hasBigButton:hasBigButton,
                    hasDPadDownButton: hasDPadDownButton,
                    hasDPadLeftButton: hasDPadLeftButton,
                    hasDPadRightButton: hasDPadRightButton,
                    hasDPadUpButton: hasDPadUpButton,
                    hasLeftShoulderButton: hasLeftShoulderButton,
                    hasLeftStickButton: hasLeftStickButton,
                    hasRightShoulderButton:hasRightShoulderButton,
                    hasRightStickButton:hasRightStickButton,
                    hasLeftXThumbStick:hasLeftXThumbStick,
                    hasLeftYThumbStick:hasLeftYThumbStick,
                    hasRightXThumbStick:hasRightXThumbStick,
                    hasRightYThumbStick:hasRightYThumbStick,
                    hasLeftTrigger:hasLeftTrigger,
                    hasRightTrigger:hasRightTrigger,
                    hasLeftVibrationMotor:hasLeftVibrationMotor,
                    hasRightVibrationMotor:hasRightVibrationMotor,
                    hasVoiceSupport:hasVoiceSupport
                );
        }

        Buttons _virtualButtons;

        public ConcreteTouchControllerStrategy(OvrDevice ovrDevice)
        {
            this._ovrDevice = ovrDevice;
        }

        TouchControllerState IOculusInput.GetState(TouchControllerType controllerType)
        {
            var session = _ovrDevice.Session;
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
            var session = _ovrDevice.Session;
            if (session == null)
                return false;

            int result = session.SetControllerVibration((OvrControllerType)controllerType, 0.5f, amplitude);
            if (result < 0)
                return false;
            
            return true;
        }
    }
}
