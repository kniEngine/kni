// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;


namespace Microsoft.Xna.Platform
{
    abstract class GameStrategy : IDisposable
    {
        #region Fields

        private GameServiceContainer _services;
        internal GameComponentCollection _components;
        internal ContentManager _content;

        private TimeSpan _targetElapsedTime = TimeSpan.FromTicks(166666); // 60fps
        private TimeSpan _inactiveSleepTime = TimeSpan.FromMilliseconds(20.0);

        private TimeSpan _maxElapsedTime = TimeSpan.FromMilliseconds(500);
        
        private bool _isFixedTimeStep = true;

        private bool _shouldExit;
        private bool _suppressDraw;

        bool _isDisposed;

        protected bool InFullScreenMode = false;
        protected bool IsDisposed { get { return _isDisposed; } }

        #endregion

        #region Construction/Destruction

		protected GameStrategy(Game game)
        {
            if (game == null)
                throw new ArgumentNullException("game");

            Game = game;

            _services = new GameServiceContainer();
            _components = new GameComponentCollection();
            _content = new ContentManager(_services);
        }

        ~GameStrategy()
        {
            Dispose(false);
        }

        #endregion Construction/Destruction

        #region Public Properties

        /// <summary>
        /// Get a container holding service providers attached to this <see cref="Game"/>.
        /// </summary>
        public GameServiceContainer Services { get { return _services; } }

        /// <summary>
        /// A collection of game components attached to this <see cref="Game"/>.
        /// </summary>
        public GameComponentCollection Components { get { return _components; } }

        /// <summary>
        /// The <see cref="ContentManager"/> of this <see cref="Game"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">If Content is set to <code>null</code>.</exception>
        public ContentManager Content
        {
            get { return _content; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                _content = value;
            }
        }

        /// <summary>
        /// Gets the Game instance that owns this GamePlatform instance.
        /// </summary>
        public readonly Game Game;

        private readonly GameTime Time = new GameTime();
        protected Stopwatch Timer;

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

