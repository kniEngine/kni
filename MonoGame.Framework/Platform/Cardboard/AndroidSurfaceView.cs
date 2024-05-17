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
using Microsoft.Xna.Platform.Input;
using VRCardboard = Com.Google.Vrtoolkit.Cardboard;


namespace Microsoft.Xna.Framework
{
    [CLSCompliant(false)]
    public class AndroidSurfaceView : VRCardboard.CardboardView
        , ISurfaceView
        , VRCardboard.CardboardView.IRenderer
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


        volatile AppState _appState = AppState.Exited;

        volatile bool _isAndroidSurfaceChanged = false;
        bool _isAndroidSurfaceAvailable = false;

        bool _isGLContextLost;

        bool? _isCancellationRequested = null;

        private readonly AndroidGameWindow _gameWindow;
        private readonly Game _game;

        internal event EventHandler Tick;


        public AndroidSurfaceView(Context context, AndroidGameWindow gameWindow, Game game)
            : base(context)
        {
            _gameWindow = gameWindow;
            _game = game;

            // Holder.SetType is deprecated. The SurfaceType value is ignored.
            if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.Honeycomb)
                Holder.SetType(SurfaceType.Gpu);

            Input.Cardboard.Headset.View = this;

            this.Holder.SetFormat(Android.Graphics.Format.Rgba8888);

            ((VRCardboard.CardboardActivity)context).CardboardView = this;
            ((Android.App.Activity)context).SetContentView(this);

