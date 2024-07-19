// Copyright (C)2024 Nick Kastellanos

using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Platform.Input
{
    public sealed class ConcreteKeyboardInput : KeyboardInputStrategy
    {
        public override Task<string> PlatformShow(string title, string description, string defaultText, bool usePasswordMode)
        {
            throw new PlatformNotSupportedException();
        }

        public override void PlatformCancel(string result)
        {
            throw new PlatformNotSupportedException();
        }

    }
}
