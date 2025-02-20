// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022-2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Util;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Platform;
using Microsoft.Xna.Platform.Input.Touch;


namespace Microsoft.Xna.Framework
{
    [CLSCompliant(false)]
    public class AndroidGameWindow : GameWindow, IDisposable
    {
        // What is the state of the app.
        internal enum AppState
        {
            Paused,
            Resumed,
            Exited,
        }

        private static Dictionary<IntPtr, AndroidGameWindow> _instances = new Dictionary<IntPtr, AndroidGameWindow>();

        internal static AndroidGameWindow FromHandle(IntPtr windowHandle)
        {
            return _instances[windowHandle];
        }

        internal static AndroidGameActivity Activity { get; set; }

        internal AndroidSurfaceView GameView { get; private set; }

        internal RunnableObject _runner;

        internal AndroidGameActivity _activity;
        private readonly Game _game;
        private bool _isActivated = false;
        private AndroidGameWindow.AppState _appState = AndroidGameWindow.AppState.Exited;
        MediaState _mediaPlayer_PrevState = MediaState.Stopped;

        private Rectangle _clientBounds;
        internal DisplayOrientation _supportedOrientations = DisplayOrientation.Default;
        private DisplayOrientation _currentOrientation;

        private OrientationListener _orientationListener;
        internal ScreenReceiver _screenReceiver;
        private TouchEventListener _touchEventListener;

        public override IntPtr Handle { get { return GameView.Handle; } }

        public AndroidGameWindow(AndroidGameActivity activity, Game game)
        {
            _activity = activity;
            _game = game;

            _activity.Paused += Activity_Paused;
            _activity.Resumed += Activity_Resumed;
            _activity.Destroyed += Activity_Destroyed;

            _activity.WindowFocused += Activity_WindowFocused;
            _activity.WindowUnfocused += Activity_WindowUnfocused;

            Point size;
            // GetRealSize() was defined in JellyBeanMr1 / API 17 / Android 4.2
            if (Build.VERSION.SdkInt < BuildVersionCodes.JellyBeanMr1)
            {
                size.X = activity.Resources.DisplayMetrics.WidthPixels;
                size.Y = activity.Resources.DisplayMetrics.HeightPixels;
            }
            else
            {
                Android.Graphics.Point p = new Android.Graphics.Point();
                activity.WindowManager.DefaultDisplay.GetRealSize(p);
                size.X = p.X;
                size.Y = p.Y;
            }

            _clientBounds = new Rectangle(0, 0, size.X, size.Y);
            
            GameView = new AndroidSurfaceView(activity);
            GameView.LayoutChange += GameView_LayoutChange;
            _runner = new RunnableObject();
            _runner.Tick += OnTick;

            GameView.RequestFocus();
            GameView.FocusableInTouchMode = true;

            _instances.Add(this.Handle, this);

            _orientationListener = new OrientationListener(this, _activity);

            IntentFilter filter = new IntentFilter();
            filter.AddAction(Intent.ActionScreenOff);
            filter.AddAction(Intent.ActionScreenOn);
            filter.AddAction(Intent.ActionUserPresent);
            filter.AddAction(Android.Telephony.TelephonyManager.ActionPhoneStateChanged);
            _screenReceiver = new ScreenReceiver(_activity);
            _activity.RegisterReceiver(_screenReceiver, filter);
            _screenReceiver.Unlocked += ScreenReceiver_Unlocked;
            _screenReceiver.Locked += ScreenReceiver_Locked;

            _touchEventListener = new TouchEventListener();
            _touchEventListener.SetTouchListener(this);

            if (TouchPanel.WindowHandle == IntPtr.Zero)
                TouchPanel.WindowHandle = this.Handle;
        }

        private void ScreenReceiver_Unlocked(object sender, EventArgs e)
        {
            MediaPlayer.IsMuted = false;

            _appState = AndroidGameWindow.AppState.Resumed;
            this._runner.RequestFrame();

            try
            {
                if (!this.GameView.IsFocused)
                    this.GameView.RequestFocus();
            }
            catch (Exception ex)
            {
                Log.Verbose("RequestFocus()", ex.ToString());
            }
        }

        private void ScreenReceiver_Locked(object sender, EventArgs e)
        {
            MediaPlayer.IsMuted = true;
        }

        void Activity_Resumed(object sender, EventArgs e)
        {
            if (!_isActivated)
            {
                _isActivated = true;
                OnActivated();
            }

            _appState = AndroidGameWindow.AppState.Resumed;
            _runner.RequestFrame();
            try
            {
                if (!GameView.IsFocused)
                    GameView.RequestFocus();
            }
            catch (Exception ex)
            {
                Log.Verbose("RequestFocus()", ex.ToString());
            }
            Microsoft.Xna.Platform.Audio.AudioService.Resume();
            if (_mediaPlayer_PrevState == MediaState.Playing && _activity.AutoPauseAndResumeMediaPlayer)
                MediaPlayer.Resume();
            if (!this.GameView.IsFocused)
                this.GameView.RequestFocus();

            if (_game != null)
            {
                IGraphicsDeviceManager deviceManager = (IGraphicsDeviceManager)_game.Services.GetService(typeof(IGraphicsDeviceManager));
                if (deviceManager != null)
                {
                    // TODO: Set fullscreen in ApplyChanges()
                    GraphicsDeviceManager gdm = (GraphicsDeviceManager)deviceManager;
                    this.ForceSetFullScreen(gdm.IsFullScreen);
                }
            }

            if (_game != null)
            {
                if (_orientationListener.CanDetectOrientation())
                    _orientationListener.Enable();
            }
        }

