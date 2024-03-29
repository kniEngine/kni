// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Input
{
    public sealed partial class Mouse
    {
        private IntPtr _wndHandle = IntPtr.Zero;
        private MouseState _mouseState;
        private nkast.Wasm.Dom.Window _domWindow;

        private IntPtr PlatformGetWindowHandle()
        {
            return _wndHandle;
        }

        private void PlatformSetWindowHandle(IntPtr windowHandle)
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

            BlazorGameWindow gameWindow = BlazorGameWindow.FromHandle(windowHandle);
            _domWindow = gameWindow.wasmWindow;

            _domWindow.OnMouseMove += OnMouseMove;
            _domWindow.OnMouseDown += OnMouseDown;
            _domWindow.OnMouseUp += OnMouseUp;
            _domWindow.OnMouseWheel += OnMouseWheel;
        }

        private bool PlatformIsRawInputAvailable()
        {
            return false;
        }

        private MouseState PlatformGetState()
        {
            return _mouseState;
        }

        private void PlatformSetPosition(int x, int y)
        {
            throw new NotImplementedException();
        }

        private void PlatformSetCursor(MouseCursor cursor)
        {
            throw new NotImplementedException();
        }


        private void OnMouseMove(object sender, int x, int y)
        {
            _mouseState.X = x;
            _mouseState.Y = y;
        }

        private void OnMouseDown(object sender, int x, int y, int buttons)
        {
            _mouseState.X = x;
            _mouseState.Y = y;
            _mouseState.LeftButton = ((buttons & 1) != 0) ? ButtonState.Pressed : ButtonState.Released;
            _mouseState.RightButton = ((buttons & 2) != 0) ? ButtonState.Pressed : ButtonState.Released;
            _mouseState.MiddleButton = ((buttons & 4) != 0) ? ButtonState.Pressed : ButtonState.Released;
        }

        private void OnMouseUp(object sender, int x, int y, int buttons)
        {
            _mouseState.X = x;
            _mouseState.Y = y;
            _mouseState.LeftButton = ((buttons & 1) != 0) ? ButtonState.Pressed : ButtonState.Released;
            _mouseState.RightButton = ((buttons & 2) != 0) ? ButtonState.Pressed : ButtonState.Released;
            _mouseState.MiddleButton = ((buttons & 4) != 0) ? ButtonState.Pressed : ButtonState.Released;
        }

        public void OnMouseWheel(object sender, int deltaX, int deltaY, int deltaZ, int deltaMode)
        {
            _mouseState.HorizontalScrollWheelValue -= deltaX;
            _mouseState.ScrollWheelValue -= deltaY;
        }

    }
}
