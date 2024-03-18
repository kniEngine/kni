// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;


namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// This class is the entry point for most games. Handles setting up
    /// a window and graphics and runs a game loop that calls <see cref="Update"/> and <see cref="Draw"/>.
    /// </summary>
    public class Game : IDisposable
    {
        internal GameStrategy Strategy { get; private set; }

        /// <summary>
        /// Create a <see cref="Game"/>.
        /// </summary>
        public Game()
        {
            LaunchParameters = new LaunchParameters();

            Strategy = new ConcreteGame(this);

            Strategy.Activated += Platform_Activated;
            Strategy.Deactivated += Platform_Deactivated;
            Services.AddService(typeof(GameStrategy), Strategy);

        }

        ~Game()
        {
            Dispose(false);
        }

        void Platform_Activated(object sender, EventArgs e) { OnActivated(e); }
        void Platform_Deactivated(object sender, EventArgs e) { OnDeactivated(e); }

        #region IDisposable Implementation

        private bool _isDisposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);

            var handler = Disposed;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    Strategy.DisposeComponentsAndContentAndManager(disposing);

                    if (Strategy != null)
                    {
                        Strategy.Activated -= Platform_Activated;
                        Strategy.Deactivated -= Platform_Deactivated;

                        Services.RemoveService(typeof(GameStrategy));

                        Strategy.Dispose();
                        Strategy = null;
                    }

                    Microsoft.Xna.Platform.Audio.AudioService.Shutdown();
                }

#if ANDROID
                AndroidGameWindow.Activity = null;
