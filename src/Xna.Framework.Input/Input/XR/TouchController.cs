// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Oculus;
using Microsoft.Xna.Platform.Input;
using Microsoft.Xna.Platform.Input.Oculus;


namespace Microsoft.Xna.Platform.Input
{
    public interface ITouchController
    {
        IOculusInput DeviceHandle { get; set; }

        GamePadCapabilities GetCapabilities(TouchControllerType type);
        GamePadState GetState(TouchControllerType type);
        bool SetVibration(TouchControllerType type, float amplitude);
    }
}

namespace Microsoft.Xna.Framework.Input.Oculus
{
    public class TouchController : ITouchController
    {
        private static TouchController _current;

        /// <summary>
        /// Returns the current GamePad instance.
        /// </summary> 
        public static TouchController Current
        {
            get
            {
                if (_current != null)
                    return _current;

                lock (typeof(TouchController))
                {
                    if (_current == null)
                        _current = new TouchController();

                    return _current;
                }
            }
        }

        public static IOculusInput DeviceHandle
        {
            get { return ((ITouchController)TouchController.Current).DeviceHandle; }
            set { ((ITouchController)TouchController.Current).DeviceHandle = value; }
        }

        public static GamePadCapabilities GetCapabilities(TouchControllerType type)
        {
            return ((ITouchController)TouchController.Current).GetCapabilities(type);
        }

        public static GamePadState GetState(TouchControllerType type)
        {
            return ((ITouchController)TouchController.Current).GetState(type);
        }

        public static bool SetVibration(TouchControllerType type, float amplitude)
        {
            return ((ITouchController)TouchController.Current).SetVibration(type, amplitude);
        }


        private TouchController()
        {
        }

        #region ITouchController

        IOculusInput ITouchController.DeviceHandle
        {
            get; set;
        }

        GamePadCapabilities ITouchController.GetCapabilities(TouchControllerType type)
        {
            IOculusInput device = ((ITouchController)this).DeviceHandle;
            if (device != null)
            {
                //--
                GamePadType gamePadType = GamePadType.Unknown;
                string displayName = String.Empty;
                string identifier = String.Empty;
                bool isConnected = false;
                Buttons buttons = (Buttons)0;
                bool hasLeftVibrationMotor = false;
                bool hasRightVibrationMotor = false;
                bool hasVoiceSupport = false;
                //--

                device.GetCapabilities(type,
                    ref gamePadType, ref displayName, ref identifier, ref isConnected,
                    ref buttons,
                    ref hasLeftVibrationMotor, ref hasRightVibrationMotor,
                    ref hasVoiceSupport
                    );

                return new GamePadCapabilities(
                        gamePadType: gamePadType,
                        displayName: displayName,
                        identifier: identifier,
                        isConnected: isConnected,
                        buttons: buttons,
                        hasLeftVibrationMotor: hasLeftVibrationMotor,
                        hasRightVibrationMotor: hasRightVibrationMotor,
                        hasVoiceSupport: hasVoiceSupport
                    );
            }
            else
            {
                return new GamePadCapabilities(
                        gamePadType: GamePadType.Unknown,
                        displayName: null,
                        identifier: null,
                        isConnected: false,
                        buttons: (Buttons)0,
                        hasLeftVibrationMotor: false,
                        hasRightVibrationMotor: false,
                        hasVoiceSupport: false
                    );
            }
        }

        GamePadState ITouchController.GetState(TouchControllerType type)
        {
            IOculusInput device = ((ITouchController)this).DeviceHandle;
            if (device != null)
                return device.GetState(type);
            else
                return new GamePadState();
        }

        bool ITouchController.SetVibration(TouchControllerType type, float amplitude)
        {
            IOculusInput device = ((ITouchController)this).DeviceHandle;
            if (device != null)
                return device.SetVibration(type, amplitude);
            else
                return false;
        }

        #endregion ITouchController

    }
}
