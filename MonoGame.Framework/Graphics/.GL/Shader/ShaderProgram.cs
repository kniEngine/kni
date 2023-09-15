// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ShaderProgram
    {
        public readonly int Program;

        internal readonly Dictionary<string, int> _uniformLocationCache = new Dictionary<string, int>();

        public ShaderProgram(int program)
        {
            Program = program;
        }
    }
}
