// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Threading;
using Android.Content;
using Android.Media;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Javax.Microedition.Khronos.Egl;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Platform;
using Microsoft.Xna.Platform.Graphics;
using Microsoft.Xna.Platform.Graphics.OpenGL;


namespace Microsoft.Xna.Framework
{
    [CLSCompliant(false)]
    public class AndroidSurfaceView : SurfaceView
        , ISurfaceView
        , ISurfaceHolderCallback
        , Java.Lang.IRunnable
    {
        // What is the state of the app, for tracking surface recreation inside this class.
        enum AppState
        {
            Pausing,  // set by android UI thread process it and transitions into 'Paused' state
            Resuming, // set by android UI thread process it and transitions into 'Running' state

            Paused,  // set by game thread after processing 'Pausing' state
            Running, // set by game thread after processing 'Resuming' state
            Exited,  // set by game thread after processing 'Exiting' state
        }

        ISurfaceHolder _surfaceHolder;

        volatile AppState _appState = AppState.Exited;

        volatile bool _forceRecreateSurface = false;
        bool _androidSurfaceAvailable = false;

        bool _glSurfaceAvailable;
        bool _glContextAvailable;
        bool _lostglContext;

        System.Diagnostics.Stopwatch _stopWatch;
        DateTime _prevTickTime;

        bool? _isCancellationRequested = null;
        private int _frameRequests = 0;

        private readonly AndroidGameWindow _gameWindow;
        private readonly Game _game;

        internal event EventHandler Tick;


        public AndroidSurfaceView(Context context, AndroidGameWindow gameWindow, Game game)
            : base(context)
        {
            _gameWindow = gameWindow;
            _game = game;
            Init();
        }

        private void Init()
        {
            // default
            _surfaceHolder = Holder;
            // Add callback to get the SurfaceCreated etc events
            _surfaceHolder.AddCallback((ISurfaceHolderCallback)this);

            // Holder.SetType is deprecated. The SurfaceType value is ignored.
            if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.Honeycomb)
                _surfaceHolder.SetType(SurfaceType.Gpu);
        }

        void ISurfaceHolderCallback.SurfaceChanged(ISurfaceHolder holder, global::Android.Graphics.Format format, int width, int height)
        {
            // Set flag to recreate gl surface or rendering can be bad on orientation change or if app 
            // is closed in one orientation and re-opened in another.

            // can only be triggered when main loop is running, is unsafe to overwrite other states
            if (_appState == AppState.Running)
                _forceRecreateSurface = true;
        }

        void ISurfaceHolderCallback.SurfaceCreated(ISurfaceHolder holder)
        {
            _androidSurfaceAvailable = true;
        }

        void ISurfaceHolderCallback.SurfaceDestroyed(ISurfaceHolder holder)
        {
            _androidSurfaceAvailable = false;
        }

        internal void SwapBuffers()
        {
            if (!_egl.EglSwapBuffers(_eglDisplay, _eglSurface))
            {
                if (_egl.EglGetError() == 0)
                {
                    if (_lostglContext)
                        System.Diagnostics.Debug.WriteLine("Lost EGL context" + GetErrorAsString());
                    _lostglContext = true;
                }
            }
        }

        internal void MakeCurrentContext()
        {
            if (!_egl.EglMakeCurrent(_eglDisplay, _eglSurface, _eglSurface, _eglContext))
                System.Diagnostics.Debug.WriteLine("Error Make Current" + GetErrorAsString());
        }

        internal void ClearCurrentContext()
        {
            if (!_egl.EglMakeCurrent(_eglDisplay, EGL10.EglNoSurface, EGL10.EglNoSurface, EGL10.EglNoContext))
            {
                Log.Verbose("AndroidGameView", "Could not unbind EGL surface" + GetErrorAsString());
                //System.Diagnostics.Debug.WriteLine("Error Clearing Current" + GetErrorAsString());
            }
        }

        internal void BeforeRun()
        {
            _isCancellationRequested = false;

            // prepare gameLoop
            Threading.MakeMainThread();
            _stopWatch = System.Diagnostics.Stopwatch.StartNew();
            _prevTickTime = DateTime.Now;
            var looper = Android.OS.Looper.MainLooper;
            _handler = new Android.OS.Handler(looper); // why this.Handler is null? Do we initialize the game too soon?

            // request first tick.
            RequestFrame();
        }

        private void RequestFrame()
        {
            if (_frameRequests == 0)
            {
                _handler.Post((Java.Lang.IRunnable)this);
                _frameRequests++;
            }
        }

