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