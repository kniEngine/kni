// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Android.Views;

namespace Microsoft.Xna.Platform.Input
{
    internal class AndroidGamePadDevice : GamePadDevice
    {
        public InputDevice _device;
        public int _deviceId;
        public string _descriptor;
        public bool _isConnected;
        public bool DPadButtons;

        public Buttons _buttons;
        public float _leftTrigger, _rightTrigger;
        public Vector2 _leftStick, _rightStick;

        public AndroidGamePadDevice(InputDevice device, GamePadCapabilities caps)
            : base()
        {
            _device = device;
            _deviceId = device.Id;
            _descriptor = device.Descriptor;
            _isConnected = true;
            Capabilities = caps;
        }

    }
}
