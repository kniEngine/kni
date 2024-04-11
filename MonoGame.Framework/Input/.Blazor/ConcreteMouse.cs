// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Platform.Input
{
    public sealed class ConcreteMouse : MouseStrategy
    {
        private IntPtr _wndHandle = IntPtr.Zero;
        private nkast.Wasm.Dom.Window _domWindow;

        private Point _pos;
        private int _scrollX, _scrollY;
        private int _rawX, _rawY;
        private ButtonState _leftButton, _rightButton, _middleButton;


        public override IntPtr PlatformGetWindowHandle()
        {
            return _wndHandle;
        }

        public override void PlatformSetWindowHandle(IntPtr windowHandle)
        {
            _wndHandle = windowHandle;

            // Unregister old window
            if (_domWindow != null)
            {
                _pos = default(Point);
                _scrollX = 0;
                _scrollY = 0;
                _leftButton = default(ButtonState);
                _rightButton = default(ButtonState);
                _middleButton = default(ButtonState);

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

        public override bool PlatformIsRawInputAvailable()
        {
            return false;
        }

        public override MouseState PlatformGetState()
        {
            MouseState mouseState = new MouseState(
                    x: _pos.X, y: _pos.Y,
                    scrollWheel: _scrollY, horizontalScrollWheel: _scrollX,
                    rawX: _rawX, rawY: _rawY,
                    leftButton: _leftButton,
                    middleButton: _middleButton,
                    rightButton: _rightButton,
                    xButton1: ButtonState.Released,
                    xButton2: ButtonState.Released
                    );

            return mouseState;
        }

        public override void PlatformSetPosition(int x, int y)
        {
            throw new NotImplementedException();
        }

        public override void PlatformSetCursor(MouseCursor cursor)
        {
            throw new NotImplementedException();
        }


        private void OnMouseMove(object sender, int x, int y)
        {
            _pos.X = x;
            _pos.Y = y;
        }

        private void OnMouseDown(object sender, int x, int y, int buttons)
        {
            _pos.X = x;
            _pos.Y = y;
            ButtonState _leftButton   = ((buttons & 1) != 0) ? ButtonState.Pressed : ButtonState.Released;
            ButtonState _rightButton  = ((buttons & 2) != 0) ? ButtonState.Pressed : ButtonState.Released;
            ButtonState _middleButton = ((buttons & 4) != 0) ? ButtonState.Pressed : ButtonState.Released;
        }

        private void OnMouseUp(object sender, int x, int y, int buttons)
        {
            _pos.X = x;
            _pos.Y = y;
            ButtonState _leftButton   = ((buttons & 1) != 0) ? ButtonState.Pressed : ButtonState.Released;
            ButtonState _rightButton  = ((buttons & 2) != 0) ? ButtonState.Pressed : ButtonState.Released;
            ButtonState _middleButton = ((buttons & 4) != 0) ? ButtonState.Pressed : ButtonState.Released;
        }

        public void OnMouseWheel(object sender, int deltaX, int deltaY, int deltaZ, int deltaMode)
        {
            _scrollX -= deltaX;
            _scrollY -= deltaY;
        }

    }
}
