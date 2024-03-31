// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Platform;
using Microsoft.Xna.Platform.Graphics;
using Microsoft.Xna.Platform.Input;
using MonoGame.Framework.Utilities;


namespace Microsoft.Xna.Framework
{
    internal class SdlGameWindow : GameWindow, IDisposable
    {
        internal Sdl SDL { get { return Sdl.Current; } }

        public override bool AllowUserResizing
        {
            get { return _isResizable && !_isBorderless; }
            set
            {
                _isResizable = value;

                Sdl.Version nonResizeableVersion = new Sdl.Version(2, 0, 4);
                if (SDL.version <= nonResizeableVersion)
                    throw new Exception("SDL " + nonResizeableVersion + " does not support changing resizable parameter of the window after it's already been created, please use a newer version of it.");
                
                SDL.WINDOW.SetResizable(_handle, _isResizable);

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
                _isBorderless = value;

                if (!_isBorderless)
                {
                    SDL.WINDOW.SetBordered(_handle, 1);
                }
                else
                {
                    SDL.WINDOW.SetBordered(_handle, 0);
                }
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


        public static GameWindow Instance;
        public uint? Id;
        public bool IsFullScreen;

        internal readonly Game _game;
        private IntPtr _handle;
        private IntPtr _pIcon;
        private bool _disposed;
        private bool _isResizable, _isBorderless;
        private bool _mouseVisible, _hardwareSwitch;
        private string _screenDeviceName;
        private int _width, _height;
        private bool _wasMoved, _supressMoved;

        private readonly List<Keys> _keys;
        private readonly List<string> _dropList;

        public SdlGameWindow(Game game)
        {
            _game = game;
            _screenDeviceName = "";

            Instance = this;

            _keys = new List<Keys>();
            ((IPlatformKeyboard)Keyboard.Current).GetStrategy<ConcreteKeyboard>().SetKeys(_keys);
            _dropList = new List<string>();

            _width = GraphicsDeviceManager.DefaultBackBufferWidth;
            _height = GraphicsDeviceManager.DefaultBackBufferHeight;

            SDL.SetHint("SDL_VIDEO_MINIMIZE_ON_FOCUS_LOSS", "0");
            SDL.SetHint("SDL_JOYSTICK_ALLOW_BACKGROUND_EVENTS", "1");

            _pIcon = LoadAppIcon();

            _handle = SDL.WINDOW.Create("", 0, 0,
                GraphicsDeviceManager.DefaultBackBufferWidth, GraphicsDeviceManager.DefaultBackBufferHeight,
                Sdl.Window.State.Hidden | Sdl.Window.State.FullscreenDesktop);

            Title = AssemblyHelper.GetDefaultWindowTitle();
        }

        private IntPtr LoadAppIcon()
        {
            Stream stream = null;
            Assembly entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null) // when running NUnit tests entry assembly can be null
            {
                stream = entryAssembly.GetManifestResourceStream(entryAssembly.GetName().Name + ".Icon.bmp");
                if (stream == null)
                    stream = entryAssembly.GetManifestResourceStream("Icon.bmp");
            }
            if (stream == null)
                stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Kni.bmp");

            try
            {
                if (stream != null)
                {
                    using (stream)
                    {
                        byte[] data = new byte[stream.Length];
                        stream.Read(data, 0, data.Length);
                        IntPtr pSrc = SDL.RwFromMem(data, data.Length);
                        IntPtr pIcon = SDL.LoadBMP_RW(pSrc, 1);
                        return pIcon;
                    }
                }
            }
            catch { }

            return IntPtr.Zero;
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

            if (_pIcon != IntPtr.Zero)
                SDL.WINDOW.SetIcon(_handle, _pIcon);

            SDL.WINDOW.SetBordered(_handle, _isBorderless ? 0 : 1);
            SDL.WINDOW.SetResizable(_handle, _isResizable);

            SetCursorVisible(_mouseVisible);
        }


        internal bool SdlRunLoop()
        {
            bool isExiting = false;

            Sdl.Event ev;

            while (SDL.PollEvent(out ev) == 1)
            {
                switch (ev.Type)
                {
                    case Sdl.EventType.Quit:
                        isExiting = true;
                        break;
                    case Sdl.EventType.JoyDeviceAdded:
                        ((IPlatformJoystick)Joystick.Current).GetStrategy<ConcreteJoystick>().AddDevices();
                        break;
                    case Sdl.EventType.JoyDeviceRemoved:
                        ((IPlatformJoystick)Joystick.Current).GetStrategy<ConcreteJoystick>().RemoveDevice(ev.JoystickDevice.Which);
                        break;
                    case Sdl.EventType.ControllerDeviceRemoved:
                        ((IPlatformGamePad)GamePad.Current).GetStrategy<ConcreteGamePad>().RemoveDevice(ev.ControllerDevice.Which);
                        break;
                    case Sdl.EventType.ControllerButtonUp:
                    case Sdl.EventType.ControllerButtonDown:
                    case Sdl.EventType.ControllerAxisMotion:
                        ((IPlatformGamePad)GamePad.Current).GetStrategy<ConcreteGamePad>().UpdatePacketInfo(ev.ControllerDevice.Which, ev.ControllerDevice.TimeStamp);
                        break;
                    case Sdl.EventType.MouseMotion:
                        unchecked
                        {
                            this.MouseState.RawX += ev.Motion.Xrel;
                            this.MouseState.RawY += ev.Motion.Yrel;
                        }
                        break;
                    case Sdl.EventType.MouseWheel:
                        const int wheelDelta = 120;
                        ((IPlatformMouse)Mouse.Current).GetStrategy<ConcreteMouse>().ScrollY += ev.Wheel.Y * wheelDelta;
                        ((IPlatformMouse)Mouse.Current).GetStrategy<ConcreteMouse>().ScrollX += ev.Wheel.X * wheelDelta;
                        break;
                    case Sdl.EventType.KeyDown:
                    {
                        Keys key = KeyboardUtil.ToXna(ev.Key.Keysym.Sym);
                        if (!_keys.Contains(key))
                                _keys.Add(key);
                        char character = (char)ev.Key.Keysym.Sym;
                        this.Platform_OnKeyDown(key);
                        if (char.IsControl(character))
                                this.Platform_OnTextInput(character, key);
                        break;
                    }
                    case Sdl.EventType.KeyUp:
                    {
                            Keys key = KeyboardUtil.ToXna(ev.Key.Keysym.Sym);
                        _keys.Remove(key);
                        this.Platform_OnKeyUp(key);
                        break;
                    }
                    case Sdl.EventType.TextInput:
                        if (this.Platform_IsTextInputAttached())
                        {
                            int len = 0;
                            int utf8character = 0; // using an int to encode multibyte characters longer than 2 bytes
                            byte currentByte = 0;
                            int charByteSize = 0; // UTF8 char length to decode
                            int remainingShift = 0;
                            unsafe
                            {
                                while ((currentByte = Marshal.ReadByte((IntPtr)ev.Text.Text, len)) != 0)
                                {
                                    // we're reading the first UTF8 byte, we need to check if it's multibyte
                                    if (charByteSize == 0)
                                    {
                                        if (currentByte < 192)
                                            charByteSize = 1;
                                        else if (currentByte < 224)
                                            charByteSize = 2;
                                        else if (currentByte < 240)
                                            charByteSize = 3;
                                        else
                                            charByteSize = 4;

                                        utf8character = 0;
                                        remainingShift = 4;
                                    }

                                    // assembling the character
                                    utf8character <<= 8;
                                    utf8character |= currentByte;

                                    charByteSize--;
                                    remainingShift--;

                                    if (charByteSize == 0) // finished decoding the current character
                                    {
                                        utf8character <<= remainingShift * 8; // shifting it to full UTF8 scope

                                        // SDL returns UTF8-encoded characters while C# char type is UTF16-encoded (and limited to the 0-FFFF range / does not support surrogate pairs)
                                        // so we need to convert it to Unicode codepoint and check if it's within the supported range
                                        int codepoint = UTF8ToUnicode(utf8character);

                                        if (codepoint >= 0 && codepoint < 0xFFFF)
                                        {
                                            this.Platform_OnTextInput((char)codepoint, KeyboardUtil.ToXna(codepoint));
                                            // UTF16 characters beyond 0xFFFF are not supported (and would require a surrogate encoding that is not supported by the char type)
                                        }
                                    }

                                    len++;
                                }
                            }
                        }
                        break;
                    case Sdl.EventType.WindowEvent:

                        // If the ID is not the same as our main window ID
                        // that means that we received an event from the
                        // dummy window, so don't process the event.
                        if (ev.Window.WindowID != this.Id)
                            break;

                        switch (ev.Window.EventID)
                        {
                            case Sdl.Window.EventId.Resized:
                            case Sdl.Window.EventId.SizeChanged:
                                ClientResize(ev.Window.Data1, ev.Window.Data2);
                                break;
                            case Sdl.Window.EventId.FocusGained:
                                _game.Strategy.IsActive = true;
                                break;
                            case Sdl.Window.EventId.FocusLost:
                                _game.Strategy.IsActive = false;
                                break;
                            case Sdl.Window.EventId.Moved:
                                if (!_supressMoved)
                                    _wasMoved = true;
                                _supressMoved = false;
                                break;
                            case Sdl.Window.EventId.Close:
                                isExiting = true;
                                break;
                        }
                        break;

                    case Sdl.EventType.DropFile:
                        if (ev.Drop.WindowId != this.Id)
                            break;

                        string path = InteropHelpers.Utf8ToString(ev.Drop.File);
                        SDL.SDL_Free(ev.Drop.File);
                        _dropList.Add(path);

                        break;

                    case Sdl.EventType.DropComplete:
                        if (ev.Drop.WindowId != this.Id)
                            break;

                        if (_dropList.Count > 0)
                        {
                            OnFileDrop(new FileDropEventArgs(_dropList.ToArray()));
                            _dropList.Clear();
                        }

                        break;
                }
            }

            return isExiting;
        }

        internal int UTF8ToUnicode(int utf8)
        {
            int
                byte4 = utf8 & 0xFF,
                byte3 = (utf8 >> 8) & 0xFF,
                byte2 = (utf8 >> 16) & 0xFF,
                byte1 = (utf8 >> 24) & 0xFF;

            if (byte1 < 0x80)
                return byte1;
            else if (byte1 < 0xC0)
                return -1;
            else if (byte1 < 0xE0 && byte2 >= 0x80 && byte2 < 0xC0)
                return (byte1 % 0x20) * 0x40 + (byte2 % 0x40);
            else if (byte1 < 0xF0 && byte2 >= 0x80 && byte2 < 0xC0 && byte3 >= 0x80 && byte3 < 0xC0)
                return (byte1 % 0x10) * 0x40 * 0x40 + (byte2 % 0x40) * 0x40 + (byte3 % 0x40);
            else if (byte1 < 0xF8 && byte2 >= 0x80 && byte2 < 0xC0 && byte3 >= 0x80 && byte3 < 0xC0 && byte4 >= 0x80 && byte4 < 0xC0)
                return (byte1 % 0x8) * 0x40 * 0x40 * 0x40 + (byte2 % 0x40) * 0x40 * 0x40 + (byte3 % 0x40) * 0x40 + (byte4 % 0x40);
            else
                return -1;
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

            if (!((IPlatformGraphicsContext)((IPlatformGraphicsDevice)device).Strategy.MainContext).Strategy.IsRenderTargetBound)
            {
                device.Viewport = new Viewport(0, 0, width, height);
                device.ScissorRectangle = new Rectangle(0, 0, width, height);
            }
            
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

            if (_pIcon != IntPtr.Zero)
                SDL.FreeSurface(_pIcon);

            _disposed = true;
        }
    }
}
