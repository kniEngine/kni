﻿// MonoGame - Copyright (C) The MonoGame Team
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
                throw new Exception("This effect was built for a different platform.");

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

            var GL = contextStrategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

            // NOTE: We assume here the program has
            // already been set on the device.
            ShaderProgram program = contextStrategy.ToConcrete<ConcreteGraphicsContext>().ShaderProgram;

            // If the program changed then lookup the
            // uniform again and apply the state.
            if (_shaderProgram != program)
            {
                int location = contextStrategy.ToConcrete<ConcreteGraphicsContext>().GetUniformLocation(program, Name);
                if (location == -1)
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
                GL.Uniform4(_location, this.BufferData.Length >> 4, (Vector4*)bytePtr);
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
