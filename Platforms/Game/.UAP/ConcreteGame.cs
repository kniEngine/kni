// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Platform.Graphics;
using Microsoft.Xna.Platform.Input;


#if UAP
using Windows.UI.Xaml;
#endif

#if WINUI
using Microsoft.UI.Xaml;
#endif

namespace Microsoft.Xna.Platform
{
    sealed class ConcreteGame : GameStrategy
    {
        internal static string LaunchParameters;

        [CLSCompliant(false)]
        public static ApplicationExecutionState PreviousExecutionState { get; internal set; }

        private static ConcreteGame _concreteGameInstance = null;
        internal static ConcreteGame ConcreteGameInstance { get { return ConcreteGame._concreteGameInstance; } }

        public ConcreteGame(Game game) : base(game)
        {
            ConcreteGame._concreteGameInstance = this;

            UAPGameWindow uapGameWindow = UAPGameWindow.Instance;
            base.Window = uapGameWindow;
            uapGameWindow.Game = game;
            base.SetWindowListeners();
            if (Mouse.WindowHandle == IntPtr.Zero)
                Mouse.WindowHandle = base.Window.Handle;
            if (TouchPanel.WindowHandle == IntPtr.Zero)
                TouchPanel.WindowHandle = base.Window.Handle;

            // Register the CoreWindow with the services registry
            Services.AddService(typeof(CoreWindow), uapGameWindow.CoreWindow);

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
        }

        private void CoreApplication_Suspending(object sender, SuspendingEventArgs e)
        {
            _enableRunLoop = false;

            GraphicsDeviceManager gdm = this.GraphicsDeviceManager;
            {
                if (gdm.GraphicsDevice != null)
                    ((IPlatformGraphicsDevice)gdm.GraphicsDevice).Strategy.ToConcrete<ConcreteGraphicsDevice>().Trim();
            }
        }

        private void CoreApplication_Resuming(object sender, Object e)
        {
            if (!_enableRunLoop)
            {
                _enableRunLoop = true;
                ((UAPGameWindow)Window).CoreWindow.Dispatcher.RunIdleAsync(OnRenderFrame);
            }
        }

        internal void Internal_SetIsVisible(bool isVisible)
        {
            base.IsVisible = isVisible;
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

        bool _enableRunLoop = false;
        private void StartGameLoop()
        {
            if (!_enableRunLoop)
            {
                _enableRunLoop = true;
                CoreWindow coreWindow = ((UAPGameWindow)Window).CoreWindow;
                coreWindow.Dispatcher.RunIdleAsync(OnRenderFrame);
            }
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
                CoreWindow coreWindow = ((UAPGameWindow)Window).CoreWindow;
                if (coreWindow.Dispatcher.ShouldYield(CoreDispatcherPriority.Idle))
                    coreWindow.Dispatcher.RunIdleAsync(OnRenderFrame);
                else
                    OnRenderFrame(true);
            }
        }

        private void OnRenderFrame(bool isQueueEmpty)
        {
            ((UAPGameWindow)Window).Tick();
            ((IPlatformGamePad)GamePad.Current).GetStrategy<ConcreteGamePad>().Back = false;

            // Request next frame
            CoreWindow coreWindow = ((UAPGameWindow)Window).CoreWindow;
            if (isQueueEmpty)
                coreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Low, OnRenderFrame);
            else
                coreWindow.Dispatcher.RunIdleAsync(OnRenderFrame);
        }

        public override void TickExiting()
        {
            if (!((UAPGameWindow)Window).IsExiting)
            {
                ((UAPGameWindow)Window).IsExiting = true;
                Application.Current.Exit();
            }
        }

        public override bool IsMouseVisible
        {
            get { return base.IsMouseVisible; }
            set
            {
                if (base.IsMouseVisible != value)
                {
                    base.IsMouseVisible = value;
                    ((UAPGameWindow)Window).SetCursor(Game.IsMouseVisible);
                }
                else base.IsMouseVisible = value;
            }
        }
        
        protected override void Dispose(bool disposing)
        {
            // Make sure we dispose the graphics system.
            var gdm = this.GraphicsDeviceManager;
            if (gdm != null)
                gdm.Dispose();

            if (Mouse.WindowHandle == Window.Handle)
                Mouse.WindowHandle = IntPtr.Zero;
            if (TouchPanel.WindowHandle == Window.Handle)
                TouchPanel.WindowHandle = IntPtr.Zero;

            ((UAPGameWindow)Window).Dispose();

            base.Dispose(disposing);
        }
    }
}
