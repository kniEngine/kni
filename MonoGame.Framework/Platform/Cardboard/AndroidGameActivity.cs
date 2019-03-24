// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Platform;
using VRCardboard = Com.Google.Vrtoolkit.Cardboard;


namespace Microsoft.Xna.Framework
{
    [CLSCompliant(false)]
    public class AndroidGameActivity : VRCardboard.CardboardActivity
    {
        internal Game Game { private get; set; }

        private ScreenReceiver screenReceiver;
        internal OrientationListener _orientationListener;

        public bool AutoPauseAndResumeMediaPlayer = true;


        internal event EventHandler WindowFocused;
        internal event EventHandler WindowUnfocused;

        /// <summary>
        /// OnCreate called when the activity is launched from cold or after the app
        /// has been killed due to a higher priority app needing the memory
        /// </summary>
        /// <param name='savedInstanceState'>
        /// Saved instance state.
        /// </param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            // Detection of NaturalOrientation. This must happend as soon as possible at start up.
            AndroidCompatibility.Initialize(this);
            
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.P)
            {
                // Enable drawing over the camera cutoff. Cardboard renderer requires the entire screen.
                // fixes: [CardboardView] Surface size 2340x1036 does not match the expected screen size 2340x1080. Rendering is disabled.
                Window.Attributes.LayoutInDisplayCutoutMode |= Android.Views.LayoutInDisplayCutoutMode.ShortEdges;
            }
            
            RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(savedInstanceState);

            IntentFilter filter = new IntentFilter();
            filter.AddAction(Intent.ActionScreenOff);
            filter.AddAction(Intent.ActionScreenOn);
            filter.AddAction(Intent.ActionUserPresent);
            filter.AddAction(Android.Telephony.TelephonyManager.ActionPhoneStateChanged);
            
            screenReceiver = new ScreenReceiver();
            RegisterReceiver(screenReceiver, filter);

            _orientationListener = new OrientationListener(this);

            AndroidGameWindow.Activity = this;
        }

        private bool _isActivityActive = false;
        internal bool IsActivityActive { get { return _isActivityActive; } }

        public event EventHandler Paused;
        public event EventHandler Resumed;

        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            // we need to refresh the viewport here.
            base.OnConfigurationChanged(newConfig);
        }

        protected override void OnPause()
        {
            base.OnPause();

            if (_isActivityActive)
            {
                _isActivityActive = false;

                var handler = Paused;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }

            if (_orientationListener.CanDetectOrientation())
                _orientationListener.Disable();
        }

        protected override void OnResume()
        {
            base.OnResume();

            if (!_isActivityActive)
            {
                _isActivityActive = true;

                var handler = Resumed;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }

            if (Game != null)
            {
                if (_orientationListener.CanDetectOrientation())
                    _orientationListener.Enable();
            }
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);

            var handler = (hasFocus) ? WindowFocused : WindowUnfocused;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        protected override void OnDestroy()
        {
            UnregisterReceiver(screenReceiver);
            ScreenReceiver.ScreenLocked = false;
            _orientationListener = null;

            if (Game != null)
            {
                Game.Dispose();
                Game = null;
            }

            base.OnDestroy();
        }
    }

    [CLSCompliant(false)]
    public static class ActivityExtensions
    {
        public static ActivityAttribute GetActivityAttribute(this AndroidGameActivity obj)
        {			
            var attr = obj.GetType().GetCustomAttributes(typeof(ActivityAttribute), true);
            if (attr != null)
            {
                return ((ActivityAttribute)attr[0]);
            }
            return null;
        }
    }

}
