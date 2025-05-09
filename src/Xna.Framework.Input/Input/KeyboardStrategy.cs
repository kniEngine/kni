﻿// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Platform.Input
{
    public abstract class KeyboardStrategy
    {
        public abstract KeyboardState PlatformGetState();

        protected KeyboardState CreateKeyboardState(List<Keys> keys, bool capsLock = false, bool numLock = false)
        {
            return new KeyboardState(keys, capsLock, numLock);
        }

        protected void InternalSetKey(ref KeyboardState keyboardState, Keys key)
        {
            keyboardState.InternalSetKey(key);
        }

        protected void InternalResetKey(ref KeyboardState keyboardState, Keys key)
        {
            keyboardState.InternalResetKey(key);
        }

        protected void InternalResetKeys(ref KeyboardState keyboardState)
        {
            keyboardState.InternalResetKeys();
        }
    }
}