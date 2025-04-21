// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
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


        public ConcreteConstantBuffer(GraphicsContextStrategy contextStrategy, string name, int[] parameters, int[] offsets, int sizeInBytes, ShaderProfileType profile)
            : base(contextStrategy, name, parameters, offsets, sizeInBytes, profile)
        {
            if (profile != ShaderProfileType.OpenGL_Mojo)
                throw new Exception("Effect profile '"+profile+"' is not compatible with the graphics backend '"+((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.Adapter.Backend+"'.");

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

            var GL = ccontextStrategy.GL;

            // If the program changed then lookup the uniform again and apply the state.
            bool isSameShaderProgram = _shaderProgram == shaderProgram;
            if (!isSameShaderProgram)
            {
                int location = ccontextStrategy.GetUniformLocation(shaderProgram, Name);
                if (location == -1)
                    return;

                _shaderProgram = shaderProgram;
                _location = location;
            }

            // If the shader program is the same, the effect may still be different and have different values in the buffer
            bool isLastConstantBufferApplied = Object.ReferenceEquals(this, ConcreteConstantBuffer._lastConstantBufferApplied);

            if (base.Dirty
            ||  !isSameShaderProgram
            ||  !isLastConstantBufferApplied
            )
            {
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
