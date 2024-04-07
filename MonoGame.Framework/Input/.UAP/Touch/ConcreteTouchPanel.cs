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
    public sealed class ConcreteTouchPanel : TouchPanelStrategy
    {

        public override IntPtr WindowHandle
        {
            get { return base.WindowHandle; }
            set { base.WindowHandle = value; }
        }

        public override int DisplayWidth
        {
            get { return base.DisplayWidth; }
            set { base.DisplayWidth = value; }
        }

        public override int DisplayHeight
        {
            get { return base.DisplayHeight; }
            set { base.DisplayHeight = value; }
        }

        public override DisplayOrientation DisplayOrientation
        {
            get { return base.DisplayOrientation; }
            set { base.DisplayOrientation = value; }
        }

        public override GestureType EnabledGestures
        {
            get { return base.EnabledGestures; }
            set { base.EnabledGestures = value; }
        }


        public override bool IsGestureAvailable
        {
            get { return base.IsGestureAvailable; }
        }

        public ConcreteTouchPanel()
            : base()
        {
            // Initialize Capabilities
            int maximumTouchCount = 0;
            bool isConnected = false;
            bool hasPressure = false;
            IReadOnlyList<WinInput.PointerDevice> pointerDevices = WinInput.PointerDevice.GetPointerDevices();
            // Iterate through all pointer devices and find the maximum number of concurrent touches possible
            foreach (WinInput.PointerDevice pointerDevice in pointerDevices)
            {
                maximumTouchCount = Math.Max(maximumTouchCount, (int)pointerDevice.MaxContacts);

                if (pointerDevice.PointerDeviceType == WinInput.PointerDeviceType.Touch)
                    isConnected = true;
            }
            _capabilities = base.CreateTouchPanelCapabilities(maximumTouchCount, isConnected, hasPressure);
        }

        public override TouchPanelCapabilities GetCapabilities()
        {
            return _capabilities;
        }

        public override TouchCollection GetState()
        {
            return base.GetState();
        }

        public override GestureSample ReadGesture()
        {
            return base.ReadGesture();
        }

        public override void AddEvent(int id, TouchLocationState state, Vector2 position)
        {
            IntPtr wndHandle = this.WindowHandle;
            if (wndHandle != IntPtr.Zero)
            {
                GameWindow gameWindow = UAPGameWindow.FromHandle(wndHandle);
                Rectangle windowsBounds = gameWindow.ClientBounds;

                Point winSize = new Point(windowsBounds.Width, windowsBounds.Height);
                base.LegacyAddEvent(id, state, position, winSize);
            }
        }

        public override void InvalidateTouches()
        {
            base.InvalidateTouches();
        }

    }
}