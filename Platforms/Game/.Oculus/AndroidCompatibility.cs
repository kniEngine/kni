using System;
using System.Linq;
using Android.App;
using Android.Content.Res;
using Android.OS;
using Android.Views;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Properties that change from how XNA works by default
    /// </summary>
    public class AndroidCompatibility
    {
        private static AndroidCompatibility _current;

        /// <summary>
        /// Because the Kindle Fire devices default orientation is fliped by 180 degrees from all the other android devices
        /// on the market we need to do some special processing to make sure that LandscapeLeft is the correct way round.
        /// This list contains all the Build.Model strings of the effected devices, it should be added to if and when
        /// more devices exhibit the same issues.
        /// </summary>
        private readonly string[] Kindles = new[] { "KFTT", "KFJWI", "KFJWA", "KFSOWI", "KFTHWA", "KFTHWI", "KFAPWA", "KFAPWI" };

        public bool FlipLandscape { get; private set; }

        [CLSCompliant(false)]
        public Orientation NaturalOrientation { get; private set; }


        [CLSCompliant(false)]
        public static AndroidCompatibility Current
        {
            get
            {
                if (_current != null)
                    return _current;
                else
                    throw new InvalidOperationException("Not initialized.");
            }
        }

        internal static void Initialize(Activity activity)
        {
            if (_current == null)
                _current = new AndroidCompatibility(activity);
        }

        private AndroidCompatibility(Activity activity)
        {
            FlipLandscape = Kindles.Contains(Build.Model);
            NaturalOrientation = GetDeviceNaturalOrientation(activity);
        }

        private Orientation GetDeviceNaturalOrientation(Activity activity)
        {
            var orientation = activity.Resources.Configuration.Orientation;
            SurfaceOrientation rotation = activity.WindowManager.DefaultDisplay.Rotation;

            // check if MainActivity setup is correct. 
            var screenOrientation = activity.RequestedOrientation;
            if (screenOrientation != Android.Content.PM.ScreenOrientation.Landscape)
                throw new InvalidOperationException("Invalid orientation. Set ScreenOrientation in MainActivity to Landscape.");

            if (((rotation == SurfaceOrientation.Rotation0 || rotation == SurfaceOrientation.Rotation180) &&
                orientation == Orientation.Landscape)
                || ((rotation == SurfaceOrientation.Rotation90 || rotation == SurfaceOrientation.Rotation270) &&
                orientation == Orientation.Portrait))
            {
                return Orientation.Landscape;
            }
            else
            {
                return Orientation.Portrait;
            }
        }

        internal DisplayOrientation GetAbsoluteOrientation(int degrees)
        {
            // Orientation is reported by the device in degrees compared to the natural orientation
            // Some tablets have a natural landscape orientation, which we need to account for
            if (NaturalOrientation == Orientation.Landscape)
                degrees += 270;

            // Round orientation into one of 8 positions, either 0, 45, 90, 135, 180, 225, 270, 315. 
            degrees = ( ((degrees + 22) / 45) * 45) % 360;

            // Surprisingly 90 degree is landscape right, except on Kindle devices
            var disporientation = DisplayOrientation.Unknown;
            switch (degrees)
            {
                case 90: disporientation = FlipLandscape ? DisplayOrientation.LandscapeLeft : DisplayOrientation.LandscapeRight;
                    break;
                case 270: disporientation = FlipLandscape ? DisplayOrientation.LandscapeRight : DisplayOrientation.LandscapeLeft;
                    break;
                case 0: disporientation = DisplayOrientation.Portrait;
                    break;
                case 180: disporientation = DisplayOrientation.PortraitDown;
                    break;
                default:
                    disporientation = DisplayOrientation.Unknown;
                    break;
            }

            return disporientation;
        }

        /// <summary>
        /// Get the absolute orientation of the device, accounting for platform differences.
        /// </summary>
        /// <returns></returns>
        [CLSCompliant(false)]
        public DisplayOrientation GetAbsoluteOrientation(Activity activity)
        {
            var orientation = activity.WindowManager.DefaultDisplay.Rotation;

            // Landscape degrees (provided by the OrientationListener) are swapped by default
            // Since we use the code used by OrientationListener, we have to swap manually
            int degrees;
            switch (orientation)
            {
                case SurfaceOrientation.Rotation90:
                    degrees = 270;
                    break;
                case SurfaceOrientation.Rotation180:
                    degrees = 180;
                    break;
                case SurfaceOrientation.Rotation270:
                    degrees = 90;
                    break;
                default:
                    degrees = 0;
                    break;
            }

            return GetAbsoluteOrientation(degrees);
        }
    }
}
