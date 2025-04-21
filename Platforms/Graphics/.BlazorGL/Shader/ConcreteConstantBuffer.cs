// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Platform.Graphics
{

    internal sealed class ConcreteConstantBuffer : ConstantBufferStrategy
    {
        private ShaderProgram _shaderProgram = null;
        private WebGLUniformLocation _location;

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

        internal unsafe void PlatformApply(ConcreteGraphicsContext ccontextStrategy, ShaderProgram shaderProgram, int slot)
        {
            System.Diagnostics.Debug.Assert(slot == 0);

            var GL = ccontextStrategy.GL;

            // If the program changed then lookup the
            // uniform again and apply the state.
            if (_shaderProgram != shaderProgram)
            {
                WebGLUniformLocation location = ccontextStrategy.GetUniformLocation(shaderProgram, Name);
                if (location == null)
                    return;

                _shaderProgram = shaderProgram;
                _location = location;
                Dirty = true;
            }

            // If the shader program is the same, the effect may still be different and have different values in the buffer
            if (!Object.ReferenceEquals(this, ConcreteConstantBuffer._lastConstantBufferApplied))
                Dirty = true;

            if (base.Dirty)
            {
                fixed (void* bytePtr = this.BufferData)
                {
                    // TODO: We need to know the type of buffer float/int/bool
                    // and cast this correctly... else it doesn't work as i guess
                    // GL is checking the type of the uniform.

                    System.Diagnostics.Debug.Assert((this.BufferData.Length % 16) == 0);
                    GL.Uniform4fv(_location, this.BufferData);
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
