// Copyright (C)2023 Nick Kastellanos

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Platform.Graphics
{
    public sealed class ConcreteGraphicsFactory : GraphicsFactory
    {
        public override GraphicsAdaptersProviderStrategy CreateGraphicsAdaptersProviderStrategy()
        {
            return new ConcreteGraphicsAdaptersProvider();
        }

        public override GraphicsDeviceStrategy CreateGraphicsDeviceStrategy(GraphicsDevice device, GraphicsAdapter adapter, GraphicsProfile graphicsProfile, bool preferHalfPixelOffset, PresentationParameters presentationParameters)
        {
            return new ConcreteGraphicsDevice(device, adapter, graphicsProfile, preferHalfPixelOffset, presentationParameters);
        }

        public override SpriteBatcherStrategy CreateSpriteBatcher(GraphicsDevice graphicsDevice, int capacity)
        {
            return new SpriteBatcher(graphicsDevice, capacity);
        }
    }
}
