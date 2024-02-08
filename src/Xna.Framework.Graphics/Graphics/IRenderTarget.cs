//// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    /// <summary>
    /// Represents a render target.
    /// </summary>
    public interface IRenderTarget
    {
        /// <summary>
        /// Gets the width of the render target in pixels
        /// </summary>
        /// <value>The width of the render target in pixels.</value>
        int Width { get; }

        /// <summary>
        /// Gets the height of the render target in pixels
        /// </summary>
        /// <value>The height of the render target in pixels.</value>
        int Height { get; }

        DepthFormat DepthStencilFormat { get; }

        int MultiSampleCount { get; }

        /// <summary>
        /// Gets the usage mode of the render target.
        /// </summary>
        /// <value>The usage mode of the render target.</value>
        RenderTargetUsage RenderTargetUsage { get; }

        IRenderTargetStrategy RenderTargetStrategy { get; }
    }
}
