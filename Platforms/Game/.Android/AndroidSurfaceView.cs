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
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Platform;
using Microsoft.Xna.Platform.Input;


namespace Microsoft.Xna.Framework
{
    [CLSCompliant(false)]
    public class AndroidSurfaceView : SurfaceView
        , ISurfaceView
        , ISurfaceHolderCallback
    {
        ISurfaceHolder _surfaceHolder;

        internal bool _isAndroidSurfaceAvailable = false;


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

        void ISurfaceHolderCallback.SurfaceCreated(ISurfaceHolder holder)
        {
            Log.Debug("AndroidGameView", "SurfaceCreated");

            _isAndroidSurfaceAvailable = true;

            var handler = _surfaceCreatedEvent;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        void ISurfaceHolderCallback.SurfaceChanged(ISurfaceHolder holder, global::Android.Graphics.Format format, int width, int height)
        {
            Log.Debug("AndroidGameView", "SurfaceCreated: width=" + width + ", width=" + height + ", format=" + format.ToString());

            var handler = _surfaceChangedEvent;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }


        void ISurfaceHolderCallback.SurfaceDestroyed(ISurfaceHolder holder)
        {
            Log.Debug("AndroidGameView", "SurfaceDestroyed");

            _isAndroidSurfaceAvailable = false;

            var handler = _surfaceDestroyedEvent;
            if (handler != null)
                handler(this, EventArgs.Empty);
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

    }
}
