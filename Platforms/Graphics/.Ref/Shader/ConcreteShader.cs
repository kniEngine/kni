// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    public abstract class ConcreteShader : ShaderStrategy
    {
    
        internal ConcreteShader(GraphicsContextStrategy contextStrategy, ShaderVersion shaderVersion, byte[] shaderBytecode, SamplerInfo[] samplers, int[] cBuffers, VertexAttribute[] attributes)
            : base(contextStrategy, shaderVersion, shaderBytecode, samplers, cBuffers, attributes)
        {
            throw new PlatformNotSupportedException();

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
