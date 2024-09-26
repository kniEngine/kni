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
    {
        ISurfaceHolder _surfaceHolder;

        internal volatile AndroidGameWindow.AppState _appState = AndroidGameWindow.AppState.Exited;

        internal bool _isAndroidSurfaceAvailable = false;

        internal CancellationRequested _isCancellationRequested = CancellationRequested.Null;



        public AndroidSurfaceView(Context context)
            : base(context)
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
            if (_eglSurface != null)
            {
                var adapter = ((IPlatformGraphicsAdapter)GraphicsAdapter.DefaultAdapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
                var GL = adapter.Ogl;

                // unbind Context and Surface
                if (!GL.Egl.EglMakeCurrent(adapter.EglDisplay, EGL10.EglNoSurface, EGL10.EglNoSurface, EGL10.EglNoContext))
                    Log.Verbose("AndroidGameView", "Could not unbind EGL surface" + GL.GetEglErrorAsString());

                // destroy the old _eglSurface
                GlDestroySurface(adapter);
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
                GlDestroySurface(adapter);
            }

            _isAndroidSurfaceAvailable = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_isCancellationRequested != CancellationRequested.Null)
                {
                    _appState = AndroidGameWindow.AppState.Exited;
                    _isCancellationRequested = CancellationRequested.True;
                }
            }

            base.Dispose(disposing);
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

        private EGLSurface _eglSurface;

        #endregion

        #region ISurfaceView

        EGLSurface ISurfaceView.EglSurface { get { return _eglSurface; } }

        #endregion

        internal void GLCreateSurface(ConcreteGraphicsAdapter adapter, EGLConfig eglConfig)
        {
            System.Diagnostics.Debug.Assert(_eglSurface == null);

            OGL_DROID GL = adapter.Ogl;

            _eglSurface = GL.Egl.EglCreateWindowSurface(adapter.EglDisplay, eglConfig, (Java.Lang.Object)this.Holder, null);
            if (_eglSurface == EGL10.EglNoSurface)
                _eglSurface = null;

            if (_eglSurface == null)
                throw new Exception("Could not create EGL window surface" + GL.GetEglErrorAsString());
        }

        internal void GlDestroySurface(ConcreteGraphicsAdapter adapter)
        {
            System.Diagnostics.Debug.Assert(_eglSurface != null);

            OGL_DROID GL = adapter.Ogl;

            if (!GL.Egl.EglDestroySurface(adapter.EglDisplay, _eglSurface))
                Log.Verbose("AndroidGameView", "Could not destroy EGL surface" + GL.GetEglErrorAsString());
            _eglSurface = null;
        }
    }
}
