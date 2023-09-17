// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.OpenGL;


namespace Microsoft.Xna.Platform.Graphics
{
    public sealed class ConcretePixelShader : ConcreteShader
    {
        public override ShaderStage Stage { get { return ShaderStage.Pixel; } }

        internal ConcretePixelShader(GraphicsContextStrategy contextStrategy, byte[] shaderBytecode, SamplerInfo[] samplers, int[] cBuffers, VertexAttribute[] attributes, ShaderProfileType profile)
            : base(contextStrategy, shaderBytecode, samplers, cBuffers, attributes, profile)
        {
        }

        internal int GetPixelShaderHandle()
        {
            // If the shader has already been created then return it.
            if (base.ShaderHandle != -1)
                return base.ShaderHandle;

            base.CreateShader(ShaderType.FragmentShader);
            return base.ShaderHandle;
        }

        internal void ApplySamplerTextureUnits(int program)
        {
            // Assign the texture unit index to the sampler uniforms.
            foreach (SamplerInfo sampler in Samplers)
            {
                int loc = GL.GetUniformLocation(program, sampler.name);
                GraphicsExtensions.CheckGLError();
                if (loc != -1)
                {
                    GL.Uniform1(loc, sampler.textureSlot);
                    GraphicsExtensions.CheckGLError();
                }
            }
        }

        internal override void PlatformGraphicsDeviceResetting()
        {

            base.PlatformGraphicsDeviceResetting();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            base.Dispose(disposing);
        }
    }
}
