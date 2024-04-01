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
using Microsoft.Xna.Platform.Input;
using MonoGame.Framework.Utilities;


namespace Microsoft.Xna.Platform
{
    sealed class ConcreteGame : GameStrategy
    {
        private SdlGameWindow _gameWindow;

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

            ((Microsoft.Xna.Platform.Input.IPlatformGamePad)GamePad.Current).GetStrategy<Microsoft.Xna.Platform.Input.ConcreteGamePad>().InitDatabase();
            _gameWindow = new SdlGameWindow(Game);
            base.Window = _gameWindow;
        }

        public override void BeforeInitialize()
        {
            bool isExiting = _gameWindow.SdlRunLoop();
            _isExiting |= isExiting;
        }

        public override void Initialize()
        {
            // TODO: This should be moved to GraphicsDeviceManager or GraphicsDevice
            {
                GraphicsDevice graphicsDevice = this.GraphicsDevice;
                PresentationParameters pp = graphicsDevice.PresentationParameters;
                graphicsDevice.Viewport = new Viewport(0, 0, pp.BackBufferWidth, pp.BackBufferHeight);

                _gameWindow.EndScreenDeviceChange(string.Empty, pp.BackBufferWidth, pp.BackBufferHeight, pp.IsFullScreen);
            }

            base.Initialize();
        }

        public override bool IsMouseVisible
        {
            get { return base.IsMouseVisible; }
            set
            {
                if (base.IsMouseVisible != value)
                {
                    base.IsMouseVisible = value;
                    _gameWindow.SetCursorVisible(Game.IsMouseVisible);
                }
            }
        }

        internal override void OnPresentationChanged(PresentationParameters pp)
        {
            int displayIndex = SDL.WINDOW.GetDisplayIndex(Window.Handle);
            string displayName = SDL.DISPLAY.GetDisplayName(displayIndex);
            _gameWindow.EndScreenDeviceChange(displayName, pp.BackBufferWidth, pp.BackBufferHeight, pp.IsFullScreen);
        }

        private void RunLoop()
        {
            SDL.WINDOW.Show(Window.Handle);

            while (true)
            {
                bool isExiting  = _gameWindow.SdlRunLoop();
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

        public override void Android_BeforeUpdate()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (_gameWindow != null)
            {
                _gameWindow.Dispose();
                _gameWindow = null;

                ((IPlatformJoystick)Joystick.Current).GetStrategy<ConcreteJoystick>().CloseDevices();

                SDL.Quit();
            }

            base.Dispose(disposing);
        }
    }
}
