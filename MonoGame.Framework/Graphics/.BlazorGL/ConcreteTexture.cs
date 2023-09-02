// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteTexture : GraphicsResourceStrategy, ITextureStrategy
    {
        private readonly GraphicsContextStrategy _contextStrategy;
        private readonly SurfaceFormat _format;        
        private readonly int _levelCount;

        internal ConcreteTexture(GraphicsContextStrategy contextStrategy, SurfaceFormat format, int levelCount)
        {
            this._contextStrategy = contextStrategy;
            this._format = format;
            this._levelCount = levelCount;
        }


        #region ITextureStrategy
        public SurfaceFormat Format { get { return _format; } }
        public int LevelCount { get { return _levelCount; } }
        #endregion #region ITextureStrategy


        internal WebGLTexture _glTexture;
        internal WebGLTextureTarget _glTarget;
        internal WebGLInternalFormat _glInternalFormat;
        internal WebGLFormat _glFormat;
        internal WebGLTexelType _glType;
        internal bool _glIsCompressedTexture;
        internal SamplerState _glLastSamplerState;


    }
}
