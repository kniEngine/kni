// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;


namespace Microsoft.Xna.Framework
{
    abstract partial class GamePlatform : IDisposable
    {
        #region Fields

        protected TimeSpan _inactiveSleepTime = TimeSpan.FromMilliseconds(20.0);
        protected bool _needsToResetElapsedTime = false;

        bool _isDisposed;

        protected bool InFullScreenMode = false;
        protected bool IsDisposed { get { return _isDisposed; } }

        #endregion

        #region Construction/Destruction

		protected GamePlatform(Game game)
        {
            if (game == null)
                throw new ArgumentNullException("game");

            Game = game;
        }

        ~GamePlatform()
        {
            Dispose(false);
        }

        #endregion Construction/Destruction

        #region Public Properties


        /// <summary>
        /// Gets the Game instance that owns this GamePlatform instance.
        /// </summary>
        public readonly Game Game;

        public readonly GameTime Time = new GameTime();
        public Stopwatch Timer;

        private bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            internal set
            {
                if (_isActive != value)
                {
                    _isActive = value;

                    var handler = _isActive ? Activated : Deactivated;
                    if (handler != null)
                        handler(this, EventArgs.Empty);
                }
            }
        }

        private bool _isVisible = true;
        public bool IsVisible
        {
            get { return _isVisible; }
            internal set { _isVisible = value; }
        }
        
        private bool _isMouseVisible;
        public virtual bool IsMouseVisible
        {
            get { return _isMouseVisible; }
            set { _isMouseVisible = value; }
        }

        private GameWindow _window;
        public GameWindow Window
        {
            get { return _window; }
            protected set
            {
                if (_window == null)
                {
                    Mouse.PrimaryWindow = value;
                    TouchPanel.PrimaryWindow = value;
                }

                _window = value;
            }
        }

        #endregion

        #region Events

        public event EventHandler<EventArgs> Activated;
        public event EventHandler<EventArgs> Deactivated;

        #endregion Events

        #region Methods

        internal abstract void Run();

        internal virtual void Run_UAP_XAML()
        {
            throw new PlatformNotSupportedException("This method is valid only for the UAP/XAML template.");
        }

        /// <summary>
        /// Gives derived classes an opportunity to do work before any
        /// components are initialized.  Note that the base implementation sets
        /// IsActive to true, so derived classes should either call the base
        /// implementation or set IsActive to true by their own means.
        /// </summary>
        public virtual void BeforeInitialize()
        {
            IsActive = true;
        }

        /// <summary>
        /// Gives derived classes an opportunity to do work just before the
        /// run loop is begun.  Implementations may also return false to prevent
        /// the run loop from starting.
        /// </summary>
        /// <returns></returns>
        public virtual bool ANDROID_BeforeRun()
        {
            return true;
        }

        /// <summary>
        /// When implemented in a derived, ends the active run loop.
        /// </summary>
        public abstract void Exit();

        public abstract void TickExiting();

        /// <summary>
        /// Gives derived classes an opportunity to do work just before Update
        /// is called for all IUpdatable components.  Returning false from this
        /// method will result in this round of Update calls being skipped.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public abstract bool BeforeUpdate(GameTime gameTime);

        /// <summary>
        /// Gives derived classes an opportunity to do work just before Draw
        /// is called for all IDrawable components.  Returning false from this
        /// method will result in this round of Draw calls being skipped.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public abstract bool BeforeDraw(GameTime gameTime);

        /// <summary>
        /// When implemented in a derived class, causes the game to enter
        /// full-screen mode.
        /// </summary>
        public abstract void EnterFullScreen();

        /// <summary>
        /// When implemented in a derived class, causes the game to exit
        /// full-screen mode.
        /// </summary>
        public abstract void ExitFullScreen();

        /// <summary>
        /// Gives derived classes an opportunity to modify
        /// Game.TargetElapsedTime before it is set.
        /// </summary>
        /// <param name="value">The proposed new value of TargetElapsedTime.</param>
        /// <returns>The new value of TargetElapsedTime that will be set.</returns>
        public virtual TimeSpan TargetElapsedTimeChanging(TimeSpan value)
        {
            return value;
        }
        /// <summary>
        /// Starts a device transition (windowed to full screen or vice versa).
        /// </summary>
        /// <param name='willBeFullScreen'>
        /// Specifies whether the device will be in full-screen mode upon completion of the change.
        /// </param>
        public abstract void BeginScreenDeviceChange (
                 bool willBeFullScreen
        );

        /// <summary>
        /// Completes a device transition.
        /// </summary>
        /// <param name='screenDeviceName'>
        /// Screen device name.
        /// </param>
        /// <param name='clientWidth'>
        /// The new width of the game's client window.
        /// </param>
        /// <param name='clientHeight'>
        /// The new height of the game's client window.
        /// </param>
        public abstract void EndScreenDeviceChange (
                 string screenDeviceName,
                 int clientWidth,
                 int clientHeight
        );

        /// <summary>
        /// Gives derived classes an opportunity to take action after
        /// Game.TargetElapsedTime has been set.
        /// </summary>
        public virtual void TargetElapsedTimeChanged() {}

        /// <summary>
        /// MSDN: Use this method if your game is recovering from a slow-running state, and ElapsedGameTime is too large to be useful.
        /// Frame timing is generally handled by the Game class, but some platforms still handle it elsewhere. Once all platforms
        /// rely on the Game class's functionality, this method and any overrides should be removed.
        /// </summary>
        public virtual void ResetElapsedTime()
        {
            if (Timer != null)
            {
                Timer.Reset();
                Timer.Start();
            }

            Time.ElapsedGameTime = TimeSpan.Zero;
        }

        public virtual void Present() { }

        /// <summary>
        /// Called by the GraphicsDeviceManager to notify the platform
        /// that the presentation parameters have changed.
        /// </summary>
        /// <param name="pp">The new presentation parameters.</param>
        internal virtual void OnPresentationChanged(PresentationParameters pp)
        {
        }

        #endregion Methods

        #region IDisposable implementation

        /// <summary>
        /// Performs application-defined tasks associated with freeing,
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                Mouse.PrimaryWindow = null;
                TouchPanel.PrimaryWindow = null;

                _isDisposed = true;
            }
        }
		
		/// <summary>
		/// Log the specified Message.
		/// </summary>
		/// <param name='Message'>
		/// 
		/// </param>
		[System.Diagnostics.Conditional("DEBUG")]
		public virtual void Log(string Message) {}
			

        #endregion
    }
}

