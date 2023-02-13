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
            System.Diagnostics.Debug.Assert(Game.Activity != null, "Must set Game.Activity before creating the Game instance");
            Game.Activity.Game = Game;
            AndroidGameActivity.Paused += Activity_Paused;
            AndroidGameActivity.Resumed += Activity_Resumed;

            _gameWindow = new AndroidGameWindow(Game.Activity, game);
            Window = _gameWindow;
            Services.AddService(typeof(View), _gameWindow.GameView);

            MediaLibrary.Context = Game.Activity;
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

        private bool _initialized;
        public static bool IsPlayingVdeo { get; set; }
        private AndroidGameWindow _gameWindow;

        public override void Exit()
        {
            throw new InvalidOperationException("This platform's policy does not allow programmatically closing.");
        }

        public override void TickExiting()
        {
            // Do Nothing: Android games do not "exit" or shut down.
            throw new NotImplementedException();
        }

        public override bool BeforeUpdate(GameTime gameTime)
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
            return !IsPlayingVdeo;
        }

        public override void BeforeInitialize()
        {
            var currentOrientation = AndroidCompatibility.Current.GetAbsoluteOrientation(Game.Activity);

            switch (Game.Activity.Resources.Configuration.Orientation)
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
            Game.graphicsDeviceManager.GetStrategy<Platform.ConcreteGraphicsDeviceManager>().InternalResetClientBounds();
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
                if (_mediaPlayer_PrevState == MediaState.Playing && Game.Activity.AutoPauseAndResumeMediaPlayer)
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
                if (Game.Activity.AutoPauseAndResumeMediaPlayer)
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

            //if (!Game.Initialized)
            //{
            //    Game.DoInitialize();
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

        public override void Log(string Message)
        {
#if LOGGING
            Android.Util.Log.Debug("MonoGameDebug", Message);
#endif
        }

        public override void EndDraw()
        {
            try
            {
                var device = Game.GraphicsDevice;
                if (device != null)
                    device.Present();

                _gameWindow.GameView.SwapBuffers();
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error("Error in swap buffers", ex.ToString());
            }
        }
    }
}
