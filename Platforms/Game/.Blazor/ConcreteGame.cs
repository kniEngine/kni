// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;


namespace Microsoft.Xna.Platform
{
    sealed class ConcreteGame : GameStrategy
    {
        //internal static string LaunchParameters;

        private BlazorGameWindow _gameWindow;

        // this disable the Tick(), when we are in WebXR mode
        // and running under XRSession.RequestAnimationFrame.
        internal bool _suppressTick;

        public ConcreteGame(Game game) : base(game)
        {
            _gameWindow = new BlazorGameWindow(this);
            base.Window = _gameWindow;
            base.SetWindowListeners();
        }

        protected override void Run()
        {
            this.CallInitialize();
            this.CallBeginRun();
            // XNA runs one Update even before showing the window
            this.CallUpdate(new GameTime());

            _gameWindow.InitFocus();

            StartGameLoop();
            return;

            //this.CallEndRun();
            //this.DoExiting();
        }

        private void StartGameLoop()
        {
            // request next frame
        }

        public override bool IsMouseVisible
        {
            get { return base.IsMouseVisible; }
            set
            {
                if (base.IsMouseVisible != value)
                {
                    base.IsMouseVisible = value;
                    _gameWindow.MouseVisibleToggled();
                }
            }
        }
        public override void Tick()
        {
            if (_suppressTick)
                return;

            base.Tick();
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_gameWindow != null)
                {
                    _gameWindow.Dispose();
                    _gameWindow = null;
                    Window = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
