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


        private bool _isExiting;
        private SdlGameWindow _window;

        public ConcreteGame(Game game) : base(game)
        {
            var minVersion = new Sdl.Version(2,0,5);

            if (SDL.version < minVersion)
                Debug.WriteLine("Please use SDL " + minVersion + " or higher.");

            // Needed so VS can debug the project on Windows
            if (SDL.version >= minVersion && CurrentPlatform.OS == OS.Windows && Debugger.IsAttached)
                SDL.SetHint("SDL_WINDOWS_DISABLE_THREAD_NAMING", "1");

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
            bool isExiting = _window.SdlRunLoop();
            _isExiting |= isExiting;
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
                bool isExiting  = _window.SdlRunLoop();
                _isExiting |= isExiting;

                if (!_isExiting)
                    Game.Tick();
                else
                    break;
            }
        }

        public override void TickExiting()
        {
            _isExiting = true;
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
