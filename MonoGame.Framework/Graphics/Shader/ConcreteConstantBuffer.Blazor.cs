// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Framework.Graphics
{

    internal sealed class ConcreteConstantBufferStrategy : ConstantBufferStrategy
    {
        private IWebGLRenderingContext GL { get { return GraphicsDevice._glContext; } }

        private ShaderProgram _shaderProgram = null;
        private WebGLUniformLocation _location;

        static ConcreteConstantBufferStrategy _lastConstantBufferApplied = null;

        /// <summary>
        /// A hash value which can be used to compare constant buffers.
        /// </summary>
        internal int HashKey { get; private set; }


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

        internal override void PlatformInitialize()
        {
            var data = new byte[_parameters.Length];
            for (var i = 0; i < _parameters.Length; i++)
            {
                unchecked
                {
                    data[i] = (byte)(_parameters[i] | _offsets[i]);
                }
            }

            HashKey = MonoGame.Framework.Utilities.Hash.ComputeHash(data);
        }

        internal override void PlatformClear()
        {
            // Force the uniform location to be looked up again
            _shaderProgram = null;
        }

        internal unsafe override void PlatformApply(ShaderStage stage, int slot)
        {
            System.Diagnostics.Debug.Assert(slot == 0);

            // NOTE: We assume here the program has
            // already been set on the device.
            ShaderProgram program = GraphicsDevice.PlatformShaderProgram;

            // If the program changed then lookup the
            // uniform again and apply the state.
            if (_shaderProgram != program)
            {
                var location = program.GetUniformLocation(_name);
                if (location == null)
                    return;

                _shaderProgram = program;
                _location = location;
                _dirty = true;
            }

            // If the shader program is the same, the effect may still be different and have different values in the buffer
            if (!Object.ReferenceEquals(this, _lastConstantBufferApplied))
                _dirty = true;

            // If the buffer content hasn't changed then we're
            // done... use the previously set uniform state.
            if (!_dirty)
                return;

            fixed (void* bytePtr = _buffer)
            {
                // TODO: We need to know the type of buffer float/int/bool
                // and cast this correctly... else it doesn't work as i guess
                // GL is checking the type of the uniform.

                GL.Uniform4fv(_location, _buffer);
                GraphicsExtensions.CheckGLError();
            }

            // Clear the dirty flag.
            _dirty = false;

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