        Android.OS.Handler _handler;
        void Java.Lang.IRunnable.Run()
        {
            _frameRequests--;

            if (_isCancellationRequested.Value == false)
            {
                try
                {
                    // tick
                    RunStep();
                }
                finally
                {
                    // request next tick
                    if (_appState == AppState.Resuming
                    ||  _appState == AppState.Running
                    ||  _appState == AppState.Pausing)
                        RequestFrame();
                }
            }
            else
            {
                _isCancellationRequested = null;

                if (_glSurfaceAvailable)
                {
                    if (_eglSurface != null && _eglSurface != EGL10.EglNoSurface)
                    {
                        ClearCurrentContext();
                        DestroyGLSurface();
                    }
                    _eglSurface = null;
                    _glSurfaceAvailable = false;
                }

                if (_glContextAvailable)
                {
                    if (_eglContext != null)
                        DestroyGLContext();
                    _eglContext = null;
                    if (_eglDisplay != null)
                        DestroyGLDisplay();
                    _eglDisplay = null;

                    _glContextAvailable = false;

                    if (_game.Strategy.GraphicsDevice != null)
                    {
                        ((IPlatformGraphicsDevice)_game.Strategy.GraphicsDevice).Strategy.ToConcrete<ConcreteGraphicsDevice>().Android_OnContextLost();
                    }
                }
                
                _appState = AppState.Exited;
            }
            
            return;
        }

        void RunStep()
        {
            Threading.MakeMainThread();

            switch (_appState)
            {
                case AppState.Resuming: // when ui thread wants to resume
                    processStateResuming();
                    break;

                case AppState.Running: // when we are running game 
                    if (_forceRecreateSurface == true)
                        ForceSurfaceRecreation();
                    else
                        processStateRunning();
                    break;

                case AppState.Pausing: // when ui thread wants to pause              
                    processStatePausing();
                    break;

                case AppState.Paused: // when game thread processed pausing event
                    // this must be processed outside of this loop, in the new task thread!
                    break;

                case AppState.Exited:
                    _isCancellationRequested = true;
                    break;
                
                default:
                    throw new InvalidOperationException("currentState");
                    break;
            }

            return;
        }

        void processStateResuming()
        {
            // this can happen if pause is triggered immediately after resume so that SurfaceCreated callback doesn't get called yet,
            // in this case we skip the resume process and pause sets a new state.
            if (!_androidSurfaceAvailable)
                return;

            // create surface if context is available
            if (_glContextAvailable && !_lostglContext)
            {
                try
                {
                    if (!_glSurfaceAvailable)
                        CreateGLSurface();
                }
                catch (Exception ex)
                {
                    // We failed to create the surface for some reason
                    Log.Verbose("AndroidGameView", ex.ToString());
                }
            }

            // create context if not available
            if (_lostglContext || !_glContextAvailable)
            {
                // Start or Restart due to context loss
                bool contextLost = false;
                if (_lostglContext)
                {
                    // we actually lost the context
                    // so we need to free up our existing 
                    // objects and re-create one.
                    if (_eglContext != null)
                        DestroyGLContext();
                    _eglContext = null;
                    if (_eglDisplay != null)
                        DestroyGLDisplay();
                    _eglDisplay = null;

                    _glContextAvailable = false;
                    contextLost = true;

                    if (_game.Strategy.GraphicsDevice != null)
                    {
                        ((IPlatformGraphicsDevice)_game.Strategy.GraphicsDevice).Strategy.ToConcrete<ConcreteGraphicsDevice>().Android_OnContextLost();
                    }
                }

                if (OGL_DROID.Current == null)
                    OGL_DROID.Initialize();

                CreateGLContext();
                _glContextAvailable = true;

                if (!_glSurfaceAvailable)
                    CreateGLSurface();

                // OGL.InitExtensions() must be called while we have a gl context.
                if (OGL_DROID.Current.Extensions == null)
                    OGL_DROID.Current.InitExtensions();

                if (contextLost && _glContextAvailable)
                {
                    // we lost the gl context, we need to let the programmer
                    // know so they can re-create textures etc.
                    if (_lostglContext)
                        ContextSetInternal();
                }

                return;
            }

            // finish state if surface created, may take a frame or two until the android UI thread callbacks fire
            if (_glSurfaceAvailable) 
            {
                // must resume openAL device here
                Microsoft.Xna.Platform.Audio.AudioService.Resume();

                // go to next state
                _appState = AppState.Running;
                _forceRecreateSurface = false;
            }
        }

