// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Devices;
using Microsoft.Xna.Platform.Devices;

namespace Microsoft.Xna.Platform.Devices
{
    public interface IVibrator
    {
        void Vibrate(TimeSpan duration);
    }

    public interface IPlatformVibrator
    {
        T GetStrategy<T>() where T : VibratorStrategy;
    }
}

namespace Microsoft.Xna.Framework.Devices
{
    public sealed class Vibrator : IVibrator
        , IPlatformVibrator
    {
        private static Vibrator _current;

        /// <summary>
        /// Returns the current Vibrator instance.
        /// </summary> 
        public static IVibrator Current
        {
            get
            {
                if (_current != null)
                    return _current;

                lock (typeof(Vibrator))
                {
                    if (_current == null)
                        _current = new Vibrator();

                    return _current;
                }
            }
        }

        private VibratorStrategy _strategy;

        T IPlatformVibrator.GetStrategy<T>()
        {
            return (T)_strategy;
        }

        private Vibrator()
        {
            _strategy = DevicesFactory.Current.CreateVibratorStrategy();
        }


        #region IVibrator

        void IVibrator.Vibrate(TimeSpan duration)
        {
            lock (typeof(Vibrator))
            {
                _strategy.Vibrate(duration);
            }
        }

        #endregion IVibrator

    }
}
