// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace Microsoft.Xna.Framework.Graphics
{
    using Microsoft.Xna.Platform.Graphics;

    partial class GraphicsAdapter
    {
        /// <summary>
        /// Defines the driver type for graphics adapter.
        /// </summary>
        /// <remarks>Usable only on DirectX platforms.</remarks>
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

        /// <summary>
        /// Used to request creation of a specific kind of driver.
        /// </summary>
        /// <remarks>
        /// These values only work on DirectX platforms and must be defined before the graphics device
        /// is created. <see cref="DriverType.Hardware"/> by default.
        /// </remarks>
        public static DriverType UseDriverType
        {
            get { return GraphicsAdaptersProviderStrategy.Current.ToConcrete<ConcreteGraphicsAdaptersProvider>().PlatformDX_UseDriverType; }
            set { GraphicsAdaptersProviderStrategy.Current.ToConcrete<ConcreteGraphicsAdaptersProvider>().PlatformDX_UseDriverType = value; }
        }

    }
}
