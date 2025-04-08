﻿// Copyright (C)2022 Nick Kastellanos

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

        internal unsafe void PlatformApply(GraphicsContextStrategy contextStrategy, int slot)
        {
            System.Diagnostics.Debug.Assert(slot == 0);

            var GL = contextStrategy.ToConcrete<ConcreteGraphicsContext>().GL;

            // NOTE: We assume here the program has
            // already been set on the device.
            ShaderProgram program = contextStrategy.ToConcrete<ConcreteGraphicsContext>().ShaderProgram;

            // If the program changed then lookup the
            // uniform again and apply the state.
            if (_shaderProgram != program)
            {
                WebGLUniformLocation location = contextStrategy.ToConcrete<ConcreteGraphicsContext>().GetUniformLocation(program, Name);
                if (location == null)
                    return;

                _shaderProgram = program;
                _location = location;
                Dirty = true;
            }

            // If the shader program is the same, the effect may still be different and have different values in the buffer
            if (!Object.ReferenceEquals(this, _lastConstantBufferApplied))
                Dirty = true;

            // If the buffer content hasn't changed then we're
            // done... use the previously set uniform state.
            if (!Dirty)
                return;

            fixed (void* bytePtr = this.BufferData)
            {
                // TODO: We need to know the type of buffer float/int/bool
                // and cast this correctly... else it doesn't work as i guess
                // GL is checking the type of the uniform.

                System.Diagnostics.Debug.Assert((this.BufferData.Length % 16) == 0);
                GL.Uniform4fv(_location, this.BufferData);
                GL.CheckGLError();
            }

            // Clear the dirty flag.
            Dirty = false;

            _lastConstantBufferApplied = this;
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
