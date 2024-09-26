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
        internal ScreenReceiver _screenReceiver;

        public bool AutoPauseAndResumeMediaPlayer = true;


        internal event EventHandler WindowFocused;
        internal event EventHandler WindowUnfocused;

        public event EventHandler Paused;
        public event EventHandler Resumed;
        internal event EventHandler Destroyed;

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
            
            _screenReceiver = new ScreenReceiver(this);
            RegisterReceiver(_screenReceiver, filter);

            AndroidGameWindow.Activity = this;
        }

        private bool _isActivityActive = false;
        internal bool IsActivityActive { get { return _isActivityActive; } }


        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            // we need to refresh the viewport here.
            base.OnConfigurationChanged(newConfig);
        }

        protected override void OnPause()
        {
            base.OnPause();

            _isActivityActive = false;

            var handler = Paused;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        protected override void OnResume()
        {
            base.OnResume();

            _isActivityActive = true;

            var handler = Resumed;
            if (handler != null)
                handler(this, EventArgs.Empty);
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
            UnregisterReceiver(_screenReceiver);
            _screenReceiver.IsScreenLocked = false;

            var handler = Destroyed;
            if (handler != null)
                handler(this, EventArgs.Empty);

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
