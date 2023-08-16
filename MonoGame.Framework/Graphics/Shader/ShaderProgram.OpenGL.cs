
using System;
using System.Collections.Generic;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
    internal class ShaderProgram
    {
        public readonly int Program;

        private readonly Dictionary<string, int> _uniformLocations = new Dictionary<string, int>();

        public ShaderProgram(int program)
        {
            Program = program;
        }

        public int GetUniformLocation(string name)
        {
            int location;
            if (_uniformLocations.TryGetValue(name, out location))
                return location;

            location = GL.GetUniformLocation(Program, name);
            GraphicsExtensions.CheckGLError();
            _uniformLocations[name] = location;
            return location;
        }
    }
}
