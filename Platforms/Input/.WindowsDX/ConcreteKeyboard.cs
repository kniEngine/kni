// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Platform.Input
{
    public sealed class ConcreteKeyboard : KeyboardStrategy
    {
        private readonly byte[] DefinedKeyCodes;

        private readonly byte[] _keyState = new byte[256];
        private readonly List<Keys> _keys = new List<Keys>(10);

        private bool _isActive;

        [DllImport("user32.dll")]
        private static extern bool GetKeyboardState(byte[] lpKeyState);

        public ConcreteKeyboard()
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

        public override KeyboardState PlatformGetState()
        {
            if (_isActive && GetKeyboardState(_keyState))
            {
                _keys.RemoveAll( (keyCode) => !IsKeyPressed((byte)keyCode) );

                foreach (byte keyCode in DefinedKeyCodes)
                {
                    if (!IsKeyPressed(keyCode))
                        continue;

                    Keys key = (Keys)keyCode;
                    if (!_keys.Contains(key))
                        _keys.Add(key);
                }
            }

            bool isCapsLocked = Console.CapsLock;
            bool isNumLocked = Console.NumberLock;
            return base.CreateKeyboardState(_keys, isCapsLocked, isNumLocked);
        }

        private bool IsKeyPressed(byte keyCode)
        {
            return ((_keyState[keyCode] & 0x80) != 0);
        }

        private bool IsKeyToggled(Keys key)
        {
            return ((_keyState[(int)key] & 0x01) != 0);
        }

        internal void SetActive(bool isActive)
        {
            _isActive = isActive;
            if (!_isActive)
                _keys.Clear();
        }
    }
}
