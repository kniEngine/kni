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
    public sealed class ConcretePixelShader : ConcreteShader
    {

        internal ConcretePixelShader(GraphicsContextStrategy contextStrategy, byte[] shaderBytecode, SamplerInfo[] samplers, int[] cBuffers, VertexAttribute[] attributes, ShaderProfileType profile)
            : base(contextStrategy, shaderBytecode, samplers, cBuffers, attributes, profile)
        {
            base.CreateShader(contextStrategy, ShaderType.FragmentShader, shaderBytecode);
        }

        internal void ApplySamplerTextureUnits(GraphicsContextStrategy contextStrategy, int program)
        {
            var GL = contextStrategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

            // Assign the texture unit index to the sampler uniforms.
            foreach (SamplerInfo sampler in Samplers)
            {
                int loc = GL.GetUniformLocation(program, sampler.GLsamplerName);
                GL.CheckGLError();
                if (loc != -1)
                {
                    GL.Uniform1(loc, sampler.textureSlot);
                    GL.CheckGLError();
                }
            }
        }

        protected override void PlatformGraphicsContextLost()
        {

            base.PlatformGraphicsContextLost();
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
