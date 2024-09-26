// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Diagnostics;
using Android.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Platform.Media;


namespace Microsoft.Xna.Platform
{
    sealed class ConcreteGame : GameStrategy
    {
        private static ConcreteGame _concreteGameInstance = null;
        internal static ConcreteGame GameConcreteInstance { get { return ConcreteGame._concreteGameInstance; } }

        private AndroidGameWindow _gameWindow;

        public ConcreteGame(Game game) : base(game)
        {
            ConcreteGame._concreteGameInstance = this;

            System.Diagnostics.Debug.Assert(AndroidGameWindow.Activity != null, "Must set Game.Activity before creating the Game instance");

            _gameWindow = new AndroidGameWindow(AndroidGameWindow.Activity, game);
            base.Window = _gameWindow;
            base.SetWindowListeners();

            Services.AddService(typeof(View), _gameWindow.GameView);

            ConcreteMediaLibraryStrategy.Context = AndroidGameWindow.Activity;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            AndroidGameWindow.Activity = null;

            base.Dispose(disposing);
        }


        public override void Exit()
        {
            throw new PlatformNotSupportedException("Android platform does not allow programmatically closing.");
        }

        public override void TickExiting()
        {
            // Android games do not "exit" or shut down.
            throw new PlatformNotSupportedException();
        }
        
        protected override void Run()
        {
            StartGameLoop();
            // Prevent the default run loop from starting.
            // We will run the loop from the GameView's IRunnable.Run().
            return;

            // StartRunLoop
            //_gameWindow.GameView.Resume();

            //this.CallEndRun();
            //this.DoExiting();
        }

        private void StartGameLoop()
        {
            _gameWindow.StartGameLoop();
        }

        private void OnFrameTickBegin()
        {
            this.CallInitialize();
            this.CallBeginRun();
            // XNA runs one Update even before showing the window
            this.CallUpdate(new GameTime());
        }

        bool _isReadyToRun = false;
        internal void OnFrameTick()
        {
            if (_isReadyToRun == false)
            {
                OnFrameTickBegin();
                _isReadyToRun = true;
            }

            if (AndroidGameWindow.Activity.IsActivityActive)
            {
                this.Game.Tick();
            }
        }
    }
}
