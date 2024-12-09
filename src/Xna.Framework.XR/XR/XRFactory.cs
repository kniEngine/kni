// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.XR;

namespace Microsoft.Xna.Platform.XR
{
    public abstract class XRFactory
    {
        private volatile static XRFactory _current;

        internal static XRFactory Current
        {
            get
            {
                XRFactory current = _current;
                if (current != null)
                    return current;

                lock (typeof(XRFactory))
                {
                    if (_current != null)
                        return _current;

                    Console.WriteLine("XRFactory not found.");
                    Console.WriteLine("Initialize XR with 'XRFactory.RegisterXRFactory(new ConcreteXRFactory());'.");
                    XRFactory xrFactory = CreateXRFactory();
                    XRFactory.RegisterXRFactory(xrFactory);
                }

                return _current;
            }
        }

        private static XRFactory CreateXRFactory()
        {
            Console.WriteLine("Registering ConcreteXRFactoryStrategy through reflection.");

            Type type = Type.GetType("Microsoft.Xna.Platform.XR.ConcreteXRFactory, MonoGame.Framework", false);
            if (type != null)
                if (type.IsSubclassOf(typeof(XRFactory)) && !type.IsAbstract)
                    return (XRFactory)Activator.CreateInstance(type);

            return null;
        }

        public static void RegisterXRFactory(XRFactory xrFactory)
        {
            if (xrFactory == null)
                throw new NullReferenceException("xrFactory");

            lock (typeof(XRFactory))
            {
                if (_current == null)
                    _current = xrFactory;
                else
                    throw new InvalidOperationException("xrFactory allready registered.");
            }
        }

        public abstract XRDeviceStrategy CreateXRDeviceStrategy(string applicationName, IServiceProvider services, XRMode mode);
        public abstract XRDeviceStrategy CreateXRDeviceStrategy(string applicationName, Game game, XRMode mode);
    }

}
