// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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

            if (GL is IWebGL2RenderingContext)
            {
                // GLES 3.00 is required for dFdx/dFdy
                glslCode = ConvertGLES100ToGLES300(contextStrategy, shaderType, glslCode);
            }

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

        static Regex rgxOES = new Regex(
                @"^#extension GL_OES_standard_derivatives : enable", RegexOptions.Multiline);
        static Regex rgxAttribute = new Regex(
                @"^attribute(?=\s)", RegexOptions.Multiline);
        static Regex rgxVarying = new Regex(
                @"^varying(?=\s)", RegexOptions.Multiline);
        static Regex rgxFragColor = new Regex(
                @"^#define ps_oC0 gl_FragColor", RegexOptions.Multiline);
        static Regex rgxTexture = new Regex(
                @"texture(2D|3D|Cube)(?=\()", RegexOptions.Multiline);

        private string ConvertGLES100ToGLES300(object glsl, WebGLShaderType shaderType, string glslCode)
        {
            switch (shaderType)
            {
                case WebGLShaderType.VERTEX:
                    {
                        glslCode = rgxVarying.Replace(glslCode, "out");
                    }
                    break;

                case WebGLShaderType.FRAGMENT:
                    {
                        glslCode = rgxOES.Replace(glslCode, "");
                        glslCode = rgxVarying.Replace(glslCode, "in");
                        glslCode = rgxFragColor.Replace(glslCode, "out vec4 ps_oC0;");
                    }
                    break;
            }

            glslCode = rgxAttribute.Replace(glslCode, "in");
            glslCode = rgxTexture.Replace(glslCode, "texture");

            glslCode = "#version 300 es" + '\n' + glslCode;

            return glslCode;
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
