// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Platform.Input.Oculus;


namespace Microsoft.Xna.Framework.Input.Oculus
{
    public class TouchController
    {
        public static IOculusInput DeviceHandle { get; set; }

        static TouchController()
        {
        }

        public static GamePadCapabilities GetCapabilities(TouchControllerType type)
        {
            var device = DeviceHandle;
            if (device != null)
                return device.GetCapabilities(type);
            else
                return default(GamePadCapabilities);
        }

        public static TouchControllerState GetState(TouchControllerType type)
        {
            var device = DeviceHandle;
            if (device != null)
                return device.GetState(type);
            else
                return new TouchControllerState();
        }

        public static bool SetVibration(TouchControllerType type, float amplitude)
        {
            var device = DeviceHandle;
            if (device != null)
                return device.SetVibration(type, amplitude);
            else
                return false;
        }

    }
}
