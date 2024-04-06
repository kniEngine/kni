// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform;


namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Used to initialize and control the presentation of the graphics device.
    /// </summary>
    public class GraphicsDeviceManager : IGraphicsDeviceService, IGraphicsDeviceManager, IDisposable
    {
        GraphicsDeviceManagerStrategy _strategy;


        /// <summary>
        /// The default back buffer width.
        /// </summary>
        public static readonly int DefaultBackBufferWidth = 800;

        /// <summary>
        /// The default back buffer height.
        /// </summary>
        public static readonly int DefaultBackBufferHeight = 480;


        /// <summary>
        /// Associates this graphics device manager to a game instances.
        /// </summary>
        /// <param name="game">The game instance to attach.</param>
        public GraphicsDeviceManager(Game game)
        {
            _strategy = new ConcreteGraphicsDeviceManager(game);

            // dispatch events
            _strategy.PreparingDeviceSettings += (sender, e) => { OnPreparingDeviceSettings(e); };
            _strategy.DeviceCreated   += (sender, e) => { OnDeviceCreated(e); };
            _strategy.DeviceDisposing += (sender, e) => { OnDeviceDisposing(e); };
            _strategy.DeviceResetting += (sender, e) => { OnDeviceResetting(e); };
            _strategy.DeviceReset     += (sender, e) => { OnDeviceReset(e); };

            if (game.Services.GetService(typeof(IGraphicsDeviceManager)) != null)
                throw new ArgumentException("A graphics device manager is already registered.  The graphics device manager cannot be changed once it is set.");
            game.Services.AddService(typeof(IGraphicsDeviceManager), this);
            game.Services.AddService(typeof(IGraphicsDeviceService), this);
        }

        internal T GetStrategy<T>() where T : GraphicsDeviceManagerStrategy
        {
            return (T)_strategy;
        }

        #region IGraphicsDeviceManager

        void IGraphicsDeviceManager.CreateDevice()
        {
            _strategy.CreateDevice();
        }

        bool IGraphicsDeviceManager.BeginDraw()
        {
            return _strategy.BeginDraw();
        }

        void IGraphicsDeviceManager.EndDraw()
        {
            _strategy.EndDraw();
        }

        #endregion // IGraphicsDeviceManager

        #region IGraphicsDeviceService

        /// <summary>
        /// Returns the graphics device for this manager.
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get { return _strategy.GraphicsDevice; }
        }

        /// <inheritdoc />
        public event EventHandler<EventArgs> DeviceCreated;
        /// <inheritdoc />
        public event EventHandler<EventArgs> DeviceDisposing;
        /// <inheritdoc />
        public event EventHandler<EventArgs> DeviceResetting;
        /// <inheritdoc />
        public event EventHandler<EventArgs> DeviceReset;


        /// <summary>
        /// Called when a <see cref="GraphicsDevice"/> is created. Raises the <see cref="DeviceCreated"/> event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDeviceCreated(EventArgs e)
        {
            var handler = DeviceCreated;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Called when a <see cref="GraphicsDevice"/> is disposed. Raises the <see cref="DeviceDisposing"/> event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDeviceDisposing(EventArgs e)
        {
            var handler = DeviceDisposing;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Called before a <see cref="Graphics.GraphicsDevice"/> is reset.
        /// Raises the <see cref="DeviceResetting"/> event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDeviceResetting(EventArgs e)
        {
            var handler = DeviceResetting;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Called after a <see cref="Graphics.GraphicsDevice"/> is reset.
        /// Raises the <see cref="DeviceReset"/> event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDeviceReset(EventArgs e)
        {
            var handler = DeviceReset;
            if (handler != null)
                handler(this, e);
        }

        #endregion // IGraphicsDeviceService


        #region IDisposable

        /// <summary>
        /// Raised when this <see cref="GraphicsDeviceManager"/> is disposed.
        /// </summary>
        public event EventHandler<EventArgs> Disposed;

        ~GraphicsDeviceManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_strategy.IsDisposed)
            {
                if (disposing)
                {
                    _strategy.Dispose();
                    _strategy.PreparingDeviceSettings -= (sender, e) => { OnPreparingDeviceSettings(e); };
                    _strategy.DeviceCreated   -= (sender, e) => { OnDeviceCreated(e); };
                    _strategy.DeviceDisposing -= (sender, e) => { OnDeviceDisposing(e); };
                    _strategy.DeviceResetting -= (sender, e) => { OnDeviceResetting(e); };
                    _strategy.DeviceReset     -= (sender, e) => { OnDeviceReset(e); };
                    _strategy = null;
                }

                var handler = Disposed;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
        }

        #endregion // IDisposable


        /// <summary>
        /// The profile which determines the graphics feature level.
        /// </summary>
        public GraphicsProfile GraphicsProfile
        {
            get { return _strategy.GraphicsProfile; }
            set { _strategy.GraphicsProfile = value; }
        }

        /// <summary>
        /// Indicates if DX9 style pixel addressing or current standard
        /// pixel addressing should be used. This flag is set to
        /// <c>false</c> by default. It should be set to <c>true</c>
        /// for XNA compatibility. It is recommended to leave this flag
        /// set to <c>false</c> for projects that are not ported from
        /// XNA. This value is passed to <see cref="GraphicsDevice.UseHalfPixelOffset"/>.
        /// </summary>
        /// <remarks>
        /// XNA uses DirectX9 for its graphics. DirectX9 interprets UV
        /// coordinates differently from other graphics API's. This is
        /// typically referred to as the half-pixel offset. MonoGame
        /// replicates XNA behavior if this flag is set to <c>true</c>.
        /// </remarks>
        public bool PreferHalfPixelOffset
        {
            get { return _strategy.PreferHalfPixelOffset; }
            set { _strategy.PreferHalfPixelOffset = value; }
        }

        /// <summary>
        /// Indicates the desired back buffer width in pixels.
        /// </summary>
        /// <remarks>
        /// When called at startup this will automatically set the width during initialization.  If
        /// set after startup you must call ApplyChanges() for the width to be changed.
        /// </remarks>
        public int PreferredBackBufferWidth
        {
            get { return _strategy.PreferredBackBufferWidth; }
            set { _strategy.PreferredBackBufferWidth = value; }
        }

        /// <summary>
        /// Indicates the desired back buffer height in pixels.
        /// </summary>
        /// <remarks>
        /// When called at startup this will automatically set the height during initialization.  If
        /// set after startup you must call ApplyChanges() for the height to be changed.
        /// </remarks>
        public int PreferredBackBufferHeight
        {
            get { return _strategy.PreferredBackBufferHeight; }
            set { _strategy.PreferredBackBufferHeight = value; }
        }

        /// <summary>
        /// Indicates the desired back buffer color format.
        /// </summary>
        /// <remarks>
        /// When called at startup this will automatically set the format during initialization.  If
        /// set after startup you must call ApplyChanges() for the format to be changed.
        /// </remarks>
        public SurfaceFormat PreferredBackBufferFormat
        {
            get { return _strategy.PreferredBackBufferFormat; }
            set { _strategy.PreferredBackBufferFormat = value; }
        }

        /// <summary>
        /// Indicates the desired depth-stencil buffer format.
        /// </summary>
        /// <remarks>
        /// The depth-stencil buffer format defines the scene depth precision and stencil bits available for effects during rendering.
        /// When called at startup this will automatically set the format during initialization.  If
        /// set after startup you must call ApplyChanges() for the format to be changed.
        /// </remarks>
        public DepthFormat PreferredDepthStencilFormat
        {
            get { return _strategy.PreferredDepthStencilFormat; }
            set { _strategy.PreferredDepthStencilFormat = value; }
        }

        /// <summary>
        /// Indicates the desire for a multisampled back buffer.
        /// </summary>
        /// <remarks>
        /// When called at startup this will automatically set the MSAA mode during initialization.  If
        /// set after startup you must call ApplyChanges() for the MSAA mode to be changed.
        /// </remarks>
        public bool PreferMultiSampling
        {
            get { return _strategy.PreferMultiSampling; }
            set { _strategy.PreferMultiSampling = value; }
        }

        /// <summary>
        /// Indicates the desire for vsync when presenting the back buffer.
        /// </summary>
        /// <remarks>
        /// Vsync limits the frame rate of the game to the monitor referesh rate to prevent screen tearing.
        /// When called at startup this will automatically set the vsync mode during initialization.  If
        /// set after startup you must call ApplyChanges() for the vsync mode to be changed.
        /// </remarks>
        public bool SynchronizeWithVerticalRetrace
        {
            get { return _strategy.SynchronizeWithVerticalRetrace; }
            set { _strategy.SynchronizeWithVerticalRetrace = value; }
        }

        /// <summary>
        /// Indicates the desire to switch into fullscreen mode.
        /// </summary>
        /// <remarks>
        /// When called at startup this will automatically set fullscreen mode during initialization.  If
        /// set after startup you must call ApplyChanges() for the fullscreen mode to be changed.
        /// Note that for some platforms that do not support windowed modes this property has no affect.
        /// </remarks>
        public bool IsFullScreen
        {
            get { return _strategy.IsFullScreen; }
            set { _strategy.IsFullScreen = value; }
        }

        /// <summary>
        /// Gets or sets the boolean which defines how window switches from windowed to fullscreen state.
        /// "Hard" mode(true) is slow to switch, but more effecient for performance, while "soft" mode(false) is vice versa.
        /// The default value is <c>true</c>.
        /// </summary>
        public bool HardwareModeSwitch
        {
            get { return _strategy.HardwareModeSwitch; }
            set { _strategy.HardwareModeSwitch = value; }
        }

        /// <summary>
        /// Indicates the desired allowable display orientations when the device is rotated.
        /// </summary>
        /// <remarks>
        /// This property only applies to mobile platforms with automatic display rotation.
        /// When called at startup this will automatically apply the supported orientations during initialization.  If
        /// set after startup you must call ApplyChanges() for the supported orientations to be changed.
        /// </remarks>
        public DisplayOrientation SupportedOrientations
        {
            get { return _strategy.SupportedOrientations; }
            set { _strategy.SupportedOrientations = value; }
        }

        /// <summary>
        /// Raised by <see cref="CreateDevice()"/> or <see cref="ApplyChanges"/>. Allows users
        /// to override the <see cref="PresentationParameters"/> to pass to the
        /// <see cref="Graphics.GraphicsDevice"/>.
        /// </summary>
        public event EventHandler<PreparingDeviceSettingsEventArgs> PreparingDeviceSettings;

        protected virtual void OnPreparingDeviceSettings(PreparingDeviceSettingsEventArgs e)
        {
            var handler = PreparingDeviceSettings;
            if (handler != null)
                handler(this, e);
        }

        protected virtual GraphicsDeviceInformation FindBestDevice(bool anySuitableDevice)
        {
            return _strategy.FindBestDevice(anySuitableDevice);
        }

        protected virtual void RankDevices(List<GraphicsDeviceInformation> foundDevices)
        {
            _strategy.RankDevices(foundDevices);
        }

        protected virtual bool CanResetDevice(GraphicsDeviceInformation newDeviceInfo)
        {
            return _strategy.CanResetDevice(newDeviceInfo);
        }

        /// <summary>
        /// Toggles between windowed and fullscreen modes.
        /// </summary>
        /// <remarks>
        /// Note that on platforms that do not support windowed modes this has no affect.
        /// </remarks>
        public void ToggleFullScreen()
        {
            _strategy.ToggleFullScreen();
        }

        /// <summary>
        /// Applies any pending property changes to the graphics device.
        /// </summary>
        public void ApplyChanges()
        {
            _strategy.ApplyChanges();
        }

    }
}
