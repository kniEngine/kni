// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    public abstract class GraphicsDeviceStrategy : IDisposable
    {
        private bool _isDisposed;

        private GraphicsAdapter _graphicsAdapter;
        private readonly GraphicsProfile _graphicsProfile;
        private bool _useHalfPixelOffset;
        private PresentationParameters _presentationParameters;

        public bool IsDisposed
        {
            get { return _isDisposed; }
        }

        public GraphicsAdapter Adapter
        {
            get { return _graphicsAdapter; }
            internal set { _graphicsAdapter = value; }
        }

        public GraphicsProfile GraphicsProfile
        {
            get { return _graphicsProfile; }
        }

        public bool UseHalfPixelOffset
        {
            get { return _useHalfPixelOffset; }
        }

        public PresentationParameters PresentationParameters
        {
            get { return _presentationParameters; }
            internal set { _presentationParameters = value; }
        }

        protected GraphicsDeviceStrategy(GraphicsAdapter adapter, GraphicsProfile graphicsProfile, bool preferHalfPixelOffset, PresentationParameters presentationParameters)
        {
            if (adapter == null)
                throw new ArgumentNullException("adapter");
            if (!adapter.IsProfileSupported(graphicsProfile))
                throw new NoSuitableGraphicsDeviceException(String.Format("Adapter '{0}' does not support the {1} profile.", adapter.Description, graphicsProfile));
            if (presentationParameters == null)
                throw new ArgumentNullException("presentationParameters");

            this._graphicsAdapter = adapter;
            this._graphicsProfile = graphicsProfile;
            this._useHalfPixelOffset = preferHalfPixelOffset;
            this._presentationParameters = presentationParameters;

        }

        #region IDisposable Members

        ~GraphicsDeviceStrategy()
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
            if (_isDisposed)
                return;

            if (disposing)
            {

                _isDisposed = true;
            }

        }

        #endregion IDisposable Members

    }
}
