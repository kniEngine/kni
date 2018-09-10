// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Android.App;
using Android.Content;
using Android.Hardware;
using Android.Views;
using Android.Provider;

namespace Microsoft.Xna.Framework
{
    internal class OrientationListener : OrientationEventListener
    {
        internal DisplayOrientation targetOrientation = DisplayOrientation.Unknown;
        double elapsed = 0;

        /// <summary>
        /// Constructor. SensorDelay.Ui is passed to the base class as this orientation listener 
        /// is just used for flipping the screen orientation, therefore high frequency data is not required.
        /// </summary>
        public OrientationListener(Context context)
            : base(context, SensorDelay.Ui)
        {
        }

        public override void OnOrientationChanged(int orientation)
        {
            if (orientation == OrientationEventListener.OrientationUnknown)
            {                
                targetOrientation = DisplayOrientation.Unknown;
                elapsed = 0;
                return;
            }

            // Avoid changing orientation whilst the screen is locked
            if (ScreenReceiver.ScreenLocked)
                return;

            var disporientation = AndroidCompatibility.Current.GetAbsoluteOrientation(orientation);

            AndroidGameWindow gameWindow = (AndroidGameWindow)Game.Instance.Window;
            if ((gameWindow.GetEffectiveSupportedOrientations() & disporientation) == 0 ||
                disporientation == gameWindow.CurrentOrientation ||
                disporientation == DisplayOrientation.Unknown)
            {
                targetOrientation = DisplayOrientation.Unknown;
                elapsed = 0;
                return;
            }

            // Delay changing of Orientation. Filter random shocks.
            if (targetOrientation != disporientation)
            {
                targetOrientation = disporientation;
                elapsed = 0;
            }

            return;
        }
        
        internal void Update(MonoGameAndroidGameView.FrameEventArgs updateEventArgs)
        {            
            if (targetOrientation != DisplayOrientation.Unknown)
            {
                elapsed += updateEventArgs.Time;
                // orientation must be stable for 0.5 seconds before changing.
                if (elapsed > (1000 * 0.5))
                {
                    AndroidGameWindow gameWindow = (AndroidGameWindow)Game.Instance.Window;
                    gameWindow.SetOrientation(targetOrientation, true);
                    targetOrientation = DisplayOrientation.Unknown;
                    elapsed = 0;
                }
            }
        }
    }
}