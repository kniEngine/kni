// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.OpenGL;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteRenderTarget3D : ConcreteTexture3D, IRenderTarget3DStrategy, IRenderTargetStrategy,
        IRenderTargetStrategyGL
    {
        private readonly DepthFormat _depthStencilFormat;
        internal int _multiSampleCount;
        private readonly RenderTargetUsage _renderTargetUsage;

        internal ConcreteRenderTarget3D(GraphicsContextStrategy contextStrategy, int width, int height, int depth, bool mipMap, RenderTargetUsage usage,
            SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat)
            : base(contextStrategy, width, height, depth, mipMap, preferredSurfaceFormat,
                   isRenderTarget: true)
        {
            this._renderTargetUsage = usage;
            this._depthStencilFormat = preferredDepthFormat;
        }


        #region IRenderTargetStrategy
        public DepthFormat DepthStencilFormat
        {
            get { return _depthStencilFormat; }
        }

        public int MultiSampleCount
        {
            get { return _multiSampleCount; }
        }

        public RenderTargetUsage RenderTargetUsage
        {
            get { return _renderTargetUsage; }
        }
        #endregion #region IRenderTarget2DStrategy


        int _glColorBuffer;
        int _glDepthBuffer;
        int _glStencilBuffer;

        #region IRenderTargetStrategyGL
        int IRenderTargetStrategyGL.GLTexture { get { return _glTexture; } }
        TextureTarget IRenderTargetStrategyGL.GLTarget { get { return _glTarget; } }
        int IRenderTargetStrategyGL.GLColorBuffer
        {
            get { return _glColorBuffer; }
            set { _glColorBuffer = value; }
        }
        int IRenderTargetStrategyGL.GLDepthBuffer
        {
            get { return _glDepthBuffer; }
            set { _glDepthBuffer = value; }
        }
        int IRenderTargetStrategyGL.GLStencilBuffer
        {
            get { return _glStencilBuffer; }
            set { _glStencilBuffer = value; }
        }

        TextureTarget IRenderTargetStrategyGL.GetFramebufferTarget(int arraySlice)
        {
            if (arraySlice != 0)
                throw new NotImplementedException("arraySlice is not implemented for Texture2D");

            return _glTarget;
        }
        #endregion #region IRenderTargetStrategyGL


    }
}
