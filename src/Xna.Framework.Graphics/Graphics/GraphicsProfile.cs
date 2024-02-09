// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Defines a set of graphic capabilities.
    /// </summary>
	public enum GraphicsProfile
	{
        /// <summary>
        /// Use a limited set of graphic features and capabilities, allowing the game to support the widest variety of devices.
        /// </summary>
        Reach = 0,
        /// <summary>
        /// Use the largest available set of graphic features and capabilities to target devices, that have more enhanced graphic capabilities.        
        /// </summary>
        HiDef = 1,

        FL10_0 = 2,
        FL10_1 = 3,
        FL11_0 = 4,
        FL11_1 = 5,
	}
}
