// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Devices;
using Microsoft.Xna.Platform.Devices.Sensors;

namespace Microsoft.Xna.Platform.Devices
{
    public abstract class DevicesFactory
    {
        private volatile static DevicesFactory _current;

        internal static DevicesFactory Current
        {
            get
            {
                DevicesFactory current = _current;
                if (current != null)
                    return current;

                lock (typeof(DevicesFactory))
                {
                    if (_current != null)
                        return _current;

                    Console.WriteLine("DevicesFactory not found.");
                    Console.WriteLine("Initialize devices with 'DevicesFactory.RegisterDevicesFactory(new ConcreteDevicesFactory());'.");
                    DevicesFactory devicesFactory = CreateDevicesFactory();
                    DevicesFactory.RegisterDevicesFactory(devicesFactory);
                }

                return _current;
            }
        }

        private static DevicesFactory CreateDevicesFactory()
        {
            Console.WriteLine("Registering ConcreteDevicesFactoryStrategy through reflection.");

            Type type = Type.GetType("Microsoft.Xna.Platform.Devices.ConcreteDevicesFactory, Xna.Platform", false);
            if (type != null)
                if (type.IsSubclassOf(typeof(DevicesFactory)) && !type.IsAbstract)
                    return (DevicesFactory)Activator.CreateInstance(type);

            return null;
        }

        public static void RegisterDevicesFactory(DevicesFactory devicesFactory)
        {
            if (devicesFactory == null)
                throw new NullReferenceException("devicesFactory");

            lock (typeof(DevicesFactory))
            {
                if (_current == null)
                    _current = devicesFactory;
                else
                    throw new InvalidOperationException("devicesFactory allready registered.");
            }
        }

        public abstract VibratorStrategy CreateVibratorStrategy();

        public abstract SensorServiceStrategy CreateSensorServiceStrategy();
        public abstract AccelerometerStrategy CreateAccelerometerStrategy();
        public abstract CompassStrategy CreateCompassStrategy();
    }

}
