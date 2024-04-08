// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Platform
{
    sealed class ConcreteGame : GameStrategy
    {
        //internal static string LaunchParameters;

        private WinFormsGameWindow _gameWindow;

        public ConcreteGame(Game game) : base(game)
        {
            _gameWindow = new WinFormsGameWindow(this);
            base.Window = _gameWindow;
            if (TouchPanel.WindowHandle == IntPtr.Zero)
                TouchPanel.WindowHandle = base.Window.Handle;
        }

        public override void RunOneFrame()
        {
            if (!_initialized)
            {
                this.Game.AssertNotDisposed();

                if (this.GraphicsDevice == null)
                {
                    GraphicsDeviceManager gdm = this.GraphicsDeviceManager;
                    if (gdm != null)
                        ((IGraphicsDeviceManager)gdm).CreateDevice();
                }

                // BeforeInitialize
                {
                    GraphicsDeviceManager gdm = this.GraphicsDeviceManager;
                    if (gdm != null)
                    {
                        PresentationParameters pp = this.GraphicsDevice.PresentationParameters;
                        _gameWindow.Initialize(pp);
                    }
                    else
                    {
                        _gameWindow.Initialize(
                            GraphicsDeviceManager.DefaultBackBufferWidth,
                            GraphicsDeviceManager.DefaultBackBufferHeight);
                    }
                }

                this.Game.CallInitialize();

                this.InitializeComponents();

                _initialized = true;
            }

            Game.CallBeginRun();
            Timer = Stopwatch.StartNew();

            //Not quite right..
            Game.Tick();

            Game.CallEndRun();
        }

        internal override void Run()
        {
            if (!_initialized)
            {
                this.Game.AssertNotDisposed();

                if (this.GraphicsDevice == null)
                {
                    GraphicsDeviceManager gdm = this.GraphicsDeviceManager;
                    if (gdm != null)
                        ((IGraphicsDeviceManager)gdm).CreateDevice();
                }

                // BeforeInitialize
                {
                    GraphicsDeviceManager gdm = this.GraphicsDeviceManager;
                    if (gdm != null)
                    {
                        PresentationParameters pp = this.GraphicsDevice.PresentationParameters;
                        _gameWindow.Initialize(pp);
                    }
                    else
                    {
                        _gameWindow.Initialize(
                            GraphicsDeviceManager.DefaultBackBufferWidth,
                            GraphicsDeviceManager.DefaultBackBufferHeight);
                    }
                }

                this.Game.CallInitialize();

                this.InitializeComponents();

                _initialized = true;
            }

            Game.CallBeginRun();
            Timer = Stopwatch.StartNew();

            // XNA runs one Update even before showing the window
            // DoUpdate
            {
                this.Game.AssertNotDisposed();
                ((IFrameworkDispatcher)FrameworkDispatcher.Current).Update();
                this.Game.CallUpdate(new GameTime());
            }

            _gameWindow.RunLoop();

            Game.CallEndRun();
            Game.DoExiting();
        }

        public override bool IsMouseVisible
        {
            get { return base.IsMouseVisible; }
            set
            {
                if (base.IsMouseVisible != value)
                {
                    base.IsMouseVisible = value;
                    _gameWindow.MouseVisibleToggled();
                }
            }
        }
                
        public override void TickExiting()
        {
            if (_gameWindow != null)
                _gameWindow.Dispose();

            _gameWindow = null;
            Window = null;
        }

        internal override void OnPresentationChanged(PresentationParameters pp)
        {
            _gameWindow.OnPresentationChanged(pp);
        }

        protected override void Dispose(bool disposing)
        {
            if (_gameWindow != null)
            {
                if (TouchPanel.WindowHandle == _gameWindow.Handle)
                    TouchPanel.WindowHandle = IntPtr.Zero;
            }

            if (disposing)
            {
                if (_gameWindow != null)
                {
                    _gameWindow.Dispose();
                    _gameWindow = null;
                    Window = null;
                }
                
            }

            base.Dispose(disposing);
        }
    }
}
