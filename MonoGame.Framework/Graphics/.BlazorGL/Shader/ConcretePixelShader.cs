// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Platform.Graphics
{
    public sealed class ConcretePixelShader : ConcreteShader
    {

        internal ConcretePixelShader(GraphicsContextStrategy contextStrategy, byte[] shaderBytecode, SamplerInfo[] samplers, int[] cBuffers, VertexAttribute[] attributes, ShaderProfileType profile)
            : base(contextStrategy, shaderBytecode, samplers, cBuffers, attributes, profile)
        {
            base.CreateShader(contextStrategy, WebGLShaderType.FRAGMENT, shaderBytecode);
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
