// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Diagnostics;
using Android.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Platform.Input.Touch;
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
            if (base.Window == null)
            {
                ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<ConcreteTouchPanel>().PrimaryWindow = _gameWindow;
            }
            base.Window = _gameWindow;

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

            AndroidGameWindow.Activity = null;

            base.Dispose(disposing);

            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<ConcreteTouchPanel>().PrimaryWindow = null;
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

        public override void Android_BeforeUpdate()
        {
            if (!_initialized)
            {
                Game.DoInitialize();
                _initialized = true;
            }
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

            _gameWindow._touchEventListener = new TouchEventListener();
            _gameWindow._touchEventListener.SetTouchListener(this._gameWindow);
        }

        public override void Initialize()
        {
            // TODO: This should be moved to GraphicsDeviceManager or GraphicsDevice
            {
                GraphicsDevice graphicsDevice = this.GraphicsDevice;
                PresentationParameters pp = graphicsDevice.PresentationParameters;
                graphicsDevice.Viewport = new Viewport(0, 0, pp.BackBufferWidth, pp.BackBufferHeight);

                // Force the Viewport to be correctly set
                GraphicsDeviceManager gdm = this.GraphicsDeviceManager;
                gdm.GetStrategy<ConcreteGraphicsDeviceManager>().InternalResetClientBounds();
            }

            base.Initialize();
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
