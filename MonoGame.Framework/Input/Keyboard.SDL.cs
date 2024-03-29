// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Input
{
    public sealed partial class Keyboard
    {
        private Sdl SDL { get { return Sdl.Current; } }

        private List<Keys> _keys;

        private KeyboardState PlatformGetState()
        {
            Sdl.Keyboard.Keymod modifiers = SDL.KEYBOARD.GetModState();
            return new KeyboardState(_keys,
                                     (modifiers & Sdl.Keyboard.Keymod.CapsLock) == Sdl.Keyboard.Keymod.CapsLock,
                                     (modifiers & Sdl.Keyboard.Keymod.NumLock)  == Sdl.Keyboard.Keymod.NumLock);
        }

        internal void SetKeys(List<Keys> keys)
        {
            _keys = keys;
        }
    }
}
