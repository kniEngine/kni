// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;


namespace Microsoft.Xna.Platform
{
    sealed class ConcreteGame : GameStrategy
    {
        internal static string LaunchParameters;

        internal static readonly TouchQueue TouchQueue = new TouchQueue();

        internal static ApplicationExecutionState PreviousExecutionState { get; set; }

        public ConcreteGame(Game game) : base(game)
        {
            // Setup the game window.
            Window = UAPGameWindow.Instance;
			UAPGameWindow.Instance.Game = game;
            UAPGameWindow.Instance.RegisterCoreWindowService();

            // Setup the launch parameters.
            // - Parameters can optionally start with a forward slash.
            // - Keys can be separated from values by a colon or equals sign
            // - Double quotes can be used to enclose spaces in a key or value.
            int pos = 0;
            int paramStart = 0;
            bool inQuotes = false;
            var keySeperators = new char[] { ':', '=' };

            while (pos <= LaunchParameters.Length)
            {
                string arg = string.Empty;
                if (pos < LaunchParameters.Length)
                {
                    char c = LaunchParameters[pos];
                    if (c == '"')
                        inQuotes = !inQuotes;
                    else if ((c == ' ') && !inQuotes)
                    {
                        arg = LaunchParameters.Substring(paramStart, pos - paramStart).Replace("\"", "");
                        paramStart = pos + 1;
                    }
                }
                else
                {
                    arg = LaunchParameters.Substring(paramStart).Replace("\"", "");
                }
                ++pos;

                if (string.IsNullOrWhiteSpace(arg))
                    continue;

                string key = string.Empty;
                string value = string.Empty;
                int keyStart = 0;

                if (arg.StartsWith("/"))
                    keyStart = 1;

                if (arg.Length > keyStart)
                {
                    int keyEnd = arg.IndexOfAny(keySeperators, keyStart);

                    if (keyEnd >= 0)
                    {
                        key = arg.Substring(keyStart, keyEnd - keyStart);
                        int valueStart = keyEnd + 1;
                        if (valueStart < arg.Length)
                            value = arg.Substring(valueStart);
                    }
                    else
                    {
                        key = arg.Substring(keyStart);
                    }

                    Game.LaunchParameters.Add(key, value);
                }
            }

            CoreApplication.Suspending += this.CoreApplication_Suspending;
            CoreApplication.Resuming += this.CoreApplication_Resuming;

            Game.PreviousExecutionState = PreviousExecutionState;
        }

        private void CoreApplication_Suspending(object sender, SuspendingEventArgs e)
        {
            if (_runInMainThread)
                _enableRunLoop = false;

            if (this.Game.GraphicsDevice != null)
                this.Game.GraphicsDevice.Trim();
        }

        private void CoreApplication_Resuming(object sender, Object e)
        {
            if (_runInMainThread)
            {
                if (!_enableRunLoop)
                {
                    _enableRunLoop = true;
                    UAPGameWindow.Instance.CoreWindow.Dispatcher.RunIdleAsync(OnRenderFrame);
                }
            }
        }

        internal override void Run()
        {
            if (!Game.Initialized)
            {
                Game.DoInitialize();
            }

            Game.DoBeginRun();
            Timer = Stopwatch.StartNew();
            // XNA runs one Update even before showing the window
            Game.DoUpdate(new GameTime());

            UAPGameWindow.Instance.RunLoop();

            Game.DoEndRun();
            Game.DoExiting();
        }

        public override void Tick()
        {
            base.Tick();
        }

        //TODO: merge Run_UAP_XAML() with Run()
        internal override void Run_UAP_XAML()
        {
            if (!Game.Initialized)
            {
                Game.DoInitialize();
            }

            Game.DoBeginRun();
            Timer = Stopwatch.StartNew();

            StartRunLoop();

            //Game.DoEndRun();
            //Game.DoExiting();
        }

