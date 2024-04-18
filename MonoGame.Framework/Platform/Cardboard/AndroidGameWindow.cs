// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Platform;
using Microsoft.Xna.Platform.Graphics.OpenGL;
using Microsoft.Xna.Platform.Input.Touch;


namespace Microsoft.Xna.Framework
{
    [CLSCompliant(false)]
    public class AndroidGameWindow : GameWindow, IDisposable
    {
        private static Dictionary<IntPtr, AndroidGameWindow> _instances = new Dictionary<IntPtr, AndroidGameWindow>();

        internal static AndroidGameWindow FromHandle(IntPtr windowHandle)
        {
            return _instances[windowHandle];
        }

        internal static AndroidGameActivity Activity { get; set; }

        internal AndroidSurfaceView GameView { get; private set; }

        private AndroidGameActivity _activity;
        private readonly Game _game;
        private bool _hasWindowFocus = true;
        MediaState _mediaPlayer_PrevState = MediaState.Stopped;

        private Rectangle _clientBounds;
        internal DisplayOrientation _supportedOrientations = DisplayOrientation.Default;
        private DisplayOrientation _currentOrientation;

        private TouchEventListener _touchEventListener;

        public override IntPtr Handle { get { return GameView.Handle; } }

        public AndroidGameWindow(AndroidGameActivity activity, Game game)
        {
            _activity = activity;
            _game = game;

            _activity.Paused += _activity_Paused;
            _activity.Resumed += _activity_Resumed;

            _activity.WindowFocused += _activity_WindowFocused;
            _activity.WindowUnfocused += _activity_WindowUnfocused;

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
            
            GameView = new AndroidSurfaceView(activity, this, _game);
            GameView.Tick += OnTick;

            GameView.RequestFocus();
            GameView.FocusableInTouchMode = true;

            _instances.Add(this.Handle, this);

            _touchEventListener = new TouchEventListener();
            _touchEventListener.SetTouchListener(this);
        }

        void _activity_Resumed(object sender, EventArgs e)
        {
            if (!_hasWindowFocus)
            {
                _hasWindowFocus = true;
                OnActivated();
            }

            GameView.Resume();
            if (_mediaPlayer_PrevState == MediaState.Playing && _activity.AutoPauseAndResumeMediaPlayer)
                MediaPlayer.Resume();
            if (!this.GameView.IsFocused)
                this.GameView.RequestFocus();

            if (_game != null)
            {
                IGraphicsDeviceManager deviceManager = (IGraphicsDeviceManager)_game.Services.GetService(typeof(IGraphicsDeviceManager));
                if (deviceManager != null)
                {
                    ((IPlatformGraphicsDeviceManager)deviceManager).GetStrategy<Platform.ConcreteGraphicsDeviceManager>().InternalForceSetFullScreen();

                    this.GameView.RequestFocus();
                }
            }
        }

        void _activity_Paused(object sender, EventArgs e)
        {
            if (!_hasWindowFocus)
            {
                _hasWindowFocus = true;
                OnActivated();
            }

            _mediaPlayer_PrevState = MediaPlayer.State;
            this.GameView.Pause();
            this.GameView.ClearFocus();
            if (_activity.AutoPauseAndResumeMediaPlayer)
                MediaPlayer.Pause();
        }

        private void _activity_WindowFocused(object sender, EventArgs e)
        {
            if (!_hasWindowFocus)
            {
                _hasWindowFocus = true;
                OnActivated();
            }
        }

        private void _activity_WindowUnfocused(object sender, EventArgs e)
        {
            if (_hasWindowFocus)
            {
                _hasWindowFocus = false;
                OnDeactivated();
            }
        }

        #region AndroidGameView Methods

        private void OnTick(object sender, EventArgs args)
        {
            GameView.MakeCurrentContext();

            if (_game != null && !ScreenReceiver.ScreenLocked)
            {
                ((IPlatformGame)_game).GetStrategy<ConcreteGame>().OnFrameTick();
            }
        }

