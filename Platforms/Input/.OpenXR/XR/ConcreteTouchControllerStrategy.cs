// Copyright (C)2022-24 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.XR;
using Microsoft.Xna.Framework.XR;
using Microsoft.Xna.Platform.XR;
using nkast.LibOXR;
using Silk.NET.OpenXR;
using XrAction = Silk.NET.OpenXR.Action;

namespace Microsoft.Xna.Platform.Input.XR
{
    public sealed class ConcreteTouchControllerStrategy : ITouchControllerInput
    {
        private ConcreteXRDevice _xrDevice;

        unsafe void ITouchControllerInput.GetCapabilities(TouchControllerType controllerType,
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

            //OvrControllerType connectedControllerTypes = session.GetConnectedControllerTypes();
            TouchControllerType connectedControllerTypes = default;

            Result xrResult;

            ActionPath leftHandPath, rightHandPath;
            _xrDevice.Instance.StringToPath("/user/hand/left", out leftHandPath);
            _xrDevice.Instance.StringToPath("/user/hand/right", out rightHandPath);

            ActionPath oculusTouchProfilePath;
            _xrDevice.Instance.StringToPath("/interaction_profiles/oculus/touch_controller", out oculusTouchProfilePath);

            session.GetCurrentInteractionProfile(leftHandPath, out InteractionProfileState leftProfileState);
            session.GetCurrentInteractionProfile(rightHandPath, out InteractionProfileState rightProfileState);

            if (leftProfileState.InteractionProfile == oculusTouchProfilePath.Handle)
                connectedControllerTypes |= TouchControllerType.LTouch;

            if (rightProfileState.InteractionProfile == oculusTouchProfilePath.Handle)
                connectedControllerTypes |= TouchControllerType.RTouch;


            isConnected = ((int)connectedControllerTypes & (int)controllerType) != 0;

            connectedControllerTypes = (TouchControllerType)((int)connectedControllerTypes & (int)controllerType);

            if (((int)connectedControllerTypes & (int)TouchControllerType.LTouch) == (int)TouchControllerType.LTouch)
            {
                buttons |= Buttons.X;
                buttons |= Buttons.Y;
                buttons |= Buttons.LeftThumbstickLeft & Buttons.LeftThumbstickRight;
                buttons |= Buttons.LeftThumbstickDown & Buttons.LeftThumbstickUp;
                buttons |= Buttons.LeftTrigger & Buttons.LeftGrip;
                //buttons |= Buttons.LeftStick;
                //buttons |= Buttons.Start;
            }

            if (((int)connectedControllerTypes & (int)TouchControllerType.RTouch) == (int)TouchControllerType.RTouch)
            {
                buttons |= Buttons.A;
                buttons |= Buttons.B;
                buttons |= Buttons.RightThumbstickLeft & Buttons.RightThumbstickRight;
                buttons |= Buttons.RightThumbstickDown & Buttons.RightThumbstickUp;
                buttons |= Buttons.RightTrigger & Buttons.RightGrip;
                //buttons |= Buttons.RightStick;
                //buttons |= Buttons.Back;
            }

        }

        Buttons _virtualButtons;

        internal ConcreteTouchControllerStrategy(ConcreteXRDevice xrDevice)
        {
            this._xrDevice = xrDevice;
        }

