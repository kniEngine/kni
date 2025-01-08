// Copyright (C)2024 Nick Kastellanos

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Devices;
using Microsoft.Xna.Platform.Devices.Sensors;

namespace Microsoft.Xna.Platform.Devices
{
    public sealed class ConcreteDevicesFactory : DevicesFactory
    {

        public override HapticsStrategy CreateConcreteHapticsStrategy()
        {
            return new ConcreteHaptics();
        }

        public override SensorServiceStrategy CreateSensorServiceStrategy()
        {
            return new ConcreteSensorService();
        }

        public override AccelerometerStrategy CreateAccelerometerStrategy()
        {
            return new ConcreteAccelerometer();
        }

        public override CompassStrategy CreateCompassStrategy()
        {
            return new ConcreteCompass();
        }
    }
}
