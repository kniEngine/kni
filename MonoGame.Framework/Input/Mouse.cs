// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021-2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Platform.Input;

namespace Microsoft.Xna.Platform.Input
{
    public interface IMouse
    {
        IntPtr WindowHandle { get; set; }
        bool IsRawInputAvailable { get; }

        MouseState GetState();
        void SetPosition(int x, int y);
        void SetCursor(MouseCursor cursor);
    }

    public interface IPlatformMouse
    {
        T GetStrategy<T>() where T : MouseStrategy;
    }
}

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// Allows reading position and button click information from mouse.
    /// </summary>
    public sealed class Mouse : IMouse
        , IPlatformMouse
    {
        private static Mouse _current;

        private static readonly MouseState _defaultState = new MouseState();

        private MouseCursor _mouseCursor;

        /// <summary>
        /// Returns the current Mouse instance.
        /// </summary> 
        public static Mouse Current
        {
            get
            {
                if (_current != null)
                    return _current;

                lock (typeof(Mouse))
                {
                    if (_current == null)
                        _current = new Mouse();

                    return _current;
                }
            }
        }

        /// <summary>
        /// Gets or sets the window handle for current mouse processing.
        /// </summary> 
        public static IntPtr WindowHandle
        {
            get { return ((IMouse)Mouse.Current).WindowHandle; }
            set { ((IMouse)Mouse.Current).WindowHandle = value; }
        }

        /// <summary>
        /// Gets if RawInput is available.
        /// </summary>
        public static bool IsRawInputAvailable
        {
            get { return ((IMouse)Mouse.Current).IsRawInputAvailable; }
        }

        /// <summary>
        /// Gets mouse state information that includes position and button presses
        /// for the primary window
        /// </summary>
        /// <returns>Current state of the mouse.</returns>
        public static MouseState GetState()
        {
            return ((IMouse)Mouse.Current).GetState();
        }

        /// <summary>
        /// Sets mouse cursor's relative position to game-window.
        /// </summary>
        /// <param name="x">Relative horizontal position of the cursor.</param>
        /// <param name="y">Relative vertical position of the cursor.</param>
        public static void SetPosition(int x, int y)
        {
            ((IMouse)Mouse.Current).SetPosition(x, y);
        }

        /// <summary>
        /// Sets the cursor image to the specified MouseCursor.
        /// </summary>
        /// <param name="cursor">Mouse cursor to use for the cursor image.</param>
        public static void SetCursor(MouseCursor cursor)
        {
            ((IMouse)Mouse.Current).SetCursor(cursor);
        }

        private MouseStrategy _strategy;

        T IPlatformMouse.GetStrategy<T>()
        {
            return (T)_strategy;
        }

        private Mouse()
        {
            _strategy = new ConcreteMouse();
        }


        #region IMouse

        IntPtr IMouse.WindowHandle
        {
            get { return _strategy.PlatformGetWindowHandle(); }
            set { _strategy.PlatformSetWindowHandle(value); }
        }

        bool IMouse.IsRawInputAvailable
        {
            get { return _strategy.PlatformIsRawInputAvailable(); }
        }

        MouseState IMouse.GetState()
        {
#if DESKTOPGL || IOS || ANDROID || (UAP || WINUI)
            if (((ConcreteMouse)_strategy).PrimaryWindow != null)
                return ((ConcreteMouse)_strategy).PlatformGetState(((ConcreteMouse)_strategy).PrimaryWindow);
            else
                return _defaultState;
#endif

            return _strategy.PlatformGetState();
        }

        void IMouse.SetPosition(int x, int y)
        {
            _strategy.PlatformSetPosition(x, y);
        }

        void IMouse.SetCursor(MouseCursor cursor)
        {
            _strategy.PlatformSetCursor(cursor);
            _mouseCursor = cursor;
        }

        #endregion IMouse

    }
}
