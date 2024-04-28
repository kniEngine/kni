// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework
{   
    /// <summary>
    /// An object that can be attached to a <see cref="Microsoft.Xna.Framework.Game"/> and have its <see cref="Update"/>
    /// method called when <see cref="Microsoft.Xna.Framework.Game.Update"/> is called.
    /// </summary>
    public class GameComponent : IGameComponent, IUpdateable, IDisposable
    {
        bool _enabled = true;
        int _updateOrder;

        /// <summary>
        /// The <see cref="Game"/> that owns this <see cref="GameComponent"/>.
        /// </summary>
        public Game Game { get; private set; }

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    OnEnabledChanged(EventArgs.Empty);
                }
            }
        }

        public int UpdateOrder
        {
            get { return _updateOrder; }
            set
            {
                if (_updateOrder != value)
                {
                    _updateOrder = value;
                    OnUpdateOrderChanged(EventArgs.Empty);
                }
            }
        }

        /// <inheritdoc />
        public event EventHandler<EventArgs> EnabledChanged;

        /// <inheritdoc />
        public event EventHandler<EventArgs> UpdateOrderChanged;

        /// <summary>
        /// Create a <see cref="GameComponent"/>.
        /// </summary>
        /// <param name="game">The game that this component will belong to.</param>
        public GameComponent(Game game)
        {
            this.Game = game;
        }

        ~GameComponent()
        {
            Dispose(false);
        }

        public virtual void Initialize() { }

        /// <summary>
        /// Update the component.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> of the <see cref="Game"/>.</param>
        public virtual void Update(GameTime gameTime) { }

        /// <summary>
        /// Called when <see cref="UpdateOrder"/> changed. Raises the <see cref="UpdateOrderChanged"/> event.
        /// </summary>
        /// <param name="args">Arguments to the <see cref="UpdateOrderChanged"/> event.</param>
        protected virtual void OnUpdateOrderChanged(EventArgs args)
        {
            var handler = UpdateOrderChanged;
            if (handler != null)
                handler(this, args);
        }

        /// <summary>
        /// Called when <see cref="Enabled"/> changed. Raises the <see cref="EnabledChanged"/> event.
        /// </summary>
        /// <param name="args">Arguments to the <see cref="EnabledChanged"/> event.</param>
        protected virtual void OnEnabledChanged(EventArgs args)
        {
            var handler = EnabledChanged;
            if (handler != null)
                handler(this, args);
        }

        /// <summary>
        /// Shuts down the component.
        /// </summary>
        protected virtual void Dispose(bool disposing) { }
        
        /// <summary>
        /// Shuts down the component.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
