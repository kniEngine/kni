﻿// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using nkast.Wasm.Dom;

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
            int maximumTouchCount = Window.Current.Navigator.MaxTouchPoints;
            bool isConnected = maximumTouchCount > 0;
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

        public override void AddPressedEvent(int nativeTouchId, Vector2 position)
        {
            IntPtr wndHandle = this.WindowHandle;
            if (wndHandle != IntPtr.Zero)
            {
                GameWindow gameWindow = BlazorGameWindow.FromHandle(wndHandle);
                Rectangle windowsBounds = gameWindow.ClientBounds;
                Point winSize = new Point(windowsBounds.Width, windowsBounds.Height);

                base.AddPressedEvent(nativeTouchId, position, winSize);
            }
        }
        
        public override void AddMovedEvent(int nativeTouchId, Vector2 position)
        {
            IntPtr wndHandle = this.WindowHandle;
            if (wndHandle != IntPtr.Zero)
            {
                GameWindow gameWindow = BlazorGameWindow.FromHandle(wndHandle);
                Rectangle windowsBounds = gameWindow.ClientBounds;
                Point winSize = new Point(windowsBounds.Width, windowsBounds.Height);

                base.AddMovedEvent(nativeTouchId, position, winSize);
            }
        }

        public override void AddReleasedEvent(int nativeTouchId, Vector2 position)
        {
            IntPtr wndHandle = this.WindowHandle;
            if (wndHandle != IntPtr.Zero)
            {
                GameWindow gameWindow = BlazorGameWindow.FromHandle(wndHandle);
                Rectangle windowsBounds = gameWindow.ClientBounds;
                Point winSize = new Point(windowsBounds.Width, windowsBounds.Height);

                base.AddReleasedEvent(nativeTouchId, position, winSize);
            }
        }
        public override void AddCanceledEvent(int nativeTouchId, Vector2 position)
        {
            IntPtr wndHandle = this.WindowHandle;
            if (wndHandle != IntPtr.Zero)
            {
                GameWindow gameWindow = BlazorGameWindow.FromHandle(wndHandle);
                Rectangle windowsBounds = gameWindow.ClientBounds;
                Point winSize = new Point(windowsBounds.Width, windowsBounds.Height);

                base.AddReleasedEvent(nativeTouchId, position, winSize);
            }
        }

    }
}