// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoGame.Framework
{
    class BlazorGamePlatform : GamePlatform
    {
        //internal static string LaunchParameters;

        private BlazorGameWindow _window;

        public BlazorGamePlatform(Game game)
            : base(game)
        {
            IsActive = true;

            _window = new BlazorGameWindow(this);

            Window = _window;
        }

        public override GameRunBehavior DefaultRunBehavior
        {
            get { return GameRunBehavior.Synchronous; }
        }

        internal override void Run()
        {
            Run(DefaultRunBehavior);
        }

        internal override void Run(GameRunBehavior runBehavior)
        {
            Game.Game_AssertNotDisposed();

            if (!BeforeRun())
            {
                Game.Game_BeginRun();
                return;
            }

            if (!Game.Initialized)
            {
                Game.DoInitialize();
            }

            Game.Game_BeginRun();

            switch (runBehavior)
            {
                case GameRunBehavior.Asynchronous:
                    AsyncRunLoopEnded += Platform_AsyncRunLoopEnded;
                    StartRunLoop();
                    break;
                case GameRunBehavior.Synchronous:
                    // XNA runs one Update even before showing the window
                    Game.DoUpdate(new GameTime());

                    RunLoop();
                    Game.Game_EndRun();
                    Game.DoExiting();
                    break;

                default:
                    throw new ArgumentException(string.Format(
                        "Handling for the run behavior {0} is not implemented.", runBehavior));
            }
        }

        internal void Platform_AsyncRunLoopEnded(object sender, EventArgs e)
        {
            Game.Game_AssertNotDisposed();

            var platform = (GamePlatform)sender;
            platform.AsyncRunLoopEnded -= Platform_AsyncRunLoopEnded;
            Game.Game_EndRun();
            Game.DoExiting();
        }

        protected override void OnIsMouseVisibleChanged()
        {
            _window.MouseVisibleToggled();
        }

        public override void BeforeInitialize()
        {
            base.BeforeInitialize();

            var gdm = Game.graphicsDeviceManager;
            if (gdm == null)
            {
                _window.Initialize(GraphicsDeviceManager.DefaultBackBufferWidth, GraphicsDeviceManager.DefaultBackBufferHeight);
            }
            else
            {
                var pp = Game.GraphicsDevice.PresentationParameters;
                _window.Initialize(pp);
            }
        }

        public override void RunLoop()
        {
            _window.RunLoop();
        }

        public override void StartRunLoop()
        {
            throw new NotSupportedException("The platform does not support asynchronous run loops");
        }
        
        public override void Exit()
        {
            if (_window != null)
                _window.Dispose();
            _window = null;
            Window = null;
        }

        public override bool BeforeUpdate(GameTime gameTime)
        {
            return true;
        }

        public override bool BeforeDraw(GameTime gameTime)
        {
            return true;
        }

        public override void EnterFullScreen()
        {
        }

        public override void ExitFullScreen()
        {
        }

        internal override void OnPresentationChanged(PresentationParameters pp)
        {
            _window.OnPresentationChanged(pp);
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
        }

        public override void Log(string message)
        {
            Debug.WriteLine(message);
        }

        public override void Present()
        {
            var device = Game.GraphicsDevice;
            if ( device != null )
                device.Present();
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_window != null)
                {
                    _window.Dispose();
                    _window = null;
                    Window = null;
                }
                //Microsoft.Xna.Framework.Media.MediaManagerState.CheckShutdown();
            }

            base.Dispose(disposing);
        }
    }
}
