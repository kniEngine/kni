// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Platform.Graphics
{
    public abstract class ConcreteShader : ShaderStrategy
    {
        // The shader handle.
        private WebGLShader _shaderHandle = null;

        internal WebGLShader ShaderHandle { get { return _shaderHandle; } }

        internal ConcreteShader(GraphicsContextStrategy contextStrategy, byte[] shaderBytecode, SamplerInfo[] samplers, int[] cBuffers, VertexAttribute[] attributes, ShaderProfileType profile)
            : base(contextStrategy, shaderBytecode, samplers, cBuffers, attributes, profile)
        {
            if (profile != ShaderProfileType.OpenGL_Mojo)
                throw new Exception("This effect was built for a different platform.");

            // TODO: precompute shader's hashKey in the processor.
            _hashKey = MonoGame.Framework.Utilities.Hash.ComputeHash(shaderBytecode);
        }

        internal void CreateShader(GraphicsContextStrategy contextStrategy, WebGLShaderType shaderType, byte[] shaderBytecode)
        {
            var GL = contextStrategy.ToConcrete<ConcreteGraphicsContext>().GL;

            _shaderHandle = GL.CreateShader(shaderType);
            GL.CheckGLError();
            string glslCode = System.Text.Encoding.ASCII.GetString(shaderBytecode);
            GL.ShaderSource(_shaderHandle, glslCode);
            GL.CheckGLError();
            GL.CompileShader(_shaderHandle);
            GL.CheckGLError();
            bool compiled = false;
            compiled = GL.GetShaderParameter(_shaderHandle, WebGLShaderStatus.COMPILE);
            GL.CheckGLError();
            if (compiled != true)
            {
                string log = GL.GetShaderInfoLog(_shaderHandle);
                _shaderHandle.Dispose();
                _shaderHandle = null;

                throw new InvalidOperationException("Shader Compilation Failed."
                    + Environment.NewLine + log);
            }
        }

        protected override void PlatformGraphicsContextLost()
        {
            if (_shaderHandle != null)
            {
                _shaderHandle.Dispose();
                _shaderHandle = null;
            }

            base.PlatformGraphicsContextLost();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_shaderHandle != null)
                {
                    _shaderHandle.Dispose();
                    _shaderHandle = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
