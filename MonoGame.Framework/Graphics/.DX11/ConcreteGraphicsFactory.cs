// Copyright (C)2023 Nick Kastellanos

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Platform.Graphics
{
    public sealed class ConcreteGraphicsFactory : GraphicsFactory
    {
        internal override GraphicsAdaptersProviderStrategy CreateGraphicsAdaptersProviderStrategy()
        {
            return new ConcreteGraphicsAdaptersProvider();
        }

        internal override GraphicsDeviceStrategy CreateGraphicsDeviceStrategy(GraphicsDevice device, GraphicsAdapter adapter, GraphicsProfile graphicsProfile, bool preferHalfPixelOffset, PresentationParameters presentationParameters)
        {
            return new ConcreteGraphicsDevice(device, adapter, graphicsProfile, preferHalfPixelOffset, presentationParameters);
        }

        internal override SpriteBatcherStrategy CreateSpriteBatcher(GraphicsDevice graphicsDevice, int capacity)
        {
            return new SpriteBatcher(graphicsDevice, capacity);
        }
    }
}
