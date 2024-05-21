// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Threading;
using Android.Content;
using Android.Media;
using Android.Util;
using Android.Views;
using Javax.Microedition.Khronos.Egl;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Platform;
using Microsoft.Xna.Platform.Graphics;
using Microsoft.Xna.Platform.Graphics.OpenGL;
using Microsoft.Xna.Platform.Input;


namespace Microsoft.Xna.Framework
{
    [CLSCompliant(false)]
    public class AndroidSurfaceView : SurfaceView
        , ISurfaceView
        , ISurfaceHolderCallback
        , Java.Lang.IRunnable
    {
        // What is the state of the app.
        enum AppState
        {
            Paused,
            Resumed,
            Exited,
        }

        ISurfaceHolder _surfaceHolder;

        volatile AppState _appState = AppState.Exited;

        bool _isAndroidSurfaceAvailable = false;

        bool _isGLContextLost;

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
            if (_eglSurface != null)
            {
                var adapter = ((IPlatformGraphicsAdapter)GraphicsAdapter.DefaultAdapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
                var GL = adapter.Ogl;

                // unbind Context and Surface
                if (!GL.Egl.EglMakeCurrent(adapter.EglDisplay, EGL10.EglNoSurface, EGL10.EglNoSurface, EGL10.EglNoContext))
                    Log.Verbose("AndroidGameView", "Could not unbind EGL surface" + GL.GetEglErrorAsString());
                // destroy the old _eglSurface
                if (!GL.Egl.EglDestroySurface(adapter.EglDisplay, _eglSurface))
                    Log.Verbose("AndroidGameView", "Could not destroy EGL surface" + GL.GetEglErrorAsString());
                _eglSurface = null;
            }

        }

        void ISurfaceHolderCallback.SurfaceCreated(ISurfaceHolder holder)
        {
            _isAndroidSurfaceAvailable = true;
        }

        void ISurfaceHolderCallback.SurfaceDestroyed(ISurfaceHolder holder)
        {
            if (_eglSurface != null)
            {
                var adapter = ((IPlatformGraphicsAdapter)GraphicsAdapter.DefaultAdapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
                var GL = adapter.Ogl;

                // unbind Context and Surface
                if (!GL.Egl.EglMakeCurrent(adapter.EglDisplay, EGL10.EglNoSurface, EGL10.EglNoSurface, EGL10.EglNoContext))
                    Log.Verbose("AndroidGameView", "Could not unbind EGL surface" + GL.GetEglErrorAsString());
                // destroy the old _eglSurface
                if (!GL.Egl.EglDestroySurface(adapter.EglDisplay, _eglSurface))
                    Log.Verbose("AndroidGameView", "Could not destroy EGL surface" + GL.GetEglErrorAsString());
                _eglSurface = null;
            }

            _isAndroidSurfaceAvailable = false;
        }

        internal void SwapBuffers()
        {
            var adapter = ((IPlatformGraphicsAdapter)GraphicsAdapter.DefaultAdapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
            var GL = adapter.Ogl;

            if (!GL.Egl.EglSwapBuffers(adapter.EglDisplay, _eglSurface))
            {
                if (GL.Egl.EglGetError() == 0)
                {
                    if (_isGLContextLost)
                        System.Diagnostics.Debug.WriteLine("Lost EGL context" + GL.GetEglErrorAsString());
                    _isGLContextLost = true;
                }
            }
        }

        internal void StartGameLoop()
        {
            _isCancellationRequested = false;

            // prepare gameLoop
            Threading.MakeMainThread();

            Android.OS.Looper looper = Android.OS.Looper.MainLooper;
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
                    if (_appState == AppState.Resumed)
                        RequestFrame();
                }
            }
            else
            {
                _isCancellationRequested = null;

                if (_eglSurface != null)
                {
                    var adapter = ((IPlatformGraphicsAdapter)GraphicsAdapter.DefaultAdapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
                    var GL = adapter.Ogl;

                    if (!GL.Egl.EglMakeCurrent(adapter.EglDisplay, EGL10.EglNoSurface, EGL10.EglNoSurface, EGL10.EglNoContext))
                        Log.Verbose("AndroidGameView", "Could not unbind EGL surface" + GL.GetEglErrorAsString());
                    if (!GL.Egl.EglDestroySurface(adapter.EglDisplay, _eglSurface))
                        Log.Verbose("AndroidGameView", "Could not destroy EGL surface" + GL.GetEglErrorAsString());
                    _eglSurface = null;
                }

                if (_eglContext != null)
                {
                    var adapter = ((IPlatformGraphicsAdapter)GraphicsAdapter.DefaultAdapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
                    var GL = adapter.Ogl;

                    if (_eglContext != null)
                    {
                        if (!GL.Egl.EglDestroyContext(adapter.EglDisplay, _eglContext))
                            throw new Exception("Could not destroy EGL context" + GL.GetEglErrorAsString());
                    }
                    _eglContext = null;

                    if (_game.GraphicsDevice != null)
                    {
                        ((IPlatformGraphicsDevice)_game.GraphicsDevice).Strategy.ToConcrete<ConcreteGraphicsDevice>().Android_OnContextLost();
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
                case AppState.Resumed:
                    ProcessStateResumed();
                    break;

                case AppState.Paused:
                    break;

                case AppState.Exited:
                    _isCancellationRequested = true;
                    break;
                
                default:
                    throw new InvalidOperationException("currentState");
            }

            return;
        }

        void ProcessStateResumed()
        {
            // do not run game if surface is not available
            if (_isAndroidSurfaceAvailable)
            {
                if (_isCancellationRequested.Value == true)
                {
                    _appState = AppState.Exited;
                    return;
                }

                var adapter = ((IPlatformGraphicsAdapter)GraphicsAdapter.DefaultAdapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
                var GL = adapter.Ogl;

                // Restart due to context loss
                bool contextLost = false;
                if (_isGLContextLost)
                {
                    // we actually lost the context so we need to free up our existing 
                    // objects and re-create one.
                    if (_eglContext != null)
                    {
                        if (!GL.Egl.EglDestroyContext(adapter.EglDisplay, _eglContext))
                            throw new Exception("Could not destroy EGL context" + GL.GetEglErrorAsString());
                    }
                    _eglContext = null;

                    if (_game.GraphicsDevice != null)
                    {
                        ((IPlatformGraphicsDevice)_game.GraphicsDevice).Strategy.ToConcrete<ConcreteGraphicsDevice>().Android_OnContextLost();
                    }

                    contextLost = true;
                    _isGLContextLost = false;
                }

                // create context if not available
                if (_eglContext == null)
                {
                    CreateGLContext();

                    if (_eglSurface == null)
                    {
                        _eglSurface = GL.Egl.EglCreateWindowSurface(adapter.EglDisplay, _eglConfig, (Java.Lang.Object)this.Holder, null);
                        if (_eglSurface == EGL10.EglNoSurface) _eglSurface = null;
                        if (_eglSurface == null)
                            throw new Exception("Could not create EGL window surface" + GL.GetEglErrorAsString());

                        if (!GL.Egl.EglMakeCurrent(adapter.EglDisplay, _eglSurface, _eglSurface, _eglContext))
                            throw new Exception("Could not make EGL current" + GL.GetEglErrorAsString());

                        // OGL.InitExtensions() must be called while we have a current gl context.
                        if (OGL_DROID.Current.Extensions == null)
                            OGL_DROID.Current.InitExtensions();

                        GraphicsDeviceManager gdm = ((IPlatformGame)_game).GetStrategy<ConcreteGame>().GraphicsDeviceManager;
                        if (gdm != null)
                        {
                            if (gdm.GraphicsDevice != null)
                            {
                                ConcreteGraphicsDevice gd = (ConcreteGraphicsDevice)((IPlatformGraphicsDevice)gdm.GraphicsDevice).Strategy;
                                gd.Android_UpdateBackBufferBounds(this.Width, this.Height);
                            }
                        }
                        _gameWindow.ChangeClientBounds(new Rectangle(0, 0, this.Width, this.Height));
                    }

                    if (_isGLContextLost)
                    {
                        // we lost the gl context, we need to let the programmer
                        // know so they can re-create textures etc.
                        if (_game.GraphicsDevice != null)
                            ((IPlatformGraphicsDevice)_game.GraphicsDevice).Strategy.ToConcrete<ConcreteGraphicsDevice>().Android_OnDeviceReset();
                    }
                }

                if (_eglSurface == null)
                {
                    _eglSurface = GL.Egl.EglCreateWindowSurface(adapter.EglDisplay, _eglConfig, (Java.Lang.Object)this.Holder, null);
                    if (_eglSurface == EGL10.EglNoSurface) _eglSurface = null;
                    if (_eglSurface == null)
                        throw new Exception("Could not create EGL window surface" + GL.GetEglErrorAsString());

                    if (!GL.Egl.EglMakeCurrent(adapter.EglDisplay, _eglSurface, _eglSurface, _eglContext))
                        throw new Exception("Could not make EGL current" + GL.GetEglErrorAsString());

                    GraphicsDeviceManager gdm = ((IPlatformGame)_game).GetStrategy<ConcreteGame>().GraphicsDeviceManager;
                    if (gdm != null)
                    {
                        if (gdm.GraphicsDevice != null)
                        {
                            ConcreteGraphicsDevice gd = (ConcreteGraphicsDevice)((IPlatformGraphicsDevice)gdm.GraphicsDevice).Strategy;
                            gd.Android_UpdateBackBufferBounds(this.Width, this.Height);

                        }
                    }
                    _gameWindow.ChangeClientBounds(new Rectangle(0, 0, this.Width, this.Height));
                }

                AndroidGameWindow.Activity._orientationListener.Update();

                try
                {
                    var handler = Tick;
                    if (handler != null)
                        handler(this, EventArgs.Empty);
                }
                catch (OpenGLException ex)
                {
                    Log.Error("AndroidGameView", "OpenGL Exception occurred during RunIteration {0}", ex.Message);
                }
            }
        }

        internal void Resume()
        {
            _appState = AppState.Resumed;
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
            _appState = AppState.Paused;
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

        protected void CreateGLContext()
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

            foreach (GLESVersion ver in ((OGL_DROID)OGL.Current).GetSupportedGLESVersions())
            {
                Log.Verbose("AndroidGameView", "Creating GLES {0} Context", ver);

                _eglContext = GL.Egl.EglCreateContext(adapter.EglDisplay, results[0], EGL10.EglNoContext, ver.GetAttributes());

                if (_eglContext == null || _eglContext == EGL10.EglNoContext)
                {
                    _eglContext = null;
                    Log.Verbose("AndroidGameView", string.Format("GLES {0} Not Supported. {1}", ver, GL.GetEglErrorAsString()));
                    continue;
                }
                _glesVersion = ver;
                break;
            }

            if (_eglContext == EGL10.EglNoContext) _eglContext = null;
            if (_eglContext == null)
                throw new Exception("Could not create EGL context" + GL.GetEglErrorAsString());

            Log.Verbose("AndroidGameView", "Created GLES {0} Context", _glesVersion);
            _eglConfig = results[0];
        }

        protected EGLSurface CreatePBufferSurface(EGLConfig config, int[] attribList)
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

        #region Key and Motion

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            bool handled = false;
            if (((IPlatformGamePad)GamePad.Current).GetStrategy<ConcreteGamePad>().OnKeyDown(keyCode, e))
                return true;

            handled = ((IPlatformKeyboard)Keyboard.Current).GetStrategy<ConcreteKeyboard>().KeyDown(keyCode);

            // we need to handle the Back key here because it doesn't work any other way
            if (keyCode == Keycode.Back)
            {
                ((IPlatformGamePad)GamePad.Current).GetStrategy<ConcreteGamePad>().Back = true;
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
                ((IPlatformGamePad)GamePad.Current).GetStrategy<ConcreteGamePad>().Back = false;
            if (((IPlatformGamePad)GamePad.Current).GetStrategy<ConcreteGamePad>().OnKeyUp(keyCode, e))
                return true;
            return ((IPlatformKeyboard)Keyboard.Current).GetStrategy<ConcreteKeyboard>().KeyUp(keyCode);
        }

        public override bool OnGenericMotionEvent(MotionEvent e)
        {
            if (((IPlatformGamePad)GamePad.Current).GetStrategy<ConcreteGamePad>().OnGenericMotionEvent(e))
                return true;

            return base.OnGenericMotionEvent(e);
        }

        #endregion

        #region Properties

        private GLESVersion _glesVersion;
        private EGLConfig _eglConfig;
        private EGLContext _eglContext;
        private EGLSurface _eglSurface;

        #endregion

        #region ISurfaceView
        
        GLESVersion ISurfaceView.GLesVersion { get { return _glesVersion; } }
        EGLConfig ISurfaceView.EglConfig { get { return _eglConfig; } }
        EGLContext ISurfaceView.EglContext { get { return _eglContext; } }
        
        #endregion

    }
}
