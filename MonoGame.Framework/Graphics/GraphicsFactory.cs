// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Platform.Graphics
{
    public abstract class GraphicsFactory
    {
        private volatile static GraphicsFactory _current;

        internal static GraphicsFactory Current
        {
            get
            {
                GraphicsFactory current = _current;
                if (current != null)
                    return current;

                GraphicsFactory graphicsFactory = CreateGraphicsFactory();
                GraphicsFactory.RegisterGraphicsFactory(graphicsFactory);

                return _current;
            }
        }

        private static GraphicsFactory CreateGraphicsFactory()
        {
            return new ConcreteGraphicsFactory();
        }

        public static void RegisterGraphicsFactory(GraphicsFactory graphicsFactory)
        {
            if (graphicsFactory == null)
                throw new NullReferenceException("graphicsFactory");

            lock (typeof(GraphicsFactory))
            {
                if (_current == null)
                    _current = graphicsFactory;
                else
                    throw new InvalidOperationException("GraphicsFactory allready registered.");
            }
        }

        internal abstract GraphicsAdaptersProviderStrategy CreateGraphicsAdaptersProviderStrategy();
        internal abstract GraphicsDeviceStrategy CreateGraphicsDeviceStrategy(GraphicsDevice device, GraphicsAdapter adapter, GraphicsProfile graphicsProfile, bool preferHalfPixelOffset, PresentationParameters presentationParameters);
    }

}