        void ForceSurfaceRecreation()
        {
            // needed at app start
            if (!_androidSurfaceAvailable || !_glContextAvailable)
                return;

            if (_eglSurface != null && _eglSurface != EGL10.EglNoSurface)
            {
                ClearCurrentContext();
                DestroyGLSurface();
            }
            _eglSurface = null;
            _glSurfaceAvailable = false;

            CreateGLSurface();

            // go to next state
            _forceRecreateSurface = false;
        }

        void processStateRunning()
        {
            // do not run game if surface is not available
            if (!_androidSurfaceAvailable)
                return;

            // check if app wants to exit
            if (_isCancellationRequested.Value == true)
            {
                // change state to exit and skip game loop
                _appState = AppState.Exited;
                return;
            }

            try
            {
                var currTickTime = DateTime.Now;
                TimeSpan dt = TimeSpan.Zero;
                if (_prevTickTime.Ticks != 0)
                {
                    dt = (currTickTime - _prevTickTime);
                    if (dt.TotalMilliseconds < 0)
                        dt = TimeSpan.Zero;
                }
                _prevTickTime = currTickTime;

                try { AndroidGameWindow.Activity._orientationListener.Update(dt); }
                catch (Exception) { }

                var handler = Tick;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
            catch (OpenGLException ex)
            {
                Log.Error("AndroidGameView", "OpenGL Exception occurred during RunIteration {0}", ex.Message);
            }
        }

        void processStatePausing()
        {
            if (_glSurfaceAvailable)
            {
                // Surface we are using needs to go away
                if (_eglSurface != null && _eglSurface != EGL10.EglNoSurface)
                {
                    ClearCurrentContext();
                    DestroyGLSurface();
                }
                _eglSurface = null;
                _glSurfaceAvailable = false;
            }

            // must pause openAL device here
            Microsoft.Xna.Platform.Audio.AudioService.Suspend();

            // go to next state
            _appState = AppState.Paused;
        }

        internal void Resume()
        {
            _appState = AppState.Resuming;
            RequestFrame();

            try
            {
                if (!IsFocused)
                    RequestFocus();
            }
            catch(Exception ex)
            {
                Log.Verbose("RequestFocus()", ex.ToString());
            }
        }

        internal void Pause()
        {
            // if triggered in quick succession and blocked by graphics device creation, 
            // pause can be triggered twice, without resume in between on some phones.
            if (_appState != AppState.Running)
            {
                if (_forceRecreateSurface == false)
                    return;
            }

            if (!_androidSurfaceAvailable)
            {
                // happens if pause is called immediately after resume so that the surfaceCreated callback was not called yet.
                _appState = AppState.Paused; // prepare for next game loop iteration
            }
            else
            {
                // processing the pausing state only if the surface was created already
                _appState = AppState.Pausing;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_isCancellationRequested != null)
                {
                    _appState = AppState.Exited;
                    _isCancellationRequested = true;
                }
            }

            base.Dispose(disposing);
        }

        protected void DestroyGLContext()
        {
            System.Diagnostics.Debug.Assert(_eglContext != null);

            if (!_egl.EglDestroyContext(_eglDisplay, _eglContext))
                throw new Exception("Could not destroy EGL context" + GetErrorAsString());
        }

        private void DestroyGLDisplay()
        {
            System.Diagnostics.Debug.Assert(_eglDisplay != null);

            if (!_egl.EglTerminate(_eglDisplay))
                throw new Exception("Could not terminate EGL connection" + GetErrorAsString());
        }

        protected void DestroyGLSurface()
        {
            System.Diagnostics.Debug.Assert(_eglSurface != null && _eglSurface != EGL10.EglNoSurface);

            if (!_egl.EglDestroySurface(_eglDisplay, _eglSurface))
                Log.Verbose("AndroidGameView", "Could not destroy EGL surface" + GetErrorAsString());
        }

        internal struct SurfaceConfig
        {
            public int Red;
            public int Green;
            public int Blue;
            public int Alpha;
            public int Depth;
            public int Stencil;
            public int SampleBuffers;
            public int Samples;

