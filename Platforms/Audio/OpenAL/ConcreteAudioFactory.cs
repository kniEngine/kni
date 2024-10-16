// Copyright (C)2022 Nick Kastellanos

namespace Microsoft.Platform.Devices.Sensors
{
    public sealed class ConcreteAudioFactory : AudioFactory
    {
        public override SensorServiceStrategy CreateSensorServiceStrategy()
        {
            return new ConcreteAudioService();
        }

        public override MicrophoneStrategy CreateAccelerometerStrategy()
        {
            return new ConcreteMicrophone();
        }

        public override CompassStrategy CreateCompassStrategy()
        {
            return new ConcreteSoundEffect();
        }
    }
}
