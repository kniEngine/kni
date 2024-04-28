// Copyright (C)2024 Nick Kastellanos

using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Platform.Input
{
    public abstract class KeyboardInputStrategy
    {
        public abstract void PlatformCancel(string result);
        public abstract Task<string> PlatformShow(string title, string description, string defaultText, bool usePasswordMode);
    }
}