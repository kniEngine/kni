// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022-2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Android.Content;
using Android.Media;
using Android.Util;
using Android.Views;
using Javax.Microedition.Khronos.Egl;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Platform;
using Microsoft.Xna.Platform.Input;
using VRCardboard = Com.Google.Vrtoolkit.Cardboard;


namespace Microsoft.Xna.Framework
{
    [CLSCompliant(false)]
    public class AndroidSurfaceView : VRCardboard.CardboardView
        , ISurfaceView
        , VRCardboard.CardboardView.IRenderer
    {

        internal bool _isAndroidSurfaceAvailable = false;

        private readonly AndroidGameWindow _gameWindow;

        public AndroidSurfaceView(Context context, AndroidGameWindow gameWindow)
            : base(context)
        {
            _gameWindow = gameWindow;

            // Holder.SetType is deprecated. The SurfaceType value is ignored.
            if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.Honeycomb)
                Holder.SetType(SurfaceType.Gpu);

            this.Holder.SetFormat(Android.Graphics.Format.Rgba8888);

            ((VRCardboard.CardboardActivity)context).CardboardView = this;
            ((Android.App.Activity)context).SetContentView(this);

            this.SetRenderer(this);
            //gameView.DistortionCorrectionEnabled = false;
            //gameView.SetVRModeEnabled(false);
        }

        public override void SurfaceCreated(ISurfaceHolder holder)
        {
            Log.Debug("AndroidSurfaceView", "SurfaceCreated");

            _isAndroidSurfaceAvailable = true;

            var handler = _surfaceCreatedEvent;
            if (handler != null)
                handler(this, EventArgs.Empty);

            base.SurfaceCreated(holder);
        }

        public override void SurfaceChanged(ISurfaceHolder holder, global::Android.Graphics.Format format, int width, int height)
        {
            Log.Debug("AndroidSurfaceView", "SurfaceChanged: width=" + width + ", height=" + height + ", format=" + format.ToString());

            var handler = _surfaceChangedEvent;
            if (handler != null)
                handler(this, EventArgs.Empty);

            base.SurfaceChanged(holder, format, width, height);
        }

        public override void SurfaceDestroyed(ISurfaceHolder holder)
        {
            Log.Debug("AndroidSurfaceView", "SurfaceDestroyed");

            _isAndroidSurfaceAvailable = false;

            var handler = _surfaceDestroyedEvent;
            if (handler != null)
                handler(this, EventArgs.Empty);

            base.SurfaceDestroyed(holder);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
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

            if ((e.Source & InputSourceType.Mouse) == InputSourceType.Mouse)
            {
                if (((IPlatformMouse)Mouse.Current).GetStrategy<ConcreteMouse>().OnGenericMotionEvent(e))
                    return true;
            }

            return base.OnGenericMotionEvent(e);
        }

        #endregion


        #region ISurfaceView

        private event EventHandler<EventArgs> _surfaceCreatedEvent;
        private event EventHandler<EventArgs> _surfaceChangedEvent;
        private event EventHandler<EventArgs> _surfaceDestroyedEvent;

        event EventHandler<EventArgs> ISurfaceView.SurfaceCreated
        {
            add { _surfaceCreatedEvent += value; }
            remove { _surfaceCreatedEvent -= value; }
        }

        event EventHandler<EventArgs> ISurfaceView.SurfaceChanged
        {
            add { _surfaceChangedEvent += value; }
            remove { _surfaceChangedEvent -= value; }
        }

        event EventHandler<EventArgs> ISurfaceView.SurfaceDestroyed
        {
            add { _surfaceDestroyedEvent += value; }
            remove { _surfaceDestroyedEvent -= value; }
        }

        #endregion

        #region CardboardView.IRenderer

        void IRenderer.OnDrawFrame(VRCardboard.HeadTransform headTransform, VRCardboard.EyeParams eyeParams1, VRCardboard.EyeParams eyeParams2)
        {
            _gameWindow.VrRendererOnDrawFrame(headTransform, eyeParams1, eyeParams2);
        }

        void IRenderer.OnFinishFrame(VRCardboard.Viewport viewport)
        {
            _gameWindow.VrRendererOnFinishFrame(viewport);
        }

        void IRenderer.OnRendererShutdown()
        {
            _gameWindow.VrRendererOnRendererShutdown();
        }

        void IRenderer.OnSurfaceChanged(int width, int height)
        {
            _gameWindow.VrRendererOnSurfaceChanged(width, height);
        }

        void IRenderer.OnSurfaceCreated(EGLConfig config)
        {
            _gameWindow.VrRendererOnSurfaceCreated(config);
        }


        #endregion CardboardView.IRenderer
    }
}
