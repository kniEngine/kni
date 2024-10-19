// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Devices.Sensors
{
    /// <summary>
    /// Provides data for Calibrate and events.
    /// </summary>
    public class CalibrationEventArgs : EventArgs
    {
        public static readonly CalibrationEventArgs Empty = new CalibrationEventArgs();

        /// <summary>
        /// Initializes a new instance of the CalibrationEventArgs class.
        /// </summary>
        /// <remarks>
        /// Obtain a CalibrationEventArgs object by implementing a handler for the Compass.Calibrate event.
        /// </remarks>
        public CalibrationEventArgs()
        {
        }
    }
}