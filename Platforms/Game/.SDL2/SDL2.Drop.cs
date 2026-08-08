// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

internal partial class Sdl
{
    public class Drop
    {
        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct Event
        {
            public uint TimeStamp;
            public IntPtr File;
            public uint WindowId;
        }
    }
}
