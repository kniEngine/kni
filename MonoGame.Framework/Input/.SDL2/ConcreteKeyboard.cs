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
        private Sdl SDL { get { return Sdl.Current; } }

        private List<Keys> _keys;

        public override KeyboardState PlatformGetState()
        {
            Sdl.Keyboard.Keymod modifiers = SDL.KEYBOARD.GetModState();

            return base.CreateKeyboardState(_keys,
                                            (modifiers & Sdl.Keyboard.Keymod.CapsLock) == Sdl.Keyboard.Keymod.CapsLock,
                                            (modifiers & Sdl.Keyboard.Keymod.NumLock) == Sdl.Keyboard.Keymod.NumLock);
        }

        internal void SetKeys(List<Keys> keys)
        {
            _keys = keys;
        }
    }
}
