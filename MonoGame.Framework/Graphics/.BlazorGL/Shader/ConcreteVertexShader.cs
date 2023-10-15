// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Platform.Graphics
{
    public sealed class ConcreteVertexShader : ConcreteShader
    {

        internal ConcreteVertexShader(GraphicsContextStrategy contextStrategy, byte[] shaderBytecode, SamplerInfo[] samplers, int[] cBuffers, VertexAttribute[] attributes, ShaderProfileType profile)
            : base(contextStrategy, shaderBytecode, samplers, cBuffers, attributes, profile)
        {
            base.CreateShader(contextStrategy, WebGLShaderType.VERTEX, shaderBytecode);
        }

        internal void GetVertexAttributeLocations(GraphicsContextStrategy contextStrategy, WebGLProgram program)
        {
            var GL = contextStrategy.ToConcrete<ConcreteGraphicsContext>().GL;

            for (int i = 0; i < Attributes.Length; i++)
            {
                Attributes[i].location = GL.GetAttribLocation(program, Attributes[i].name);
                GL.CheckGLError();
            }
        }

        internal int GetAttributeLocation(VertexElementUsage usage, int index)
        {
            for (int i = 0; i < Attributes.Length; i++)
            {
                if ((Attributes[i].usage == usage) && (Attributes[i].index == index))
                    return Attributes[i].location;
            }
            return -1;
        }

        internal override void PlatformGraphicsContextLost()
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
