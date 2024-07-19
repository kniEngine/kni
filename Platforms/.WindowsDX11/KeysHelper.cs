// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Platform
{
    internal static class KeysHelper
    {
        static HashSet<int> _keyMaps = new HashSet<int>();

        static KeysHelper()
        {
            Keys[] allKeys = (Keys[])Enum.GetValues(typeof(Keys));
            foreach (Keys key in allKeys)
                _keyMaps.Add((int)key);
        }

        /// <summary>
        /// Checks if specified value is valid Key.
        /// </summary>
        /// <param name="value">Keys base value</param>
        /// <returns>Returns true if value is valid Key, false otherwise</returns>
        public static bool IsKey(int value)
        {
            return _keyMaps.Contains(value);
        }
    }
}
