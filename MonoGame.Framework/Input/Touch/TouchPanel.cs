// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Input.Touch
{
    /// <summary>
    /// Allows retrieval of information from Touch Panel device.
    /// </summary>
    public static class TouchPanel
    {
        internal static GameWindow PrimaryWindow;

        /// <summary>
        /// Gets the current state of the touch panel.
        /// </summary>
        /// <returns><see cref="TouchCollection"/></returns>
        public static TouchCollection GetState()
        {
            return PrimaryWindow.TouchPanelState.GetState();
        }

        public static TouchPanelState GetState(GameWindow window)
        {
            return window.TouchPanelState;
        }

        public static TouchPanelCapabilities GetCapabilities()
        {
            return PrimaryWindow.TouchPanelState.GetCapabilities();
        }

        internal static void AddEvent(int id, TouchLocationState state, Vector2 position)
        {         
            PrimaryWindow.TouchPanelState.AddEvent(id, state, position);
        }

        /// <summary>
        /// Returns the next available gesture on touch panel device.
        /// </summary>
        /// <returns><see cref="GestureSample"/></returns>
        public static GestureSample ReadGesture()
        {
            // Return the next gesture.
            return PrimaryWindow.TouchPanelState.GestureList.Dequeue();			
        }

        /// <summary>
        /// The window handle of the touch panel. Purely for Xna compatibility.
        /// </summary>
        public static IntPtr WindowHandle
        {
            get { return PrimaryWindow.TouchPanelState.WindowHandle; }
            set { PrimaryWindow.TouchPanelState.WindowHandle = value; }
        }

        /// <summary>
        /// Gets or sets the display height of the touch panel.
        /// </summary>
        public static int DisplayHeight
        {
            get { return PrimaryWindow.TouchPanelState.DisplayHeight; }
            set { PrimaryWindow.TouchPanelState.DisplayHeight = value; }
        }

        /// <summary>
        /// Gets or sets the display orientation of the touch panel.
        /// </summary>
        public static DisplayOrientation DisplayOrientation
        {
            get { return PrimaryWindow.TouchPanelState.DisplayOrientation; }
            set { PrimaryWindow.TouchPanelState.DisplayOrientation = value; }
        }

        /// <summary>
        /// Gets or sets the display width of the touch panel.
        /// </summary>
        public static int DisplayWidth
        {
            get { return PrimaryWindow.TouchPanelState.DisplayWidth; }
            set { PrimaryWindow.TouchPanelState.DisplayWidth = value; }
        }
        
        /// <summary>
        /// Gets or sets enabled gestures.
        /// </summary>
        public static GestureType EnabledGestures
        {
            get { return PrimaryWindow.TouchPanelState.EnabledGestures; }
            set { PrimaryWindow.TouchPanelState.EnabledGestures = value; }
        }
        
        /// <summary>
        /// Returns true if a touch gesture is available.
        /// </summary>
        public static bool IsGestureAvailable
        {
            get { return PrimaryWindow.TouchPanelState.IsGestureAvailable; }
        }
    }
}
