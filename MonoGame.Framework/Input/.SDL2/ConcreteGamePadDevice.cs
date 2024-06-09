// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Platform.Input
{
    internal class SdlGamePadDevice : GamePadDevice
    {
        public IntPtr Handle { get; private set; }

        internal GamePadState State;
        public int PacketNumber;

        public SdlGamePadDevice(IntPtr handle)
            : base()
        {
            this.Handle = handle;
        }
    }
}
