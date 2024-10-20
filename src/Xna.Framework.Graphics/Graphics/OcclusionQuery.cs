// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Platform.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
    public class OcclusionQuery : GraphicsResource
        , IPlatformOcclusionQuery
    {
        private OcclusionQueryStrategy _strategy;

        OcclusionQueryStrategy IPlatformOcclusionQuery.Strategy { get { return _strategy; } }

        /// <summary>
        /// Gets a value indicating whether the occlusion query has completed.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the occlusion query has completed; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool IsComplete { get { return _strategy.IsComplete; } }

        /// <summary>
        /// Gets the number of visible pixels.
        /// </summary>
        /// <value>The number of visible pixels.</value>
        /// <exception cref="InvalidOperationException">
        /// The occlusion query has not yet completed. Check <see cref="IsComplete"/> before reading
        /// the result!
        /// </exception>
        public int PixelCount
        {
            get
            {
                if (!IsComplete)
                    throw new InvalidOperationException("The occlusion query has not yet completed. Check IsComplete before reading the result.");

                return _strategy.PixelCount;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OcclusionQuery"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="graphicsDevice"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The current graphics profile does not support occlusion queries.
        /// </exception>
        public OcclusionQuery(GraphicsDevice graphicsDevice)
            : base()
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException("graphicsDevice");
            if (graphicsDevice.GraphicsProfile == GraphicsProfile.Reach)
                throw new NotSupportedException("The Reach profile does not support occlusion queries.");

            _strategy = ((IPlatformGraphicsContext)graphicsDevice.CurrentContext).Strategy.CreateOcclusionQueryStrategy();
            SetResourceStrategy((IGraphicsResourceStrategy)_strategy);
        }

        /// <summary>
        /// Begins the occlusion query.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <see cref="Begin"/> is called again before calling <see cref="End"/>.
        /// </exception>
        public void Begin()
        {
            _strategy.PlatformBegin();
        }

        /// <summary>
        /// Ends the occlusion query.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <see cref="End"/> is called before calling <see cref="Begin"/>.
        /// </exception>
        public void End()
        {
            _strategy.PlatformEnd();
        }

        protected override void Dispose(bool disposing)
        {
            System.Diagnostics.Debug.Assert(!IsDisposed);

            if (disposing)
            {
                if (_strategy != null)
                    _strategy.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
