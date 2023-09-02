// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;


namespace Microsoft.Xna.Framework
{
    internal class SdlGameWindow : GameWindow, IDisposable
    {
        internal Sdl SDL { get { return Sdl.Current; } }

        public override bool AllowUserResizing
        {
            get { return !_isBorderless && _isResizable; }
            set
            {
                var nonResizeableVersion = new Sdl.Version(2, 0, 4);
                if (SDL.version > nonResizeableVersion)
                    SDL.WINDOW.SetResizable(_handle, value);
                else
                    throw new Exception("SDL " + nonResizeableVersion + " does not support changing resizable parameter of the window after it's already been created, please use a newer version of it.");

                _isResizable = value;
            }
        }

        public override Rectangle ClientBounds
        {
            get
            {
                int x = 0, y = 0;
                SDL.WINDOW.GetPosition(Handle, out x, out y);
                return new Rectangle(x, y, _width, _height);
            }
        }

        public override DisplayOrientation CurrentOrientation
        {
            get { return DisplayOrientation.Default; }
        }

        public override IntPtr Handle
        {
            get { return _handle; }
        }

        public override string ScreenDeviceName
        {
            get { return _screenDeviceName; }
        }

        public override bool IsBorderless
        {
            get { return _isBorderless; }
            set
            {
                SDL.WINDOW.SetBordered(_handle, value ? 0 : 1);
                _isBorderless = value;
            }
        }

        public static GameWindow Instance;
        public uint? Id;
        public bool IsFullScreen;

        internal readonly Game _game;
        private IntPtr _handle, _icon;
        private bool _disposed;
        private bool _isResizable, _isBorderless;
        private bool _mouseVisible, _hardwareSwitch;
        private string _screenDeviceName;
        private int _width, _height;
        private bool _wasMoved, _supressMoved;

        public SdlGameWindow(Game game)
        {
            _game = game;
            _screenDeviceName = "";

            Instance = this;

            _width = GraphicsDeviceManager.DefaultBackBufferWidth;
            _height = GraphicsDeviceManager.DefaultBackBufferHeight;

            SDL.SetHint("SDL_VIDEO_MINIMIZE_ON_FOCUS_LOSS", "0");
            SDL.SetHint("SDL_JOYSTICK_ALLOW_BACKGROUND_EVENTS", "1");

            // load app icon
            Assembly entryAssembly = Assembly.GetEntryAssembly();
            // when running NUnit tests entry assembly can be null
            if (Assembly.GetEntryAssembly() != null)
            {
                using (
                    Stream stream =
                        entryAssembly.GetManifestResourceStream(entryAssembly.GetName().Name + ".Icon.bmp") ??
                        entryAssembly.GetManifestResourceStream("Icon.bmp") ??
                        Assembly.GetExecutingAssembly().GetManifestResourceStream("MonoGame.bmp"))
                {
                    if (stream != null)
                        using (BinaryReader br = new BinaryReader(stream))
                        {
                            try
                            {
                                IntPtr src = SDL.RwFromMem(br.ReadBytes((int)stream.Length), (int)stream.Length);
                                _icon = SDL.LoadBMP_RW(src, 1);
                            }
                            catch { }
                        }
                }
            }

            _handle = SDL.WINDOW.Create("", 0, 0,
                GraphicsDeviceManager.DefaultBackBufferWidth, GraphicsDeviceManager.DefaultBackBufferHeight,
                Sdl.Window.State.Hidden | Sdl.Window.State.FullscreenDesktop);

            Title = AssemblyHelper.GetDefaultWindowTitle();
        }

        internal void CreateWindow()
        {
            Sdl.Window.State initflags =
                Sdl.Window.State.OpenGL |
                Sdl.Window.State.Hidden |
                Sdl.Window.State.InputFocus |
                Sdl.Window.State.MouseFocus;

            if (_handle != IntPtr.Zero)
            {
                SDL.WINDOW.Destroy(_handle);
                _handle = IntPtr.Zero;
            }

            int winx = Sdl.Window.PosCentered;
            int winy = Sdl.Window.PosCentered;

            // if we are on Linux, start on the current screen
            if (CurrentPlatform.OS == OS.Linux)
            {
                winx |= GetMouseDisplay();
                winy |= GetMouseDisplay();
            }

            _width = GraphicsDeviceManager.DefaultBackBufferWidth;
            _height = GraphicsDeviceManager.DefaultBackBufferHeight;

            _handle = SDL.WINDOW.Create(Title, winx, winy, _width, _height, initflags);

            Id = SDL.WINDOW.GetWindowId(_handle);

            if (_icon != IntPtr.Zero)
                SDL.WINDOW.SetIcon(_handle, _icon);

            SDL.WINDOW.SetBordered(_handle, _isBorderless ? 0 : 1);
            SDL.WINDOW.SetResizable(_handle, _isResizable);

            SetCursorVisible(_mouseVisible);
        }

        ~SdlGameWindow()
        {
            Dispose(false);
        }

