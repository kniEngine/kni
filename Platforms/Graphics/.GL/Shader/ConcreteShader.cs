// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.OpenGL;
using Microsoft.Xna.Platform.Graphics.Utilities;


namespace Microsoft.Xna.Platform.Graphics
{
    public abstract class ConcreteShader : ShaderStrategy
    {
        // The shader handle.
        private int _shaderHandle = -1;

        internal int ShaderHandle { get { return _shaderHandle; } }


        internal ConcreteShader(GraphicsContextStrategy contextStrategy, byte[] shaderBytecode, SamplerInfo[] samplers, int[] cBuffers, VertexAttribute[] attributes, ShaderProfileType profile)
            : base(contextStrategy, shaderBytecode, samplers, cBuffers, attributes, profile)
        {
            if (profile != ShaderProfileType.OpenGL_Mojo)
                throw new Exception("This effect was built for a different platform.");

            // TODO: precompute shader's hashKey in the processor.
            _hashKey = Hash.ComputeHash(shaderBytecode);
         }

        internal void CreateShader(GraphicsContextStrategy contextStrategy, ShaderType shaderType, byte[] shaderBytecode)
        {
            var GL = contextStrategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

            _shaderHandle = GL.CreateShader(shaderType);
            GL.CheckGLError();

            if (this.GraphicsDevice.GraphicsProfile >= GraphicsProfile.HiDef
            &&  this.GraphicsDevice.Adapter.Backend == GraphicsBackend.GLES)
            {
                string glslCode = System.Text.Encoding.ASCII.GetString(shaderBytecode);
                // GLES 3.00 is required for gl_FragData
                glslCode = ConvertGLES100ToGLES300(shaderType, glslCode);
                shaderBytecode = System.Text.Encoding.ASCII.GetBytes(glslCode);
            }

            GL.ShaderSource(_shaderHandle, shaderBytecode);
            GL.CheckGLError();
            GL.CompileShader(_shaderHandle);
            GL.CheckGLError();
            int compiled = 0;
            GL.GetShader(_shaderHandle, ShaderParameter.CompileStatus, out compiled);
            GL.CheckGLError();
            if (compiled != (int)Bool.True)
            {
                string log = GL.GetShaderInfoLog(_shaderHandle);
                Debug.WriteLine(log);

                if (!GraphicsDevice.IsDisposed)
                {
                    if (GL.IsShader(_shaderHandle))
                    {
                        GL.DeleteShader(_shaderHandle);
                        GL.CheckGLError();
                    }
                }
                _shaderHandle = -1;

                throw new InvalidOperationException("Shader Compilation Failed."
                    + Environment.NewLine + log);
            }
        }

        static Regex rgxOES = new Regex(
                @"^#extension GL_OES_standard_derivatives : enable", RegexOptions.Multiline);
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

        private string ConvertGLES100ToGLES300(ShaderType shaderType, string glslCode)
        {
            switch (shaderType)
            {
                case ShaderType.VertexShader:
                    {
                        glslCode = rgxVarying.Replace(glslCode, "out");
                    }
                    break;

                case ShaderType.FragmentShader:
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

            glslCode = "#version 300 es" + '\n' + glslCode;

            return glslCode;
        }

        protected override void PlatformGraphicsContextLost()
        {
            var GL = _contextStrategy.ToConcrete<ConcreteGraphicsContext>().GL;

            if (_shaderHandle != -1)
            {
                if (GL.IsShader(_shaderHandle))
                {
                    GL.DeleteShader(_shaderHandle);
                    GL.CheckGLError();
                }
                _shaderHandle = -1;
            }

            base.PlatformGraphicsContextLost();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            if (_shaderHandle != -1)
            {
                _contextStrategy.ToConcrete<ConcreteGraphicsContextGL>().BindDisposeContext();
                try
                {
                    var GL = _contextStrategy.ToConcrete<ConcreteGraphicsContext>().GL;

                    if (GL.IsShader(_shaderHandle))
                    {
                        GL.DeleteShader(_shaderHandle);
                        GL.CheckGLError();
                    }
                }
                finally
                {
                    _contextStrategy.ToConcrete<ConcreteGraphicsContextGL>().UnbindDisposeContext();
                }
            }

            _shaderHandle = -1;

            base.Dispose(disposing);
        }
    }
}
