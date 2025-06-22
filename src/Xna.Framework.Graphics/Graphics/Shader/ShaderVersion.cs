// Copyright (C)2025 Nick Kastellanos

using System;

namespace Microsoft.Xna.Platform.Graphics
{
    public struct ShaderVersion
    {
        public readonly ushort Major;
        public readonly ushort Minor;

        public uint PackedValue { get { return ((uint)Major << 16) | (uint)Minor; } }

        public ShaderVersion(ushort major, ushort minor)
        {
            this.Major = major;
            this.Minor = minor;
        }

        public static bool operator <(ShaderVersion l, ShaderVersion r)
        {
            return l.PackedValue < r.PackedValue;
        }

        public static bool operator >(ShaderVersion l, ShaderVersion r)
        {
            return l.PackedValue > r.PackedValue;
        }

        public static bool operator <=(ShaderVersion l, ShaderVersion r)
        {
            return l.PackedValue <= r.PackedValue;
        }

        public static bool operator >=(ShaderVersion l, ShaderVersion r)
        {
            return l.PackedValue >= r.PackedValue;
        }

        public static bool operator ==(ShaderVersion l, ShaderVersion r)
        {
            return l.PackedValue == r.PackedValue;
        }

        public static bool operator !=(ShaderVersion l, ShaderVersion r)
        {
            return l.PackedValue != r.PackedValue;
        }

        public override string ToString()
        {
            return String.Format("{0}.{1}", Major, Minor);
        }
    }
}
