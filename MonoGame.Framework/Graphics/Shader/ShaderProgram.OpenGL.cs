
using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
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