        void Activity_Paused(object sender, EventArgs e)
        {
            if (_isActivated)
            {
                _isActivated = false;
                OnDeactivated();
            }

            _mediaPlayer_PrevState = MediaPlayer.State;
            _appState = AndroidGameWindow.AppState.Paused;
            this.GameView.ClearFocus();
            Microsoft.Xna.Platform.Audio.AudioService.Suspend();
            if (_activity.AutoPauseAndResumeMediaPlayer)
                MediaPlayer.Pause();

            if (_orientationListener.CanDetectOrientation())
                _orientationListener.Disable();
        }

        void Activity_Destroyed(object sender, EventArgs e)
        {
            _screenReceiver.Unlocked -= ScreenReceiver_Unlocked;
            _screenReceiver.Locked -= ScreenReceiver_Locked;
            _activity.UnregisterReceiver(_screenReceiver);
            _screenReceiver.IsScreenLocked = false;

            _orientationListener = null;

            if (_game != null)
            {
                _game.Dispose();
            }

        }

        private void Activity_WindowFocused(object sender, EventArgs e)
        {
            if (!_isActivated)
            {
                _isActivated = true;
                OnActivated();
            }
        }

        private void Activity_WindowUnfocused(object sender, EventArgs e)
        {
            if (_isActivated)
            {
                _isActivated = false;
                OnDeactivated();
            }
        }

        internal void ForceSetFullScreen(bool _isFullScreen)
        {
            if (_isFullScreen)
            {
                _activity.Window.ClearFlags(WindowManagerFlags.ForceNotFullscreen);
                _activity.Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            }
            else
            {
                _activity.Window.SetFlags(WindowManagerFlags.ForceNotFullscreen, WindowManagerFlags.ForceNotFullscreen);
            }
        }

        /// <summary>
        /// In Xna, setting SupportedOrientations = DisplayOrientation.Default (which is the default value)
        /// has the effect of setting SupportedOrientations to landscape only or portrait only, based on the
        /// aspect ratio of PreferredBackBufferWidth / PreferredBackBufferHeight
        /// </summary>
        /// <returns></returns>
        internal DisplayOrientation GetEffectiveSupportedOrientations()
        {
            if (_supportedOrientations == DisplayOrientation.Default)
            {
                GraphicsDeviceManager deviceManager = (_game.Services.GetService(typeof(IGraphicsDeviceManager)) as GraphicsDeviceManager);
                if (deviceManager != null)
                {
                    if (deviceManager.PreferredBackBufferWidth > deviceManager.PreferredBackBufferHeight)
                        return (DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight);
                    else
                        return (DisplayOrientation.Portrait | DisplayOrientation.PortraitDown);
                }
                else
                {
                    return (DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight);
                }
            }
            else
            {
                return _supportedOrientations;
            }
        }

        /// <summary>
        /// Updates the screen orientation. Filters out requests for unsupported orientations.
        /// </summary>
        internal void SetOrientation(DisplayOrientation newOrientation, bool applyGraphicsChanges)
        {
            DisplayOrientation supported = GetEffectiveSupportedOrientations();

            // If the new orientation is not supported, force a supported orientation
            if ((supported & newOrientation) == 0)
            {
                if ((supported & DisplayOrientation.LandscapeLeft) != 0)
                    newOrientation = DisplayOrientation.LandscapeLeft;
                else if ((supported & DisplayOrientation.LandscapeRight) != 0)
                    newOrientation = DisplayOrientation.LandscapeRight;
                else if ((supported & DisplayOrientation.Portrait) != 0)
                    newOrientation = DisplayOrientation.Portrait;
                else if ((supported & DisplayOrientation.PortraitDown) != 0)
                    newOrientation = DisplayOrientation.PortraitDown;
            }

            DisplayOrientation oldOrientation = CurrentOrientation;

            SetDisplayOrientation(newOrientation);
            TouchPanel.DisplayOrientation = newOrientation;

            if (applyGraphicsChanges && oldOrientation != CurrentOrientation)
            {
                GraphicsDeviceManager gdm = ((IPlatformGame)_game).GetStrategy<ConcreteGame>().GraphicsDeviceManager;
                if (gdm != null)
                    gdm.ApplyChanges();
            }
        }

        public override string ScreenDeviceName
        {
            get
            {
                throw new NotImplementedException();
            }
        }


