// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

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
            int maximumTouchCount = GetSystemMetrics(SM_MAXIMUMTOUCHES);
            bool isConnected = (maximumTouchCount > 0);
            bool hasPressure = false;
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

        public override void AddEvent(int nativeTouchId, TouchLocationState state, Vector2 position)
        {
            IntPtr wndHandle = this.WindowHandle;
            if (wndHandle != IntPtr.Zero)
            {
                GameWindow gameWindow = WinFormsGameWindow.FromHandle(wndHandle);
                Rectangle windowsBounds = gameWindow.ClientBounds;
                Point winSize = new Point(windowsBounds.Width, windowsBounds.Height);

                switch (state)
                {
                    case TouchLocationState.Pressed:
                        base.AddPressedEvent(nativeTouchId, position, winSize);
                        break;

                    case TouchLocationState.Moved:
                        base.AddMovedEvent(nativeTouchId, position, winSize);
                        break;

                    case TouchLocationState.Released:
                        base.AddReleasedEvent(nativeTouchId, position, winSize);
                        break;

                    case TouchLocationState.Invalid:
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public override void InvalidateTouches()
        {
            base.InvalidateTouches();
        }


        const int SM_MAXIMUMTOUCHES = 95;

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, ExactSpelling = true)]
        static extern int GetSystemMetrics(int nIndex);

    }
}