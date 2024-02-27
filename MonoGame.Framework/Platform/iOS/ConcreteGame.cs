#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009-2011 The MonoGame Team

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

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Foundation;
using OpenGLES;
using UIKit;
using CoreAnimation;
using ObjCRuntime;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
//using Microsoft.Xna.Framework.GamerServices;


namespace Microsoft.Xna.Platform
{
    sealed class ConcreteGame : GameStrategy
    {
        private iOSGameViewController _viewController;
        private UIWindow _mainWindow;
        private List<NSObject> _applicationObservers;
        private CADisplayLink _displayLink;

        private static ConcreteGame _concreteGameInstance = null;
        internal static ConcreteGame ConcreteGameInstance { get { return ConcreteGame._concreteGameInstance; } }

        public ConcreteGame(Game game) : base(game)
        {
            ConcreteGame._concreteGameInstance = this;

            // register factories
            try { Microsoft.Xna.Platform.TitleContainerFactory.RegisterTitleContainerFactory(new Microsoft.Xna.Platform.ConcreteTitleContainerFactory()); }
            catch (InvalidOperationException) { }
            try { Microsoft.Xna.Platform.Graphics.GraphicsFactory.RegisterGraphicsFactory(new Microsoft.Xna.Platform.Graphics.ConcreteGraphicsFactory()); }
            catch (InvalidOperationException) { }
            try { Microsoft.Xna.Platform.Audio.AudioFactory.RegisterAudioFactory(new Microsoft.Xna.Platform.Audio.ConcreteAudioFactory()); }
            catch (InvalidOperationException) { }
            try { Microsoft.Xna.Platform.Media.MediaFactory.RegisterMediaFactory(new Microsoft.Xna.Platform.Media.ConcreteMediaFactory()); }
            catch (InvalidOperationException) { }

            game.Services.AddService(typeof(ConcreteGame), this);

            string appLocation = ((ITitleContainer)TitleContainer.Current).Location;
            Directory.SetCurrentDirectory(appLocation);

            _applicationObservers = new List<NSObject>();

            #if !TVOS
            UIApplication.SharedApplication.SetStatusBarHidden(true, UIStatusBarAnimation.Fade);
            #endif

            // Create a full-screen window
            _mainWindow = new UIWindow(UIScreen.MainScreen.Bounds);
            //_mainWindow.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
            
            game.Services.AddService(typeof(UIWindow), _mainWindow);

            _viewController = new iOSGameViewController(this);
            game.Services.AddService(typeof(UIViewController), _viewController);
            Window = new iOSGameWindow(_viewController);

            _mainWindow.Add(_viewController.View);

            _viewController.InterfaceOrientationChanged += ViewController_InterfaceOrientationChanged;

            //(SJ) Why is this called here when it's not in any other project
            //Guide.Initialise(game);
        }

        public override TimeSpan TargetElapsedTime
        {
            get { return base.TargetElapsedTime; }
            set
            {
                if (base.TargetElapsedTime != value)
                {
                    base.TargetElapsedTime = value;
                    CreateDisplayLink();
                }
            }
        }

        private void CreateDisplayLink()
        {
            if (_displayLink != null)
                _displayLink.RemoveFromRunLoop(NSRunLoop.Main, NSRunLoopMode.Default);

            _displayLink = UIScreen.MainScreen.CreateDisplayLink(_viewController.View as iOSGameView, new Selector("doTick"));

            // FrameInterval represents how many frames must pass before the selector
            // is called again. We calculate this by dividing our target elapsed time by
            // the duration of a frame on iOS (Which is 1/60.0f at the time of writing this).
            _displayLink.FrameInterval = (int)Math.Round(60f * Game.TargetElapsedTime.TotalSeconds);

            _displayLink.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Default);
        }

        internal override void Run()
        {
            if (!_initialized)
            {
                Game.DoInitialize();
                _initialized = true;
            }

            Game.DoBeginRun();
            Timer = Stopwatch.StartNew();

            StartRunLoop();

            //Game.DoEndRun();
            //Game.DoExiting();
        }

        public override void Tick()
        {
            base.Tick();
        }

