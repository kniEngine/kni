// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.Utilities;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Platform.Graphics
{
    public abstract class ConcreteShader : ShaderStrategy
    {
        // The shader handle.
        private WebGLShader _shaderHandle = null;

        internal WebGLShader ShaderHandle { get { return _shaderHandle; } }

        internal ConcreteShader(GraphicsContextStrategy contextStrategy, ShaderVersion shaderVersion, byte[] shaderBytecode, SamplerInfo[] samplers, int[] cBuffers, VertexAttribute[] attributes, ShaderProfileType profile)
            : base(contextStrategy, shaderVersion, shaderBytecode, samplers, cBuffers, attributes, profile)
        {
            if (profile != ShaderProfileType.OpenGL_Mojo)
                throw new Exception("Effect profile '"+profile+"' is not compatible with the graphics backend '"+((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.Adapter.Backend+"'.");

            GraphicsProfile graphicsProfile = this.GraphicsDeviceStrategy.GraphicsProfile;
            ShaderVersion maxVersion = MaxShaderVersions[graphicsProfile];
            if (shaderVersion != default
            && shaderVersion > maxVersion)
            {
                throw new NotSupportedException(
                    $"Shader model {shaderVersion} is not supported by the current graphics profile '{graphicsProfile}'.");
            }

        }

        internal void CreateShader(GraphicsContextStrategy contextStrategy, WebGLShaderType shaderType, ShaderVersion shaderVersion, byte[] shaderBytecode)
        {
            {
                var GL = contextStrategy.ToConcrete<ConcreteGraphicsContext>().GL;

                _shaderHandle = GL.CreateShader(shaderType);
                GL.CheckGLError();

                if (shaderVersion == default) // Handle legacy MGFX
                {
                    if (this.GraphicsDevice.Adapter.Backend == GraphicsBackend.WebGL
                    &&  this.GraphicsDevice.GraphicsProfile >= GraphicsProfile.HiDef)
                    {
                        string glslCode = System.Text.Encoding.ASCII.GetString(shaderBytecode);
                        // GLES 3.00 is required for dFdx/dFdy
                        string glsl300esCode = ConvertGLSLToGLSL300es(shaderType, glslCode);

                        GL.ShaderSource(_shaderHandle, glsl300esCode);
                        GL.CheckGLError();
                    }
                    else
                    {
                        string glslCode = System.Text.Encoding.ASCII.GetString(shaderBytecode);
                        GL.ShaderSource(_shaderHandle, glslCode);
                        GL.CheckGLError();
                    }
                }
                else // Handle KNIFX
                {
                    if (this.GraphicsDevice.Adapter.Backend == GraphicsBackend.WebGL
                    &&  this.GraphicsDevice.GraphicsProfile >= GraphicsProfile.HiDef)
                    {
                        string glslCode = System.Text.Encoding.ASCII.GetString(shaderBytecode, 0, shaderBytecode.Length);
                        // GLES 3.00 is required for dFdx/dFdy
                        string glsl300esCode = ConvertGLSLToGLSL300es(shaderType, glslCode);

                        GL.ShaderSource(_shaderHandle, glsl300esCode);
                        GL.CheckGLError();
                    }
                    else
                    {
                        string glslCode = System.Text.Encoding.ASCII.GetString(shaderBytecode, 0, shaderBytecode.Length);
                        GL.ShaderSource(_shaderHandle, glslCode);
                        GL.CheckGLError();
                    }
                }

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
        }

        static Regex rgxOES = new Regex(
                @"^#extension GL_OES_standard_derivatives : enable\n", RegexOptions.Multiline);
        static Regex rgxPrecision = new Regex(
                @"precision mediump (float|int);", RegexOptions.Multiline);
        static Regex rgxAttribute = new Regex(
                @"^attribute(?=\s)", RegexOptions.Multiline);
        static Regex rgxVarying = new Regex(
                @"^varying(?=\s)", RegexOptions.Multiline);
        static Regex rgxFragColor = new Regex(
                @"^#define (\w+) gl_FragColor", RegexOptions.Multiline);
        static Regex rgxFragData = new Regex(
                @"^#define (\w+) gl_FragData\[(\d+)\]", RegexOptions.Multiline);
        static Regex rgxTexture = new Regex(
                @"texture(2D|3D|Cube)(?=\()", RegexOptions.Multiline);

        private string ConvertGLSLToGLSL300es(WebGLShaderType shaderType, string glslCode)
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
                        glslCode = rgxFragColor.Replace(glslCode, "out vec4 $1;");
                        glslCode = rgxFragData .Replace(glslCode, "layout(location=$2) out vec4 $1;");
                    }
                    break;
            }

            glslCode = rgxPrecision.Replace(glslCode, "precision highp $1;");
            glslCode = rgxAttribute.Replace(glslCode, "in");
            glslCode = rgxTexture.Replace(glslCode, "texture");

            glslCode = "#version 300 es\n"
                     + glslCode;

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
