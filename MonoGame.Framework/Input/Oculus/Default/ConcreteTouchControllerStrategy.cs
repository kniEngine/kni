using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Oculus;


namespace Microsoft.Xna.Platform.Input.Oculus
{
    public sealed class ConcreteTouchControllerStrategy : TouchControllerStrategy
    {
        internal override GamePadCapabilities GetCapabilities(TouchControllerType controllerType)
        {
            return new GamePadCapabilities();
        }

        internal override TouchControllerState GetState(TouchControllerType controllerType)
        {
            return new TouchControllerState();
        }

        internal override bool SetVibration(TouchControllerType controllerType, float amplitude)
        {
            return false;
        }
    }
}
