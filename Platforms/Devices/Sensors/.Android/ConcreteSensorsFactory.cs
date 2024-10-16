// Copyright (C)2024 Nick Kastellanos

namespace Microsoft.Platform.Devices.Sensors
{
    public sealed class ConcreteSensorsFactory : SensorsFactory
    {
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
