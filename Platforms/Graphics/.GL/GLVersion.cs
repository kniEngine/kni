// Copyright (C)2025 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct GLVersion
    {
        [FieldOffset(0)]
        public short Major;
        [FieldOffset(2)]
        public short Minor;

        [FieldOffset(0)]
        public int PackedValue;

        public GLVersion(short major, short minor) : this()
        {
            this.Major = major;
            this.Minor = minor;
        }

        public GLVersion(int major, int minor) : this()
        {
            this.Major = (short)major;
            this.Minor = (short)minor;
        }

        public static bool operator <(GLVersion l, GLVersion r)
        {
            return l.PackedValue < r.PackedValue;
        }

        public static bool operator >(GLVersion l, GLVersion r)
        {
            return l.PackedValue > r.PackedValue;
        }

        public static bool operator <=(GLVersion l, GLVersion r)
        {
            return l.PackedValue <= r.PackedValue;
        }

        public static bool operator >=(GLVersion l, GLVersion r)
        {
            return l.PackedValue >= r.PackedValue;
        }

        public override string ToString()
        {
            return String.Format("{0}.{1}", Major, Minor);
        }
    }
}
