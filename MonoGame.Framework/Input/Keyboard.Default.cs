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
        private List<Keys> _keys;

        public override KeyboardState PlatformGetState()
        {
            return new KeyboardState(_keys);
        }

        internal void SetKeys(List<Keys> keys)
        {
            _keys = keys;
        }
    }
}
