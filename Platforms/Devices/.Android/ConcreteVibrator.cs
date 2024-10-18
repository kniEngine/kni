// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Devices;

namespace Microsoft.Xna.Platform.Devices
{
    public sealed class ConcreteVibrator : VibratorStrategy
    {
        Android.OS.Vibrator _nativeVibrator;
        private bool _hasVibrator = true;

        public ConcreteVibrator()
        {
            _nativeVibrator = (Android.OS.Vibrator)Android.App.Application.Context.GetSystemService(Android.Content.Context.VibratorService);
            try
            {
                _hasVibrator = _nativeVibrator.HasVibrator;
            }
            catch { /* ignore */ }
        }

        public override void Vibrate(TimeSpan duration)
        {
            if (_hasVibrator)
            {
                try
                {
                    _nativeVibrator.Vibrate((long)duration.TotalMilliseconds);
                }
                catch { /* ignore */ }
            }
        }
    }
}
