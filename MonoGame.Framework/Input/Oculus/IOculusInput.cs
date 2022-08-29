// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Oculus;


namespace Microsoft.Xna.Platform.Input.Oculus
{
    public interface IOculusInput
    {
        GamePadCapabilities GetCapabilities(TouchControllerType type);
        TouchControllerState GetState(TouchControllerType type);
        bool SetVibration(TouchControllerType type, float amplitude);
    }
}