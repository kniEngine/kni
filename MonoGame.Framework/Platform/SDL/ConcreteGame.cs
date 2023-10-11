// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.Utilities;


namespace Microsoft.Xna.Platform
{
    sealed class ConcreteGame : GameStrategy
    {
        private Sdl SDL { get { return Sdl.Current; } }

        internal override void Run()
        {
            if (!_initialized)
            {
                Game.DoInitialize();
                _initialized = true;
            }

            Game.DoBeginRun();
            Timer = Stopwatch.StartNew();
            // XNA runs one Update even before showing the window
            Game.DoUpdate(new GameTime());

            RunLoop();

            Game.DoEndRun();
            Game.DoExiting();
        }

        public override void Tick()
        {
            base.Tick();
        }

        private readonly List<Keys> _keys;

        private int _isExiting;
        private SdlGameWindow _window;

        private readonly List<string> _dropList;

        public ConcreteGame(Game game) : base(game)
        {
            _keys = new List<Keys>();
            Keyboard.SetKeys(_keys);

            var minVersion = new Sdl.Version(2,0,5);

            if (SDL.version < minVersion)
                Debug.WriteLine("Please use SDL " + minVersion + " or higher.");

            // Needed so VS can debug the project on Windows
            if (SDL.version >= minVersion && CurrentPlatform.OS == OS.Windows && Debugger.IsAttached)
                SDL.SetHint("SDL_WINDOWS_DISABLE_THREAD_NAMING", "1");

            _dropList = new List<string>();

            SDL.Init((int)(
                Sdl.InitFlags.Video |
                Sdl.InitFlags.Joystick |
                Sdl.InitFlags.GameController |
                Sdl.InitFlags.Haptic
            ));

            SDL.DisableScreenSaver();

            GamePad.InitDatabase();
            Window = _window = new SdlGameWindow(Game);
        }

        public override void BeforeInitialize()
        {
            SdlRunLoop();

            IsActive = true;
        }

        public override bool IsMouseVisible
        {
            get { return base.IsMouseVisible; }
            set
            {
                if (base.IsMouseVisible != value)
                {
                    base.IsMouseVisible = value;
                    _window.SetCursorVisible(Game.IsMouseVisible);
                }
            }
        }

        internal override void OnPresentationChanged(PresentationParameters pp)
        {
            int displayIndex = SDL.WINDOW.GetDisplayIndex(Window.Handle);
            string displayName = SDL.DISPLAY.GetDisplayName(displayIndex);
            bool willBeFullScreen = pp.IsFullScreen;
            EndScreenDeviceChange(displayName, pp.BackBufferWidth, pp.BackBufferHeight, willBeFullScreen);
        }

        private void RunLoop()
        {
            SDL.WINDOW.Show(Window.Handle);

            while (true)
            {
                SdlRunLoop();
                Game.Tick();

                if (_isExiting > 0)
                    break;
            }
        }

        private void SdlRunLoop()
        {
            Sdl.Event ev;

            while (SDL.PollEvent(out ev) == 1)
            {
                switch (ev.Type)
                {
                    case Sdl.EventType.Quit:
                        _isExiting++;
                        break;
                    case Sdl.EventType.JoyDeviceAdded:
                        Joystick.AddDevices();
                        break;
                    case Sdl.EventType.JoyDeviceRemoved:
                        Joystick.RemoveDevice(ev.JoystickDevice.Which);
                        break;
                    case Sdl.EventType.ControllerDeviceRemoved:
                        GamePad.RemoveDevice(ev.ControllerDevice.Which);
                        break;
                    case Sdl.EventType.ControllerButtonUp:
                    case Sdl.EventType.ControllerButtonDown:
                    case Sdl.EventType.ControllerAxisMotion:
                        GamePad.UpdatePacketInfo(ev.ControllerDevice.Which, ev.ControllerDevice.TimeStamp);
                        break;
                    case Sdl.EventType.MouseMotion:
                        unchecked
                        {
                            _window.MouseState.RawX += ev.Motion.Xrel;
                            _window.MouseState.RawY += ev.Motion.Yrel;
                        }
                        break;
                    case Sdl.EventType.MouseWheel:
                        const int wheelDelta = 120;
                        Mouse.ScrollY += ev.Wheel.Y * wheelDelta;
                        Mouse.ScrollX += ev.Wheel.X * wheelDelta;
                        break;
                    case Sdl.EventType.KeyDown:
                    {
                        var key = KeyboardUtil.ToXna(ev.Key.Keysym.Sym);
                        if (!_keys.Contains(key))
                            _keys.Add(key);
                        char character = (char)ev.Key.Keysym.Sym;
                        _window.Platform_OnKeyDown(key);
                        if (char.IsControl(character))
                            _window.Platform_OnTextInput(character, key);
                        break;
                    }
                    case Sdl.EventType.KeyUp:
                    {
                        var key = KeyboardUtil.ToXna(ev.Key.Keysym.Sym);
                        _keys.Remove(key);
                        _window.Platform_OnKeyUp(key);
                        break;
                    }
                    case Sdl.EventType.TextInput:
                        if (_window.Platform_IsTextInputAttached())
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
                                            _window.Platform_OnTextInput((char)codepoint, KeyboardUtil.ToXna(codepoint));
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
                        if (ev.Window.WindowID != _window.Id)
                            break;

                        switch (ev.Window.EventID)
                        {
                            case Sdl.Window.EventId.Resized:
                            case Sdl.Window.EventId.SizeChanged:
                                _window.ClientResize(ev.Window.Data1, ev.Window.Data2);
                                break;
                            case Sdl.Window.EventId.FocusGained:
                                IsActive = true;
                                break;
                            case Sdl.Window.EventId.FocusLost:
                                IsActive = false;
                                break;
                            case Sdl.Window.EventId.Moved:
                                _window.Moved();
                                break;
                            case Sdl.Window.EventId.Close:
                                _isExiting++;
                                break;
                        }
                        break;

                    case Sdl.EventType.DropFile:
                        if (ev.Drop.WindowId != _window.Id)
                            break;

                        string path = InteropHelpers.Utf8ToString(ev.Drop.File);
                        SDL.SDL_Free(ev.Drop.File);
                        _dropList.Add(path);

                        break;

                    case Sdl.EventType.DropComplete:
                        if (ev.Drop.WindowId != _window.Id)
                            break;

                        if (_dropList.Count > 0)
                        {
                            _window.OnFileDrop(new FileDropEventArgs(_dropList.ToArray()));
                            _dropList.Clear();
                        }

                        break;
                }
            }
        }

        private int UTF8ToUnicode(int utf8)
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

        public override void TickExiting()
        {
            Interlocked.Increment(ref _isExiting);
        }

        public override bool BeforeUpdate()
        {
            return true;
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight, bool willBeFullScreen)
        {
            _window.EndScreenDeviceChange(screenDeviceName, clientWidth, clientHeight, willBeFullScreen);
        }

        protected override void Dispose(bool disposing)
        {
            if (_window != null)
            {
                _window.Dispose();
                _window = null;

                Joystick.CloseDevices();

                SDL.Quit();
            }

            base.Dispose(disposing);
        }
    }
}
