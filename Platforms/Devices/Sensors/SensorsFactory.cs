// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;

namespace Microsoft.Platform.Devices.Sensors
{
    public abstract class SensorsFactory
    {
        private volatile static SensorsFactory _current;

        internal static SensorsFactory Current
        {
            get
            {
                SensorsFactory current = _current;
                if (current != null)
                    return current;

                lock (SensorService.SyncHandle)
                {
                    if (_current != null)
                        return _current;

                    Console.WriteLine("SensorsFactory not found.");
                    Console.WriteLine("Initialize sensors with 'SensorsFactory.RegisterSensorsFactory(new ConcreteSensorsFactory());'.");
                    SensorsFactory sensorsFactory = CreateSensorsFactory();
                    SensorsFactory.RegisterAudioFactory(sensorsFactory);
                }

                return _current;
            }
        }

        private static SensorsFactory CreateSensorsFactory()
        {
            Console.WriteLine("Registering Concrete SensorsFactoryStrategy through reflection.");

            Type type = Type.GetType("Microsoft.Platform.Devices.Sensors.ConcreteSensorsFactory, MonoGame.Framework", false);
            if (type != null)
                if (type.IsSubclassOf(typeof(SensorsFactory)) && !type.IsAbstract)
                    return (SensorsFactory)Activator.CreateInstance(type);

            return null;
        }

        public static void RegisterAudioFactory(SensorsFactory sensorsFactory)
        {
            if (sensorsFactory == null)
                throw new NullReferenceException("sensorsFactory");

            lock (SensorService.SyncHandle)
            {
                if (_current == null)
                    _current = sensorsFactory;
                else
                    throw new InvalidOperationException("SensorsFactory allready registered.");
            }
        }

        public abstract SensorServiceStrategy CreateSensorServiceStrategy();
        public abstract AccelerometerStrategy CreateAccelerometerStrategy();
        public abstract CompassStrategy CreateCompassStrategy();
    }

}

