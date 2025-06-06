// Copyright (C)2025 Nick Kastellanos

namespace Microsoft.Xna.Platform.Graphics
{
    public struct ShaderVersion
    {
        public readonly int Major;
        public readonly int Minor;

        public ShaderVersion(int major, int minor)
        {
            this.Major = major;
            this.Minor = minor;
        }
    }
}