        bool _runInMainThread = true;
        bool _enableRunLoop = false;
        private void StartRunLoop()
        {
            if (UAPGameWindow.Instance.CoreWindow.CustomProperties.ContainsKey("RunInMainThread"))
                _runInMainThread = (bool)UAPGameWindow.Instance.CoreWindow.CustomProperties["RunInMainThread"];
            if (_runInMainThread)
            {
                if (!_enableRunLoop)
                {
                    _enableRunLoop = true;
                    UAPGameWindow.Instance.CoreWindow.Dispatcher.RunIdleAsync(OnRenderFrame);
                }
                return;
            }
            
            var workItemHandler = new WorkItemHandler((action) =>
            {
                while (true)
                {
                    UAPGameWindow.Instance.Tick();
                }
            });
            var tickWorker = ThreadPool.RunAsync(workItemHandler, WorkItemPriority.High, WorkItemOptions.TimeSliced);
        }
        
        private void OnRenderFrame(IdleDispatchedHandlerArgs e)
        {
            if (_enableRunLoop)
                OnRenderFrame(e.IsDispatcherIdle);
        }

        private void OnRenderFrame()
        {
            if (_enableRunLoop)
            {
                var dispatcher = UAPGameWindow.Instance.CoreWindow.Dispatcher;
                if (dispatcher.ShouldYield(CoreDispatcherPriority.Idle))
                    dispatcher.RunIdleAsync(OnRenderFrame);
                else
                    OnRenderFrame(true);
            }
        }

        private void OnRenderFrame(bool isQueueEmpty)
        {
            UAPGameWindow.Instance.Tick();
            GamePad.Back = false;

            // Request next frame
            var dispatcher = UAPGameWindow.Instance.CoreWindow.Dispatcher;
            if (isQueueEmpty)
                dispatcher.RunAsync(CoreDispatcherPriority.Low, OnRenderFrame);
            else
                dispatcher.RunIdleAsync(OnRenderFrame);
        }

        public override void TickExiting()
        {
            if (!UAPGameWindow.Instance.IsExiting)
            {
				UAPGameWindow.Instance.IsExiting = true;
                Application.Current.Exit();
            }
        }

        public override bool BeforeUpdate(GameTime gameTime)
        {
            TouchQueue.ProcessQueued();
            return true;
        }

        public override bool BeforeDraw(GameTime gameTime)
        {
            var device = Game.GraphicsDevice;
            if (device != null)
            {
				// For a UAP app we need to re-apply the
				// render target before every draw.  
				// 
				// I guess the OS changes it and doesn't restore it?
				device.UAP_ResetRenderTargets();
            }

            return true;
        }

        public override void EnterFullScreen()
        {
            if (UAPGameWindow.Instance.AppView.TryEnterFullScreenMode())
            {
                ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
            }
		}

		public override void ExitFullScreen()
        {
            UAPGameWindow.Instance.AppView.ExitFullScreenMode();
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.Auto;
        }

        internal override void OnPresentationChanged(PresentationParameters pp)
        {
            if (pp.IsFullScreen)
                EnterFullScreen();
            else
                ExitFullScreen();
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
        }

        public override void Log(string Message)
        {
            Debug.WriteLine(Message);
        }

        public override void EndDraw()
        {
            var device = Game.GraphicsDevice;
            if (device != null)
                device.Present();
        }

        public override bool IsMouseVisible
        {
            get { return base.IsMouseVisible; }
            set
            {
                if (base.IsMouseVisible != value)
                {
                    base.IsMouseVisible = value;
                    UAPGameWindow.Instance.SetCursor(Game.IsMouseVisible);
                }
                else base.IsMouseVisible = value;
            }
        }
		
        protected override void Dispose(bool disposing)
        {
            // Make sure we dispose the graphics system.
            var graphicsDeviceManager = Game.graphicsDeviceManager;
            if (graphicsDeviceManager != null)
                graphicsDeviceManager.Dispose();

			UAPGameWindow.Instance.Dispose();
			
			base.Dispose(disposing);
        }
    }
}
