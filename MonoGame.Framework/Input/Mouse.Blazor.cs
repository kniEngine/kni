// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Input
{
    public static partial class Mouse
    {
        static IntPtr _wndHandle = IntPtr.Zero;
        static nkast.Wasm.Dom.Window _domWindow;
        static MouseState _mouseState;

        private static IntPtr PlatformGetWindowHandle()
        {
            return _wndHandle;
        }

        private static void PlatformSetWindowHandle(IntPtr windowHandle)
        {
            // Unregister old window
            if (_domWindow != null)
            {
                _mouseState = default(MouseState);
                _domWindow.OnMouseMove -= OnMouseMove;
                _domWindow.OnMouseDown -= OnMouseDown;
                _domWindow.OnMouseUp -= OnMouseUp;
                _domWindow.OnMouseWheel -= OnMouseWheel;
            }

            var gameWindow = BlazorGameWindow.FromHandle(windowHandle);
            _domWindow = gameWindow.wasmWindow;

            _domWindow.OnMouseMove += OnMouseMove;
            _domWindow.OnMouseDown += OnMouseDown;
            _domWindow.OnMouseUp += OnMouseUp;
            _domWindow.OnMouseWheel += OnMouseWheel;
        }

        private static bool PlatformIsRawInputAvailable()
        {
            return false;
        }

        private static MouseState PlatformGetState()
        {
            return _mouseState;
        }

        private static MouseState PlatformGetState(GameWindow window)
        {
            return _mouseState;
        }

        private static void PlatformSetPosition(int x, int y)
        {
            throw new NotImplementedException();
        }

        private static void PlatformSetCursor(MouseCursor cursor)
        {
            throw new NotImplementedException();
        }


        private static void OnMouseMove(object sender, int x, int y)
        {
            _mouseState.X = x;
            _mouseState.Y = y;
        }

        private static void OnMouseDown(object sender, int x, int y, int buttons)
        {
            _mouseState.X = x;
            _mouseState.Y = y;
            _mouseState.LeftButton = ((buttons & 1) != 0) ? ButtonState.Pressed : ButtonState.Released;
            _mouseState.RightButton = ((buttons & 2) != 0) ? ButtonState.Pressed : ButtonState.Released;
            _mouseState.MiddleButton = ((buttons & 4) != 0) ? ButtonState.Pressed : ButtonState.Released;
        }

        private static void OnMouseUp(object sender, int x, int y, int buttons)
        {
            _mouseState.X = x;
            _mouseState.Y = y;
            _mouseState.LeftButton = ((buttons & 1) != 0) ? ButtonState.Pressed : ButtonState.Released;
            _mouseState.RightButton = ((buttons & 2) != 0) ? ButtonState.Pressed : ButtonState.Released;
            _mouseState.MiddleButton = ((buttons & 4) != 0) ? ButtonState.Pressed : ButtonState.Released;
        }

        public static void OnMouseWheel(object sender, int deltaX, int deltaY, int deltaZ, int deltaMode)
        {
            _mouseState.HorizontalScrollWheelValue -= deltaX;
            _mouseState.ScrollWheelValue -= deltaY;
        }

    }
}
