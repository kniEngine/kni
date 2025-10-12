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
        private readonly Keys[] DefinedKeyCodes;

        private readonly byte[] _keyState = new byte[256];
        bool _isCapsLocked;
        bool _isNumLocked;


        private readonly List<Keys> _keys = new List<Keys>(10);

        private bool _isActive;

        [DllImport("user32.dll")]
        private static extern bool GetKeyboardState(byte[] lpKeyState);

        public ConcreteKeyboard()
        {
            Array definedKeys = Enum.GetValues(typeof(Keys));
            List<Keys> keyCodes = new List<Keys>(Math.Min(definedKeys.Length, 255));
            foreach (object key in definedKeys)
            {
                int keyCode = (int)key;
                if ((keyCode >= 1) && (keyCode <= 255))
                    keyCodes.Add((Keys)keyCode);
            }
            DefinedKeyCodes = keyCodes.ToArray();
        }

        public override KeyboardState PlatformGetState()
        {
            if (_isActive == false)
            {
                // return the cleared _keys, if _isActive == false
                _isCapsLocked = Console.CapsLock;
                _isNumLocked = Console.NumberLock;
                return base.CreateKeyboardState(_keys, _isCapsLocked, _isNumLocked);
            }

            bool isKeyStateValid = GetKeyboardState(_keyState);
            if (isKeyStateValid == false)
            {
                // return the previous state of _keys, if GetKeyboardState fails
                _isCapsLocked = Console.CapsLock;
                _isNumLocked = Console.NumberLock;
                return base.CreateKeyboardState(_keys, _isCapsLocked, _isNumLocked);
            }

            // remove released keys
            _keys.RemoveAll((key) => !IsKeyPressed(key));

            // add pressed keys
            foreach (Keys key in DefinedKeyCodes)
            {
                if (!IsKeyPressed(key))
                    continue;

                if (!_keys.Contains(key))
                    _keys.Add(key);
            }

            _isCapsLocked = Console.CapsLock;
            _isNumLocked = Console.NumberLock;
            return base.CreateKeyboardState(_keys, _isCapsLocked, _isNumLocked);
        }

        private bool IsKeyPressed(Keys key)
        {
            return ((_keyState[(int)key] & 0x80) != 0);
        }

        private bool IsKeyToggled(Keys key)
        {
            return ((_keyState[(int)key] & 0x01) != 0);
        }

        internal void SetActive(bool isActive)
        {
            _isActive = isActive;

            // clear _keys, if _isActive == false
            if (_isActive == false)
                _keys.Clear();
        }
    }
}