            public int[] ToConfigAttribs()
            {
                List<int> attribs = new List<int>();
                if (Red != 0)
                {
                    attribs.Add(EGL11.EglRedSize);
                    attribs.Add(Red);
                }
                if (Green != 0)
                {
                    attribs.Add(EGL11.EglGreenSize);
                    attribs.Add(Green);
                }
                if (Blue != 0)
                {
                    attribs.Add(EGL11.EglBlueSize);
                    attribs.Add(Blue);
                }
                if (Alpha != 0)
                {
                    attribs.Add(EGL11.EglAlphaSize);
                    attribs.Add(Alpha);
                }
                if (Depth != 0)
                {
                    attribs.Add(EGL11.EglDepthSize);
                    attribs.Add(Depth);
                }
                if (Stencil != 0)
                {
                    attribs.Add(EGL11.EglStencilSize);
                    attribs.Add(Stencil);
                }
                if (SampleBuffers != 0)
                {
                    attribs.Add(EGL11.EglSampleBuffers);
                    attribs.Add(SampleBuffers);
                }
                if (Samples != 0)
                {
                    attribs.Add(EGL11.EglSamples);
                    attribs.Add(Samples);
                }
                attribs.Add(EGL11.EglRenderableType);
                attribs.Add(4);
                attribs.Add(EGL11.EglNone);

                return attribs.ToArray();
            }

            static int GetAttribute(EGLConfig config, IEGL10 egl, EGLDisplay eglDisplay,int attribute)
            {
                int[] data = new int[1];
                egl.EglGetConfigAttrib(eglDisplay, config, attribute, data);
                return data[0];
            }

            public static SurfaceConfig FromEGLConfig(EGLConfig config, IEGL10 egl, EGLDisplay eglDisplay)
            {
                return new SurfaceConfig()
                {
                    Red = GetAttribute(config, egl, eglDisplay, EGL11.EglRedSize),
                    Green = GetAttribute(config, egl, eglDisplay, EGL11.EglGreenSize),
                    Blue = GetAttribute(config, egl, eglDisplay, EGL11.EglBlueSize),
                    Alpha = GetAttribute(config, egl, eglDisplay, EGL11.EglAlphaSize),
                    Depth = GetAttribute(config, egl, eglDisplay, EGL11.EglDepthSize),
                    Stencil = GetAttribute(config, egl, eglDisplay, EGL11.EglStencilSize),
                    SampleBuffers = GetAttribute(config, egl, eglDisplay, EGL11.EglSampleBuffers),
                    Samples = GetAttribute(config, egl, eglDisplay, EGL11.EglSamples)
                };
            }

            public override string ToString()
            {
                return string.Format("Red:{0} Green:{1} Blue:{2} Alpha:{3} Depth:{4} Stencil:{5} SampleBuffers:{6} Samples:{7}", Red, Green, Blue, Alpha, Depth, Stencil, SampleBuffers, Samples);
            }
        }

