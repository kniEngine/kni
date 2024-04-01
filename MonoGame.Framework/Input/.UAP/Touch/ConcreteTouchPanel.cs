// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using WinInput = Windows.Devices.Input;

namespace Microsoft.Xna.Platform.Input.Touch
{
    public class ConcreteTouchPanel : TouchPanelStrategy
    {
        internal GameWindow PrimaryWindow;

        public override IntPtr WindowHandle
        {
            get { return PrimaryWindow.TouchPanelState.WindowHandle; }
            set { PrimaryWindow.TouchPanelState.WindowHandle = value; }
        }

        public override int DisplayWidth
        {
            get { return PrimaryWindow.TouchPanelState.DisplayWidth; }
            set { PrimaryWindow.TouchPanelState.DisplayWidth = value; }
        }

        public override int DisplayHeight
        {
            get { return PrimaryWindow.TouchPanelState.DisplayHeight; }
            set { PrimaryWindow.TouchPanelState.DisplayHeight = value; }
        }

        public override DisplayOrientation DisplayOrientation
        {
            get { return PrimaryWindow.TouchPanelState.DisplayOrientation; }
            set { PrimaryWindow.TouchPanelState.DisplayOrientation = value; }
        }

        public override GestureType EnabledGestures
        {
            get { return PrimaryWindow.TouchPanelState.EnabledGestures; }
            set { PrimaryWindow.TouchPanelState.EnabledGestures = value; }
        }


        public override bool IsGestureAvailable
        {
            get { return PrimaryWindow.TouchPanelState.IsGestureAvailable; }
        }

        internal ConcreteTouchPanel()
        {
            // Initialize Capabilities
            _capabilities._maximumTouchCount = 0;
            _capabilities._isConnected = false;
            IReadOnlyList<WinInput.PointerDevice> pointerDevices = WinInput.PointerDevice.GetPointerDevices();
            // Iterate through all pointer devices and find the maximum number of concurrent touches possible
            foreach (WinInput.PointerDevice pointerDevice in pointerDevices)
            {
                _capabilities._maximumTouchCount = Math.Max(_capabilities._maximumTouchCount, (int)pointerDevice.MaxContacts);

                if (pointerDevice.PointerDeviceType == WinInput.PointerDeviceType.Touch)
                    _capabilities._isConnected = true;
            }
        }

        public override TouchPanelCapabilities GetCapabilities()
        {
            return _capabilities;
        }

        public override TouchCollection GetState()
        {
            return PrimaryWindow.TouchPanelState.GetState();
        }

        public override GestureSample ReadGesture()
        {
            return PrimaryWindow.TouchPanelState.ReadGesture();
        }

        public override void AddEvent(int id, TouchLocationState state, Vector2 position)
        {
            PrimaryWindow.TouchPanelState.AddEvent(id, state, position);
        }

        public override void ReleaseAllTouches()
        {
            PrimaryWindow.TouchPanelState.ReleaseAllTouches();
        }

    }
}