        private int GetMouseDisplay()
        {
            int x, y;
            SDL.MOUSE.GetGlobalState(out x, out y);

            int displayCount = SDL.DISPLAY.GetNumVideoDisplays();
            for (int i = 0; i < displayCount; i++)
            {
                Sdl.Rectangle rect;
                SDL.DISPLAY.GetBounds(i, out rect);

                if (x >= rect.X && x < rect.X + rect.Width &&
                    y >= rect.Y && y < rect.Y + rect.Height)
                {
                    return i;
                }
            }

            return 0;
        }

        public void SetCursorVisible(bool visible)
        {
            _mouseVisible = visible;
            SDL.MOUSE.ShowCursor(visible ? 1 : 0);
        }

        internal void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight, bool willBeFullScreen)
        {
            _screenDeviceName = screenDeviceName;

            Rectangle prevBounds = ClientBounds;
            int displayIndex = SDL.WINDOW.GetDisplayIndex(Handle);

            Sdl.Rectangle displayRect;
            SDL.DISPLAY.GetBounds(displayIndex, out displayRect);

            var gdm = _game.Strategy.GraphicsDeviceManager;
            if (willBeFullScreen != IsFullScreen || _hardwareSwitch != gdm.HardwareModeSwitch)
            {
                Sdl.Window.State fullscreenFlag = gdm.HardwareModeSwitch ? Sdl.Window.State.Fullscreen : Sdl.Window.State.FullscreenDesktop;
                SDL.WINDOW.SetFullscreen(Handle, (willBeFullScreen) ? fullscreenFlag : (Sdl.Window.State)0);
                _hardwareSwitch = gdm.HardwareModeSwitch;
            }
            // If going to exclusive full-screen mode, force the window to minimize on focus loss (Windows only)
            if (CurrentPlatform.OS == OS.Windows)
            {
                SDL.SetHint("SDL_VIDEO_MINIMIZE_ON_FOCUS_LOSS", willBeFullScreen && _hardwareSwitch ? "1" : "0");
            }

            if (!willBeFullScreen || gdm.HardwareModeSwitch)
            {
                SDL.WINDOW.SetSize(Handle, clientWidth, clientHeight);
                _width = clientWidth;
                _height = clientHeight;
            }
            else
            {
                _width = displayRect.Width;
                _height = displayRect.Height;
            }

            int ignore, minx = 0, miny = 0;
            SDL.WINDOW.GetBorderSize(_handle, out miny, out minx, out ignore, out ignore);

            int centerX = Math.Max(prevBounds.X + ((prevBounds.Width - clientWidth) / 2), minx);
            int centerY = Math.Max(prevBounds.Y + ((prevBounds.Height - clientHeight) / 2), miny);

            if (IsFullScreen && !willBeFullScreen)
            {
                // We need to get the display information again in case
                // the resolution of it was changed.
                SDL.DISPLAY.GetBounds(displayIndex, out displayRect);

                // This centering only occurs when exiting fullscreen
                // so it should center the window on the current display.
                centerX = displayRect.X + displayRect.Width / 2 - clientWidth / 2;
                centerY = displayRect.Y + displayRect.Height / 2 - clientHeight / 2;
            }

            // If this window is resizable, there is a bug in SDL 2.0.4 where
            // after the window gets resized, window position information
            // becomes wrong (for me it always returned 10 8). Solution is
            // to not try and set the window position because it will be wrong.
            Sdl.Version nonResizeableVersion = new Sdl.Version(2, 0, 4);
            if ((SDL.version > nonResizeableVersion || !AllowUserResizing) && !_wasMoved)
                SDL.WINDOW.SetPosition(Handle, centerX, centerY);

            if (IsFullScreen != willBeFullScreen)
                OnClientSizeChanged();

            IsFullScreen = willBeFullScreen;

            _supressMoved = true;
        }

        internal void Moved()
        {
            if (_supressMoved)
            {
                _supressMoved = false;
                return;
            }

            _wasMoved = true;
        }

        public void ClientResize(int width, int height)
        {
            GraphicsDevice device = _game.Strategy.GraphicsDevice;

            // SDL reports many resize events even if the Size didn't change.
            // Only call the code below if it actually changed.
            if (device.PresentationParameters.BackBufferWidth == width &&
                device.PresentationParameters.BackBufferHeight == height)
                return;

            device.PresentationParameters.BackBufferWidth = width;
            device.PresentationParameters.BackBufferHeight = height;
            device.Viewport = new Viewport(0, 0, width, height);

            SDL.WINDOW.GetSize(Handle, out _width, out _height);

            OnClientSizeChanged();
        }

        protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
        {
            // Nothing to do here
        }

        protected override void SetTitle(string title)
        {
            SDL.WINDOW.SetTitle(_handle, title);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            SDL.WINDOW.Destroy(_handle);
            _handle = IntPtr.Zero;

            if (_icon != IntPtr.Zero)
                SDL.FreeSurface(_icon);

            _disposed = true;
        }
    }
}
