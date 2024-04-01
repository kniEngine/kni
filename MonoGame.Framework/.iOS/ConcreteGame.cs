// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Platform.Input.Touch;
//using Microsoft.Xna.Framework.GamerServices;


namespace Microsoft.Xna.Platform
{
    sealed class ConcreteGame : GameStrategy
    {
        private iOSGameViewController _viewController;
        private UIWindow _uiWindow;
        private List<NSObject> _applicationObservers;
        private CADisplayLink _displayLink;

        private static ConcreteGame _concreteGameInstance = null;
        internal static ConcreteGame ConcreteGameInstance { get { return ConcreteGame._concreteGameInstance; } }

        public ConcreteGame(Game game) : base(game)
        {
            ConcreteGame._concreteGameInstance = this;

            game.Services.AddService(typeof(ConcreteGame), this);

            string appLocation = ((ITitleContainer)TitleContainer.Current).Location;
            Directory.SetCurrentDirectory(appLocation);

            _applicationObservers = new List<NSObject>();

            #if !TVOS
            UIApplication.SharedApplication.SetStatusBarHidden(true, UIStatusBarAnimation.Fade);
            #endif

            // Create a full-screen window
            _uiWindow = new UIWindow(UIScreen.MainScreen.Bounds);
            //_uiWindow.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
            
            game.Services.AddService(typeof(UIWindow), _uiWindow);

            _viewController = new iOSGameViewController(this);
            game.Services.AddService(typeof(UIViewController), _viewController);

            GameWindow gameWindow = new iOSGameWindow(_viewController);
            if (base.Window == null)
            {
                ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<ConcreteTouchPanel>().PrimaryWindow = gameWindow;
            }
            base.Window = gameWindow;

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

                if (_uiWindow != null)
                {
                    _uiWindow.Dispose();
                    _uiWindow = null;
                }
            }

            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<ConcreteTouchPanel>().PrimaryWindow = null;
        }

        public override void BeforeInitialize()
        {
            _viewController.View.LayoutSubviews();
        }

        public override void Initialize()
        {
            // TODO: This should be moved to GraphicsDeviceManager or GraphicsDevice
            {
                GraphicsDevice graphicsDevice = this.GraphicsDevice;
                PresentationParameters pp = graphicsDevice.PresentationParameters;
                graphicsDevice.Viewport = new Viewport(0, 0, pp.BackBufferWidth, pp.BackBufferHeight);
            }

            base.Initialize();
        }

        private void StartRunLoop()
        {
            // Show the window
            _uiWindow.MakeKeyAndVisible();

            // In iOS 8+ we need to set the root view controller *after* Window MakeKey
            // This ensures that the viewController's supported interface orientations
            // will be respected at launch
            _uiWindow.RootViewController = _viewController;

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

        public override void Android_BeforeUpdate()
        {
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
