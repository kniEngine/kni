﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Platform;
using nkast.Wasm.Canvas;
using nkast.Wasm.Dom;
using Microsoft.Xna.Platform.Graphics;
using Microsoft.Xna.Platform.Input;
using Microsoft.Xna.Platform.Input.Touch;

namespace Microsoft.Xna.Framework
{
    // TODO: BlazorGameWindow should be internal
    public class BlazorGameWindow : GameWindow, IDisposable
    {
        private static Dictionary<IntPtr, BlazorGameWindow> _instances = new Dictionary<IntPtr, BlazorGameWindow>();

        internal static BlazorGameWindow FromHandle(IntPtr handle)
        {
            return _instances[handle];
        }

        private Window _window;
        private ConcreteGame _concreteGame;

        private bool _isResizable;
        private bool _isBorderless;
        private bool _isMouseHidden;
        private bool _isMouseInBounds;

        private readonly List<Keys> _keys = new List<Keys>();

        private Point _locationBeforeFullScreen;

        // flag to indicate that we're switching to/from full screen and should ignore resize events
        private bool _switchingFullScreen;




        #region Internal Properties


        #endregion

        #region Public Properties

        public override IntPtr Handle { get { return new IntPtr(_window.Uid); } }

        public override string ScreenDeviceName { get { return String.Empty; } }

        public override Rectangle ClientBounds
        {
            get
            {
                return new Rectangle(0, 0, _canvas.Width, _canvas.Height);
            }
        }

        public override bool AllowUserResizing
        {
            get { return _isResizable; }
            set
            {
                if (_isResizable == value)
                    return;

                _isResizable = value;

                if (!_isBorderless)
                {
                }
            }
        }

        public override bool IsBorderless
        {
            get { return _isBorderless; }
            set
            {
                if (_isBorderless == value)
                    return;

                _isBorderless = value;

                if (!_isBorderless)
                {
                }
            }
        }

        public override DisplayOrientation CurrentOrientation
        {
            get { return DisplayOrientation.Default; }
        }


        public bool IsFullScreen { get; private set; }
        public bool HardwareModeSwitch { get; private set; }

        #endregion

        internal Canvas _canvas { get; private set; }
        internal Window wasmWindow { get { return _window; } }

        internal BlazorGameWindow(ConcreteGame concreteGame)
        {
            _concreteGame = concreteGame;

            _window = Window.Current;
            _canvas = _window.Document.GetElementById<Canvas>("theCanvas");
            _instances.Add(this.Handle, this);

            ChangeClientSize(GraphicsDeviceManager.DefaultBackBufferWidth, GraphicsDeviceManager.DefaultBackBufferHeight);

            SetIcon();

            // Capture mouse events.
            if (Mouse.WindowHandle == IntPtr.Zero)
                Mouse.WindowHandle = this.Handle;
            //Form.MouseEnter += OnMouseEnter;
            //Form.MouseLeave += OnMouseLeave;

            // Capture touch events.
            if (TouchPanel.WindowHandle == IntPtr.Zero)
                TouchPanel.WindowHandle = this.Handle;
            _window.OnTouchStart += (object sender, float x, float y, int identifier) =>
            {
                ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<ConcreteTouchPanel>().AddPressedEvent(identifier, new Vector2(x, y));
            };
            _window.OnTouchMove += (object sender, float x, float y, int identifier) =>
            {
                ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<ConcreteTouchPanel>().AddMovedEvent(identifier, new Vector2(x, y));
            };
            _window.OnTouchEnd += (object sender, float x, float y, int identifier) =>
            {
                ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<ConcreteTouchPanel>().AddReleasedEvent(identifier, new Vector2(x, y));
            };
            _window.OnTouchCancel += (object sender) =>
            {
                // TODO: Implement TouchEvent.ChangedTouches and call TouchPanelStrategy.AddCanceledEvent(...)
                //       instead of InvalidateTouches().
                //       see: https://developer.mozilla.org/en-US/docs/Web/API/TouchEvent/changedTouches
                ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().BlazorCancelAllTouches();
            };

            // keyboard events
            ((IPlatformKeyboard)Keyboard.Current).GetStrategy<ConcreteKeyboard>().SetKeys(_keys);
            _window.OnKeyDown += (object sender, char key, int keyCode, int location) =>
            {
                Keys xnakey = (Keys)keyCode;

                // map special keys
                switch (keyCode)
                {
                    case 16:
                        xnakey = (location == 2) ? Keys.RightShift: Keys.LeftShift;
                        break;
                    case 17:
                        xnakey = (location == 2) ? Keys.RightControl : Keys.LeftControl;
                        break;
                    case 18:
                        xnakey = (location == 2) ? Keys.RightAlt : Keys.LeftAlt;
                        break;
                }

                if (!_keys.Contains(xnakey))
                    _keys.Add(xnakey);

                if (IsTextInputAttached())
                {
                    bool controlKeyBlocksTextInput = false;
                    for (int i = 0; i < _keys.Count; i++)
                    {
                        Keys keyToCheck = _keys[i];
                        if (keyToCheck == Keys.LeftControl || keyToCheck == Keys.RightControl || keyToCheck == Keys.LeftAlt)
                        {
                            controlKeyBlocksTextInput = true;
                            break;
                        }
                    }

                    if (key != '\0' && !controlKeyBlocksTextInput)
                        OnTextInput(key, xnakey);
                }
            };
            _window.OnKeyUp += (object sender, char key, int keyCode, int location) =>
            {
                Keys xnakey = (Keys)keyCode;
                
                // map special keys
                switch (keyCode)
                {
                    case 16:
                        xnakey = (location == 2) ? Keys.RightShift: Keys.LeftShift;
                        break;
                    case 17:
                        xnakey = (location == 2) ? Keys.RightControl : Keys.LeftControl;
                        break;
                    case 18:
                        xnakey = (location == 2) ? Keys.RightAlt : Keys.LeftAlt;
                        break;
                }

                if (_keys.Contains(xnakey))
                    _keys.Remove(xnakey);
            };

            _window.OnFocus += OnActivated;
            _window.OnBlur += OnDeactivated;
            // Form.Resize += OnResize;
            //  Form.ResizeBegin += OnResizeBegin;
            //  Form.ResizeEnd += OnResizeEnd;
            _window.OnResize += OnResize;

           // Form.KeyPress += OnKeyPress;
        }

