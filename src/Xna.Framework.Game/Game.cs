// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Platform.Input.Touch;


namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// This class is the entry point for most games. Handles setting up
    /// a window and graphics and runs a game loop that calls <see cref="Update"/> and <see cref="Draw"/>.
    /// </summary>
    public class Game : IDisposable
        , IPlatformGame
    {
        private GameStrategy _strategy;

        GameStrategy Strategy { get { return _strategy; } }

        /// <summary>
        /// Create a <see cref="Game"/>.
        /// </summary>
        public Game()
        {
            LaunchParameters = new LaunchParameters();

            _strategy = GameFactory.Current.CreateGameStrategy(this);

            Strategy.Activated += Platform_Activated;
            Strategy.Deactivated += Platform_Deactivated;
        }

        ~Game()
        {
            Dispose(false);
        }

        T IPlatformGame.GetStrategy<T>()
        {
            return (T)_strategy;
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
                    if (_strategy != null)
                    {
                        _strategy.Activated -= Platform_Activated;
                        _strategy.Deactivated -= Platform_Deactivated;

                        _strategy.Dispose();
                        _strategy = null;
                    }
                }

                _isDisposed = true;
            }
        }

        [System.Diagnostics.DebuggerNonUserCode]
        internal void AssertNotDisposed()
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
        protected internal virtual bool BeginDraw()
        {
            return Strategy.BeginDraw();
        }

        /// <summary>
        /// Called right after <see cref="Draw"/>. Presents the
        /// rendered frame in the <see cref="GameWindow"/>.
        /// </summary>
        protected internal virtual void EndDraw()
        {
            Strategy.EndDraw();
        }

        /// <summary>
        /// Called after <see cref="Initialize"/>, but before the first call to <see cref="Update"/>.
        /// </summary>
        protected internal virtual void BeginRun() { }

        /// <summary>
        /// Called when the game loop has been terminated before exiting.
        /// </summary>
        protected internal virtual void EndRun() { }

        /// <summary>
        /// Override this to load graphical resources required by the game.
        /// </summary>
        protected virtual void LoadContent() { }

        /// <summary>
        /// Override this to unload graphical resources loaded by the game.
        /// </summary>
        protected internal virtual void UnloadContent() { }

        /// <summary>
        /// Override this to initialize the game and load any needed non-graphical resources.
        ///
        /// Initializes attached <see cref="GameComponent"/> instances and calls <see cref="LoadContent"/>.
        /// </summary>
        protected internal virtual void Initialize()
        {
            Strategy.Initialize();

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
        protected internal virtual void Update(GameTime gameTime)
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
        protected internal virtual void Draw(GameTime gameTime)
        {
            Strategy.Draw(gameTime);
        }

        /// <summary>
        /// Called when the game is exiting. Raises the <see cref="Exiting"/> event.
        /// </summary>
        /// <param name="args">The arguments to the <see cref="Exiting"/> event.</param>
        protected internal virtual void OnExiting(EventArgs args)
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

    }

}