            this.SetRenderer(this);
            //gameView.DistortionCorrectionEnabled = false;
            //gameView.SetVRModeEnabled(false);
        }

        public override void SurfaceChanged(ISurfaceHolder holder, global::Android.Graphics.Format format, int width, int height)
        {
            // Set flag to recreate gl surface or rendering can be bad on orientation change or if app 
            // is closed in one orientation and re-opened in another.

            // can only be triggered when main loop is running, is unsafe to overwrite other states
            if (_appState == AppState.Running)
                _isAndroidSurfaceChanged = true;

            base.SurfaceChanged(holder, format, width, height);
        }

        public override void SurfaceCreated(ISurfaceHolder holder)
        {
            _isAndroidSurfaceAvailable = true;

            base.SurfaceCreated(holder);
        }

        public override void SurfaceDestroyed(ISurfaceHolder holder)
        {
            _isAndroidSurfaceAvailable = false;

            base.SurfaceDestroyed(holder);
        }

        internal void SwapBuffers()
        {
            // Surface is presented by GLSurfaceView. 
            // see: OnFinishFrame.
        }

        internal void MakeCurrentContext()
        {
            // Surface & GL Context was created by GLSurfaceView.
            return;

            if (!_egl.EglMakeCurrent(_eglDisplay, _eglSurface, _eglSurface, _eglContext))
                System.Diagnostics.Debug.WriteLine("Error Make Current" + GetErrorAsString());
        }

        internal void StartGameLoop()
        {
            // Cardboard: 
            _isStarted = true;
            return;
            
            _isCancellationRequested = false;

            // prepare gameLoop
            Threading.MakeMainThread();
        }

        volatile bool _isStarted = false;

        private void RunOnDrawFrame()
        {
            if (_isCancellationRequested == null)
                _isCancellationRequested = false;

            if (_isCancellationRequested.Value == false)
            {
                try { RunStep(); } // tick
                catch (Exception ex) { /* ignore */ }
            }
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
            }

            return;
        }

        void processStateResuming()
        {
            // this can happen if pause is triggered immediately after resume so that SurfaceCreated callback doesn't get called yet,
            // in this case we skip the resume process and pause sets a new state.
            if (_isAndroidSurfaceAvailable)
            {
                // create surface if context is available
                if (_eglContext != null && !_isGLContextLost)
                {
                    if (_eglSurface == null)
                        CreateGLSurface();
                }

                // create context if not available
                if (_eglContext == null || _isGLContextLost)
                {
                    // Start or Restart due to context loss
                    bool contextLost = false;
                    if (_isGLContextLost)
                    {
                        // we actually lost the context
                        // so we need to free up our existing 
                        // objects and re-create one.
                        if (_eglContext != null)
                        {
                            if (!_egl.EglDestroyContext(_eglDisplay, _eglContext))
                                throw new Exception("Could not destroy EGL context" + GetErrorAsString());
                        }
                        _eglContext = null;
                        if (_eglDisplay != null)
                        {
                            if (!_egl.EglTerminate(_eglDisplay))
                                throw new Exception("Could not terminate EGL connection" + GetErrorAsString());
                        }
                        _eglDisplay = null;

                        contextLost = true;

                        if (_game.GraphicsDevice != null)
                        {
                            ((IPlatformGraphicsDevice)_game.GraphicsDevice).Strategy.ToConcrete<ConcreteGraphicsDevice>().Android_OnContextLost();
                        }
                    }

                    if (OGL_DROID.Current == null)
                        OGL_DROID.Initialize();

                    //CreateGLContext();

                    if (_eglSurface == null)
                        CreateGLSurface();

                    // OGL.InitExtensions() must be called while we have a gl context.
                    if (OGL_DROID.Current.Extensions == null)
                        OGL_DROID.Current.InitExtensions();

                    if (_eglContext != null && contextLost)
                    {
                        // we lost the gl context, we need to let the programmer
                        // know so they can re-create textures etc.
                        if (_isGLContextLost)
                        {
                            if (_game.GraphicsDevice != null)
                                ((IPlatformGraphicsDevice)_game.GraphicsDevice).Strategy.ToConcrete<ConcreteGraphicsDevice>().Android_OnDeviceReset();
                        }
                    }

                    return;
                }

                // finish state if surface created, may take a frame or two until the android UI thread callbacks fire
                if (_eglSurface != null)
                {
                    // go to next state
                    _appState = AppState.Running;
                    _isAndroidSurfaceChanged = false;
                }
            }
        }

        void processStateRunning()
        {
            if (_isAndroidSurfaceChanged)
            {
                // needed at app start
                if (_eglContext != null && _isAndroidSurfaceAvailable)
                {
                    // unbind Context and Surface
                    if (!_egl.EglMakeCurrent(_eglDisplay, EGL10.EglNoSurface, EGL10.EglNoSurface, EGL10.EglNoContext))
                        Log.Verbose("AndroidGameView", "Could not unbind EGL surface" + GetErrorAsString());
                    // destroy the old _eglSurface
                    if (!_egl.EglDestroySurface(_eglDisplay, _eglSurface))
                        Log.Verbose("AndroidGameView", "Could not destroy EGL surface" + GetErrorAsString());
                    _eglSurface = null;

                    CreateGLSurface();

                    // go to next state
                    _isAndroidSurfaceChanged = false;
                }

                return;
            }

            // do not run game if surface is not available
            if (_isAndroidSurfaceAvailable)
            {
                // check if app wants to exit
                if (_isCancellationRequested.Value == true)
                {
                    // change state to exit and skip game loop
                    _appState = AppState.Exited;
                    return;
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

        void processStatePausing()
        {
            // Surface we are using needs to go away
            if (_eglSurface != null)
            {
                if (!_egl.EglMakeCurrent(_eglDisplay, EGL10.EglNoSurface, EGL10.EglNoSurface, EGL10.EglNoContext))
                    Log.Verbose("AndroidGameView", "Could not unbind EGL surface" + GetErrorAsString());
                if (!_egl.EglDestroySurface(_eglDisplay, _eglSurface))
                    Log.Verbose("AndroidGameView", "Could not destroy EGL surface" + GetErrorAsString());
                _eglSurface = null;
            }


            // go to next state
            _appState = AppState.Paused;
        }

        internal void Resume()
        {
            _appState = AppState.Resuming;

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
                if (_isAndroidSurfaceChanged == false)
                    return;
            }

            if (_isAndroidSurfaceAvailable)
            {
                // processing the pausing state only if the surface was created already
                _appState = AppState.Pausing;
            }
            else
            {
                // happens if pause is called immediately after resume so that the surfaceCreated callback was not called yet.
                _appState = AppState.Paused; // prepare for next game loop iteration
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
            try
            {
                /* Cardboard: Surface was created by GLSurfaceView.
                _eglSurface = _egl.EglCreateWindowSurface(_eglDisplay, _eglConfig, (Java.Lang.Object)this.Holder, null);
                
                if (_eglSurface == EGL10.EglNoSurface)
                    _eglSurface = null;

                if (_eglSurface == null)
                    throw new Exception("Could not create EGL window surface" + GetErrorAsString());

                if (!_egl.EglMakeCurrent(_eglDisplay, _eglSurface, _eglSurface, _eglContext))
                    throw new Exception("Could not make EGL current" + GetErrorAsString());
                */

                // Must set viewport after creation, the viewport has correct values in it already as we call it, but
                // the surface is created after the correct viewport is already applied so we must do it again.
                GraphicsDeviceManager gdm = ((IPlatformGame)_game).GetStrategy<ConcreteGame>().GraphicsDeviceManager;
                if (gdm != null)
                {
                    if (gdm.GraphicsDevice != null)
                    {
                        ((IPlatformGraphicsDeviceManager)gdm).GetStrategy<Platform.ConcreteGraphicsDeviceManager>().InternalResetClientBounds();
                    }
                }
            }
            catch (Exception ex)
            {
                _eglSurface = null;
                Log.Error("AndroidGameView", ex.ToString());
            }
        }

        protected EGLSurface CreatePBufferSurface(EGLConfig config, int[] attribList)
        {
            EGLSurface result = _egl.EglCreatePbufferSurface(_eglDisplay, config, attribList);

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


        #region CardboardView.IRenderer

        internal int _glFramebuffer; // the current frame buffer
        int[] _parameterFramebufferBinding = new int[3];


        private void GetRenderBufferSize(out int w, out int h)
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

        void IRenderer.OnDrawFrame(VRCardboard.HeadTransform headTransform, VRCardboard.EyeParams eyeParams1, VRCardboard.EyeParams eyeParams2)
        {
            if (_eglContext == null)
            {
                if (OGL_DROID.Current == null)
                    OGL_DROID.Initialize();
                if (OGL_DROID.Current.Extensions == null)
                    OGL_DROID.Current.InitExtensions();

                _isGLContextLost = false;
                _egl = EGLContext.EGL.JavaCast<IEGL10>();
                _eglDisplay = _egl.EglGetCurrentDisplay();
                _eglContext = _egl.EglGetCurrentContext();
                if (_eglContext == EGL10.EglNoContext)
                    _eglContext = null;
            }

            if (!_isStarted)
                return;
            

            UpdateLocalHeadsetState(headTransform, eyeParams1, eyeParams2);

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
                //GetRenderBufferSize(out w, out h);
                //_game.GraphicsDevice.Viewport = new Viewport(0,0,w,h);
            }

            RunOnDrawFrame();

            Android.Opengl.GLES20.GlGetIntegerv(Android.Opengl.GLES20.GlFramebufferBinding, _parameterFramebufferBinding, 1);
            System.Diagnostics.Debug.Assert(_glFramebuffer == _parameterFramebufferBinding[1],
                "framebuffer must be restored back to the one set at the beggining of CardboardView.IRenderer.OnDrawFrame()");
        }

        void IRenderer.OnFinishFrame(VRCardboard.Viewport viewport)
        {
            Android.Opengl.GLES20.GlGetIntegerv(Android.Opengl.GLES20.GlFramebufferBinding, _parameterFramebufferBinding, 2);
            _glFramebuffer = _parameterFramebufferBinding[2];
        }

        void IRenderer.OnRendererShutdown()
        {
            
        }

        void IRenderer.OnSurfaceChanged(int width, int height)
        {
            
        }

        void IRenderer.OnSurfaceCreated(EGLConfig config)
        {
            _eglConfig = config;
        }


        Input.Cardboard.HeadsetState _hsState;
        internal void UpdateLocalHeadsetState(VRCardboard.HeadTransform headTransform, VRCardboard.EyeParams eyeParams1, VRCardboard.EyeParams eyeParams2)
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
        
        internal void UpdateHeadsetState(out Input.Cardboard.HeadsetState state)
        {
            state = _hsState;
        }
        

        #endregion CardboardView.IRenderer
    }
}
