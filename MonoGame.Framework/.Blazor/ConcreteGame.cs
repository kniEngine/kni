// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;


namespace Microsoft.Xna.Platform
{
    sealed class ConcreteGame : GameStrategy
    {
        //internal static string LaunchParameters;

        private BlazorGameWindow _gameWindow;

        public ConcreteGame(Game game) : base(game)
        {
            _gameWindow = new BlazorGameWindow(this);
            base.Window = _gameWindow;
            if (TouchPanel.WindowHandle == IntPtr.Zero)
                TouchPanel.WindowHandle = base.Window.Handle;
        }

        protected internal override void Run()
        {
            this.CallInitialize();
            this.CallBeginRun();
            // XNA runs one Update even before showing the window
            this.CallUpdate(new GameTime());

            IsActive = _gameWindow.wasmWindow.Document.HasFocus();

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
            if (_gameWindow != null)
            {
                if (TouchPanel.WindowHandle == _gameWindow.Handle)
                    TouchPanel.WindowHandle = IntPtr.Zero;
            }

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
