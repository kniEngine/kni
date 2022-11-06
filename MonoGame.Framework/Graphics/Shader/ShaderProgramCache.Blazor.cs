
using System;
using System.Collections.Generic;
using nkast.Wasm.Canvas.WebGL;

namespace Microsoft.Xna.Framework.Graphics
{

    internal class ShaderProgram
    {
        GraphicsDevice _device;
        public readonly WebGLProgram Program;

        private readonly Dictionary<string, WebGLUniformLocation> _uniformLocations = new Dictionary<string, WebGLUniformLocation>();

        private IWebGLRenderingContext GL { get { return _device._glContext; } }

        public ShaderProgram(WebGLProgram program, GraphicsDevice device)
        {
            _device = device;
            Program = program;
        }

        public WebGLUniformLocation GetUniformLocation(string name)
        {
            WebGLUniformLocation location;
            if (_uniformLocations.TryGetValue(name, out location))
                return location;

            location = GL.GetUniformLocation(Program, name);
            GraphicsExtensions.CheckGLError();
            _uniformLocations[name] = location;
            return location;
        }
    }

    /// <summary>
    /// This class is used to Cache the links between Vertex/Pixel Shaders and Constant Buffers.
    /// It will be responsible for linking the programs under OpenGL if they have not been linked
    /// before. If an existing link exists it will be resused.
    /// </summary>
    internal class ShaderProgramCache : IDisposable
    {
        GraphicsDevice _device;

        private readonly Dictionary<int, ShaderProgram> _programCache = new Dictionary<int, ShaderProgram>();
        bool _isDisposed;

        private IWebGLRenderingContext GL { get { return _device._glContext; } }

        public ShaderProgramCache(GraphicsDevice device)
        {
            _device = device;
        }


        /// <summary>
        /// Clear the program cache releasing all shader programs.
        /// </summary>
        public void DisposePrograms()
        {
            foreach (var value in _programCache.Values)
            {
                value.Program.Dispose();
            }
            _programCache.Clear();
        }

        public ShaderProgram GetProgram(Shader vertexShader, Shader pixelShader, int shaderProgramHash)
        {
            // TODO: We should be hashing in the mix of constant
            // buffers here as well.  This would allow us to optimize
            // setting uniforms to only when a constant buffer changes.

            ShaderProgram program;
            if(_programCache.TryGetValue(shaderProgramHash, out program))
                return program;

            // the key does not exist so we need to link the programs
            program = CreateProgram(vertexShader, pixelShader);
            _programCache.Add(shaderProgramHash, program);
            return program;
        }

        private ShaderProgram CreateProgram(Shader vertexShader, Shader pixelShader)
        {
            var program = GL.CreateProgram();
            GraphicsExtensions.CheckGLError();

            GL.AttachShader(program, vertexShader.GetShaderHandle());
            GraphicsExtensions.CheckGLError();

            GL.AttachShader(program, pixelShader.GetShaderHandle());
            GraphicsExtensions.CheckGLError();

            //vertexShader.BindVertexAttributes(program);

            GL.LinkProgram(program);
            GraphicsExtensions.CheckGLError();

            GL.UseProgram(program);
            GraphicsExtensions.CheckGLError();

            vertexShader.GetVertexAttributeLocations(program);

            pixelShader.ApplySamplerTextureUnits(program);

            bool linkStatus;
            linkStatus = GL.GetProgramParameter(program, WebGLProgramStatus.LINK);

            if (linkStatus == true)
            {
                return new ShaderProgram(program, _device);
            }
            else
            { 
                var log = GL.GetProgramInfoLog(program);
                vertexShader.Dispose();
                pixelShader.Dispose();
                program.Dispose();
                throw new InvalidOperationException("Unable to link effect program");
            }
        }

        ~ShaderProgramCache()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    DisposePrograms();
                    _device = null;
                }

                _isDisposed = true;
            }
        }

    }
}
