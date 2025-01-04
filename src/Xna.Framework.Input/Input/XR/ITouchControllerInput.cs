// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Oculus;


namespace Microsoft.Xna.Platform.Input.XR
{
    public interface ITouchControllerInput
    {
        void GetCapabilities(TouchControllerType type,
            ref GamePadType gamePadType, ref string displayName, ref string identifier, ref bool isConnected,
            ref Buttons buttons,
            ref bool hasLeftVibrationMotor, ref bool hasRightVibrationMotor,
            ref bool hasVoiceSupport
            );
        GamePadState GetState(TouchControllerType type);
        bool SetVibration(TouchControllerType type, float amplitude);
    }
}