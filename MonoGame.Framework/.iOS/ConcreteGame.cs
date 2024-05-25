// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Foundation;
using UIKit;
using CoreAnimation;
using ObjCRuntime;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;


namespace Microsoft.Xna.Platform
{
    sealed class ConcreteGame : GameStrategy
    {
        private iOSGameViewController _viewController;
        private UIWindow _uiWindow;
        private NSObject WillTerminateHolder;
        private CADisplayLink _displayLink;

        private static ConcreteGame _concreteGameInstance = null;
        internal static ConcreteGame ConcreteGameInstance { get { return ConcreteGame._concreteGameInstance; } }

        private iOSGameWindow _gameWindow;


        public ConcreteGame(Game game) : base(game)
        {
            ConcreteGame._concreteGameInstance = this;

            this.Services.AddService(typeof(ConcreteGame), this);

            string appLocation = ((ITitleContainer)TitleContainer.Current).Location;
            Directory.SetCurrentDirectory(appLocation);

            #if !TVOS
            UIApplication.SharedApplication.SetStatusBarHidden(true, UIStatusBarAnimation.Fade);
            #endif

            // Create a full-screen window
            _uiWindow = new UIWindow(UIScreen.MainScreen.Bounds);
            //_uiWindow.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
            
            this.Services.AddService(typeof(UIWindow), _uiWindow);

            _viewController = new iOSGameViewController(this);
            this.Services.AddService(typeof(UIViewController), _viewController);

            _gameWindow = new iOSGameWindow(_viewController);
            base.Window = _gameWindow;
            base.SetWindowListeners();
            if (TouchPanel.WindowHandle == IntPtr.Zero)
                TouchPanel.WindowHandle = base.Window.Handle;

            _uiWindow.Add(_viewController.View);

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

        protected override void Run()
        {
            this.CallInitialize();
            this.CallBeginRun();
            // XNA runs one Update even before showing the window
            this.CallUpdate(new GameTime());

            StartGameLoop();
            return;

            //this.CallEndRun();
            //this.DoExiting();
        }

        private void StartGameLoop()
        {
            // Show the window
            _uiWindow.MakeKeyAndVisible();

            // In iOS 8+ we need to set the root view controller *after* Window MakeKey
            // This ensures that the viewController's supported interface orientations
            // will be respected at launch
            _uiWindow.RootViewController = _viewController;

            _gameWindow.BeginObservingUIApplication();
            BeginObservingUIApplicationExit();

            _viewController.View.BecomeFirstResponder();
            CreateDisplayLink();
        }


        // FIXME: VideoPlayer 'needs' this to set up its own movie player view
        //        controller.
        public iOSGameViewController ViewController
        {
            get { return _viewController; }
        }

        protected override void Dispose(bool disposing)
        {
            if (Window != null)
            {
                if (TouchPanel.WindowHandle == Window.Handle)
                    TouchPanel.WindowHandle = IntPtr.Zero;
            }
            
            base.Dispose(disposing);

            if (disposing)
            {
                if (_gameWindow != null)
                {
                    _gameWindow.Dispose();
                }

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

            GraphicsDeviceManager gdm = this.GraphicsDeviceManager;
            {
                if (gdm.GraphicsDevice != null)
                    gdm.GraphicsDevice.Present();
            }

            _viewController.View.Present();
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

        private void BeginObservingUIApplicationExit()
        {
            WillTerminateHolder = NSNotificationCenter.DefaultCenter.AddObserver(
                    UIApplication.WillTerminateNotification,
                    new Action<NSNotification>(Application_WillTerminate));
        }

        #region Notification Handling

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
                PresentationParameters presentParams = gdm.GraphicsDevice.PresentationParameters;
                presentParams.BackBufferWidth  = gdm.PreferredBackBufferWidth;
                presentParams.BackBufferHeight = gdm.PreferredBackBufferHeight;

                presentParams.DisplayOrientation = orientation;

                // Recalculate our views.
                ViewController.View.LayoutSubviews();
                
                gdm.ApplyChanges();
            }
            
        }
    }
}