        // FIXME: VideoPlayer 'needs' this to set up its own movie player view
        //        controller.
        public iOSGameViewController ViewController
        {
            get { return _viewController; }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (_viewController != null)
                {
                    _viewController.View.RemoveFromSuperview();
                    _viewController.RemoveFromParentViewController();
                    _viewController.Dispose();
                    _viewController = null;
                }

                if (_mainWindow != null)
                {
                    _mainWindow.Dispose();
                    _mainWindow = null;
                }
            }
        }

        public override void BeforeInitialize()
        {
            _viewController.View.LayoutSubviews();
        }

        private void StartRunLoop()
        {
            // Show the window
            _mainWindow.MakeKeyAndVisible();

            // In iOS 8+ we need to set the root view controller *after* Window MakeKey
            // This ensures that the viewController's supported interface orientations
            // will be respected at launch
            _mainWindow.RootViewController = _viewController;

            BeginObservingUIApplication();

            _viewController.View.BecomeFirstResponder();
            CreateDisplayLink();
        }

        internal void iOSTick()
        {
            if (!Game.IsActive)
                return;

            // FIXME: Remove this call, and the whole Tick method, once
            //        GraphicsDevice is where platform-specific Present
            //        functionality is actually implemented.  At that
            //        point, it should be possible to pass Game.Tick
            //        directly to NSTimer.CreateRepeatingTimer.
            _viewController.View.MakeCurrent();
            Game.Tick();

            if (this.GraphicsDevice != null)
                this.GraphicsDevice.Present();

            _viewController.View.Present();
        }

        public override bool BeforeUpdate()
        {
            return true;
        }

        public override void Exit()
        {
            throw new PlatformNotSupportedException("iOS platform does not allow programmatically closing.");
        }

        public override void TickExiting()
        {
            // iOS games do not "exit" or shut down.
            throw new PlatformNotSupportedException();
        }

        private void BeginObservingUIApplication()
        {
            var events = new Tuple<NSString, Action<NSNotification>>[]
            {
                Tuple.Create(
                    UIApplication.DidBecomeActiveNotification,
                    new Action<NSNotification>(Application_DidBecomeActive)),
                Tuple.Create(
                    UIApplication.WillResignActiveNotification,
                    new Action<NSNotification>(Application_WillResignActive)),
                Tuple.Create(
                    UIApplication.WillTerminateNotification,
                    new Action<NSNotification>(Application_WillTerminate)),
             };

            foreach (var entry in events)
                _applicationObservers.Add(NSNotificationCenter.DefaultCenter.AddObserver(entry.Item1, entry.Item2));
        }

        #region Notification Handling

        private void Application_DidBecomeActive(NSNotification notification)
        {
            IsActive = true;
            #if TVOS
            _viewController.ControllerUserInteractionEnabled = false;
            #endif
            //TouchPanel.Reset();
        }

        private void Application_WillResignActive(NSNotification notification)
        {
            IsActive = false;
        }

        private void Application_WillTerminate(NSNotification notification)
        {
            // FIXME: Cleanly end the run loop.
            if (Game != null)
            {
                // TODO MonoGameGame.Terminate();
            }
        }

        #endregion Notification Handling

        #region Helper Property

        private DisplayOrientation CurrentOrientation
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

        #endregion

        private void ViewController_InterfaceOrientationChanged(object sender, EventArgs e)
        {
            var orientation = CurrentOrientation;

            // FIXME: The presentation parameters for the GraphicsDevice should
            //            be managed by the GraphicsDevice itself.  Not by ConcreteGame.
            var gdm = (GraphicsDeviceManager)Game.Services.GetService(typeof(IGraphicsDeviceManager));

            TouchPanel.DisplayOrientation = orientation;

            if (gdm != null)
            {	

                var presentParams = gdm.GraphicsDevice.PresentationParameters;
                presentParams.BackBufferWidth = gdm.PreferredBackBufferWidth;
                presentParams.BackBufferHeight = gdm.PreferredBackBufferHeight;

                presentParams.DisplayOrientation = orientation;

                // Recalculate our views.
                ViewController.View.LayoutSubviews();
                
                gdm.ApplyChanges();
            }
            
        }
    }
}
