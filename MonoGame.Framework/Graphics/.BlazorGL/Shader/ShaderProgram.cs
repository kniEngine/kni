// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Platform.Graphics
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
