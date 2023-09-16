// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Diagnostics;
using Microsoft.Xna.Platform.Graphics;
using nkast.Wasm.Canvas.WebGL;

namespace Microsoft.Xna.Framework.Graphics
{
    internal partial class Shader
    {
        // The shader handle.
        private WebGLShader _shaderHandle = null;

        // We keep this around for recompiling on context lost and debugging.
        private string _glslCode;

        private void PlatformConstructShader(ShaderStage stage, byte[] shaderBytecode)
        {
            _glslCode = System.Text.Encoding.ASCII.GetString(shaderBytecode);

            _strategy._hashKey = MonoGame.Framework.Utilities.Hash.ComputeHash(shaderBytecode);
        }

        internal WebGLShader GetShaderHandle()
        {
            // If the shader has already been created then return it.
            if (_shaderHandle != null)
                return _shaderHandle;

            var GL = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            //
            _shaderHandle = GL.CreateShader(Stage == ShaderStage.Vertex ? WebGLShaderType.VERTEX : WebGLShaderType.FRAGMENT);
            GraphicsExtensions.CheckGLError();
            GL.ShaderSource(_shaderHandle, _glslCode);
            GraphicsExtensions.CheckGLError();
            GL.CompileShader(_shaderHandle);
            GraphicsExtensions.CheckGLError();
            bool compiled = false;
            compiled = GL.GetShaderParameter(_shaderHandle, WebGLShaderStatus.COMPILE);
            GraphicsExtensions.CheckGLError();
            if (compiled != true)
            {
                var log = GL.GetShaderInfoLog(_shaderHandle);
                _shaderHandle.Dispose();
                _shaderHandle = null;
                throw new InvalidOperationException("Shader Compilation Failed");
            }

            return _shaderHandle;
        }

        internal void GetVertexAttributeLocations(WebGLProgram program)
        {
            var GL = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            for (int i = 0; i < Attributes.Length; i++)
            {
                Attributes[i].location = GL.GetAttribLocation(program, Attributes[i].name);
                GraphicsExtensions.CheckGLError();
            }
        }

        internal int GetAttribLocation(VertexElementUsage usage, int index)
        {
            for (int i = 0; i < Attributes.Length; i++)
            {
                if ((Attributes[i].usage == usage) && (Attributes[i].index == index))
                    return Attributes[i].location;
            }
            return -1;
        }

        internal void ApplySamplerTextureUnits(WebGLProgram program)
        {
            var GL = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            // Assign the texture unit index to the sampler uniforms.
            foreach (var sampler in Samplers)
            {
                var loc = GL.GetUniformLocation(program, sampler.name);
                GraphicsExtensions.CheckGLError();
                if (loc != null)
                {
                    GL.Uniform1i(loc, sampler.textureSlot);
                    GraphicsExtensions.CheckGLError();
                }
            }
        }

        private void PlatformGraphicsDeviceResetting()
        {
            throw new NotImplementedException();
        }
    }
}
