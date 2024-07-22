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
        public int InstanceID { get; private set; }
        public IntPtr Handle { get; private set; }

        internal GamePadState State;
        public int PacketNumber;

        public SdlGamePadDevice(int instanceID, IntPtr handle)
            : base()
        {
            this.InstanceID = instanceID;
            this.Handle = handle;
        }
    }
}
