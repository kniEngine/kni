// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using Android.Content.PM;

namespace Microsoft.Xna.Platform.Input.Touch
{
    public sealed class ConcreteTouchPanel : TouchPanelStrategy
    {
        internal GameWindow PrimaryWindow;

        public override IntPtr WindowHandle
        {
            get { return base.WindowHandle; }
            set { base.WindowHandle = value; }
        }

        public override int DisplayWidth
        {
            get { return base.DisplayWidth; }
            set { base.DisplayWidth = value; }
        }

        public override int DisplayHeight
        {
            get { return base.DisplayHeight; }
            set { base.DisplayHeight = value; }
        }

        public override DisplayOrientation DisplayOrientation
        {
            get { return base.DisplayOrientation; }
            set { base.DisplayOrientation = value; }
        }

        public override GestureType EnabledGestures
        {
            get { return base.EnabledGestures; }
            set { base.EnabledGestures = value; }
        }


        public override bool IsGestureAvailable
        {
            get { return base.IsGestureAvailable; }
        }

        internal ConcreteTouchPanel()
        {
            // Initialize Capabilities
            // http://developer.android.com/reference/android/content/pm/PackageManager.html#FEATURE_TOUCHSCREEN
            PackageManager pm = AndroidGameWindow.Activity.PackageManager;
            _capabilities._isConnected = pm.HasSystemFeature(PackageManager.FeatureTouchscreen);
            if (pm.HasSystemFeature(PackageManager.FeatureTouchscreenMultitouchJazzhand))
                _capabilities._maximumTouchCount = 5;
            else if (pm.HasSystemFeature(PackageManager.FeatureTouchscreenMultitouchDistinct))
                _capabilities._maximumTouchCount = 2;
            else
                _capabilities._maximumTouchCount = 1;
        }

        public override TouchPanelCapabilities GetCapabilities()
        {
            return _capabilities;
        }

        public override TouchCollection GetState()
        {
            return base.GetState();
        }

        public override GestureSample ReadGesture()
        {
            return base.ReadGesture();
        }

        public override void AddEvent(int id, TouchLocationState state, Vector2 position)
        {
            Point winSize = new Point(this.PrimaryWindow.ClientBounds.Width, this.PrimaryWindow.ClientBounds.Height);

            base.LegacyAddEvent(id, state, position, winSize);
        }

        public override void ReleaseAllTouches()
        {
            base.ReleaseAllTouches();
        }

    }
}