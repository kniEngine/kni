// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Platform.Input
{
    public sealed class ConcreteKeyboard : KeyboardStrategy
    {
        private KeyboardState _keyboardState;
        private KeyboardState _nextKeyboardState;

        public override KeyboardState PlatformGetState()
        {
            return _keyboardState;
        }

        internal void UpdateState()
        {
            _keyboardState = _nextKeyboardState;
        }

        internal void SetKey(Keys key)
        {
            base.InternalSetKey(ref _nextKeyboardState, key);
        }

        internal void ResetKey(Keys key)
        {
            base.InternalResetKey(ref _nextKeyboardState, key);
        }
                
        internal void ResetKeys()
        {
            base.InternalResetKeys(ref _nextKeyboardState);
        }

    }
}
