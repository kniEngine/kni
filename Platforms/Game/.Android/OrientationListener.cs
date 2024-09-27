// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Platform;
using Android.App;
using Android.Content;
using Android.Hardware;
using Android.Views;
using Android.Provider;


namespace Microsoft.Xna.Framework
{
    internal class OrientationListener : OrientationEventListener
    {
        private AndroidGameWindow _gameWindow;

        internal DisplayOrientation targetOrientation = DisplayOrientation.Unknown;
        DateTime prevTickTime = DateTime.Now;
        TimeSpan elapsed = TimeSpan.Zero;

        /// <summary>
        /// Constructor. SensorDelay.Ui is passed to the base class as this orientation listener 
        /// is just used for flipping the screen orientation, therefore high frequency data is not required.
        /// </summary>
        public OrientationListener(AndroidGameWindow gameWindow, Context context)
            : base(context, SensorDelay.Ui)
        {
            this._gameWindow = gameWindow;
        }

        public override void OnOrientationChanged(int orientation)
        {
            if (orientation == OrientationEventListener.OrientationUnknown)
            {                
                targetOrientation = DisplayOrientation.Unknown;
                elapsed = TimeSpan.Zero;
                return;
            }

            // Avoid changing orientation whilst the screen is locked
            if (_gameWindow._screenReceiver.IsScreenLocked)
                return;

            DisplayOrientation disporientation = AndroidCompatibility.Current.GetAbsoluteOrientation(orientation);

            if ((_gameWindow.GetEffectiveSupportedOrientations() & disporientation) == 0
            ||  disporientation == _gameWindow.CurrentOrientation
            ||  disporientation == DisplayOrientation.Unknown
               )
            {
                targetOrientation = DisplayOrientation.Unknown;
                elapsed = TimeSpan.Zero;
                return;
            }

            // Delay changing of Orientation. Filter random shocks.
            if (targetOrientation != disporientation)
            {
                targetOrientation = disporientation;
                elapsed = TimeSpan.Zero;
            }

            return;
        }
        
        internal void Update()
        {
            DateTime currTickTime = DateTime.Now;
            TimeSpan elapsedTime = TimeSpan.Zero;
            if (prevTickTime.Ticks != 0)
            {
                elapsedTime = (currTickTime - prevTickTime);
                if (elapsedTime.TotalMilliseconds < 0)
                    elapsedTime = TimeSpan.Zero;
            }
            prevTickTime = currTickTime;

            try
            {
                if (targetOrientation != DisplayOrientation.Unknown)
                {
                    elapsed += elapsedTime;
                    // orientation must be stable for 0.5 seconds before changing.
                    if (elapsed.TotalSeconds > 0.5)
                    {
                        _gameWindow.SetOrientation(targetOrientation, true);
                        targetOrientation = DisplayOrientation.Unknown;
                        elapsed = TimeSpan.Zero;
                    }
                }
            }
            catch (Exception) { /* ignore */ }
        }
    }
}
