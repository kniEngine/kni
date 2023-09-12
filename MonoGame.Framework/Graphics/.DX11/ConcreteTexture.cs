// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteTexture : GraphicsResourceStrategy, ITextureStrategy
    {
        private readonly GraphicsContextStrategy _contextStrategy;
        private readonly SurfaceFormat _format;        
        private readonly int _levelCount;

        internal ConcreteTexture(GraphicsContextStrategy contextStrategy, SurfaceFormat format, int levelCount)
            : base(contextStrategy)
        {
            this._contextStrategy = contextStrategy;
            this._format = format;
            this._levelCount = levelCount;
        }


        #region ITextureStrategy
        public SurfaceFormat Format { get { return _format; } }
        public int LevelCount { get { return _levelCount; } }
        #endregion ITextureStrategy


        internal D3D11.Resource _texture;
        internal D3D11.ShaderResourceView _resourceView;

        internal D3D11.Resource GetTexture()
        {
            System.Diagnostics.Debug.Assert(_texture != null);

            return _texture;
        }

        internal D3D11.ShaderResourceView GetShaderResourceView()
        {
            System.Diagnostics.Debug.Assert(_resourceView != null);

            return _resourceView;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DX.Utilities.Dispose(ref _resourceView);
                DX.Utilities.Dispose(ref _texture);
            }

            base.Dispose(disposing);
        }
    }
}
