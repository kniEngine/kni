// Copyright (C)2024 Nick Kastellanos

using System;
using Android.Hardware;

namespace Microsoft.Devices.Sensors
{
    internal class SensorListener : Java.Lang.Object, ISensorEventListener
    {
        public event EventHandler<EventArgs> AccuracyChanged;
        public event EventHandler<SensorListener.SensorChangedEventArgs> SensorChanged;

        public SensorListener()
        {
        }

        void ISensorEventListener.OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
            var handler = AccuracyChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        void ISensorEventListener.OnSensorChanged(SensorEvent e)
        {
            var handler = SensorChanged;
            if (handler != null)
                handler(this, new SensorListener.SensorChangedEventArgs(e));
        }

        public class SensorChangedEventArgs : EventArgs
        {
            public readonly SensorEvent Event;

            internal SensorChangedEventArgs(SensorEvent sensorEvent)
            {
                this.Event = sensorEvent;
            }
        }
    }
}

