#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009-2012 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software,
you accept this license. If you do not accept the license, do not use the
software.

1. Definitions

The terms "reproduce," "reproduction," "derivative works," and "distribution"
have the same meaning here as under U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the
software.

A "contributor" is any person that distributes its contribution under this
license.

"Licensed patents" are a contributor's patent claims that read directly on its
contribution.

2. Grant of Rights

(A) Copyright Grant- Subject to the terms of this license, including the
license conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free copyright license to reproduce its
contribution, prepare derivative works of its contribution, and distribute its
contribution or any derivative works that you create.

(B) Patent Grant- Subject to the terms of this license, including the license
conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free license under its licensed patents to
make, have made, use, sell, offer for sale, import, and/or otherwise dispose of
its contribution in the software or derivative works of the contribution in the
software.

3. Conditions and Limitations

(A) No Trademark License- This license does not grant you rights to use any
contributors' name, logo, or trademarks.

(B) If you bring a patent claim against any contributor over patents that you
claim are infringed by the software, your patent license from such contributor
to the software ends automatically.

(C) If you distribute any portion of the software, you must retain all
copyright, patent, trademark, and attribution notices that are present in the
software.

(D) If you distribute any portion of the software in source code form, you may
do so only under this license by including a complete copy of this license with
your distribution. If you distribute any portion of the software in compiled or
object code form, you may only do so under a license that complies with this
license.

(E) The software is licensed "as-is." You bear the risk of using it. The
contributors give no express warranties, guarantees or conditions. You may have
additional consumer rights under your local laws which this license cannot
change. To the extent permitted under your local laws, the contributors exclude
the implied warranties of merchantability, fitness for a particular purpose and
non-infringement.
*/
#endregion License

using System;
using System.Collections.Generic;
using Microsoft.Xna.Platform;

using Foundation;
using UIKit;

namespace Microsoft.Xna.Framework
{
    class iOSGameWindow : GameWindow, IDisposable
    {
        private static Dictionary<IntPtr, iOSGameWindow> _instances = new Dictionary<IntPtr, iOSGameWindow>();

        internal static iOSGameWindow FromHandle(IntPtr windowHandle)
        {
            return _instances[windowHandle];
        }

        private iOSGameViewController _viewController;
        private UIWindow _uiWindow;

        internal iOSGameViewController ViewController { get { return _viewController; } }
        internal UIWindow UIWindow { get { return _uiWindow; } }

        private NSObject DidBecomeActiveHolder;
        private NSObject WillResignActiveHolder;


        public iOSGameWindow(ConcreteGame concreteGame)
        {
#if !TVOS
            UIApplication.SharedApplication.SetStatusBarHidden(true, UIStatusBarAnimation.Fade);
#endif

            // Create a full-screen window
            _uiWindow = new UIWindow(UIScreen.MainScreen.Bounds);
            //_uiKitWindow.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
            concreteGame.Services.AddService(typeof(UIWindow), _uiWindow);

            _viewController = new iOSGameViewController(concreteGame);
            concreteGame.Services.AddService(typeof(UIViewController), _viewController);

            _viewController.InterfaceOrientationChanged += HandleInterfaceOrientationChanged;

            _instances.Add(this.Handle, this);
        }

        ~iOSGameWindow()
        {
            Dispose(false);
        }

        void HandleInterfaceOrientationChanged(object sender, EventArgs e)
        {
            OnOrientationChanged();
        }

        #region GameWindow Members

        public override bool AllowUserResizing
        {
            get { return false; }
            set { /* Ignore */ }
        }

        public override Rectangle ClientBounds
        {
            get
            {
                var bounds = _viewController.View.Bounds;
                var scale = _viewController.View.ContentScaleFactor;

                // TODO: Calculate this only when dirty.
                if (_viewController is iOSGameViewController)
                {

                    var currentOrientation = CurrentOrientation;

                    int width;
                    int height;

                    if (currentOrientation == DisplayOrientation.LandscapeLeft || 
                        currentOrientation == DisplayOrientation.LandscapeRight)
                    {
                        width = (int)Math.Max(bounds.Width, bounds.Height);
                        height = (int)Math.Min(bounds.Width, bounds.Height);

                    }
                    else
                    {
                        width = (int)Math.Min(bounds.Width, bounds.Height);
                        height = (int)Math.Max(bounds.Width, bounds.Height);
                    }

                    width *= (int)scale;
                    height *= (int)scale;

                    return new Rectangle( (int)(bounds.X * scale), (int)(bounds.Y * scale), width, height);
                }

                return new Rectangle(
                    (int)(bounds.X * scale), (int)(bounds.Y * scale),
                    (int)(bounds.Width * scale), (int)(bounds.Height * scale));
            }
        }

        public override DisplayOrientation CurrentOrientation
        {
            get
            {
                #if TVOS
                return DisplayOrientation.LandscapeLeft;
                #else
                return OrientationConverter.ToDisplayOrientation(_viewController.InterfaceOrientation);
                #endif
            }
        }

        public override IntPtr Handle
        {
            get 
            {
                // TODO: Verify that View.Handle is a sensible
                //       value to return here.
                return _viewController.View.Handle;
            }
        }

        public override string ScreenDeviceName 
        {
            get 
            {
                var screen = _viewController.View.Window.Screen;
                if (screen == UIScreen.MainScreen)
                    return "Main Display";
                else
                    return "External Display";
            }
        }

        protected override void SetTitle(string title)
        {
            _viewController.Title = title;
        }

        #endregion GameWindow Members


        internal void BeginObservingUIApplication()
        {
            DidBecomeActiveHolder = NSNotificationCenter.DefaultCenter.AddObserver(
                    UIApplication.DidBecomeActiveNotification,
                    new Action<NSNotification>(Application_DidBecomeActive));
            WillResignActiveHolder = NSNotificationCenter.DefaultCenter.AddObserver(
                    UIApplication.WillResignActiveNotification,
                    new Action<NSNotification>(Application_WillResignActive));
        }


        #region Notification Handling

        private void Application_DidBecomeActive(NSNotification notification)
        {
            OnActivated();

#if TVOS
            _viewController.ControllerUserInteractionEnabled = false;
#endif
            //TouchPanel.Reset();
        }

        private void Application_WillResignActive(NSNotification notification)
        {
            OnDeactivated();
        }

        #endregion Notification Handling


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_viewController != null)
                {
                    _viewController.View.RemoveFromSuperview();
                    _viewController.RemoveFromParentViewController();
                    _viewController.Dispose();
                    _viewController = null;
                }

                if (_uiWindow != null)
                {
                    _uiWindow.Dispose();
                    _uiWindow = null;
                }
            }
            
        }
    }
}
