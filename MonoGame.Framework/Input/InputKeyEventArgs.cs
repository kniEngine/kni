// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Input
{
    public class InputKeyEventArgs : EventArgs
    {
        /// <summary>
        /// The key that was either pressed or released.
        /// </summary>
        public readonly Keys Key;

        /// <summary>
        /// Create a new keyboard input event
        /// </summary>
        /// <param name="key">The key involved in this event</param>
        public InputKeyEventArgs(Keys key)
        {
            Key = key;
        }
    }
}
