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
            &&  shaderVersion > maxVersion)
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
                    int bytecodeOffset;

                    if (this.GraphicsDevice.Adapter.Backend == GraphicsBackend.WebGL
                    &&  this.GraphicsDevice.GraphicsProfile >= GraphicsProfile.HiDef)
                    {
                        bytecodeOffset = FindShaderByteCode(shaderBytecode, major:3, minor:0, es:true);
                    }
                    else
                    {
                        bytecodeOffset = FindShaderByteCode(shaderBytecode, major:0, minor:0, es:false);
                    }

                    int bytecodeLength = BitConverter.ToInt16(shaderBytecode, bytecodeOffset); bytecodeOffset += 2;
                    string glslCode = System.Text.Encoding.ASCII.GetString(shaderBytecode, bytecodeOffset, bytecodeLength);
                    GL.ShaderSource(_shaderHandle, glslCode);
                    GL.CheckGLError();
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

        private int FindShaderByteCode(byte[] shaderBytecode, int major, int minor, bool es)
        {
            int pos = 0;
            
            short reserved0 = BitConverter.ToInt16(shaderBytecode, pos); pos += 2;
            if (reserved0 != 0)
                throw new Exception("Invalid shader bytecode");

            short count = BitConverter.ToInt16(shaderBytecode, pos); pos += 2;

            for (int i = 0; i < count; i++)
            {
                int major0 = shaderBytecode[pos]; pos += 1;
                int minor0 = shaderBytecode[pos]; pos += 1;
                bool es0 = BitConverter.ToBoolean(shaderBytecode, pos); pos += 1;
                int bytecodeOffset0 = BitConverter.ToInt32(shaderBytecode, pos); pos += 4;

                if (major == major0
                &&  minor == minor0
                &&  es == es0)
                {
                    return bytecodeOffset0;
                }
            }

            throw new InvalidOperationException("GLSL bytecode not found.");
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
