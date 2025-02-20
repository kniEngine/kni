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
using Javax.Microedition.Khronos.Egl;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Platform;
using Microsoft.Xna.Platform.Graphics.OpenGL;
using Microsoft.Xna.Platform.Input.Touch;
using Microsoft.Xna.Platform.Graphics;
using Microsoft.Xna.Framework.Graphics;
using VRCardboard = Com.Google.Vrtoolkit.Cardboard;
using Android.Runtime;


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
            
            GameView = new AndroidSurfaceView(activity, this);
            GameView.LayoutChange += GameView_LayoutChange;

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
                
                return (DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight);
            }
            
            return _supportedOrientations;
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

            if (newOrientation != _currentOrientation)
            {
                // Android 2.3 and above support reverse orientations
                int sdkVer = (int)Android.OS.Build.VERSION.SdkInt;
                if (sdkVer < 10)
                {
                    if (newOrientation == DisplayOrientation.LandscapeRight)
                        newOrientation = DisplayOrientation.LandscapeLeft;
                    if (newOrientation == DisplayOrientation.PortraitDown)
                        newOrientation = DisplayOrientation.PortraitDown;
                }

                DisplayOrientation oldOrientation = CurrentOrientation;
                if ((supported & newOrientation) != 0)
                {
                    _currentOrientation = newOrientation;
                    _activity.RequestedOrientation = XnaOrientationToAndroid(newOrientation);

                    OnOrientationChanged();
                }

                TouchPanel.DisplayOrientation = newOrientation;

                if (applyGraphicsChanges && oldOrientation != CurrentOrientation)
                {
                    GraphicsDeviceManager gdm = ((IPlatformGame)_game).GetStrategy<ConcreteGame>().GraphicsDeviceManager;
                    if (gdm != null)
                        gdm.ApplyChanges();
                }
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
            // Cardboard:
            _isStarted = true;
            return;
        }

        private void RunFrame()
        {
            try
            {
                RunStep(); // tick
            }
            catch (Exception ex) { /* ignore */ }
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

        private EGLConfig _eglConfig;
        internal EGLConfig EglConfig { get { return _eglConfig; } }



        Microsoft.Xna.Framework.XR.CardboardHeadsetState _hsState;
        private void UpdateLocalHeadsetState(VRCardboard.HeadTransform headTransform, VRCardboard.EyeParams eyeParams1, VRCardboard.EyeParams eyeParams2)
        {
            Viewport2XNA(eyeParams1.Viewport, ref _hsState.LeftEye.Viewport);
            Matrix2XNA(eyeParams1.Transform.GetEyeView(), ref _hsState.LeftEye.View);
            Matrix2XNA(eyeParams1.Transform.GetPerspective(), ref _hsState.LeftEye.Projection);

            if (eyeParams2 == null)
            {
                _hsState.RightEye.Viewport = new Viewport();
                _hsState.RightEye.View = Microsoft.Xna.Framework.Matrix.Identity;
                _hsState.RightEye.Projection = Microsoft.Xna.Framework.Matrix.Identity;
            }
            else
            {
                Viewport2XNA(eyeParams2.Viewport, ref _hsState.RightEye.Viewport);
                Matrix2XNA(eyeParams2.Transform.GetEyeView(), ref _hsState.RightEye.View);
                Matrix2XNA(eyeParams2.Transform.GetPerspective(), ref _hsState.RightEye.Projection);
            }
        }

        private void Viewport2XNA(VRCardboard.Viewport viewport, ref Viewport result)
        {
            result.X = viewport.X;
            result.Y = viewport.Y;
            result.Width = viewport.Width;
            result.Height = viewport.Height;
            result.MinDepth = 0f;
            result.MaxDepth = 1f;
        }

        private void Matrix2XNA(float[] matrix, ref Matrix result)
        {
            result.M11 = matrix[0];
            result.M12 = matrix[1];
            result.M13 = matrix[2];
            result.M14 = matrix[3];
            result.M21 = matrix[4];
            result.M22 = matrix[5];
            result.M23 = matrix[6];
            result.M24 = matrix[7];
            result.M31 = matrix[8];
            result.M32 = matrix[9];
            result.M33 = matrix[10];
            result.M34 = matrix[11];
            result.M41 = matrix[12];
            result.M42 = matrix[13];
            result.M43 = matrix[14];
            result.M44 = matrix[15];
        }

        internal void UpdateHeadsetState(out Microsoft.Xna.Framework.XR.CardboardHeadsetState state)
        {
            state = _hsState;
        }


        #region CardboardView.IRenderer

        volatile bool _isStarted = false;

        private int _glFramebuffer; // the current frame buffer
        private int[] _parameterFramebufferBinding = new int[3];


        private void GLGetRenderBufferSize(out int w, out int h)
        {
            var device = _game.GraphicsDevice;

            GraphicsContext graphicsContext = ((IPlatformGraphicsDevice)device).Strategy.CurrentContext;
            var GL = ((IPlatformGraphicsContext)graphicsContext).Strategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

            int[] renderbufferParameters = new int[2];

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _glFramebuffer);
            GL.CheckGLError();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _glFramebuffer);
            GL.CheckGLError();

            Android.Opengl.GLES20.GlGetRenderbufferParameteriv(
                (int)RenderbufferTarget.Renderbuffer,
                Android.Opengl.GLES20.GlRenderbufferWidth,
                renderbufferParameters, 0
                );
            Android.Opengl.GLES20.GlGetRenderbufferParameteriv(
                (int)RenderbufferTarget.Renderbuffer,
                Android.Opengl.GLES20.GlRenderbufferHeight,
                renderbufferParameters, 1
                );

            w = renderbufferParameters[0];
            h = renderbufferParameters[1];

            renderbufferParameters = new int[10];
            Android.Opengl.GLES20.GlGetRenderbufferParameteriv(
                (int)RenderbufferTarget.Renderbuffer,
                Android.Opengl.GLES20.GlRenderbufferInternalFormat,
                renderbufferParameters, 0
                );
            int internalFormat = renderbufferParameters[0];

            renderbufferParameters = new int[10];
            Android.Opengl.GLES20.GlGetRenderbufferParameteriv(
                (int)RenderbufferTarget.Renderbuffer,
                Android.Opengl.GLES20.GlRenderbufferRedSize,
                renderbufferParameters, 0
                );
            int redSize = renderbufferParameters[0];

            renderbufferParameters = new int[10];
            Android.Opengl.GLES20.GlGetRenderbufferParameteriv(
                (int)RenderbufferTarget.Renderbuffer,
                Android.Opengl.GLES20.GlRenderbufferDepthSize,
                renderbufferParameters, 0
                );
            int depthSize = renderbufferParameters[0];

            renderbufferParameters = new int[10];
            Android.Opengl.GLES20.GlGetRenderbufferParameteriv(
                (int)RenderbufferTarget.Renderbuffer,
                Android.Opengl.GLES20.GlRenderbufferStencilSize,
                renderbufferParameters, 0
                );
            int stencilSize = renderbufferParameters[0];
        }


        internal void VrRendererOnDrawFrame(VRCardboard.HeadTransform headTransform, VRCardboard.EyeParams eyeParams1, VRCardboard.EyeParams eyeParams2)
        {
            if (!_isStarted)
                return;

            this.UpdateLocalHeadsetState(headTransform, eyeParams1, eyeParams2);

            // If distortion correction is enabled the GL context will be set to draw into a framebuffer backed 
            // by a texture at the time of this call. If an implementor needs to change the current framebuffer, 
            // it must be reset back afterwards to the one obtained viaglGetIntegerv(GL_FRAMEBUFFER_BINDING, ...) 
            // at the beginning of this call.
            Android.Opengl.GLES20.GlGetIntegerv(Android.Opengl.GLES20.GlFramebufferBinding, _parameterFramebufferBinding, 0);
            _glFramebuffer = _parameterFramebufferBinding[0];

            if (_game.GraphicsDevice != null)
            {
                ((IPlatformGraphicsDevice)_game.GraphicsDevice).Strategy.ToConcrete<ConcreteGraphicsDevice>()._glDefaultFramebuffer = _glFramebuffer;

                // GL Context is modified by cardboard. We have to reset your cached state.
                GraphicsContext mainContext = ((IPlatformGraphicsDevice)_game.GraphicsDevice).Strategy.MainContext;
                ((IPlatformGraphicsContext)mainContext).Strategy.ToConcrete<ConcreteGraphicsContext>().Cardboard_ResetContext();
            }

            if (_game.GraphicsDevice != null)
            {
                //int w, h;
                //GLGetRenderBufferSize(out w, out h);
                //_game.GraphicsDevice.Viewport = new Viewport(0,0,w,h);
            }

            this.RunFrame();

            Android.Opengl.GLES20.GlGetIntegerv(Android.Opengl.GLES20.GlFramebufferBinding, _parameterFramebufferBinding, 1);
            System.Diagnostics.Debug.Assert(_glFramebuffer == _parameterFramebufferBinding[1],
                "framebuffer must be restored back to the one set at the beggining of CardboardView.IRenderer.OnDrawFrame()");

        }

        internal void VrRendererOnFinishFrame(VRCardboard.Viewport viewport)
        {
            Android.Opengl.GLES20.GlGetIntegerv(Android.Opengl.GLES20.GlFramebufferBinding, _parameterFramebufferBinding, 2);
            _glFramebuffer = _parameterFramebufferBinding[2];
        }

        internal void VrRendererOnRendererShutdown()
        {
        }

        internal void VrRendererOnSurfaceChanged(int width, int height)
        {
        }

        internal void VrRendererOnSurfaceCreated(EGLConfig config)
        {
            _eglConfig = config;

#if DEBUG
            IEGL10 egl10 = Javax.Microedition.Khronos.Egl.EGLContext.EGL.JavaCast<IEGL10>();
            _eglDisplay = egl10.EglGetDisplay(EGL10.EglDefaultDisplay);
            _eglSurface = egl10.EglGetCurrentSurface(EGL10.EglDraw);
            _eglContext = egl10.EglGetCurrentContext();
            if (_eglDisplay == EGL10.EglNoDisplay)
                _eglDisplay = null;
            if (_eglSurface == EGL10.EglNoSurface)
                _eglSurface = null;
            if (_eglContext == EGL10.EglNoContext)
                _eglContext = null;
#endif
        }

#if DEBUG
        private EGLDisplay _eglDisplay;
        private EGLSurface _eglSurface;
        private EGLContext _eglContext;
#endif

        #endregion CardboardView.IRenderer
    }
}