        private void SetIcon()
        {
          
        }

        ~BlazorGameWindow()
        {
            Dispose(false);
        }

        private void OnActivated(object sender)
        {
            base.OnActivated();
            //Keyboard.SetActive(true);
        }

        private void OnDeactivated(object sender)
        {
            _keys.Clear();

            base.OnDeactivated();
        }

        private void OnMouseEnter(object sender, EventArgs e)
        {
            _isMouseInBounds = true;
            if (!_concreteGame.IsMouseVisible && !_isMouseHidden)
            {
                _isMouseHidden = true;
                //Cursor.Hide();
            }
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            _isMouseInBounds = false;
            if (_isMouseHidden)
            {
                _isMouseHidden = false;
                //Cursor.Show();
            }
        }

        internal void OnResize(object sender)
        {

            UpdateBackBufferSize();

            OnClientSizeChanged();
        }

        // TODO: move UpdateBackBufferSize() in graphicsDeviceManager
        private void UpdateBackBufferSize()
        {
            GraphicsDeviceManager gdm = _concreteGame.GraphicsDeviceManager;
            if (gdm != null)
            {
                if (gdm.GraphicsDevice == null)
                    return;

                _canvas.Width = _window.InnerWidth;
                _canvas.Height = _window.InnerHeight;

                int newWidth  = _canvas.Width;
                int newHeight = _canvas.Height;
                if (newWidth  != gdm.PreferredBackBufferWidth
                ||  newHeight != gdm.PreferredBackBufferHeight)
                {
                    // Set the default new back buffer size
                    gdm.PreferredBackBufferWidth = newWidth;
                    gdm.PreferredBackBufferHeight = newHeight;
                    gdm.ApplyChanges();
                }
            }
        }

        protected override void SetTitle(string title)
        {
            _window.Document.Title = title;
        }

        internal void ChangeClientSize(int width, int height)
        {

        }

        #region Public Methods

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
               
            }

            if (Mouse.WindowHandle == this.Handle)
                Mouse.WindowHandle = IntPtr.Zero;
            if (TouchPanel.WindowHandle == this.Handle)
                TouchPanel.WindowHandle = IntPtr.Zero;

            _instances.Remove(this.Handle);
            _canvas = null;

            _concreteGame = null;
        }

        public void MouseVisibleToggled()
        {
            if (_concreteGame.IsMouseVisible)
            {
                if (_isMouseHidden)
                {
                    //Cursor.Show();
                    _isMouseHidden = false;
                }
            }
            else if (!_isMouseHidden && _isMouseInBounds)
            {
                //Cursor.Hide();
                _isMouseHidden = true;
            }
        }

        internal void OnPresentationChanged(PresentationParameters pp)
        {
            var raiseClientSizeChanged = false;
            if (pp.IsFullScreen && pp.HardwareModeSwitch && IsFullScreen && HardwareModeSwitch)
            {
                if(_concreteGame.IsActive)
                {
                    // stay in hardware full screen, need to call ResizeTargets so the displaymode can be switched
                   // _concreteGame.GraphicsDevice.ResizeTargets();
                }
                else
                {
                    // This needs to be called in case the user presses the Windows key while the focus is on the second monitor,
                    //	which (sometimes) causes the window to exit fullscreen mode, but still keeps it visible
                    MinimizeFullScreen();
                }
            }
            else if (pp.IsFullScreen && (!IsFullScreen || pp.HardwareModeSwitch != HardwareModeSwitch))
            {
                EnterFullScreen(pp);
                raiseClientSizeChanged = true;
            }
            else if (!pp.IsFullScreen && IsFullScreen)
            {
                ExitFullScreen();
                raiseClientSizeChanged = true;
            }

            ChangeClientSize(pp.BackBufferWidth, pp.BackBufferHeight);

            if (raiseClientSizeChanged)
                OnClientSizeChanged();
        }

        #endregion

        internal void EnterFullScreen(PresentationParameters pp)
        {

        }


        private void ExitFullScreen()
        {

        }

        private void MinimizeFullScreen()
        {

        }

        internal void InitFocus()
        {
            if (this.wasmWindow.Document.HasFocus())
                OnActivated();
        }
    }
}

