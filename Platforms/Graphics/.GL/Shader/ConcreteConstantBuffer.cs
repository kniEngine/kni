// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.OpenGL;


namespace Microsoft.Xna.Platform.Graphics
{

    internal sealed class ConcreteConstantBuffer : ConstantBufferStrategy
    {
        private ShaderProgram _shaderProgram = null;
        private int _location;

        static ConcreteConstantBuffer _lastConstantBufferApplied = null;


        public ConcreteConstantBuffer(GraphicsContextStrategy contextStrategy, string name, int[] parameters, int[] offsets, int sizeInBytes, bool integersAsFloats)
            : base(contextStrategy, name, parameters, offsets, sizeInBytes, integersAsFloats)
        {

        }

        private ConcreteConstantBuffer(ConcreteConstantBuffer source)
            : base(source)
        {
        }

        public override object Clone()
        {
            return new ConcreteConstantBuffer(this);
        }

        internal unsafe void PlatformApply(ConcreteGraphicsContextGL ccontextStrategy, ShaderProgram shaderProgram, int slot)
        {
            System.Diagnostics.Debug.Assert(slot == 0);

            bool isSameShaderProgram = _shaderProgram == shaderProgram;
            if (!isSameShaderProgram)
            {
                // If the program changed then lookup the uniform location again.
                int location = ccontextStrategy.GetUniformLocation(shaderProgram, Name);
                if (location == -1)
                    return;

                _shaderProgram = shaderProgram;
                _location = location;
            }

            if (base.Dirty
            // If the shader program changed then apply the buffer.
            ||  !isSameShaderProgram
            // If the shader program is the same, the effect may still be different and have different values in the buffer
            ||  !Object.ReferenceEquals(this, ConcreteConstantBuffer._lastConstantBufferApplied)
            )
            {
                var GL = ccontextStrategy.GL;

                fixed (void* bytePtr = this.BufferData)
                {
                    // TODO: We need to know the type of buffer float/int/bool
                    // and cast this correctly... else it doesn't work as i guess
                    // GL is checking the type of the uniform.

                    System.Diagnostics.Debug.Assert((this.BufferData.Length % 16) == 0);
                    GL.Uniform4(_location, this.BufferData.Length >> 4, (Vector4*)bytePtr);
                    GL.CheckGLError();
                }

               base.Dirty = false;
                ConcreteConstantBuffer._lastConstantBufferApplied = this;
            }
        }

        public override void PlatformContextLost()
        {
            // Force the uniform location to be looked up again
            _shaderProgram = null;
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