#endif

                _isDisposed = true;
            }
        }

        [System.Diagnostics.DebuggerNonUserCode]
        private void AssertNotDisposed()
        {
            if (_isDisposed)
            {
                string name = GetType().Name;
                throw new ObjectDisposedException(
                    name, string.Format("The {0} object was used after being Disposed.", name));
            }
        }

        #endregion IDisposable Implementation

        #region Properties

        /// <summary>
        /// The start up parameters for this <see cref="Game"/>.
        /// </summary>
        public LaunchParameters LaunchParameters { get; private set; }

        /// <summary>
        /// A collection of game components attached to this <see cref="Game"/>.
        /// </summary>
        public GameComponentCollection Components { get { return Strategy.Components; } }

        public TimeSpan InactiveSleepTime
        {
            get { return Strategy.InactiveSleepTime; }
            set { Strategy.InactiveSleepTime = value; }
        }

        /// <summary>
        /// The maximum amount of time we will frameskip over and only perform Update calls with no Draw calls.
        /// MonoGame extension.
        /// </summary>
        public TimeSpan MaxElapsedTime
        {
            get { return Strategy.MaxElapsedTime; }
            set { Strategy.MaxElapsedTime = value; }
        }

        /// <summary>
        /// Indicates if the game is the focused application.
        /// </summary>
        public bool IsActive
        {
            get { return Strategy.IsActive; }
        }


        public bool IsVisible
        {
            get { return Strategy.IsVisible; }
        }

        /// <summary>
        /// Indicates if the mouse cursor is visible on the game screen.
        /// </summary>
        public bool IsMouseVisible
        {
            get { return Strategy.IsMouseVisible; }
            set { Strategy.IsMouseVisible = value; }
        }

        /// <summary>
        /// The time between frames when running with a fixed time step. <seealso cref="IsFixedTimeStep"/>
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Target elapsed time must be strictly larger than zero.</exception>
        public TimeSpan TargetElapsedTime
        {
            get { return Strategy.TargetElapsedTime; }
            set { Strategy.TargetElapsedTime = value; }
        }


        /// <summary>
        /// Indicates if this game is running with a fixed time between frames.
        /// 
        /// When set to <code>true</code> the target time between frames is
        /// given by <see cref="TargetElapsedTime"/>.
        /// </summary>
        public bool IsFixedTimeStep
        {
            get { return Strategy.IsFixedTimeStep; }
            set { Strategy.IsFixedTimeStep = value; }
        }

        /// <summary>
        /// Get a container holding service providers attached to this <see cref="Game"/>.
        /// </summary>
        public GameServiceContainer Services { get { return Strategy.Services; } }


        /// <summary>
        /// The <see cref="ContentManager"/> of this <see cref="Game"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">If Content is set to <code>null</code>.</exception>
        public ContentManager Content
        {
            get { return Strategy.Content; }
            set { Strategy.Content = value; }
        }

        /// <summary>
        /// Gets the <see cref="GraphicsDevice"/> used for rendering by this <see cref="Game"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// There is no <see cref="Graphics.GraphicsDevice"/> attached to this <see cref="Game"/>.
        /// </exception>
        public GraphicsDevice GraphicsDevice
        {
            get { return Strategy.GraphicsDevice; }
        }

        /// <summary>
        /// The system window that this game is displayed on.
        /// </summary>
        [CLSCompliant(false)]
        public GameWindow Window
        {
            get { return Strategy.Window; }
        }

        #endregion Properties

        #region Events

        /// <summary>
        /// Raised when the game gains focus.
        /// </summary>
        public event EventHandler<EventArgs> Activated;

        /// <summary>
        /// Raised when the game loses focus.
        /// </summary>
        public event EventHandler<EventArgs> Deactivated;

        /// <summary>
        /// Raised when this game is being disposed.
        /// </summary>
        public event EventHandler<EventArgs> Disposed;

        /// <summary>
        /// Raised when this game is exiting.
        /// </summary>
        public event EventHandler<EventArgs> Exiting;

        #endregion

        #region Public Methods

        /// <summary>
        /// Exit the game at the end of this tick.
        /// </summary>
        /// <exception cref="PlatformNotSupportedException"></exception>
        public void Exit()
        {
            Strategy.Exit();
        }

        /// <summary>
        /// Reset the elapsed game time to <see cref="TimeSpan.Zero"/>.
        /// </summary>
        public void ResetElapsedTime()
        {
            Strategy.ResetElapsedTime();
        }

        /// <summary>
        /// Supress calling <see cref="Draw"/> in the game loop.
        /// </summary>
        public void SuppressDraw()
        {
            Strategy.SuppressDraw();
        }
        
        /// <summary>
        /// Run the game for one frame, then exit.
        /// </summary>
        public void RunOneFrame()
        {
            if (Strategy == null)
                return;

            Strategy.RunOneFrame();
        }

        /// <summary>
        /// Run the game for the current platform.
        /// </summary>
        public void Run()
        {
            AssertNotDisposed();

            Strategy.Run();
        }

        /// <summary>
        /// Run the game. This method is valid only for the UAP/XAML template.
        /// </summary>
        internal void Run_UAP_XAML()
        {
            AssertNotDisposed();

            Strategy.Run_UAP_XAML();
        }

        internal void DoBeginRun()
        {
            BeginRun();
        }

        internal void DoEndRun()
        {
            EndRun();
        }
        
        /// <summary>
        /// Run one iteration of the game loop.
        ///
        /// Makes at least one call to <see cref="Update"/>
        /// and exactly one call to <see cref="Draw"/> if drawing is not supressed.
        /// When <see cref="IsFixedTimeStep"/> is set to <code>false</code> this will
        /// make exactly one call to <see cref="Update"/>.
        /// </summary>
        public void Tick()
        {
            Strategy.Tick();

            if (Strategy.ShouldExit)
                Strategy.TickExiting();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Called right before <see cref="Draw"/> is normally called. Can return <code>false</code>
        /// to let the game loop not call <see cref="Draw"/>.
        /// </summary>
        /// <returns>
        ///   <code>true</code> if <see cref="Draw"/> should be called, <code>false</code> if it should not.
        /// </returns>
        protected virtual bool BeginDraw()
        {
            return Strategy.BeginDraw();
        }

        /// <summary>
        /// Called right after <see cref="Draw"/>. Presents the
        /// rendered frame in the <see cref="GameWindow"/>.
        /// </summary>
        protected virtual void EndDraw()
        {
            Strategy.EndDraw();
        }

        /// <summary>
        /// Called after <see cref="Initialize"/>, but before the first call to <see cref="Update"/>.
        /// </summary>
        protected virtual void BeginRun() { }

        /// <summary>
        /// Called when the game loop has been terminated before exiting.
        /// </summary>
        protected virtual void EndRun() { }

        /// <summary>
        /// Override this to load graphical resources required by the game.
        /// </summary>
        protected virtual void LoadContent() { }

        /// <summary>
        /// Override this to unload graphical resources loaded by the game.
        /// </summary>
        protected virtual void UnloadContent() { }

        /// <summary>
        /// Override this to initialize the game and load any needed non-graphical resources.
        ///
        /// Initializes attached <see cref="GameComponent"/> instances and calls <see cref="LoadContent"/>.
        /// </summary>
        protected virtual void Initialize()
        {
            // TODO: This should be removed once all platforms use the new GraphicsDeviceManager
#if DESKTOPGL || ANDROID || IOS || TVOS
            // ApplyChanges
            {
                GraphicsDevice graphicsDevice = Strategy.GraphicsDevice;
                PresentationParameters pp = graphicsDevice.PresentationParameters;
                graphicsDevice.Viewport = new Viewport(0, 0, pp.BackBufferWidth, pp.BackBufferHeight);

                bool willBeFullScreen = pp.IsFullScreen;
                Strategy.EndScreenDeviceChange(string.Empty, pp.BackBufferWidth, pp.BackBufferHeight, willBeFullScreen);
            }
#endif

            // According to the information given on MSDN (see link below), all
            // GameComponents in Components at the time Initialize() is called
            // are initialized.
            // http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.game.initialize.aspx
            // Initialize all existing components
            for (int i = 0; i < Strategy.Components.Count; i++)
                Strategy.Components[i].Initialize();

            Strategy._graphicsDeviceService = (IGraphicsDeviceService)Services.GetService(typeof(IGraphicsDeviceService));

            IGraphicsDeviceService graphicsDeviceService = (IGraphicsDeviceService)Services.GetService(typeof(IGraphicsDeviceService));
            if (graphicsDeviceService != null)
            {
                if (graphicsDeviceService.GraphicsDevice != null)
                    LoadContent();
            }
        }

        /// <summary>
        /// Called when the game should update.
        ///
        /// Updates the <see cref="GameComponent"/> instances attached to this game.
        /// Override this to update your game.
        /// </summary>
        /// <param name="gameTime">The elapsed time since the last call to <see cref="Update"/>.</param>
        protected virtual void Update(GameTime gameTime)
        {
            Strategy.Update(gameTime);
        }

        /// <summary>
        /// Called when the game should draw a frame.
        ///
        /// Draws the <see cref="DrawableGameComponent"/> instances attached to this game.
        /// Override this to render your game.
        /// </summary>
        /// <param name="gameTime">A <see cref="GameTime"/> instance containing the elapsed time since the last call to <see cref="Draw"/> and the total time elapsed since the game started.</param>
        protected virtual void Draw(GameTime gameTime)
        {
            Strategy.Draw(gameTime);
        }

        /// <summary>
        /// Called when the game is exiting. Raises the <see cref="Exiting"/> event.
        /// </summary>
        /// <param name="args">The arguments to the <see cref="Exiting"/> event.</param>
        protected virtual void OnExiting(EventArgs args)
        {
            var handler = Exiting;
            if (handler != null)
                handler(this, args);
        }
        
        /// <summary>
        /// Called when the game gains focus. Raises the <see cref="Activated"/> event.
        /// </summary>
        /// <param name="args">The arguments to the <see cref="Activated"/> event.</param>
        protected virtual void OnActivated(EventArgs args)
        {
            AssertNotDisposed();

            var handler = Activated;
            if (handler != null)
                handler(this, args);
        }
        
        /// <summary>
        /// Called when the game loses focus. Raises the <see cref="Deactivated"/> event.
        /// </summary>
        /// <param name="args">The arguments to the <see cref="Deactivated"/> event.</param>
        protected virtual void OnDeactivated(EventArgs args)
        {
            AssertNotDisposed();

            var handler = Deactivated;
            if (handler != null)
                handler(this, args);
        }

        #endregion Protected Methods


        #region Internal Methods

        internal void DoUpdate(GameTime gameTime)
        {
            AssertNotDisposed();

            if (Strategy.BeforeUpdate())
            {
                ((IFrameworkDispatcher)FrameworkDispatcher.Current).Update();

                Update(gameTime);

                //The TouchPanel needs to know the time for when touches arrive
                TouchPanelState.CurrentTimestamp = gameTime.TotalGameTime;
            }
        }

        internal void DoDraw(GameTime gameTime)
        {
            AssertNotDisposed();

            bool gdmBeginDraw;
            IGraphicsDeviceManager gdm = (IGraphicsDeviceManager)Strategy.Services.GetService(typeof(IGraphicsDeviceManager));
            if (gdm != null)
                gdmBeginDraw = gdm.BeginDraw();
            else // (gdm == null)
                gdmBeginDraw = true;

            // Draw and EndDraw should not be called if BeginDraw returns false.
            // http://stackoverflow.com/questions/4054936/manual-control-over-when-to-redraw-the-screen/4057180#4057180
            // http://stackoverflow.com/questions/4235439/xna-3-1-to-4-0-requires-constant-redraw-or-will-display-a-purple-screen
            if (gdmBeginDraw && BeginDraw())
            {
                Draw(gameTime);
                EndDraw();
            }
        }

        internal void DoInitialize()
        {
            AssertNotDisposed();

            if (Strategy.GraphicsDevice == null)
            {
                GraphicsDeviceManager gdm = Strategy.GraphicsDeviceManager;
                if (gdm != null)
                    ((IGraphicsDeviceManager)gdm).CreateDevice();
            }

            Strategy.BeforeInitialize();
            Initialize();

            Strategy.InitializeComponents();
        }

        internal void DoExiting()
        {
            OnExiting(EventArgs.Empty);
            UnloadContent();
        }

        #endregion Internal Methods
    }

}