        protected void CreateGLContext()
        {
            _lostglContext = false;

            _egl = EGLContext.EGL.JavaCast<IEGL10>();

            _eglDisplay = _egl.EglGetDisplay(EGL10.EglDefaultDisplay);
            if (_eglDisplay == EGL10.EglNoDisplay)
                throw new Exception("Could not get EGL display" + GetErrorAsString());

            int[] version = new int[2];
            if (!_egl.EglInitialize(_eglDisplay, version))
                throw new Exception("Could not initialize EGL display" + GetErrorAsString());

            var gdm = _game.Strategy.GraphicsDeviceManager;

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

            List<SurfaceConfig> configs = new List<SurfaceConfig>();
            if (depth > 0)
            {
                configs.Add(new SurfaceConfig() { Red = 8, Green = 8, Blue = 8, Alpha = 8, Depth = depth, Stencil = stencil, SampleBuffers = sampleBuffers, Samples = samples });
                configs.Add(new SurfaceConfig() { Red = 8, Green = 8, Blue = 8, Alpha = 8, Depth = depth, Stencil = stencil });
                configs.Add(new SurfaceConfig() { Red = 5, Green = 6, Blue = 5, Depth = depth, Stencil = stencil });
                configs.Add(new SurfaceConfig() { Depth = depth, Stencil = stencil });
                if (depth > 16)
                {
                    configs.Add(new SurfaceConfig() { Red = 8, Green = 8, Blue = 8, Alpha = 8, Depth = 16 });
                    configs.Add(new SurfaceConfig() { Red = 5, Green = 6, Blue = 5, Depth = 16 });
                    configs.Add(new SurfaceConfig() { Depth = 16 });
                }
                configs.Add(new SurfaceConfig() { Red = 8, Green = 8, Blue = 8, Alpha = 8 });
                configs.Add(new SurfaceConfig() { Red = 5, Green = 6, Blue = 5 });
            }
            else
            {
                configs.Add(new SurfaceConfig() { Red = 8, Green = 8, Blue = 8, Alpha = 8, SampleBuffers = sampleBuffers, Samples = samples });
                configs.Add(new SurfaceConfig() { Red = 8, Green = 8, Blue = 8, Alpha = 8 });
                configs.Add(new SurfaceConfig() { Red = 5, Green = 6, Blue = 5 });
            }
            configs.Add(new SurfaceConfig() { Red = 4, Green = 4, Blue = 4 });
            int[] numConfigs = new int[1];
            EGLConfig[] results = new EGLConfig[1];

            if (!_egl.EglGetConfigs(_eglDisplay, null, 0, numConfigs))
            {
                throw new Exception("Could not get config count. " + GetErrorAsString());
            }

            EGLConfig[] cfgs = new EGLConfig[numConfigs[0]];
            _egl.EglGetConfigs(_eglDisplay, cfgs, numConfigs[0], numConfigs);
            Log.Verbose("AndroidGameView", "Device Supports");
            foreach (var c in cfgs)
            {
                Log.Verbose("AndroidGameView", string.Format(" {0}", SurfaceConfig.FromEGLConfig(c, _egl, _eglDisplay)));
            }

            bool found = false;
            numConfigs[0] = 0;
            foreach (var config in configs)
            {
                Log.Verbose("AndroidGameView", string.Format("Checking Config : {0}", config));
                found = _egl.EglChooseConfig(_eglDisplay, config.ToConfigAttribs(), results, 1, numConfigs);
                Log.Verbose("AndroidGameView", "EglChooseConfig returned {0} and {1}", found, numConfigs[0]);
                if (!found || numConfigs[0] <= 0)
                {
                    Log.Verbose("AndroidGameView", "Config not supported");
                    continue;
                }
                Log.Verbose("AndroidGameView", string.Format("Selected Config : {0}", config));
                break;
            }

            if (!found || numConfigs[0] <= 0)
                throw new Exception("No valid EGL configs found" + GetErrorAsString());

            foreach (GLESVersion ver in ((OGL_DROID)OGL.Current).GetSupportedGLESVersions())
            {
                Log.Verbose("AndroidGameView", "Creating GLES {0} Context", ver);

                _eglContext = _egl.EglCreateContext(_eglDisplay, results[0], EGL10.EglNoContext, ver.GetAttributes());

                if (_eglContext == null || _eglContext == EGL10.EglNoContext)
                {
                    _eglContext = EGL10.EglNoContext;
                    Log.Verbose("AndroidGameView", string.Format("GLES {0} Not Supported. {1}", ver, GetErrorAsString()));
                    continue;
                }
                _glesVersion = ver;
                break;
            }
            if (_eglContext == null || _eglContext == EGL10.EglNoContext)
            {
                _eglContext = null;
                throw new Exception("Could not create EGL context" + GetErrorAsString());
            }
            Log.Verbose("AndroidGameView", "Created GLES {0} Context", _glesVersion);
            _eglConfig = results[0];
        }

        private string GetErrorAsString()
        {
            switch (_egl.EglGetError())
            {
                case EGL10.EglSuccess:
                    return "Success";

                case EGL10.EglNotInitialized:
                    return "Not Initialized";

                case EGL10.EglBadAccess:
                    return "Bad Access";
                case EGL10.EglBadAlloc:
                    return "Bad Allocation";
                case EGL10.EglBadAttribute:
                    return "Bad Attribute";
                case EGL10.EglBadConfig:
                    return "Bad Config";
                case EGL10.EglBadContext:
                    return "Bad Context";
                case EGL10.EglBadCurrentSurface:
                    return "Bad Current Surface";
                case EGL10.EglBadDisplay:
                    return "Bad Display";
                case EGL10.EglBadMatch:
                    return "Bad Match";
                case EGL10.EglBadNativePixmap:
                    return "Bad Native Pixmap";
                case EGL10.EglBadNativeWindow:
                    return "Bad Native Window";
                case EGL10.EglBadParameter:
                    return "Bad Parameter";
                case EGL10.EglBadSurface:
                    return "Bad Surface";

                default:
                    return "Unknown Error";
            }
        }