        GamePadState ITouchControllerInput.GetState(TouchControllerType controllerType)
        {
            var session = _xrDevice.Session;
            if (session == null)
                return new GamePadState();


            //OvrControllerType connectedControllerTypes = session.GetConnectedControllerTypes();
            TouchControllerType connectedControllerTypes = default;

            Result xrResult;

            ActionPath leftHandPath, rightHandPath;
            _xrDevice.Instance.StringToPath("/user/hand/left", out leftHandPath);
            _xrDevice.Instance.StringToPath("/user/hand/right", out rightHandPath);

            ActionPath oculusTouchProfilePath;
            _xrDevice.Instance.StringToPath("/interaction_profiles/oculus/touch_controller", out oculusTouchProfilePath);

            session.GetCurrentInteractionProfile(leftHandPath, out InteractionProfileState leftProfileState);
            session.GetCurrentInteractionProfile(rightHandPath, out InteractionProfileState rightProfileState);

            if (leftProfileState.InteractionProfile == oculusTouchProfilePath.Handle)
                connectedControllerTypes |= TouchControllerType.LTouch;

            if (rightProfileState.InteractionProfile == oculusTouchProfilePath.Handle)
                connectedControllerTypes |= TouchControllerType.RTouch;


            var isConnected = ((int)connectedControllerTypes & (int)controllerType) != 0;
            if (!isConnected)
                return new GamePadState();

            //OvrInputState ovrInputState;
            //var ovrResult = session.GetInputState(ovrControllerType, out ovrInputState);

            GamePadThumbSticks thumbSticks = default(GamePadThumbSticks);
            GamePadTriggers triggers = default(GamePadTriggers);
            GamePadTriggers grips = default(GamePadTriggers);
            Buttons buttons = default(Buttons);
            Buttons touches = default(Buttons);


            ActionStateVector2f ThumbStickL = session.GetActionStateVector2(_xrDevice._moveOnJoystickActionL);
            ActionStateVector2f ThumbStickR = session.GetActionStateVector2(_xrDevice._moveOnJoystickActionR);
         
            thumbSticks = new GamePadThumbSticks(
                leftPosition:  new Vector2(ThumbStickL.CurrentState.X, ThumbStickL.CurrentState.Y),
                rightPosition: new Vector2(ThumbStickR.CurrentState.X, ThumbStickR.CurrentState.Y));

            ActionStateFloat moveYStateL = session.GetActionStateFloat(_xrDevice._moveOnYActionL);
            ActionStateFloat moveYStateR = session.GetActionStateFloat(_xrDevice._moveOnYActionR);

            triggers = new GamePadTriggers(
                    leftTrigger:  moveYStateL.CurrentState,
                    rightTrigger: moveYStateR.CurrentState);

            ActionStateFloat moveXStateL = session.GetActionStateFloat(_xrDevice._moveOnXActionL);
            ActionStateFloat moveXStateR = session.GetActionStateFloat(_xrDevice._moveOnXActionR);

            grips = new GamePadTriggers(
                    leftTrigger:  moveXStateL.CurrentState,
                    rightTrigger: moveXStateR.CurrentState);

            //// left buttons
            ActionStateBoolean _stateButtonX = session.GetActionStateBoolean(_xrDevice._actionButtonX);
            ActionStateBoolean _stateButtonY = session.GetActionStateBoolean(_xrDevice._actionButtonY);
            ActionStateBoolean _stateButtonLTS = session.GetActionStateBoolean(_xrDevice._actionButtonLeftThumbstick);

            if (_stateButtonX.CurrentState != 0)
                buttons |= Buttons.X;
            if (_stateButtonY.CurrentState != 0)
                buttons |= Buttons.Y;
            if (_stateButtonLTS.CurrentState != 0)
                buttons |= Buttons.LeftStick;
            //if (((int)ovrInputState.Buttons & (int)OvrButton.Enter) != 0) // Menu
            //    buttons |= Buttons.Start;

            //// right buttons
            ActionStateBoolean _stateButtonA = session.GetActionStateBoolean(_xrDevice._actionButtonA);
            ActionStateBoolean _stateButtonB = session.GetActionStateBoolean(_xrDevice._actionButtonB);
            ActionStateBoolean _stateButtonRTS = session.GetActionStateBoolean(_xrDevice._actionButtonRightThumbstick);

            if (_stateButtonA.CurrentState!=0)
                buttons |= Buttons.A;
            if (_stateButtonB.CurrentState != 0)
                buttons |= Buttons.B;
            if (_stateButtonRTS.CurrentState != 0)
                buttons |= Buttons.RightStick;
            //if (((int)ovrInputState.Buttons & (int)OvrButton.Home) != 0) // Oculus button
            //    buttons |= Buttons.Back;

            float TriggerThresholdOn = 0.6f;
            float TriggerThresholdOff = 0.7f;
            float ThumbstickThresholdOn = 0.5f;
            float ThumbstickThresholdOff = 0.4f;
            //// virtual trigger buttons
            if (triggers.Left > TriggerThresholdOn && (_virtualButtons & Buttons.LeftTrigger) == 0)
                _virtualButtons |= Buttons.LeftTrigger;
            else if (triggers.Left < TriggerThresholdOff && (_virtualButtons & Buttons.LeftTrigger) != 0)
                _virtualButtons &= ~Buttons.LeftTrigger;
            if (triggers.Right > TriggerThresholdOn && (_virtualButtons & Buttons.RightTrigger) == 0)
                _virtualButtons |= Buttons.RightTrigger;
            else if (triggers.Right < TriggerThresholdOff && (_virtualButtons & Buttons.RightTrigger) != 0)
                _virtualButtons &= ~Buttons.RightTrigger;
            //// virtual grip buttons
            if (grips.Left > TriggerThresholdOn && (_virtualButtons & Buttons.LeftGrip) == 0)
                _virtualButtons |= Buttons.LeftGrip;
            else if (grips.Left < TriggerThresholdOff && (_virtualButtons & Buttons.LeftGrip) != 0)
                _virtualButtons &= ~Buttons.LeftGrip;
            if (grips.Right > TriggerThresholdOn && (_virtualButtons & Buttons.RightGrip) == 0)
                _virtualButtons |= Buttons.RightGrip;
            else if (grips.Right < TriggerThresholdOff && (_virtualButtons & Buttons.RightGrip) != 0)
                _virtualButtons &= ~Buttons.RightGrip;
            //// virtual thumbstick buttons
            if (ThumbStickL.CurrentState.X < -ThumbstickThresholdOn && (_virtualButtons & Buttons.LeftThumbstickLeft) == 0)
                _virtualButtons |= Buttons.LeftThumbstickLeft;
            else if (ThumbStickL.CurrentState.X > -ThumbstickThresholdOff && (_virtualButtons & Buttons.LeftThumbstickLeft) != 0)
                _virtualButtons &= ~Buttons.LeftThumbstickLeft;
            if (ThumbStickR.CurrentState.X < -ThumbstickThresholdOn && (_virtualButtons & Buttons.RightThumbstickLeft) == 0)
                _virtualButtons |= Buttons.RightThumbstickLeft;
            else if (ThumbStickR.CurrentState.X > -ThumbstickThresholdOff && (_virtualButtons & Buttons.RightThumbstickLeft) != 0)
                _virtualButtons &= ~Buttons.RightThumbstickLeft;
            if (ThumbStickL.CurrentState.X > ThumbstickThresholdOn && (_virtualButtons & Buttons.LeftThumbstickRight) == 0)
                _virtualButtons |= Buttons.LeftThumbstickRight;
            else if (ThumbStickL.CurrentState.X < ThumbstickThresholdOff && (_virtualButtons & Buttons.LeftThumbstickRight) != 0)
                _virtualButtons &= ~Buttons.LeftThumbstickRight;
            if (ThumbStickR.CurrentState.X > ThumbstickThresholdOn && (_virtualButtons & Buttons.RightThumbstickRight) == 0)
                _virtualButtons |= Buttons.RightThumbstickRight;
            else if (ThumbStickR.CurrentState.X < ThumbstickThresholdOff && (_virtualButtons & Buttons.RightThumbstickRight) != 0)
                _virtualButtons &= ~Buttons.RightThumbstickRight;

            if (ThumbStickL.CurrentState.Y < -ThumbstickThresholdOn && (_virtualButtons & Buttons.LeftThumbstickDown) == 0)
                _virtualButtons |= Buttons.LeftThumbstickDown;
            else if (ThumbStickL.CurrentState.Y > -ThumbstickThresholdOff && (_virtualButtons & Buttons.LeftThumbstickDown) != 0)
                _virtualButtons &= ~Buttons.LeftThumbstickDown;
            if (ThumbStickR.CurrentState.Y < -ThumbstickThresholdOn && (_virtualButtons & Buttons.RightThumbstickDown) == 0)
                _virtualButtons |= Buttons.RightThumbstickDown;
            else if (ThumbStickR.CurrentState.Y > -ThumbstickThresholdOff && (_virtualButtons & Buttons.RightThumbstickDown) != 0)
                _virtualButtons &= ~Buttons.RightThumbstickDown;
            if (ThumbStickL.CurrentState.Y > ThumbstickThresholdOn && (_virtualButtons & Buttons.LeftThumbstickUp) == 0)
                _virtualButtons |= Buttons.LeftThumbstickUp;
            else if (ThumbStickL.CurrentState.Y < ThumbstickThresholdOff && (_virtualButtons & Buttons.LeftThumbstickUp) != 0)
                _virtualButtons &= ~Buttons.LeftThumbstickUp;
            if (ThumbStickR.CurrentState.Y > ThumbstickThresholdOn && (_virtualButtons & Buttons.RightThumbstickUp) == 0)
                _virtualButtons |= Buttons.RightThumbstickUp;
            else if (ThumbStickR.CurrentState.Y < ThumbstickThresholdOff && (_virtualButtons & Buttons.RightThumbstickUp) != 0)
                _virtualButtons &= ~Buttons.RightThumbstickUp;

            buttons |= _virtualButtons;

            //// left touches
            ActionStateBoolean _stateButtonXtouch = session.GetActionStateBoolean(_xrDevice._actionButtonXtouch);
            ActionStateBoolean _stateButtonYtouch = session.GetActionStateBoolean(_xrDevice._actionButtonYtouch);
            ActionStateBoolean _stateButtonLTStouch = session.GetActionStateBoolean(_xrDevice._actionButtonLeftThumbsticktouch);
            ActionStateFloat _stateTriggerLTouch = session.GetActionStateFloat(_xrDevice._actionTriggerLeftTouch);
            // BUG: Grip touch doesn't work on native OpenXR.
            // https://communityforums.atmeta.com/t5/OpenXR-Development/unable-to-get-grip-button-input-on-quest-1-getting-path/td-p/833021
            //ActionStateFloat _stateGripLTouch = session.GetActionStateFloat(_xrDevice._actionGripLeftTouch);

            if (_stateButtonXtouch.CurrentState != 0)
                touches |= Buttons.X;
            if (_stateButtonYtouch.CurrentState != 0)
                touches |= Buttons.Y;
            if (_stateButtonLTStouch.CurrentState != 0)
                touches |= Buttons.LeftStick;
            //if (((int)ovrInputState.Buttons & (int)OvrButton.Enter) != 0) // Menu
            //    touches |= Buttons.Start;
            if (_stateTriggerLTouch.CurrentState != 0)
                touches |= Buttons.LeftTrigger;
            //if (_stateGripLTouch.CurrentState != 0)
            //    touches |= Buttons.LeftGrip;

            //// right touches
            ActionStateBoolean _stateButtonAtouch = session.GetActionStateBoolean(_xrDevice._actionButtonAtouch);
            ActionStateBoolean _stateButtonBtouch = session.GetActionStateBoolean(_xrDevice._actionButtonBtouch);
            ActionStateBoolean _stateButtonRTStouch = session.GetActionStateBoolean(_xrDevice._actionButtonRightThumbsticktouch);
            ActionStateFloat _stateTriggerRTouch = session.GetActionStateFloat(_xrDevice._actionTriggerRightTouch);
            // BUG: Grip touch doesn't work on native OpenXR.
            // https://communityforums.atmeta.com/t5/OpenXR-Development/unable-to-get-grip-button-input-on-quest-1-getting-path/td-p/833021
            //ActionStateFloat _stateGripRTouch = session.GetActionStateFloat(_xrDevice._actionGripRightTouch);

            if (_stateButtonAtouch.CurrentState != 0)
                touches |= Buttons.A;
            if (_stateButtonBtouch.CurrentState != 0)
                touches |= Buttons.B;
            if (_stateButtonRTStouch.CurrentState != 0)
                touches |= Buttons.RightStick;
            //if (((int)ovrInputState.Buttons & (int)OvrButton.Home) != 0) // Oculus button
            //    touches |= Buttons.Back;
            if (_stateTriggerRTouch.CurrentState != 0)
                touches |= Buttons.RightTrigger;
            //if (_stateGripRTouch.CurrentState != 0)
            //    touches |= Buttons.RightGrip;


            var state = new GamePadState(
                thumbSticks: thumbSticks,
                triggers: triggers,
                grips: grips,
                touchButtons: new GamePadTouchButtons(buttons, touches),
                dPad: default(GamePadDPad)
                );

            return state;
        }

        const int XR_MIN_HAPTIC_DURATION = -1;

        private long ToXrTime(double sec)
        {
            return (long)(sec * 1000 * 1000 * 1000); //nSec
        }

        bool ITouchControllerInput.SetVibration(TouchControllerType controllerType, float amplitude)
        {
            var session = _xrDevice.Session;
            if (session == null)
                return false;

            if ((controllerType & TouchControllerType.LTouch) == TouchControllerType.LTouch)
            {
                Result xrResult = session.ApplyHapticFeedback(
                        _xrDevice._vibrateLeftFeedback, amplitude: amplitude, frequency: 3000f, duration: ToXrTime(0.5));
                if (xrResult != Result.Success)
                    return false;
            }

            if ((controllerType & TouchControllerType.RTouch) == TouchControllerType.RTouch)
            {
                Result xrResult = session.ApplyHapticFeedback(
                        _xrDevice._vibrateRightFeedback, amplitude: amplitude, frequency: 3000f, duration: ToXrTime(0.5));
                if (xrResult != Result.Success)
                    return false;
            }

            return true;
        }
    }
}
