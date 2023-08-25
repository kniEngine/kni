﻿// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Framework.Graphics
{

    internal sealed class ConcreteConstantBufferStrategy : ConstantBufferStrategy
    {

        private ShaderProgram _shaderProgram = null;
        private WebGLUniformLocation _location;

        static ConcreteConstantBufferStrategy _lastConstantBufferApplied = null;


        public ConcreteConstantBufferStrategy(GraphicsDevice graphicsDevice, string name, int[] parameters, int[] offsets, int sizeInBytes)
            : base(graphicsDevice, name, parameters, offsets, sizeInBytes)
        {
            
        }

        private ConcreteConstantBufferStrategy(ConcreteConstantBufferStrategy source)
            : base(source)
        {
        
        }

        public override object Clone()
        {
            return new ConcreteConstantBufferStrategy(this);
        }

        internal override void PlatformClear()
        {
            // Force the uniform location to be looked up again
            _shaderProgram = null;
        }

        internal unsafe override void PlatformApply(GraphicsContextStrategy contextStrategy, ShaderStage stage, int slot)
        {
            System.Diagnostics.Debug.Assert(slot == 0);

            var GL = ((ConcreteGraphicsContext)contextStrategy).GL;

            // NOTE: We assume here the program has
            // already been set on the device.
            ShaderProgram program = ((ConcreteGraphicsContext)contextStrategy).ShaderProgram;

            // If the program changed then lookup the
            // uniform again and apply the state.
            if (_shaderProgram != program)
            {
                var location = ((ConcreteGraphicsContext)contextStrategy).GetUniformLocation(program, Name);
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

            fixed (void* bytePtr = Buffer)
            {
                // TODO: We need to know the type of buffer float/int/bool
                // and cast this correctly... else it doesn't work as i guess
                // GL is checking the type of the uniform.

                System.Diagnostics.Debug.Assert((Buffer.Length % 16) == 0);
                GL.Uniform4fv(_location, Buffer);
                GraphicsExtensions.CheckGLError();
            }

            // Clear the dirty flag.
            Dirty = false;

            _lastConstantBufferApplied = this;
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