        protected void CreateGLSurface()
        {
            System.Diagnostics.Debug.Assert(_glSurfaceAvailable == false);

            try
            {
                // If there is an existing surface, destroy the old one
                if (_eglSurface != null && _eglSurface != EGL10.EglNoSurface)
                {
                    ClearCurrentContext();
                    DestroyGLSurface();
                }
                _eglSurface = null;
                _glSurfaceAvailable = false;

                _eglSurface = _egl.EglCreateWindowSurface(_eglDisplay, _eglConfig, (Java.Lang.Object)this.Holder, null);
                if (_eglSurface == null || _eglSurface == EGL10.EglNoSurface)
                    throw new Exception("Could not create EGL window surface" + GetErrorAsString());

                if (!_egl.EglMakeCurrent(_eglDisplay, _eglSurface, _eglSurface, _eglContext))
                    throw new Exception("Could not make EGL current" + GetErrorAsString());

                _glSurfaceAvailable = true;

                // Must set viewport after creation, the viewport has correct values in it already as we call it, but
                // the surface is created after the correct viewport is already applied so we must do it again.
                if (_game.Strategy.GraphicsDevice != null)
                {
                    var gdm = _game.Strategy.GraphicsDeviceManager;
                    gdm.GetStrategy<Platform.ConcreteGraphicsDeviceManager>().InternalResetClientBounds();
                }

            }
            catch (Exception ex)
            {
                _glSurfaceAvailable = false;
                Log.Error("AndroidGameView", ex.ToString());
            }
        }

        protected EGLSurface CreatePBufferSurface(EGLConfig config, int[] attribList)
        {
            IEGL10 egl = EGLContext.EGL.JavaCast<IEGL10>();
            EGLSurface result = egl.EglCreatePbufferSurface(_eglDisplay, config, attribList);
            if (result == null || result == EGL10.EglNoSurface)
                throw new Exception("EglCreatePBufferSurface");
            return result;
        }

        protected void ContextSetInternal()
        {
            System.Diagnostics.Debug.Assert(_lostglContext == true);

            if (_game.Strategy.GraphicsDevice != null)
            {
                // DeviceReset events
                ((IPlatformGraphicsDevice)_game.Strategy.GraphicsDevice).Strategy.ToConcrete<ConcreteGraphicsDevice>().Android_OnDeviceReset();
            }
        }

        #region Key and Motion

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            bool handled = false;
            if (GamePad.Current.OnKeyDown(keyCode, e))
                return true;

            handled = Keyboard.KeyDown(keyCode);

            // we need to handle the Back key here because it doesn't work any other way
            if (keyCode == Keycode.Back)
            {
                GamePad.Current.Back = true;
                handled = true;
            }

            if (keyCode == Keycode.VolumeUp)
            {
                AudioManager audioManager = (AudioManager)Context.GetSystemService(Context.AudioService);
                audioManager.AdjustStreamVolume(Stream.Music, Adjust.Raise, VolumeNotificationFlags.ShowUi);
                return true;
            }

            if (keyCode == Keycode.VolumeDown)
            {
                AudioManager audioManager = (AudioManager)Context.GetSystemService(Context.AudioService);
                audioManager.AdjustStreamVolume(Stream.Music, Adjust.Lower, VolumeNotificationFlags.ShowUi);
                return true;
            }

            return handled;
        }

        public override bool OnKeyUp(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back)
                GamePad.Current.Back = false;
            if (GamePad.Current.OnKeyUp(keyCode, e))
                return true;
            return Keyboard.KeyUp(keyCode);
        }

        public override bool OnGenericMotionEvent(MotionEvent e)
        {
            if (GamePad.Current.OnGenericMotionEvent(e))
                return true;

            return base.OnGenericMotionEvent(e);
        }

        #endregion

        #region Properties

        private IEGL10 _egl;
        private GLESVersion _glesVersion;
        private EGLConfig _eglConfig;
        private EGLDisplay _eglDisplay;
        private EGLContext _eglContext;
        private EGLSurface _eglSurface;

        #endregion

        #region ISurfaceView
        
        IEGL10 ISurfaceView.Egl { get { return _egl; } }
        GLESVersion ISurfaceView.GLesVersion { get { return _glesVersion; } }
        EGLConfig ISurfaceView.EglConfig { get { return _eglConfig; } }
        EGLDisplay ISurfaceView.EglDisplay { get { return _eglDisplay; } }
        EGLContext ISurfaceView.EglContext { get { return _eglContext; } }
        
        #endregion

    }
}