        #endregion


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
                if (deviceManager == null)
                    return DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;

                if (deviceManager.PreferredBackBufferWidth > deviceManager.PreferredBackBufferHeight)
                {
                    return DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
                }
                else
                {
                    return DisplayOrientation.Portrait | DisplayOrientation.PortraitDown;
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

        internal void ChangeClientBounds(Rectangle bounds)
        {
            if (bounds != _clientBounds)
            {
                _clientBounds = bounds;
                OnClientSizeChanged();
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
                ScreenOrientation requestedOrientation = ScreenOrientation.Unspecified;
                bool wasPortrait = _currentOrientation == DisplayOrientation.Portrait || _currentOrientation == DisplayOrientation.PortraitDown;
                bool requestPortrait = false;

                bool didOrientationChange = false;
                // Android 2.3 and above support reverse orientations
                int sdkVer = (int)Android.OS.Build.VERSION.SdkInt;
                if (sdkVer >= 10)
                {
                    // Check if the requested orientation is supported. Default means all are supported.
                    if ((supported & value) != 0)
                    {
                        didOrientationChange = true;
                        _currentOrientation = value;
                        switch (value)
                        {
                            case DisplayOrientation.LandscapeLeft:
                                requestedOrientation = (ScreenOrientation)ScreenOrientationAll.Landscape;
                                requestPortrait = false;
                                break;
                            case DisplayOrientation.LandscapeRight:
                                requestedOrientation = (ScreenOrientation)ScreenOrientationAll.ReverseLandscape;
                                requestPortrait = false;
                                break;
                            case DisplayOrientation.Portrait:
                                requestedOrientation = (ScreenOrientation)ScreenOrientationAll.Portrait;
                                requestPortrait = true;
                                break;
                            case DisplayOrientation.PortraitDown:
                                requestedOrientation = (ScreenOrientation)ScreenOrientationAll.ReversePortrait;
                                requestPortrait = true;
                                break;
                        }
                    }
                }
                else
                {
                    // Check if the requested orientation is either of the landscape orientations and any landscape orientation is supported.
                    if ((value == DisplayOrientation.LandscapeLeft || value == DisplayOrientation.LandscapeRight) &&
                        ((supported & (DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight)) != 0))
                    {
                        didOrientationChange = true;
                        _currentOrientation = DisplayOrientation.LandscapeLeft;
                        requestedOrientation = ScreenOrientation.Landscape;
                        requestPortrait = false;
                    }
                    // Check if the requested orientation is either of the portrain orientations and any portrait orientation is supported.
                    else if ((value == DisplayOrientation.Portrait || value == DisplayOrientation.PortraitDown) &&
                            ((supported & (DisplayOrientation.Portrait | DisplayOrientation.PortraitDown)) != 0))
                    {
                        didOrientationChange = true;
                        _currentOrientation = DisplayOrientation.Portrait;
                        requestedOrientation = ScreenOrientation.Portrait;
                        requestPortrait = true;
                    }
                }

                if (didOrientationChange)
                {
                    // Android doesn't fire Released events for existing touches
                    // so we need to clear them out.
                    if (wasPortrait != requestPortrait)
                    {
                        ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().InvalidateTouches();
                    }

                    AndroidGameWindow.Activity.RequestedOrientation = requestedOrientation;

                    OnOrientationChanged();
                }
            }
        }


        public void Dispose()
        {
            if (_activity != null)
            {
                _activity.Paused -= _activity_Paused;
                _activity.Resumed -= _activity_Resumed;

                _activity.WindowFocused += _activity_WindowFocused;
                _activity.WindowUnfocused += _activity_WindowUnfocused;

                _activity = null;
            }

            if (GameView != null)
            {
                GameView.Dispose();
                GameView = null;
            }
            
            _instances.Remove(this.Handle);
        }

        protected override void SetTitle(string title)
        {
        }
    }
}
