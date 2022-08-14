using System;
using System.Collections.Generic;
using Microsoft.Xna.Platform.Input.Oculus;


namespace Microsoft.Xna.Framework.Input.Oculus
{
    public class TouchController
    {
        private static TouchControllerStrategy _strategy;

        static TouchController()
        {
            _strategy = new ConcreteTouchControllerStrategy();
        }

        public static GamePadCapabilities GetCapabilities(TouchControllerType type)
        {
            return _strategy.GetCapabilities(type);
        }

        public static TouchControllerState GetState(TouchControllerType type)
        {
            return _strategy.GetState(type);
        }

        public static bool SetVibration(TouchControllerType type, float amplitude)
        {
            return _strategy.SetVibration(type, amplitude);
        }

    }
}
