﻿// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Oculus;
using Microsoft.Xna.Framework.XR;
using nkast.Wasm;

namespace Microsoft.Xna.Platform.Input.Oculus
{
    public sealed class ConcreteTouchController : IOculusInput
    {
        private ConcreteXRDevice _xrDevice;

        unsafe void IOculusInput.GetCapabilities(TouchControllerType controllerType,
            ref GamePadType gamePadType, ref string displayName, ref string identifier, ref bool isConnected,
            ref Buttons buttons,
            ref bool hasLeftVibrationMotor, ref bool hasRightVibrationMotor,
            ref bool hasVoiceSupport
            )
        {
            var device = _xrDevice;
            if (device != null)
                device.GetCapabilities(controllerType,
                    ref gamePadType, ref displayName, ref identifier, ref isConnected,
                    ref buttons,
                    ref hasLeftVibrationMotor, ref hasRightVibrationMotor,
                    ref hasVoiceSupport);
        }

        internal ConcreteTouchController(ConcreteXRDevice xrDevice)
        {
            this._xrDevice = xrDevice;
        }

        GamePadState IOculusInput.GetState(TouchControllerType controllerType)
        {
            var device = _xrDevice;
            if (device != null)
                return device.GetGamePadState(controllerType);

            return new GamePadState();
        }

        bool IOculusInput.SetVibration(TouchControllerType controllerType, float amplitude)
        {
            var device = _xrDevice;
            if (device != null)
                return device.SetVibration(controllerType, amplitude);

            return false;
        }
    }
}