        public override Rectangle ClientBounds
        {
            get
            {
                return _clientBounds;
            }
        }


        private void GameView_LayoutChange(object sender, View.LayoutChangeEventArgs e)
        {
            Rectangle bounds = new Rectangle(
                GameView.Left, GameView.Top,
                GameView.Width, GameView.Height);

            if (bounds != _clientBounds)
            {
                _clientBounds = bounds;
                OnClientSizeChanged();

                Android.Util.Log.Debug("Kni", "GameWindow.ChangeClientBounds: newClientBounds=" + this.ClientBounds.Width + "," + this.ClientBounds.Height);

                // Set the new display size on the touch panel.
                TouchPanel.DisplayWidth = this.ClientBounds.Width;
                TouchPanel.DisplayHeight = this.ClientBounds.Height;
            }
        }

        public override bool AllowUserResizing
        {
            get { return false; }
            set { /* Ignore */ }
        }

        // A copy of ScreenOrientation from Android 2.3
        // This allows us to continue to support 2.2 whilst
        // utilising the 2.3 improved orientation support.
        enum ScreenOrientationAll
        {
            Unspecified = -1,
            Landscape = 0,
            Portrait = 1,
            User = 2,
            Behind = 3,
            Sensor = 4,
            Nosensor = 5,
            SensorLandscape = 6,
            SensorPortrait = 7,
            ReverseLandscape = 8,
            ReversePortrait = 9,
            FullSensor = 10,
        }

        public override DisplayOrientation CurrentOrientation
        {
            get
            {
                return _currentOrientation;
            }
        }

        
        private void SetDisplayOrientation(DisplayOrientation value)
        {
            if (value != _currentOrientation)
            {
                DisplayOrientation supported = GetEffectiveSupportedOrientations();

                // Android 2.3 and above support reverse orientations
                int sdkVer = (int)Android.OS.Build.VERSION.SdkInt;
                if (sdkVer < 10)
                {
                    if (value == DisplayOrientation.LandscapeRight)
                        value = DisplayOrientation.LandscapeLeft;
                    if (value == DisplayOrientation.PortraitDown)
                        value = DisplayOrientation.PortraitDown;
                }

                if ((supported & value) != 0)
                {
                    _currentOrientation = value;
                    _activity.RequestedOrientation = XnaOrientationToAndroid(value);

                    OnOrientationChanged();
                }
            }
        }

        private static ScreenOrientation XnaOrientationToAndroid(DisplayOrientation value)
        {
            switch (value)
            {
                case DisplayOrientation.LandscapeLeft:
                    return (ScreenOrientation)ScreenOrientationAll.Landscape;
                case DisplayOrientation.LandscapeRight:
                    return (ScreenOrientation)ScreenOrientationAll.ReverseLandscape;
                case DisplayOrientation.Portrait:
                    return (ScreenOrientation)ScreenOrientationAll.Portrait;
                case DisplayOrientation.PortraitDown:
                    return (ScreenOrientation)ScreenOrientationAll.ReversePortrait;

                default:
                    return ScreenOrientation.Unspecified;
            }
        }

        public void Dispose()
        {
            if (_activity != null)
            {
                _activity.Paused -= Activity_Paused;
                _activity.Resumed -= Activity_Resumed;
                _activity.Destroyed -= Activity_Destroyed;

                _activity.WindowFocused += Activity_WindowFocused;
                _activity.WindowUnfocused += Activity_WindowUnfocused;

                _activity = null;
            }

            _appState = AndroidGameWindow.AppState.Exited;

            if (GameView != null)
            {
                if (TouchPanel.WindowHandle == this.Handle)
                    TouchPanel.WindowHandle = IntPtr.Zero;

                GameView.LayoutChange -= GameView_LayoutChange;

                GameView.Dispose();
                GameView = null;
            }
            
            _instances.Remove(this.Handle);
        }

        protected override void SetTitle(string title)
        {
        }

        internal void StartGameLoop()
        {
            _runner.InitLoopHandler();
            _runner.RequestFrame();
        }

        internal void OnTick(object sender, EventArgs args)
        {
            try
            {
                RunStep(); // tick
            }
            finally
            {
                // request next tick
                if (_appState == AndroidGameWindow.AppState.Resumed)
                    _runner.RequestFrame();
            }
        }

        private void RunStep()
        {
            switch (_appState)
            {
                case AndroidGameWindow.AppState.Resumed:
                    if (GameView._isAndroidSurfaceAvailable) // do not run game if surface is not available
                    {
                        _orientationListener.Update();

                        if (!_screenReceiver.IsScreenLocked) // do not run game if Screen is Locked
                        {
                            if (_game != null)
                            {
                                ((IPlatformGame)_game).GetStrategy<ConcreteGame>().OnFrameTick();
                            }
                        }
                    }
                    break;

                case AndroidGameWindow.AppState.Paused:
                    break;

                case AndroidGameWindow.AppState.Exited:
                    break;

                default:
                    throw new InvalidOperationException("currentState");
            }

            return;
        }

    }
}
