// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Input
{
    public sealed partial class Keyboard
    {
        private readonly byte[] DefinedKeyCodes;

        private readonly byte[] _keyState = new byte[256];
        private readonly List<Keys> _keys = new List<Keys>(10);

        private bool _isActive;

        [DllImport("user32.dll")]
        private static extern bool GetKeyboardState(byte[] lpKeyState);

        private Keyboard()
        {
            Array definedKeys = Enum.GetValues(typeof(Keys));
            List<byte> keyCodes = new List<byte>(Math.Min(definedKeys.Length, 255));
            foreach (object key in definedKeys)
            {
                int keyCode = (int)key;
                if ((keyCode >= 1) && (keyCode <= 255))
                    keyCodes.Add((byte)keyCode);
            }
            DefinedKeyCodes = keyCodes.ToArray();
        }

        private KeyboardState PlatformGetState()
        {
            if (_isActive && GetKeyboardState(_keyState))
            {
                _keys.RemoveAll( (key) => IsKeyReleased((byte)key) );

                foreach (var keyCode in DefinedKeyCodes)
                {
                    if (IsKeyReleased(keyCode))
                        continue;
                    var key = (Keys)keyCode;
                    if (!_keys.Contains(key))
                        _keys.Add(key);
                }
            }

            return new KeyboardState(_keys, Console.CapsLock, Console.NumberLock);
        }

        private bool IsKeyReleased(byte keyCode)
        {
            return ((_keyState[keyCode] & 0x80) == 0);
        }

        internal void SetActive(bool isActive)
        {
            _isActive = isActive;
            if (!_isActive)
                _keys.Clear();
        }
    }
}
