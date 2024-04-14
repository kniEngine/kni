// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Diagnostics;
using Android.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
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
            AndroidGameWindow.Activity.Game = Game;
            AndroidGameActivity.Paused += Activity_Paused;
            AndroidGameActivity.Resumed += Activity_Resumed;

            _gameWindow = new AndroidGameWindow(AndroidGameWindow.Activity, game);
            base.Window = _gameWindow;
            base.SetWindowListeners();
            if (TouchPanel.WindowHandle == IntPtr.Zero)
                TouchPanel.WindowHandle = base.Window.Handle;

            Services.AddService(typeof(View), _gameWindow.GameView);

            ConcreteMediaLibraryStrategy.Context = AndroidGameWindow.Activity;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                AndroidGameActivity.Paused -= Activity_Paused;
                AndroidGameActivity.Resumed -= Activity_Resumed;
            }

            if (TouchPanel.WindowHandle == _gameWindow.Handle)
                TouchPanel.WindowHandle = IntPtr.Zero;

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
        
        internal bool _hasWindowFocus = true;
        
        MediaState _mediaPlayer_PrevState = MediaState.Stopped;

        // EnterForeground
        void Activity_Resumed(object sender, EventArgs e)
        {
            IsActive = _hasWindowFocus;
            _gameWindow.GameView.Resume();
            if (_mediaPlayer_PrevState == MediaState.Playing && AndroidGameWindow.Activity.AutoPauseAndResumeMediaPlayer)
                MediaPlayer.Resume();
            if (!_gameWindow.GameView.IsFocused)
                _gameWindow.GameView.RequestFocus();
        }

        // EnterBackground
        void Activity_Paused(object sender, EventArgs e)
        {
            IsActive = false;
            _mediaPlayer_PrevState = MediaPlayer.State;
            _gameWindow.GameView.Pause();
            _gameWindow.GameView.ClearFocus();
            if (AndroidGameWindow.Activity.AutoPauseAndResumeMediaPlayer)
                MediaPlayer.Pause();
        }

        protected internal override void Run()
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
            _gameWindow.GameView.StartGameLoop();
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
