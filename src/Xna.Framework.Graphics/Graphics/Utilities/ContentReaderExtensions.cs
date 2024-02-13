// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content
{
    public static class ContentReaderExtensions
    {
        /// <summary>
        /// Reads the next Color from the current stream and advances the current position of the stream by 4 bytes.
        /// </summary>
        /// <param name="input">The ContentReader.</param>
        /// <returns>The next Color read from the current stream.</returns>
        public static Color ReadColor(this ContentReader input)
        {
            Color result = new Color();
            result.R = input.ReadByte();
            result.G = input.ReadByte();
            result.B = input.ReadByte();
            result.A = input.ReadByte();
            return result;
        }

        /// <summary>
        /// Gets the GraphicsDevice from the ContentManager.ServiceProvider.
        /// </summary>
        /// <returns>The <see cref="GraphicsDevice"/>.</returns>
        public static GraphicsDevice GetGraphicsDevice(this ContentReader contentReader)
        {
            IServiceProvider serviceProvider = contentReader.ContentManager.ServiceProvider;
            IGraphicsDeviceService graphicsDeviceService = serviceProvider.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;
            if (graphicsDeviceService == null)
                throw new InvalidOperationException("No Graphics Device Service");

            return graphicsDeviceService.GraphicsDevice;
        }
    }
}
