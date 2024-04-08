// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform
{
    public abstract class GameStrategy : IDisposable
    {
        #region Fields

        private GameServiceContainer _services;
        private IGraphicsDeviceService _graphicsDeviceService;
        private IGraphicsDeviceManager _graphicsDeviceManager;

        private ContentManager _content;
        private GameComponentCollection _components;
        private DrawableComponents _drawableComponents = new DrawableComponents();
        private UpdateableComponents _updateableComponents = new UpdateableComponents();

        private TimeSpan _targetElapsedTime = TimeSpan.FromTicks(166666); // 60fps
        private TimeSpan _inactiveSleepTime = TimeSpan.FromMilliseconds(20.0);

        private TimeSpan _maxElapsedTime = TimeSpan.FromMilliseconds(500);

        private bool _isFixedTimeStep = true;

        private bool _shouldExit;
        private bool _suppressDraw;

        bool _isDisposed;

        protected bool InFullScreenMode = false;
        protected bool IsDisposed { get { return _isDisposed; } }

        protected bool _initialized = false;

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

            _services.AddService(typeof(GameStrategy), this);
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


        public GraphicsDevice GraphicsDevice
        {
            get
            {
                IGraphicsDeviceService graphicsDeviceService = this.GraphicsDeviceService;

                if (graphicsDeviceService == null)
                    throw new InvalidOperationException("No Graphics Device Service");

                return graphicsDeviceService.GraphicsDevice;
            }
        }

        private IGraphicsDeviceService GraphicsDeviceService
        {
            get
            {
                if (_graphicsDeviceService != null)
                    return _graphicsDeviceService;

                _graphicsDeviceService = (IGraphicsDeviceService)Services.GetService(typeof(IGraphicsDeviceService));

                return _graphicsDeviceService;
            }
        }

        internal GraphicsDeviceManager GraphicsDeviceManager
        {
            get
            {
                if (_graphicsDeviceManager != null)
                    return (GraphicsDeviceManager)_graphicsDeviceManager;

                _graphicsDeviceManager = (IGraphicsDeviceManager)Services.GetService(typeof(IGraphicsDeviceManager));

                return (GraphicsDeviceManager)_graphicsDeviceManager;
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
            protected set { _window = value; }
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

        public abstract void RunOneFrame();

        internal abstract void Run();

        internal virtual void Run_UAP_XAML()
        {
            throw new PlatformNotSupportedException("This method is valid only for the UAP/XAML template.");
        }

        public virtual void Initialize()
        {
            // According to the information given on MSDN (see link below), all
            // GameComponents in Components at the time Initialize() is called
            // are initialized.
            // http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.game.initialize.aspx
            // Initialize all existing components
            for (int i = 0; i < this.Components.Count; i++)
                this.Components[i].Initialize();

            this._graphicsDeviceService = (IGraphicsDeviceService)Services.GetService(typeof(IGraphicsDeviceService));
        }

        internal void InitializeComponents()
        {
            // We need to do this after virtual Initialize(...) is called.
            // 1. Categorize components into IUpdateable and IDrawable lists.
            // 2. Subscribe to Added/Removed events to keep the categorized
            //    lists synced and to Initialize future components as they are
            //    added.            

            _updateableComponents.ClearUpdatables();
            _drawableComponents.ClearDrawables();
            for (int i = 0; i < Components.Count; i++)
            {
                if (Components[i] is IUpdateable)
                    _updateableComponents.AddUpdatable((IUpdateable)Components[i]);
                if (Components[i] is IDrawable)
                    _drawableComponents.AddDrawable((IDrawable)Components[i]);
            }

            Components.ComponentAdded += Components_ComponentAdded;
            Components.ComponentRemoved += Components_ComponentRemoved;
        }

        private void Components_ComponentAdded(object sender, GameComponentCollectionEventArgs e)
        {
            // Since we only subscribe to ComponentAdded after the graphics
            // devices are set up, it is safe to just blindly call Initialize.
            e.GameComponent.Initialize();

            if (e.GameComponent is IUpdateable)
                _updateableComponents.AddUpdatable((IUpdateable)e.GameComponent);
            if (e.GameComponent is IDrawable)
                _drawableComponents.AddDrawable((IDrawable)e.GameComponent);
        }

        private void Components_ComponentRemoved(object sender, GameComponentCollectionEventArgs e)
        {
            if (e.GameComponent is IUpdateable)
                _updateableComponents.RemoveUpdatable((IUpdateable)e.GameComponent);
            if (e.GameComponent is IDrawable)
                _drawableComponents.RemoveDrawable((IDrawable)e.GameComponent);
        }


        /// <summary>
        /// When implemented in a derived, ends the active run loop.
        /// </summary>
        public virtual void Exit()
        {
            _shouldExit = true;
            SuppressDraw();
        }

        public abstract void TickExiting();

        /// <summary>
        /// Gives derived classes an opportunity to do work just before Update is called.
        /// </summary>
        public abstract void Android_BeforeUpdate();

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

        internal void Update(GameTime gameTime)
        {
            _updateableComponents.UpdateEnabledComponent(gameTime);
        }

        internal void Draw(GameTime gameTime)
        {
            _drawableComponents.DrawVisibleComponents(gameTime);
        }

        /// <summary>
        /// Suppress the next <see cref="Draw"/> call in the game loop.
        /// </summary>
        public void SuppressDraw()
        {
            _suppressDraw = true;
        }

        public virtual bool BeginDraw()
        {
            return true;
        }

        public virtual void EndDraw()
        {
            IGraphicsDeviceManager gdm = (IGraphicsDeviceManager)Services.GetService(typeof(IGraphicsDeviceManager));
            if (gdm != null)
                gdm.EndDraw();

        }

        /// <summary>
        /// Called by the GraphicsDeviceManager to notify the platform
        /// that the presentation parameters have changed.
        /// </summary>
        /// <param name="pp">The new presentation parameters.</param>
        internal virtual void OnPresentationChanged(PresentationParameters pp)
        {
        }

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
                System.Threading.Thread.Sleep((int)InactiveSleepTime.TotalMilliseconds);

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
                if ((TargetElapsedTime - _currElapsedTime).TotalMilliseconds >= 2.0)
                {
#if WINDOWSDX || DESKTOPGL || ANDROID || IOS || TVOS || (UAP || WINUI)
                    System.Threading.Thread.Sleep(0);
#endif
                }
                */

                // Keep looping until it's time to perform the next update
                goto RetryTick;
            }

            // Do not allow any update to take longer than our maximum.
            TimeSpan maxElapsedTime = TimeSpan.FromTicks(Math.Max(MaxElapsedTime.Ticks, TargetElapsedTime.Ticks));
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

                    // DoUpdate
                    {
                        this.Game.AssertNotDisposed();
                        this.Android_BeforeUpdate();
                        ((IFrameworkDispatcher)FrameworkDispatcher.Current).Update();
                        this.Game.CallUpdate(Time);
                    }
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
                // that occurred for the fixed length updates.
                Time.ElapsedGameTime = TimeSpan.FromTicks(TargetElapsedTime.Ticks * stepCount);
            }
            else
            {
                // Perform a single variable length update.
                Time.ElapsedGameTime = _currElapsedTime;
                Time.TotalGameTime += _currElapsedTime;
                _currElapsedTime = TimeSpan.Zero;

                // DoUpdate
                {
                    this.Game.AssertNotDisposed();
                    this.Android_BeforeUpdate();
                    ((IFrameworkDispatcher)FrameworkDispatcher.Current).Update();
                    this.Game.CallUpdate(Time);
                }
            }

            if (!_suppressDraw)
                Game.DoDraw(Time);
            else
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
                if (disposing)
                {
                    for (int i = 0; i < _components.Count; i++)
                    {
                        IDisposable disposable = _components[i] as IDisposable;
                        if (disposable != null)
                            disposable.Dispose();
                    }
                    _components = null;

                    if (_content != null)
                    {
                        _content.Dispose();
                        _content = null;
                    }

                    if (_graphicsDeviceManager != null)
                    {
                        ((IDisposable)_graphicsDeviceManager).Dispose();
                        _graphicsDeviceManager = null;
                    }
                }

                _services.RemoveService(typeof(GameStrategy));

                _isDisposed = true;
            }
        }

        #endregion


        #region Component Collections

        private class DrawableComponents
        {
            private readonly List<IDrawable> _drawableComponents = new List<IDrawable>();
            private readonly List<IDrawable> _visibleComponents = new List<IDrawable>();
            private bool _isVisibleCacheInvalidated = true;

            private readonly List<DrawableJournalEntry> _addDrawableJournal = new List<DrawableJournalEntry>();
            private readonly List<int> _removeDrawableJournal = new List<int>();
            private int _addDrawableJournalCount;

            public void AddDrawable(IDrawable component)
            {
                // NOTE: We subscribe to item events after components in _addDrawableJournal have been merged.
                _addDrawableJournal.Add(new DrawableJournalEntry(component, _addDrawableJournalCount++));
                _isVisibleCacheInvalidated = true;
            }

            public void RemoveDrawable(IDrawable component)
            {
                if (_addDrawableJournal.Remove(new DrawableJournalEntry(component, -1)))
                    return;

                int index = _drawableComponents.IndexOf(component);
                if (index >= 0)
                {
                    component.VisibleChanged -= Component_VisibleChanged;
                    component.DrawOrderChanged -= Component_DrawOrderChanged;
                    _removeDrawableJournal.Add(index);
                    _isVisibleCacheInvalidated = true;
                }
            }

            public void ClearDrawables()
            {
                for (int i = 0; i < _drawableComponents.Count; i++)
                {
                    _drawableComponents[i].VisibleChanged -= Component_VisibleChanged;
                    _drawableComponents[i].DrawOrderChanged -= Component_DrawOrderChanged;
                }

                _addDrawableJournal.Clear();
                _removeDrawableJournal.Clear();
                _drawableComponents.Clear();
                _isVisibleCacheInvalidated = true;
            }

            private void Component_VisibleChanged(object sender, EventArgs e)
            {
                _isVisibleCacheInvalidated = true;
            }

            private void Component_DrawOrderChanged(object sender, EventArgs e)
            {
                IDrawable component = (IDrawable)sender;
                int index = _drawableComponents.IndexOf(component);

                _addDrawableJournal.Add(new DrawableJournalEntry(component, _addDrawableJournalCount++));

                component.VisibleChanged -= Component_VisibleChanged;
                component.DrawOrderChanged -= Component_DrawOrderChanged;
                _removeDrawableJournal.Add(index);

                _isVisibleCacheInvalidated = true;
            }

            internal void DrawVisibleComponents(GameTime gameTime)
            {
                if (_removeDrawableJournal.Count > 0)
                    ProcessRemoveDrawableJournal();
                if (_addDrawableJournal.Count > 0)
                    ProcessAddDrawableJournal();

                // rebuild _visibleComponents
                if (_isVisibleCacheInvalidated)
                {
                    _visibleComponents.Clear();
                    for (int i = 0; i < _drawableComponents.Count; i++)
                        if (_drawableComponents[i].Visible)
                            _visibleComponents.Add(_drawableComponents[i]);

                    _isVisibleCacheInvalidated = false;
                }

                // draw components
                for (int i = 0; i < _visibleComponents.Count; i++)
                    _visibleComponents[i].Draw(gameTime);

                // If the cache was invalidated as a result of processing components,
                // now is a good time to clear it and give the GC (more of) a
                // chance to do its thing.
                if (_isVisibleCacheInvalidated)
                    _visibleComponents.Clear();
            }

            private void ProcessRemoveDrawableJournal()
            {
                // Remove components in reverse.
                _removeDrawableJournal.Sort();
                for (int i = _removeDrawableJournal.Count - 1; i >= 0; i--)
                    _drawableComponents.RemoveAt(_removeDrawableJournal[i]);

                _removeDrawableJournal.Clear();
            }

            private void ProcessAddDrawableJournal()
            {
                // Prepare the _addJournal to be merge-sorted with _drawableComponents.
                // _drawableComponents is always sorted.
                _addDrawableJournal.Sort(DrawableJournalEntry.CompareAddJournalEntry);
                _addDrawableJournalCount = 0;

                int iAddJournal = 0;
                int iItems = 0;

                while (iItems < _drawableComponents.Count && iAddJournal < _addDrawableJournal.Count)
                {
                    IDrawable addDrawableItem = _addDrawableJournal[iAddJournal].Component;
                    // If addDrawableItem is less than (belongs before) _items[iItems], insert it.
                    if (Comparer<int>.Default.Compare(addDrawableItem.DrawOrder, _drawableComponents[iItems].DrawOrder) < 0)
                    {
                        addDrawableItem.VisibleChanged += Component_VisibleChanged;
                        addDrawableItem.DrawOrderChanged += Component_DrawOrderChanged;
                        _drawableComponents.Insert(iItems, addDrawableItem);
                        iAddJournal++;
                    }
                    // Always increment iItems, either because we inserted and
                    // need to move past the insertion, or because we didn't
                    // insert and need to consider the next element.
                    iItems++;
                }

                // If _addJournal had any "tail" items, append them all now.
                for (; iAddJournal < _addDrawableJournal.Count; iAddJournal++)
                {
                    IDrawable addDrawableItem = _addDrawableJournal[iAddJournal].Component;
                    addDrawableItem.VisibleChanged += Component_VisibleChanged;
                    addDrawableItem.DrawOrderChanged += Component_DrawOrderChanged;
                    _drawableComponents.Add(addDrawableItem);
                }

                _addDrawableJournal.Clear();
            }

            private struct DrawableJournalEntry
            {
                private readonly int AddOrder;
                public readonly IDrawable Component;

                public DrawableJournalEntry(IDrawable component, int addOrder)
                {
                    Component = component;
                    this.AddOrder = addOrder;
                }

                public override int GetHashCode()
                {
                    return Component.GetHashCode();
                }

                public override bool Equals(object obj)
                {
                    if (!(obj is DrawableJournalEntry))
                        return false;

                    return object.ReferenceEquals(Component, ((DrawableJournalEntry)obj).Component);
                }

                internal static int CompareAddJournalEntry(DrawableJournalEntry x, DrawableJournalEntry y)
                {
                    int result = Comparer<int>.Default.Compare(x.Component.DrawOrder, y.Component.DrawOrder);
                    if (result == 0)
                        result = x.AddOrder - y.AddOrder;
                    return result;
                }
            }
        }

        private class UpdateableComponents
        {
            private readonly List<IUpdateable> _updateableComponents = new List<IUpdateable>();
            private readonly List<IUpdateable> _enabledComponents = new List<IUpdateable>();
            private bool _isEnabledCacheInvalidated = true;

            private readonly List<UpdateableJournalEntry> _addUpdateableJournal = new List<UpdateableJournalEntry>();
            private readonly List<int> _removeUpdateableJournal = new List<int>();
            private int _addUpdateableJournalCount;

            public void AddUpdatable(IUpdateable component)
            {
                // NOTE: We subscribe to item events after items in _addUpdateableJournal have been merged.
                _addUpdateableJournal.Add(new UpdateableJournalEntry(component, _addUpdateableJournalCount++));
                _isEnabledCacheInvalidated = true;
            }

            public void RemoveUpdatable(IUpdateable component)
            {
                if (_addUpdateableJournal.Remove(new UpdateableJournalEntry(component, -1)))
                    return;

                int index = _updateableComponents.IndexOf(component);
                if (index >= 0)
                {
                    component.EnabledChanged -= Component_EnabledChanged;
                    component.UpdateOrderChanged -= Component_UpdateOrderChanged;

                    _removeUpdateableJournal.Add(index);
                    _isEnabledCacheInvalidated = true;
                }
            }

            public void ClearUpdatables()
            {
                for (int i = 0; i < _updateableComponents.Count; i++)
                {
                    _updateableComponents[i].EnabledChanged -= Component_EnabledChanged;
                    _updateableComponents[i].UpdateOrderChanged -= Component_UpdateOrderChanged;
                }

                _addUpdateableJournal.Clear();
                _removeUpdateableJournal.Clear();
                _updateableComponents.Clear();

                _isEnabledCacheInvalidated = true;
            }

            private void Component_EnabledChanged(object sender, EventArgs e)
            {
                _isEnabledCacheInvalidated = true;
            }

            private void Component_UpdateOrderChanged(object sender, EventArgs e)
            {
                IUpdateable component = (IUpdateable)sender;
                int index = _updateableComponents.IndexOf(component);

                _addUpdateableJournal.Add(new UpdateableJournalEntry(component, _addUpdateableJournalCount++));

                component.EnabledChanged -= Component_EnabledChanged;
                component.UpdateOrderChanged -= Component_UpdateOrderChanged;
                _removeUpdateableJournal.Add(index);

                _isEnabledCacheInvalidated = true;
            }

            private void ProcessRemoveUpdateableJournal()
            {
                // Remove components in reverse.
                _removeUpdateableJournal.Sort();
                for (int i = _removeUpdateableJournal.Count - 1; i >= 0; i--)
                    _updateableComponents.RemoveAt(_removeUpdateableJournal[i]);

                _removeUpdateableJournal.Clear();
            }

            internal void UpdateEnabledComponent(GameTime gameTime)
            {
                if (_removeUpdateableJournal.Count > 0)
                    ProcessRemoveUpdateableJournal();
                if (_addUpdateableJournal.Count > 0)
                    ProcessAddUpdateableJournal();

                // rebuild _enabledComponents
                if (_isEnabledCacheInvalidated)
                {
                    _enabledComponents.Clear();
                    for (int i = 0; i < _updateableComponents.Count; i++)
                        if (_updateableComponents[i].Enabled)
                            _enabledComponents.Add(_updateableComponents[i]);

                    _isEnabledCacheInvalidated = false;
                }

                // update components
                for (int i = 0; i < _enabledComponents.Count; i++)
                    _enabledComponents[i].Update(gameTime);

                // If the cache was invalidated as a result of processing components,
                // now is a good time to clear it and give the GC (more of) a
                // chance to do its thing.
                if (_isEnabledCacheInvalidated)
                    _enabledComponents.Clear();
            }

            private void ProcessAddUpdateableJournal()
            {
                // Prepare the _addJournal to be merge-sorted with _updateableComponents.
                // _updateableComponents is always sorted.
                _addUpdateableJournal.Sort(UpdateableJournalEntry.CompareAddJournalEntry);
                _addUpdateableJournalCount = 0;

                int iAddJournal = 0;
                int iItems = 0;

                while (iItems < _updateableComponents.Count && iAddJournal < _addUpdateableJournal.Count)
                {
                    IUpdateable addUpdateableItem = _addUpdateableJournal[iAddJournal].Component;
                    // If addUpdateableItem is less than (belongs before) _items[iItems], insert it.
                    if (Comparer<int>.Default.Compare(addUpdateableItem.UpdateOrder, _updateableComponents[iItems].UpdateOrder) < 0)
                    {
                        addUpdateableItem.EnabledChanged += Component_EnabledChanged;
                        addUpdateableItem.UpdateOrderChanged += Component_UpdateOrderChanged;
                        _updateableComponents.Insert(iItems, addUpdateableItem);
                        iAddJournal++;
                    }
                    // Always increment iItems, either because we inserted and
                    // need to move past the insertion, or because we didn't
                    // insert and need to consider the next element.
                    iItems++;
                }

                // If _addJournal had any "tail" items, append them all now.
                for (; iAddJournal < _addUpdateableJournal.Count; iAddJournal++)
                {
                    IUpdateable addUpdateableItem = _addUpdateableJournal[iAddJournal].Component;
                    addUpdateableItem.EnabledChanged += Component_EnabledChanged;
                    addUpdateableItem.UpdateOrderChanged += Component_UpdateOrderChanged;

                    _updateableComponents.Add(addUpdateableItem);
                }

                _addUpdateableJournal.Clear();
            }

            private struct UpdateableJournalEntry
            {
                private readonly int AddOrder;
                public readonly IUpdateable Component;

                public UpdateableJournalEntry(IUpdateable component, int addOrder)
                {
                    Component = component;
                    AddOrder = addOrder;
                }

                public override int GetHashCode()
                {
                    return Component.GetHashCode();
                }

                public override bool Equals(object obj)
                {
                    if (!(obj is UpdateableJournalEntry))
                        return false;

                    return object.Equals(Component, ((UpdateableJournalEntry)obj).Component);
                }

                internal static int CompareAddJournalEntry(UpdateableJournalEntry x, UpdateableJournalEntry y)
                {
                    int result = Comparer<int>.Default.Compare(x.Component.UpdateOrder, y.Component.UpdateOrder);
                    if (result == 0)
                        result = x.AddOrder - y.AddOrder;
                    return result;
                }
            }
        }
        #endregion Component Collections

    }
}

