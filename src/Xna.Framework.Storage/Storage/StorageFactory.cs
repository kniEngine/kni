// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;

namespace Microsoft.Xna.Platform.Storage
{
    public abstract class StorageFactory
    {
        private volatile static StorageFactory _current;

        internal static StorageFactory Current
        {
            get
            {
                StorageFactory current = _current;
                if (current != null)
                    return current;

                lock (typeof(StorageFactory))
                {
                    if (_current != null)
                        return _current;

                    Console.WriteLine("StorageFactory not found.");
                    Console.WriteLine("Initialize storage with 'StorageFactory.RegisterStorageFactory(new ConcreteStorageFactory());'.");
                    StorageFactory storageFactory = CreateStorageFactory();
                    StorageFactory.RegisterStorageFactory(storageFactory);
                }

                return _current;
            }
        }

        private static StorageFactory CreateStorageFactory()
        {
            Console.WriteLine("Registering ConcreteStorageFactoryStrategy through reflection.");

            Type type = Type.GetType("Microsoft.Xna.Platform.Storage.ConcreteStorageFactory, Kni.Platform", false);
            if (type != null)
                if (type.IsSubclassOf(typeof(StorageFactory)) && !type.IsAbstract)
                    return (StorageFactory)Activator.CreateInstance(type);

            return null;
        }

        public static void RegisterStorageFactory(StorageFactory storageFactory)
        {
            if (storageFactory == null)
                throw new NullReferenceException("storageFactory");

            lock (typeof(StorageFactory))
            {
                if (_current == null)
                    _current = storageFactory;
                else
                    throw new InvalidOperationException("storageFactory allready registered.");
            }
        }

        public abstract StorageContainerStrategy CreateStorageContainerStrategy(string name, PlayerIndex? playerIndex);
        public abstract StorageDeviceStrategy CreateStorageDeviceStrategy(PlayerIndex? player, int directoryCount);
        public abstract StorageServiceStrategy CreateStorageServiceStrategy();
    }

}
