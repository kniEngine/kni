using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Oculus;


namespace Microsoft.Xna.Platform.Input.Oculus
{
    public abstract class TouchControllerStrategy
    {
        internal abstract GamePadCapabilities GetCapabilities(TouchControllerType controllerType);
        internal abstract TouchControllerState GetState(TouchControllerType controllerType);
        internal abstract bool SetVibration(TouchControllerType controllerType, float amplitude);
     
    }
}
