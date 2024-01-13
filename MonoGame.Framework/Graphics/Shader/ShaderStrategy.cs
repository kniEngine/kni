// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Platform.Graphics
{
    public interface IPlatformShader
    {
        ShaderStrategy Strategy { get; }
    }

    public abstract class ShaderStrategy : GraphicsResourceStrategy
    {
        internal readonly GraphicsContextStrategy _contextStrategy;

        private SamplerInfo[] _samplers;
        private int[] _CBuffers;
        private VertexAttribute[] _attributes;
        protected int _hashKey;

        public SamplerInfo[] Samplers { get { return _samplers; } }
        public int[] CBuffers { get { return _CBuffers; } }
        public VertexAttribute[] Attributes { get { return _attributes; } }
        internal int HashKey { get { return _hashKey; } }


        protected ShaderStrategy(GraphicsContextStrategy contextStrategy, byte[] shaderBytecode, SamplerInfo[] samplers, int[] cBuffers, VertexAttribute[] attributes, ShaderProfileType profile)
            : base(contextStrategy)
        {
            _contextStrategy = contextStrategy;

            this._samplers = samplers;
            this._CBuffers = cBuffers;
            this._attributes = attributes;

        }

        public T ToConcrete<T>() where T : ShaderStrategy
        {
            return (T)this;
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
