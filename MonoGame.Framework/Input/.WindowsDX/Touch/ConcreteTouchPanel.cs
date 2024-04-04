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
        internal GameWindow PrimaryWindow;

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

        internal ConcreteTouchPanel()
        {
            // Initialize Capabilities
            _capabilities._maximumTouchCount = GetSystemMetrics(SM_MAXIMUMTOUCHES);
            _capabilities._isConnected = (_capabilities._maximumTouchCount > 0);
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
            Point winSize = new Point(this.PrimaryWindow.ClientBounds.Width, this.PrimaryWindow.ClientBounds.Height);

            base.LegacyAddEvent(id, state, position, winSize);
        }

        public override void ReleaseAllTouches()
        {
            base.ReleaseAllTouches();
        }


        const int SM_MAXIMUMTOUCHES = 95;

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, ExactSpelling = true)]
        static extern int GetSystemMetrics(int nIndex);

    }
}