        public TimeSpan InactiveSleepTime
        {
            get { return _inactiveSleepTime; }
            set
            {
                if (value < TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException("InactiveSleepTime must be positive.");

                _inactiveSleepTime = value;
            }
        }

        /// <summary>
        /// The maximum amount of time we will frameskip over and only perform Update calls with no Draw calls.
        /// MonoGame extension.
        /// </summary>
        public TimeSpan MaxElapsedTime
        {
            get { return _maxElapsedTime; }
            set
            {
                if (value < TimeSpan.FromMilliseconds(500))
                    throw new ArgumentOutOfRangeException("MaxElapsedTime must be at least 0.5s");

                _maxElapsedTime = value;
            }
        }

        /// <summary>
        /// The time between frames when running with a fixed time step. <seealso cref="IsFixedTimeStep"/>
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Target elapsed time must be strictly larger than zero.</exception>
        public virtual TimeSpan TargetElapsedTime
        {
            get { return _targetElapsedTime; }
            set
            {            
                if (value <= TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException("TargetElapsedTime must be positive and non-zero.");

                _targetElapsedTime = value;
            }
        }

        /// <summary>
        /// Indicates if this game is running with a fixed time between frames.
        /// 
        /// When set to <code>true</code> the target time between frames is
        /// given by <see cref="TargetElapsedTime"/>.
        /// </summary>
        public bool IsFixedTimeStep
        {
            get { return _isFixedTimeStep; }
            set { _isFixedTimeStep = value; }
        }

        public bool ShouldExit { get { return _shouldExit; } }

        #endregion

        #region Events

        public event EventHandler<EventArgs> Activated;
        public event EventHandler<EventArgs> Deactivated;

        #endregion Events

        #region Methods

        public virtual void RunOneFrame()
        {
            if (!Game.Initialized)
            {
                Game.DoInitialize();
            }

            Game.DoBeginRun();
            Timer = Stopwatch.StartNew();

            //Not quite right..
            Game.Tick();

            Game.DoEndRun();
        }

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
        /// When implemented in a derived, ends the active run loop.
        /// </summary>
        public virtual void Exit()
        {
            _shouldExit = true;
            _suppressDraw = true;
        }

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
        /// Starts a device transition (windowed to full screen or vice versa).
        /// </summary>
        /// <param name='willBeFullScreen'>
        /// Specifies whether the device will be in full-screen mode upon completion of the change.
        /// </param>
        public abstract void BeginScreenDeviceChange(bool willBeFullScreen);

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
        public abstract void EndScreenDeviceChange(
                 string screenDeviceName,
                 int clientWidth,
                 int clientHeight
        );

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

            _currElapsedTime = TimeSpan.Zero;
            _prevElapsedTime = TimeSpan.Zero;
        }
        
        /// <summary>
        /// Supress calling <see cref="Draw"/> in the game loop.
        /// </summary>
        public void SuppressDraw()
        {
            _suppressDraw = true;
        }

        public virtual void EndDraw() { }

        /// <summary>
        /// Called by the GraphicsDeviceManager to notify the platform
        /// that the presentation parameters have changed.
        /// </summary>
        /// <param name="pp">The new presentation parameters.</param>
        internal virtual void OnPresentationChanged(PresentationParameters pp)
        {
        }

#if WINDOWS_UAP
        private readonly object _locker = new object();
#endif

        private TimeSpan _currElapsedTime;
        private TimeSpan _prevElapsedTime;
        private int _updateFrameLag;

        /// <summary>
        /// Run one iteration of the game loop.
        ///
        /// Makes at least one call to <see cref="Update"/>
        /// and exactly one call to <see cref="Draw"/> if drawing is not supressed.
        /// When <see cref="IsFixedTimeStep"/> is set to <code>false</code> this will
        /// make exactly one call to <see cref="Update"/>.
        /// </summary>
        public virtual void Tick()
        {
            // implement InactiveSleepTime to save battery life 
            // and/or release CPU time to other threads and processes.
            if (!IsActive)
            {
#if WINDOWS_UAP
                lock (_locker)
                    System.Threading.Monitor.Wait(_locker, (int)InactiveSleepTime.TotalMilliseconds);
#else
                System.Threading.Thread.Sleep((int)InactiveSleepTime.TotalMilliseconds);
#endif
            }

        RetryTick:

            // Advance the accumulated elapsed time.
            TimeSpan elapsedTime = Timer.Elapsed;
            TimeSpan dt = elapsedTime - _prevElapsedTime;
            _currElapsedTime += dt;
            _prevElapsedTime = elapsedTime;

            if (IsFixedTimeStep && _currElapsedTime < TargetElapsedTime)
            {
                // When game IsActive use CPU Spin.
                /*
                if ((TargetElapsedTime - _accumulatedElapsedTime).TotalMilliseconds >= 2.0)
                {
#if WINDOWS || DESKTOPGL || ANDROID || IOS || TVOS
                    System.Threading.Thread.Sleep(0);
#elif WINDOWS_UAP
                    lock (_locker)
                        System.Threading.Monitor.Wait(_locker, 0);
#endif
                }
                */

                // Keep looping until it's time to perform the next update
                goto RetryTick;
            }

            // Do not allow any update to take longer than our maximum.
            var maxElapsedTime = TimeSpan.FromTicks(Math.Max(MaxElapsedTime.Ticks, TargetElapsedTime.Ticks));
            if (_currElapsedTime > maxElapsedTime)
                _currElapsedTime = maxElapsedTime;

            if (IsFixedTimeStep)
            {
                Time.ElapsedGameTime = TargetElapsedTime;
                int stepCount = 0;

                // Perform as many full fixed length time steps as we can.
                while (_currElapsedTime >= TargetElapsedTime && !_shouldExit)
                {
                    Time.TotalGameTime += TargetElapsedTime;
                    _currElapsedTime -= TargetElapsedTime;
                    stepCount++;

                    Game.DoUpdate(Time);
                }

                //Every update after the first accumulates lag
                _updateFrameLag += Math.Max(0, stepCount - 1);
                _updateFrameLag = Math.Min(_updateFrameLag, 5);

                //If we think we are running slowly, wait until the lag clears before resetting it
                if (Time.IsRunningSlowly)
                {
                    if (_updateFrameLag == 0)
                        Time.IsRunningSlowly = false;
                }
                else if (_updateFrameLag >= 5)
                {
                    //If we lag more than 5 frames, start thinking we are running slowly
                    Time.IsRunningSlowly = true;
                }

                //Every time we just do one update and one draw, then we are not running slowly, so decrease the lag
                if (stepCount == 1 && _updateFrameLag > 0)
                    _updateFrameLag--;

                // Draw needs to know the total elapsed time
                // that occured for the fixed length updates.
                Time.ElapsedGameTime = TimeSpan.FromTicks(TargetElapsedTime.Ticks * stepCount);
            }
            else
            {
                // Perform a single variable length update.
                Time.ElapsedGameTime = _currElapsedTime;
                Time.TotalGameTime += _currElapsedTime;
                _currElapsedTime = TimeSpan.Zero;

                Game.DoUpdate(Time);
            }

            // Draw unless the update suppressed it.
            if (!_suppressDraw)
                Game.DoDraw(Time);
            _suppressDraw = false;
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

