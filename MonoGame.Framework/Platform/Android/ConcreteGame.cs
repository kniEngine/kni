// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Diagnostics;
using Android.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;


namespace Microsoft.Xna.Platform
{
    sealed class ConcreteGame : GameStrategy
    {
        public ConcreteGame(Game game) : base(game)
        {
            System.Diagnostics.Debug.Assert(AndroidGameWindow.Activity != null, "Must set Game.Activity before creating the Game instance");
            AndroidGameWindow.Activity.Game = Game;
            AndroidGameActivity.Paused += Activity_Paused;
            AndroidGameActivity.Resumed += Activity_Resumed;

            _gameWindow = new AndroidGameWindow(AndroidGameWindow.Activity, game);
            Window = _gameWindow;
            Services.AddService(typeof(View), _gameWindow.GameView);

            MediaLibrary.Context = AndroidGameWindow.Activity;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                AndroidGameActivity.Paused -= Activity_Paused;
                AndroidGameActivity.Resumed -= Activity_Resumed;
            }
            base.Dispose(disposing);
        }

        public static bool IsPlayingVideo { get; set; }
        private AndroidGameWindow _gameWindow;

        public override void Exit()
        {
            throw new PlatformNotSupportedException("This platform's policy does not allow programmatically closing.");
        }

        public override void TickExiting()
        {
            // Android games do not "exit" or shut down.
            throw new PlatformNotSupportedException();
        }

        public override bool BeforeUpdate()
        {
            if (!_initialized)
            {
                Game.DoInitialize();
                _initialized = true;
            }

            return true;
        }

        public override bool BeforeDraw()
        {
            PrimaryThreadLoader.DoLoads();
            if (ConcreteGame.IsPlayingVideo)
                return false;

            return true;
        }

        public override void BeforeInitialize()
        {
            var currentOrientation = AndroidCompatibility.Current.GetAbsoluteOrientation(AndroidGameWindow.Activity);

            switch (AndroidGameWindow.Activity.Resources.Configuration.Orientation)
            {
                case Android.Content.Res.Orientation.Portrait:
                    this._gameWindow.SetOrientation(currentOrientation == DisplayOrientation.PortraitDown ? DisplayOrientation.PortraitDown : DisplayOrientation.Portrait, false);
                    break;
                default:
                    this._gameWindow.SetOrientation(currentOrientation == DisplayOrientation.LandscapeRight ? DisplayOrientation.LandscapeRight : DisplayOrientation.LandscapeLeft, false);
                    break;
            }
            base.BeforeInitialize();
            _gameWindow.GameView.TouchEnabled = true;
        }

        public override void EnterFullScreen()
        {
        }

        public override void ExitFullScreen()
        {
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
            // Force the Viewport to be correctly set
            var gdm = this.GraphicsDeviceManager;
            gdm.GetStrategy<Platform.ConcreteGraphicsDeviceManager>().InternalResetClientBounds();
        }
        
        private bool _hasWindowFocus = true;
        private bool _isActivityActive = false;
        internal bool IsActivityActive { get { return _isActivityActive; } }
 
        internal void OnWindowFocusChanged(bool hasFocus)
        {
            _hasWindowFocus = hasFocus;
            IsActive = _isActivityActive && _hasWindowFocus;
        }
        
        MediaState _mediaPlayer_PrevState = MediaState.Stopped;

        // EnterForeground
        void Activity_Resumed(object sender, EventArgs e)
        {
            if (!_isActivityActive)
            {
                _isActivityActive = true;
                IsActive = _isActivityActive && _hasWindowFocus;
                _gameWindow.GameView.Resume();
                if (_mediaPlayer_PrevState == MediaState.Playing && AndroidGameWindow.Activity.AutoPauseAndResumeMediaPlayer)
                    MediaPlayer.Resume();
                if (!_gameWindow.GameView.IsFocused)
                    _gameWindow.GameView.RequestFocus();
            }
        }

        // EnterBackground
        void Activity_Paused(object sender, EventArgs e)
        {
            if (_isActivityActive)
            {
                _isActivityActive = false;
                IsActive = _isActivityActive && _hasWindowFocus;
                _mediaPlayer_PrevState = MediaPlayer.State;
                _gameWindow.GameView.Pause();
                _gameWindow.GameView.ClearFocus();
                if (AndroidGameWindow.Activity.AutoPauseAndResumeMediaPlayer)
                    MediaPlayer.Pause();
            }
        }

        public override void RunOneFrame()
        {
            // User called Game.Run().
            // Signal the game loop to initialize the game loop.
            _gameWindow.GameView.BeforeRun();

            // Prevent the default run loop from starting.
            // We will run the loop from the view's IRunnable.Run().
            return;

            //if (!_initialized)
            //{
            //    Game.DoInitialize();
            //    _initialized = true;
            //}

            //Game.DoBeginRun();
            //Timer = Stopwatch.StartNew();

            //Not quite right..
            //Game.Tick();

            //Game.DoEndRun();
        }

        internal override void Run()
        {
            // User called Game.Run().
            // Signal the game loop to initialize the game loop.
            _gameWindow.GameView.BeforeRun();

            Game.DoBeginRun();
            Timer = Stopwatch.StartNew();

            // Prevent the default run loop from starting.
            // We will run the loop from the view's IRunnable.Run().
            return;

            // StartRunLoop
            //_gameWindow.GameView.Resume();

            //Game.DoEndRun();
            //Game.DoExiting();
        }

        public override void Tick()
        {
            base.Tick();
        }
    }
}
