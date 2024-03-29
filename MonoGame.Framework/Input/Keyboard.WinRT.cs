// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Input
{
    public sealed partial class Keyboard
    {
        private KeyboardState _keyboardState;
        private KeyboardState _nextKeyboardState;

        private KeyboardState PlatformGetState()
        {
            return _keyboardState;
        }

        internal void UpdateState()
        {
            _keyboardState = _nextKeyboardState;
        }

        internal void SetKey(Keys key)
        {
            _nextKeyboardState.InternalSetKey(key);
        }

        internal void ClearKey(Keys key)
        {
            _nextKeyboardState.InternalClearKey(key);
        }
                
        internal void Clear()
        {
            _nextKeyboardState.InternalClearAllKeys();
        }

    }
}
