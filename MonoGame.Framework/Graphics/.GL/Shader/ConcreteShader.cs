// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.OpenGL;


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
            _hashKey = MonoGame.Framework.Utilities.Hash.ComputeHash(shaderBytecode);
         }

        internal void CreateShader(GraphicsContextStrategy contextStrategy, ShaderType shaderType, byte[] shaderBytecode)
        {
            var GL = contextStrategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

            _shaderHandle = GL.CreateShader(shaderType);
            GL.CheckGLError();
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

                throw new InvalidOperationException("Shader Compilation Failed");
            }
        }

        internal override void PlatformGraphicsContextLost()
        {
            var GL = OGL.Current;

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
                var GL = OGL.Current;

                if (GL.IsShader(_shaderHandle))
                {
                    GL.DeleteShader(_shaderHandle);
                    GL.CheckGLError();
                }
            }

            _shaderHandle = -1;

            base.Dispose(disposing);
        }
    }
}
