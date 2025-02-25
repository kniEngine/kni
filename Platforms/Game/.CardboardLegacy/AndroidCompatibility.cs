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
            // check if MainActivity setup is correct. 
            Android.Content.PM.ScreenOrientation screenOrientation = activity.RequestedOrientation;
            if (screenOrientation != Android.Content.PM.ScreenOrientation.Landscape)
                throw new InvalidOperationException("NaturalOrientation detection failed. Set ScreenOrientation in MainActivity to Landscape.");

            Orientation orientation = activity.Resources.Configuration.Orientation;
            SurfaceOrientation rotation = activity.WindowManager.DefaultDisplay.Rotation;

            if (((rotation == SurfaceOrientation.Rotation0  || rotation == SurfaceOrientation.Rotation180) && orientation == Orientation.Landscape)
            ||  ((rotation == SurfaceOrientation.Rotation90 || rotation == SurfaceOrientation.Rotation270) && orientation == Orientation.Portrait))
            {
                return Orientation.Landscape;
            }
            else
            {
                return Orientation.Portrait;
            }
        }

            }
        }

        /// <summary>
        /// Get the absolute orientation of the device, accounting for platform differences.
        /// </summary>
        /// <returns></returns>
        [CLSCompliant(false)]
        public DisplayOrientation GetAbsoluteOrientation(Activity activity)
        {
            Orientation orientation = activity.Resources.Configuration.Orientation;
            SurfaceOrientation rotation = activity.WindowManager.DefaultDisplay.Rotation;

            switch (orientation)
            {
                case Orientation.Portrait:
                    {
                        switch (rotation)
                        {
                            case SurfaceOrientation.Rotation0:
                            default:
                                System.Diagnostics.Debug.Assert(AndroidCompatibility.Current.NaturalOrientation != Orientation.Landscape);
                                return DisplayOrientation.Portrait;
                            case SurfaceOrientation.Rotation180:
                                System.Diagnostics.Debug.Assert(AndroidCompatibility.Current.NaturalOrientation != Orientation.Landscape);
                                return DisplayOrientation.PortraitDown;
                            case SurfaceOrientation.Rotation90:
                                System.Diagnostics.Debug.Assert(AndroidCompatibility.Current.NaturalOrientation == Orientation.Landscape);
                                return DisplayOrientation.PortraitDown;
                            case SurfaceOrientation.Rotation270:
                                System.Diagnostics.Debug.Assert(AndroidCompatibility.Current.NaturalOrientation == Orientation.Landscape);
                                return DisplayOrientation.Portrait;
                        }
                    }
                    break;

                case Orientation.Landscape:
                default:
                    {
                        switch (rotation)
                        {
                            case SurfaceOrientation.Rotation0:
                            default:
                                System.Diagnostics.Debug.Assert(AndroidCompatibility.Current.NaturalOrientation == Orientation.Landscape);
                                if (AndroidCompatibility.Current.FlipLandscape)
                                    return DisplayOrientation.LandscapeRight;
                                return DisplayOrientation.LandscapeLeft;
                            case SurfaceOrientation.Rotation180:
                                System.Diagnostics.Debug.Assert(AndroidCompatibility.Current.NaturalOrientation == Orientation.Landscape);
                                if (AndroidCompatibility.Current.FlipLandscape)
                                    return DisplayOrientation.LandscapeLeft;
                                return DisplayOrientation.LandscapeRight;
                            case SurfaceOrientation.Rotation90:
                                System.Diagnostics.Debug.Assert(AndroidCompatibility.Current.NaturalOrientation != Orientation.Landscape);
                                if (AndroidCompatibility.Current.FlipLandscape)
                                    return DisplayOrientation.LandscapeRight;
                                return DisplayOrientation.LandscapeLeft;
                            case SurfaceOrientation.Rotation270:
                                System.Diagnostics.Debug.Assert(AndroidCompatibility.Current.NaturalOrientation != Orientation.Landscape);
                                if (AndroidCompatibility.Current.FlipLandscape)
                                    return DisplayOrientation.LandscapeLeft;
                                return DisplayOrientation.LandscapeRight;
                        }
                    }
                    break;
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
            switch (degrees)
            {
                case 0:
                    return DisplayOrientation.Portrait;
                case 180:
                    return DisplayOrientation.PortraitDown;
                case 90:
                    if (FlipLandscape)
                        return DisplayOrientation.LandscapeLeft;
                    return DisplayOrientation.LandscapeRight;
                case 270:
                    if (FlipLandscape)
                        return DisplayOrientation.LandscapeRight;
                    return DisplayOrientation.LandscapeLeft;

                default:
                    return DisplayOrientation.Unknown;
            }
        }
    }
}
