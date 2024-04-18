using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Platform
{
    public interface IPlatformGraphicsDeviceManager
    {
        T GetStrategy<T>() where T : GraphicsDeviceManagerStrategy;
    }

    public abstract class GraphicsDeviceManagerStrategy : IDisposable
    {
        public static readonly int DefaultBackBufferWidth  = 800;
        public static readonly int DefaultBackBufferHeight = 480;

        private Game _game;
        private bool _isDrawing;
        private bool _isDisposed;
        protected /*private*/ GraphicsDevice _graphicsDevice;

        public virtual GraphicsProfile GraphicsProfile { get; set; }
        public virtual bool PreferHalfPixelOffset { get; set; }

        public virtual int PreferredBackBufferWidth  { get; set; }
        public virtual int PreferredBackBufferHeight { get; set; }
        public virtual SurfaceFormat PreferredBackBufferFormat { get; set; }
        public virtual DepthFormat PreferredDepthStencilFormat { get; set; }
        public virtual bool PreferMultiSampling { get; set; }
        public virtual bool SynchronizeWithVerticalRetrace { get; set; }
        public virtual bool IsFullScreen { get; set; }
        public virtual bool HardwareModeSwitch { get; set; }
        public virtual DisplayOrientation SupportedOrientations { get; set; }
        

        public Game Game
        {
            get { return _game; }
        }


        public GraphicsDeviceManagerStrategy(Game game)
        {
            if (game == null)
                throw new ArgumentNullException("game", "Game cannot be null.");

            _game = game;

            GraphicsProfile = GraphicsProfile.Reach;
            PreferHalfPixelOffset = false;

            PreferredBackBufferWidth  = DefaultBackBufferWidth;
            PreferredBackBufferHeight = DefaultBackBufferHeight;
            PreferredBackBufferFormat   = SurfaceFormat.Color;
            PreferredDepthStencilFormat = DepthFormat.Depth24;
            PreferMultiSampling = false;
            SynchronizeWithVerticalRetrace = true;
            IsFullScreen = false;
            HardwareModeSwitch = true;
            SupportedOrientations = DisplayOrientation.Default;
        }


        #region IGraphicsDeviceManager
        
        public bool IsDrawing
        {
            get { return _isDrawing; }
            set { _isDrawing = value; }
        }

        public abstract void CreateDevice();

        public virtual bool BeginDraw()
        {
            if (_graphicsDevice != null)
            {
                _isDrawing = true;
                return true;
            }
            return false;
        }

        public virtual void EndDraw()
        {
            GraphicsDevice device = this.GraphicsDevice;
            if (device != null)
            {
                if (_isDrawing)
                {
                    _isDrawing = false;
                    device.Present();
                }
            }
        }

        #endregion // IGraphicsDeviceManager


        #region IGraphicsDeviceService

        public virtual GraphicsDevice GraphicsDevice
        {
            get { return _graphicsDevice; }
            set
            {
                if (_graphicsDevice != null)
                {
                    _graphicsDevice.Disposing -= (sender, e) => { OnDeviceDisposing(e); };
                    _graphicsDevice.DeviceResetting -= (sender, e) => { OnDeviceResetting(e); };
                    _graphicsDevice.DeviceReset     -= (sender, e) => { OnDeviceReset(e); };
                }

                _graphicsDevice = value;

                if (_graphicsDevice != null)
                {
                    _graphicsDevice.Disposing += (sender, e) => { OnDeviceDisposing(e); };
                    _graphicsDevice.DeviceResetting += (sender, e) => { OnDeviceResetting(e); };
                    _graphicsDevice.DeviceReset += (sender, e) => { OnDeviceReset(e); };
                }
            }
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

        public bool IsDisposed
        {
            get { return _isDisposed; }
        }

        /// <summary>
        /// Raised when this <see cref="GraphicsDeviceManager"/> is disposed.
        /// </summary>
        public event EventHandler<EventArgs> Disposed;

        ~GraphicsDeviceManagerStrategy()
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
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (_graphicsDevice != null)
                    {
                        _graphicsDevice.Dispose();
                        _graphicsDevice = null;
                    }
                    _game = null;
                }
                _isDisposed = true;

                var handler = Disposed;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
        }

        #endregion // IDisposable


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

        public virtual GraphicsDeviceInformation FindBestDevice(bool anySuitableDevice)
        {
            throw new NotImplementedException();
        }

        public virtual void RankDevices(List<GraphicsDeviceInformation> foundDevices)
        {
            throw new NotImplementedException();
        }

        public virtual bool CanResetDevice(GraphicsDeviceInformation newDeviceInfo)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Toggles between windowed and fullscreen modes.
        /// </summary>
        /// <remarks>
        /// Note that on platforms that do not support windowed modes this has no affect.
        /// </remarks>
        public virtual void ToggleFullScreen()
        {
            IsFullScreen = !IsFullScreen;
            ApplyChanges();
        }

        /// <summary>
        /// Applies any pending property changes to the graphics device.
        /// </summary>
        public abstract void ApplyChanges();

        public T ToConcrete<T>() where T : GraphicsDeviceManagerStrategy
        {
            return (T)this;
        }
    }
}
