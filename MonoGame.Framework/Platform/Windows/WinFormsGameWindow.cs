// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Windows;
using Microsoft.Xna.Platform;
using SysDrawing = System.Drawing;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using Microsoft.Xna.Platform.Graphics;


namespace MonoGame.Framework
{
    class WinFormsGameWindow : GameWindow, IDisposable
    {
        internal WinFormsGameForm Form;

        private ConcreteGame _concreteGame;
        private Game _game;

        private bool _isResizable;
        private bool _isBorderless;
        private bool _isMouseHidden;
        private bool _isMouseInBounds;

        private SysDrawing.Point _locationBeforeFullScreen;
        // flag to indicate that we're switching to/from full screen and should ignore resize events
        private bool _switchingFullScreen;

        // true if window position was moved either through code or by dragging/resizing the form
        private bool _wasMoved;

        #region Internal Properties


        #endregion

        #region Public Properties

        public override IntPtr Handle { get { return Form.Handle; } }

        public override string ScreenDeviceName { get { return String.Empty; } }

        public override Rectangle ClientBounds
        {
            get
            {
                var position = Form.PointToScreen(SysDrawing.Point.Empty);
                var size = Form.ClientSize;
                return new Rectangle(position.X, position.Y, size.Width, size.Height);
            }
        }

        public override bool AllowUserResizing
        {
            get { return _isResizable; }
            set
            {
                if (_isResizable != value)
                {
                    _isResizable = value;
                    Form.MaximizeBox = _isResizable;

                    if (_isBorderless)
                        return;
                    Form.FormBorderStyle = _isResizable ? FormBorderStyle.Sizable : FormBorderStyle.FixedSingle;
                }
            }
        }

        public override bool IsBorderless
        {
            get { return _isBorderless; }
            set
            {
                if (_isBorderless != value)
                {
                    _isBorderless = value;

                    if (_isBorderless)
                        Form.FormBorderStyle = FormBorderStyle.None;
                    else
                        Form.FormBorderStyle = _isResizable ? FormBorderStyle.Sizable : FormBorderStyle.FixedSingle;
                }
            }
        }

        public override bool AllowAltF4
        {
             get { return base.AllowAltF4; }
             set
             {
                 Form.AllowAltF4 = value;
                 base.AllowAltF4 = value;
             }
        }

        public override DisplayOrientation CurrentOrientation
        {
            get { return DisplayOrientation.Default; }
        }

        protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
        {
        }


        public bool IsFullScreen { get; private set; }
        public bool HardwareModeSwitch { get; private set; }

        #endregion

