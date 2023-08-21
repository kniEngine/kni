
using System;
using System.Collections.Generic;
using nkast.Wasm.Canvas.WebGL;

namespace Microsoft.Xna.Framework.Graphics
{
    internal class ShaderProgram
    {
        public readonly WebGLProgram Program;

        internal readonly Dictionary<string, WebGLUniformLocation> _uniformLocationCache = new Dictionary<string, WebGLUniformLocation>();

        public ShaderProgram(WebGLProgram program)
        {
            Program = program;
        }
    }
}
