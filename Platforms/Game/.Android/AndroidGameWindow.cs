// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Util;
using Javax.Microedition.Khronos.Egl;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Platform;
using Microsoft.Xna.Platform.Graphics.OpenGL;
using Microsoft.Xna.Platform.Input.Touch;
using Microsoft.Xna.Platform.Graphics;
using Microsoft.Xna.Framework.Graphics;


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
                    ((IPlatformGraphicsDeviceManager)deviceManager).GetStrategy<Platform.ConcreteGraphicsDeviceManager>().InternalForceSetFullScreen();
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

                    _activity.RequestedOrientation = requestedOrientation;

                    OnOrientationChanged();
                }
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
                        ProcessStateResumed();
                    break;

                case AndroidGameWindow.AppState.Paused:
                    break;

                case AndroidGameWindow.AppState.Exited:
                        ProcessStateExited();
                    break;

                default:
                    throw new InvalidOperationException("currentState");
            }

            return;
        }

        void ProcessStateResumed()
        {
            ISurfaceView surfaceView = GameView;

            if (surfaceView.EglSurface == null)
            {
                // recreate the surface and bind the context to the thread
                if (this.EglContext != null)
                {
                    var adapter = ((IPlatformGraphicsAdapter)GraphicsAdapter.DefaultAdapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
                    var GL = adapter.Ogl;

                    GameView.GLCreateSurface(adapter, this.EglConfig);

                    if (!GL.Egl.EglMakeCurrent(adapter.EglDisplay, surfaceView.EglSurface, surfaceView.EglSurface, this.EglContext))
                    {
                        throw new Exception("Could not make EGL current" + GL.GetEglErrorAsString());
                    }
                    Threading.MakeMainThread();

                    GraphicsDeviceManager gdm = ((IPlatformGame)_game).GetStrategy<ConcreteGame>().GraphicsDeviceManager;
                    if (gdm != null)
                    {
                        if (gdm.GraphicsDevice != null)
                        {
                            ConcreteGraphicsDevice gd = (ConcreteGraphicsDevice)((IPlatformGraphicsDevice)gdm.GraphicsDevice).Strategy;
                            gd.Android_UpdateBackBufferBounds(GameView.Width, GameView.Height);
                        }
                    }
                }
            }

            _orientationListener.Update();

            try
            {
                if (_game != null && !_screenReceiver.IsScreenLocked)
                {
                    ((IPlatformGame)_game).GetStrategy<ConcreteGame>().OnFrameTick();
                }
            }
            catch (OpenGLException ex)
            {
                Log.Error("AndroidGameView", "OpenGL Exception occurred during RunIteration {0}", ex.Message);
            }
        }

        private void ProcessStateExited()
        {
        }

        private GLESVersion _glesVersion;
        private EGLConfig _eglConfig;
        internal EGLContext _eglContext;

        internal GLESVersion GLesVersion { get { return _glesVersion; } }
        internal EGLConfig EglConfig { get { return _eglConfig; } }
        internal EGLContext EglContext { get { return _eglContext; } }


        internal void GLChooseConfig()
        {
            var adapter = ((IPlatformGraphicsAdapter)GraphicsAdapter.DefaultAdapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
            var GL = adapter.Ogl;

            GraphicsDeviceManager gdm = ((IPlatformGame)_game).GetStrategy<ConcreteGame>().GraphicsDeviceManager;

            int depth = 0;
            int stencil = 0;
            int sampleBuffers = 0;
            int samples = 0;
            switch (gdm.PreferredDepthStencilFormat)
            {
                case DepthFormat.Depth16:
                    depth = 16;
                    break;
                case DepthFormat.Depth24:
                    depth = 24;
                    break;
                case DepthFormat.Depth24Stencil8:
                    depth = 24;
                    stencil = 8;
                    break;
                case DepthFormat.None:
                    break;
            }

            if (gdm.PreferMultiSampling)
            {
                sampleBuffers = 1;
                samples = 4;
            }

            List<SurfaceConfig> surfaceConfigs = new List<SurfaceConfig>();
            if (depth > 0)
            {
                surfaceConfigs.Add(new SurfaceConfig() { Red = 8, Green = 8, Blue = 8, Alpha = 8, Depth = depth, Stencil = stencil, SampleBuffers = sampleBuffers, Samples = samples });
                surfaceConfigs.Add(new SurfaceConfig() { Red = 8, Green = 8, Blue = 8, Alpha = 8, Depth = depth, Stencil = stencil });
                surfaceConfigs.Add(new SurfaceConfig() { Red = 5, Green = 6, Blue = 5, Depth = depth, Stencil = stencil });
                surfaceConfigs.Add(new SurfaceConfig() { Depth = depth, Stencil = stencil });
                if (depth > 16)
                {
                    surfaceConfigs.Add(new SurfaceConfig() { Red = 8, Green = 8, Blue = 8, Alpha = 8, Depth = 16 });
                    surfaceConfigs.Add(new SurfaceConfig() { Red = 5, Green = 6, Blue = 5, Depth = 16 });
                    surfaceConfigs.Add(new SurfaceConfig() { Depth = 16 });
                }
                surfaceConfigs.Add(new SurfaceConfig() { Red = 8, Green = 8, Blue = 8, Alpha = 8 });
                surfaceConfigs.Add(new SurfaceConfig() { Red = 5, Green = 6, Blue = 5 });
            }
            else
            {
                surfaceConfigs.Add(new SurfaceConfig() { Red = 8, Green = 8, Blue = 8, Alpha = 8, SampleBuffers = sampleBuffers, Samples = samples });
                surfaceConfigs.Add(new SurfaceConfig() { Red = 8, Green = 8, Blue = 8, Alpha = 8 });
                surfaceConfigs.Add(new SurfaceConfig() { Red = 5, Green = 6, Blue = 5 });
            }
            surfaceConfigs.Add(new SurfaceConfig() { Red = 4, Green = 4, Blue = 4 });
            EGLConfig[] results = new EGLConfig[1];

            bool found = false;
            int[] numConfigs = new int[] { 0 };
            foreach (SurfaceConfig surfaceConfig in surfaceConfigs)
            {
                Log.Verbose("AndroidGameView", string.Format("Checking Config : {0}", surfaceConfig));
                found = GL.Egl.EglChooseConfig(adapter.EglDisplay, surfaceConfig.ToConfigAttribs(), results, 1, numConfigs);
                Log.Verbose("AndroidGameView", "EglChooseConfig returned {0} and {1}", found, numConfigs[0]);
                if (!found || numConfigs[0] <= 0)
                {
                    Log.Verbose("AndroidGameView", "Config not supported");
                    continue;
                }
                Log.Verbose("AndroidGameView", string.Format("Selected Config : {0}", surfaceConfig));
                break;
            }

            if (!found || numConfigs[0] <= 0)
                throw new Exception("No valid EGL configs found" + GL.GetEglErrorAsString());
            _eglConfig = results[0];
        }

        internal void GLCreateContext()
        {
            var adapter = ((IPlatformGraphicsAdapter)GraphicsAdapter.DefaultAdapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
            var GL = adapter.Ogl;

            foreach (GLESVersion ver in ((OGL_DROID)OGL.Current).GetSupportedGLESVersions())
            {
                Log.Verbose("AndroidGameView", "Creating GLES {0} Context", ver);

                _eglContext = GL.Egl.EglCreateContext(adapter.EglDisplay, this.EglConfig, EGL10.EglNoContext, ver.GetAttributes());

                if (this.EglContext == null || this.EglContext == EGL10.EglNoContext)
                {
                    _eglContext = null;
                    Log.Verbose("AndroidGameView", string.Format("GLES {0} Not Supported. {1}", ver, GL.GetEglErrorAsString()));
                    continue;
                }
                _glesVersion = ver;
                break;
            }

            if (this.EglContext == EGL10.EglNoContext) _eglContext = null;
            if (this.EglContext == null)
                throw new Exception("Could not create EGL context" + GL.GetEglErrorAsString());

            Log.Verbose("AndroidGameView", "Created GLES {0} Context", this.GLesVersion);
        }

        internal EGLSurface GLCreatePBufferSurface(EGLConfig config, int[] attribList)
        {
            var adapter = ((IPlatformGraphicsAdapter)GraphicsAdapter.DefaultAdapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
            var GL = adapter.Ogl;

            EGLSurface result = GL.Egl.EglCreatePbufferSurface(adapter.EglDisplay, config, attribList);

            if (result == EGL10.EglNoSurface)
                result = null;

            if (result == null)
                throw new Exception("EglCreatePBufferSurface");

            return result;
        }
    }
}