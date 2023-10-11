// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Microsoft.Xna.Platform
{
    sealed class ConcreteGame : GameStrategy
    {
        //internal static string LaunchParameters;

        private BlazorGameWindow _window;

        public ConcreteGame(Game game) : base(game)
        {
            _window = new BlazorGameWindow(this);

            Window = _window;
        }

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
            IsActive = true;

            _window.RunLoop();

            //Game.DoEndRun();
            //Game.DoExiting();
        }

        public override void Tick()
        {
            base.Tick();
        }

        public override bool IsMouseVisible
        {
            get { return base.IsMouseVisible; }
            set
            {
                if (base.IsMouseVisible != value)
                {
                    base.IsMouseVisible = value;
                    _window.MouseVisibleToggled();
                }
            }
        }

        public override void BeforeInitialize()
        {
            var gdm = this.GraphicsDeviceManager;
            if (gdm == null)
            {
                _window.Initialize(GraphicsDeviceManager.DefaultBackBufferWidth, GraphicsDeviceManager.DefaultBackBufferHeight);
            }
            else
            {
                var pp = this.GraphicsDevice.PresentationParameters;
                _window.Initialize(pp);
            }
        }

        public override void Exit()
        {
            throw new PlatformNotSupportedException("BlazorGL platform does not allow programmatically closing.");
        }
        
        public override void TickExiting()
        {
            // BlazorGL games do not "exit" or shut down.
            throw new PlatformNotSupportedException();
        }

        public override bool BeforeUpdate()
        {
            return true;
        }

        internal override void OnPresentationChanged(PresentationParameters pp)
        {
            _window.OnPresentationChanged(pp);
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