        internal WinFormsGameWindow(ConcreteGame concreteGame)
        {
            _concreteGame = concreteGame;
            _game = concreteGame.Game;

            Form = new WinFormsGameForm(this);
            ChangeClientSize(GraphicsDeviceManager.DefaultBackBufferWidth, GraphicsDeviceManager.DefaultBackBufferHeight);

            SetIcon();
            Title = MonoGame.Framework.Utilities.AssemblyHelper.GetDefaultWindowTitle();

            Form.MaximizeBox = false;
            Form.FormBorderStyle = FormBorderStyle.FixedSingle;
            Form.StartPosition = FormStartPosition.Manual;

            // Capture mouse events.
            Mouse.WindowHandle = Form.Handle;
            Form.MouseEnter += OnMouseEnter;
            Form.MouseLeave += OnMouseLeave;            

            Form.Activated += OnActivated;
            Form.Deactivate += OnDeactivate;
            Form.ResizeBegin += OnResizeBegin;
            Form.Resize += OnResize;
            Form.ResizeEnd += OnResizeEnd;

            Form.KeyPress += OnKeyPress;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct POINTSTRUCT
        {
            public int X;
            public int Y;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
        private static extern IntPtr ExtractIcon(IntPtr hInst, string exeFileName, int iconIndex);
        
        [DllImport("user32.dll", ExactSpelling=true, CharSet=CharSet.Auto)]
        [return: MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(out POINTSTRUCT pt);
        
        [DllImport("user32.dll", ExactSpelling=true, CharSet=CharSet.Auto)]
        internal static extern int MapWindowPoints(HandleRef hWndFrom, HandleRef hWndTo, out POINTSTRUCT pt, int cPoints);

        [DllImport("shell32.dll")]
        private static extern void DragAcceptFiles(IntPtr hwnd, bool fAccept);

        private void SetIcon()
        {
            // When running unit tests this can return null.
            var assembly = Assembly.GetEntryAssembly();
            if (assembly == null)
                return;
            var handle = ExtractIcon(IntPtr.Zero, assembly.Location, 0);
            if (handle != IntPtr.Zero)
                Form.Icon = SysDrawing.Icon.FromHandle(handle);
        }

        ~WinFormsGameWindow()
        {
            Dispose(false);
        }

        private void OnActivated(object sender, EventArgs eventArgs)
        {
            _concreteGame.IsActive = true;
            Keyboard.SetActive(true);

            DragAcceptFiles(Handle, true); //allows drag and dropping
        }

        private void OnDeactivate(object sender, EventArgs eventArgs)
        {
            // If in exclusive mode full-screen, force it out of exclusive mode and minimize the window
			if( IsFullScreen && _concreteGame.GraphicsDevice.PresentationParameters.HardwareModeSwitch)
            {
				// This is true when the user presses the Windows key while game window has focus
				if( Form.WindowState == FormWindowState.Minimized)
					MinimizeFullScreen();				
			}
            _concreteGame.IsActive = false;
            Keyboard.SetActive(false);
        }

        private void OnMouseEnter(object sender, EventArgs e)
        {
            _isMouseInBounds = true;
            if (!_concreteGame.IsMouseVisible && !_isMouseHidden)
            {
                _isMouseHidden = true;
                Cursor.Hide();
            }
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            _isMouseInBounds = false;
            if (_isMouseHidden)
            {
                _isMouseHidden = false;
                Cursor.Show();
            }
        }

        [DllImport("user32.dll")]
        private static extern short VkKeyScanEx(char ch, IntPtr dwhkl);

        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            var key = (Keys) (VkKeyScanEx(e.KeyChar, InputLanguage.CurrentInputLanguage.Handle) & 0xff);
            Platform_OnTextInput(e.KeyChar, key);
        }

        internal void Initialize(int width, int height)
        {
            ChangeClientSize(width, height);
        }

        internal void Initialize(PresentationParameters pp)
        {
            ChangeClientSize(pp.BackBufferWidth, pp.BackBufferHeight);

            if (pp.IsFullScreen)
            {
                EnterFullScreen(pp);
                if (!pp.HardwareModeSwitch)
                    _concreteGame.GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().OnPresentationChanged();
            }
        }

        private FormWindowState _lastFormState;

        private void OnResizeBegin(object sender, EventArgs e)
        {
        }

        private void OnResize(object sender, EventArgs eventArgs)
        {
            if (_switchingFullScreen || Form.IsResizing)
            {   
                // TNC: repaint the window when resizing
                // gameloop is paused during windows resize
                try 
                {
                    _concreteGame.GraphicsDevice.Present();
                }
                catch (Exception ex)
                {
#if DEBUG 
                    throw ex;
#endif
                }
                return;
            }

            // this event can be triggered when moving the window through Windows hotkeys
            // in that case we should no longer center the window after resize
            if (_lastFormState == Form.WindowState)
                _wasMoved = true;

            if (_concreteGame.Window == this && Form.WindowState != FormWindowState.Minimized)
            {
                // we may need to restore full screen when coming back from a minimized window
                if (_lastFormState == FormWindowState.Minimized)
                    _concreteGame.GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().SetHardwareFullscreen();
                UpdateBackBufferSize();
            }

            _lastFormState = Form.WindowState;
            OnClientSizeChanged();
        }

        private void OnResizeEnd(object sender, EventArgs eventArgs)
        {
            _wasMoved = true;
            if (_concreteGame.Window == this)
            {
                UpdateBackBufferSize();

                // the display that the window is on might have changed, so we need to
                // check and possibly update the Adapter of the GraphicsDevice
                if (_concreteGame.GraphicsDevice != null)
                    _concreteGame.GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().RefreshAdapter();
            }

            OnClientSizeChanged();
        }

        private void UpdateBackBufferSize()
        {
            var gdm = _concreteGame.GraphicsDeviceManager;
            if (gdm.GraphicsDevice == null)
                return;

            var newSize = Form.ClientSize;
            if (newSize.Width == gdm.PreferredBackBufferWidth
                && newSize.Height == gdm.PreferredBackBufferHeight)
                return;

            // Set the default new back buffer size
            gdm.PreferredBackBufferWidth = newSize.Width;
            gdm.PreferredBackBufferHeight = newSize.Height;
            gdm.ApplyChanges();
        }

        protected override void SetTitle(string title)
        {
            Form.Text = title;
        }

        internal void RunLoop()
        {
            Application.Idle += Application_Idle;
            Application.Run(Form);
            Application.Idle -= Application_Idle;

            // We need to remove the WM_QUIT message in the message 
            // pump as it will keep us from restarting on this 
            // same thread.
            //
            // This is critical for some NUnit runners which
            // typically will run all the tests on the same
            // process/thread.
            var msg = new NativeMessage();
            do
            {
                if (msg.msg == WM_QUIT)
                    break;

                Thread.Sleep(100);
            } 
            while (PeekMessage(out msg, IntPtr.Zero, 0, 1 << 5, 1));
        }

        // Run game loop when the app becomes Idle.
        private void Application_Idle(object sender, EventArgs e)
        {
            NativeMessage nativeMsg = default(NativeMessage);
            while (true)
            {
                _game.Tick();
                
                if (PeekMessage(out nativeMsg, IntPtr.Zero, 0, 0, 0))
                    break;

                if (Form == null || Form.IsDisposed)
                    break;
            }
        }

        private const uint WM_QUIT = 0x12;

        [StructLayout(LayoutKind.Sequential)]
        public struct NativeMessage
        {
            public IntPtr handle;
            public uint msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public System.Drawing.Point p;
        }

        internal void ChangeClientSize(int width, int height)
        {
            var clientBounds = new SysDrawing.Size(width, height);

            var prevIsResizing = Form.IsResizing;
            // make sure we don't see the events from this as a user resize
            Form.IsResizing = true;

            if (this.Form.ClientSize != clientBounds)
                this.Form.ClientSize = clientBounds;

            // if the window wasn't moved manually and it's resized, it should be centered
            if (!_wasMoved)
                Form.CenterOnPrimaryMonitor();

            Form.IsResizing = prevIsResizing;
        }

        [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern bool PeekMessage(out NativeMessage msg, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax, uint flags);

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
                if (Form != null)
                {
                    Form.Dispose();
                    Form = null;
                }
            }
            _concreteGame = null;
            _game = null;
            Mouse.WindowHandle = IntPtr.Zero;
        }

        public void MouseVisibleToggled()
        {
            if (_concreteGame.IsMouseVisible)
            {
                if (_isMouseHidden)
                {
                    Cursor.Show();
                    _isMouseHidden = false;
                }
            }
            else if (!_isMouseHidden && _isMouseInBounds)
            {
                Cursor.Hide();
                _isMouseHidden = true;
            }
        }

        internal void OnPresentationChanged(PresentationParameters pp)
        {
            var raiseClientSizeChanged = false;
            if (pp.IsFullScreen && pp.HardwareModeSwitch && IsFullScreen && HardwareModeSwitch)
            {
                if( _concreteGame.IsActive )
                {
                    // stay in hardware full screen, need to call ResizeTargets so the displaymode can be switched
                    _concreteGame.GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().ResizeTargets();
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

        private void EnterFullScreen(PresentationParameters pp)
        {
            _switchingFullScreen = true;

            // store the location of the window so we can restore it later
            if (!IsFullScreen)
                _locationBeforeFullScreen = Form.Location;

            _concreteGame.GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().SetHardwareFullscreen();

            if (!pp.HardwareModeSwitch)
            {
                // FIXME: setting the WindowState to Maximized when the form is not shown will not update the ClientBounds
                // this causes the back buffer to be the wrong size when initializing in soft full screen
                // we show the form to bypass the issue
                Form.Show();
                IsBorderless = true;
                Form.WindowState = FormWindowState.Maximized;
                _lastFormState = FormWindowState.Maximized;
            }

            IsFullScreen = true;
            HardwareModeSwitch = pp.HardwareModeSwitch;

            _switchingFullScreen = false;
        }


        [DllImport("user32.dll")]
        static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, uint flags);

        private void ExitFullScreen()
        {
            _switchingFullScreen = true;

            _concreteGame.GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().ClearHardwareFullscreen();

            IsBorderless = false;
            Form.WindowState = FormWindowState.Normal;
            _lastFormState = FormWindowState.Normal;
            Form.Location = _locationBeforeFullScreen;
            IsFullScreen = false;

            // Windows does not always correctly redraw the desktop when exiting soft full screen, so force a redraw
            if (!HardwareModeSwitch)
                RedrawWindow(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 1);

            _switchingFullScreen = false;
        }

        private void MinimizeFullScreen()
        {
            _switchingFullScreen = true;

            _concreteGame.GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().ClearHardwareFullscreen();

            IsBorderless = false;
            Form.WindowState = FormWindowState.Minimized;
            _lastFormState = FormWindowState.Minimized;
            Form.Location = _locationBeforeFullScreen;
            IsFullScreen = false;

            // Windows does not always correctly redraw the desktop when exiting soft full screen, so force a redraw
            if (!HardwareModeSwitch)
                RedrawWindow(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 1);

            _switchingFullScreen = false;
        }
    }
}

