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
        protected readonly GraphicsContextStrategy _contextStrategy;

        ShaderVersion _shaderVersion;
        private SamplerInfo[] _samplers;
        private int[] _CBuffers;
        private VertexAttribute[] _attributes;

        public ShaderVersion ShaderModelVersion { get { return _shaderVersion; } }
        public SamplerInfo[] Samplers { get { return _samplers; } }
        public int[] CBuffers { get { return _CBuffers; } }
        public VertexAttribute[] Attributes { get { return _attributes; } }

        protected static readonly Dictionary<GraphicsProfile, ShaderVersion> MaxShaderVersions = new Dictionary<GraphicsProfile, ShaderVersion>()
        {
            { GraphicsProfile.Reach,   new ShaderVersion(2, 0) },
            { GraphicsProfile.HiDef,   new ShaderVersion(3, 0) },
            { GraphicsProfile.FL10_0,  new ShaderVersion(4, 0) },
            { GraphicsProfile.FL10_1,  new ShaderVersion(4, 1) },
            { GraphicsProfile.FL11_0,  new ShaderVersion(5, 0) },
            { GraphicsProfile.FL11_1,  new ShaderVersion(5, 0) },
        };

        protected ShaderStrategy(GraphicsContextStrategy contextStrategy, ShaderVersion shaderVersion, byte[] shaderBytecode, SamplerInfo[] samplers, int[] cBuffers, VertexAttribute[] attributes, ShaderProfileType profile)
            : base(contextStrategy)
        {
            _contextStrategy = contextStrategy;

            this._shaderVersion = shaderVersion;
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
