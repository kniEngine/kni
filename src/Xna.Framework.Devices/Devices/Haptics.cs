// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Devices;
using Microsoft.Xna.Platform.Devices;

namespace Microsoft.Xna.Platform.Devices
{
    public interface IPlatformHaptics
    {
        T GetStrategy<T>() where T : HapticsStrategy;
    }
}

namespace Microsoft.Xna.Framework.Devices
{
    public sealed class Haptics : IPlatformHaptics
    {
        private static Haptics _current;

        /// <summary>
        /// Returns the current Vibrator instance.
        /// </summary> 
        public static Haptics Current
        {
            get
            {
                if (_current != null)
                    return _current;

                lock (typeof(Haptics))
                {
                    if (_current == null)
                        _current = new Haptics();

                    return _current;
                }
            }
        }

        private HapticsStrategy _strategy;

        T IPlatformHaptics.GetStrategy<T>()
        {
            return (T)_strategy;
        }

        private Haptics()
        {
            _strategy = DevicesFactory.Current.CreateConcreteHapticsStrategy();
        }

        public void Vibrate(TimeSpan duration)
        {
            lock (typeof(Haptics))
            {
                _strategy.Vibrate(duration);
            }
        }

    }
}
