// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.ObjectModel;


namespace Microsoft.Xna.Framework.Graphics
{
    public sealed partial class GraphicsAdapter
    {
        /// <summary>
        /// Defines the driver type for graphics adapter. Usable only on DirectX platforms for now.
        /// </summary>
        public enum DriverType
        {
            /// <summary>
            /// Hardware device been used for rendering. Maximum speed and performance.
            /// </summary>
            Hardware,
            /// <summary>
            /// Emulates the hardware device on CPU. Slowly, only for testing.
            /// </summary>
            Reference,
            /// <summary>
            /// Useful when <see cref="DriverType.Hardware"/> acceleration does not work.
            /// </summary>
            FastSoftware
        }


    }
}
