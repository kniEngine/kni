// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// Allows reading position and button click information from mouse.
    /// </summary>
    public static partial class Mouse
    {
        private static readonly MouseState _defaultState = new MouseState();
        private static MouseCursor _mouseCursor;

        internal static GameWindow PrimaryWindow;


        /// <summary>
        /// Gets or sets the window handle for current mouse processing.
        /// </summary> 
        public static IntPtr WindowHandle
        {
            get { return PlatformGetWindowHandle(); }
            set { PlatformSetWindowHandle(value); }
        }

        /// <summary>
        /// Gets if RawInput is available.
        /// </summary>
        public static bool IsRawInputAvailable
        {
            get { return PlatformIsRawInputAvailable(); }
        }

        /// <summary>
        /// Gets mouse state information that includes position and button presses
        /// for the primary window
        /// </summary>
        /// <returns>Current state of the mouse.</returns>
        public static MouseState GetState()
        {
#if WINDOWSDX
            return PlatformGetState();
#endif

            if (PrimaryWindow != null)
                return PlatformGetState(PrimaryWindow);

            return _defaultState;
        }

        /// <summary>
        /// Sets mouse cursor's relative position to game-window.
        /// </summary>
        /// <param name="x">Relative horizontal position of the cursor.</param>
        /// <param name="y">Relative vertical position of the cursor.</param>
        public static void SetPosition(int x, int y)
        {
            PlatformSetPosition(x, y);
        }

        /// <summary>
        /// Sets the cursor image to the specified MouseCursor.
        /// </summary>
        /// <param name="cursor">Mouse cursor to use for the cursor image.</param>
        public static void SetCursor(MouseCursor cursor)
        {
            PlatformSetCursor(cursor);
            _mouseCursor = cursor;
        }
    }
}
