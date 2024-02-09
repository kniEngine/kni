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

                Console.WriteLine("GraphicsFactory not found..");
                Console.WriteLine("Initialize graphics with 'GraphicsFactory.RegisterGraphicsFactory(new ConcreteGraphicsFactory());'.");

                GraphicsFactory graphicsFactory = CreateGraphicsFactory();
                GraphicsFactory.RegisterGraphicsFactory(graphicsFactory);

                return _current;
            }
        }

        private static GraphicsFactory CreateGraphicsFactory()
        {
            Console.WriteLine("Registering Concrete GraphicsFactoryStrategy through reflection.");

            // find and create Concrete GraphicsFactoryStrategy through reflection.
            var currentAsm = typeof(GraphicsFactory).Assembly;

            // search in current Assembly
            foreach (var type in currentAsm.GetExportedTypes())
                if (type.IsSubclassOf(typeof(GraphicsFactory)) && !type.IsAbstract)
                    return (GraphicsFactory)Activator.CreateInstance(type);

            // search in loaded Assemblies
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var refAsm in asm.GetReferencedAssemblies())
                {
                    if (refAsm.FullName == currentAsm.FullName)
                    {
                        foreach (var type in asm.GetExportedTypes())
                            if (type.IsSubclassOf(typeof(GraphicsFactory)) && !type.IsAbstract)
                                return (GraphicsFactory)Activator.CreateInstance(type);
                    }
                }
            }

            return null;
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

        public abstract GraphicsAdaptersProviderStrategy CreateGraphicsAdaptersProviderStrategy();
        public abstract GraphicsDeviceStrategy CreateGraphicsDeviceStrategy(GraphicsDevice device, GraphicsAdapter adapter, GraphicsProfile graphicsProfile, bool preferHalfPixelOffset, PresentationParameters presentationParameters);
        public abstract SpriteBatcherStrategy CreateSpriteBatcher(GraphicsDevice graphicsDevice, int capacity);
    }

}
