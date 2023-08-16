
using System;
using System.Collections.Generic;
using Microsoft.Xna.Platform.Graphics;
using nkast.Wasm.Canvas.WebGL;

namespace Microsoft.Xna.Framework.Graphics
{
    internal class ShaderProgram
    {
        GraphicsDeviceStrategy _deviceStrategy;
        public readonly WebGLProgram Program;

        private readonly Dictionary<string, WebGLUniformLocation> _uniformLocations = new Dictionary<string, WebGLUniformLocation>();

        public ShaderProgram(WebGLProgram program, GraphicsDeviceStrategy deviceStrategy)
        {
            _deviceStrategy = deviceStrategy;
            Program = program;
        }

        public WebGLUniformLocation GetUniformLocation(string name)
        {
            WebGLUniformLocation location;
            if (_uniformLocations.TryGetValue(name, out location))
                return location;

            var GL = ((ConcreteGraphicsContext)_deviceStrategy.CurrentContext.Strategy).GL;

            location = GL.GetUniformLocation(Program, name);
            GraphicsExtensions.CheckGLError();
            _uniformLocations[name] = location;
            return location;
        }
    }
}
