// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteRenderTarget2D : ConcreteTexture2D, IRenderTarget2DStrategy, IRenderTargetStrategy,
        IRenderTargetStrategyGL
    {
        private readonly DepthFormat _depthStencilFormat;
        internal int _multiSampleCount;
        private readonly RenderTargetUsage _renderTargetUsage;

        internal ConcreteRenderTarget2D(GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap, int arraySize, bool shared, RenderTargetUsage usage,
            SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount,
              Texture2D.SurfaceType surfaceType)
            : base(contextStrategy, width, height, mipMap, preferredSurfaceFormat, arraySize, shared,
                   isRenderTarget: true)
        {
            this._renderTargetUsage = usage;
            this._depthStencilFormat = preferredDepthFormat;

            if (surfaceType == Texture2D.SurfaceType.RenderTargetSwapChain)
            {
                // Texture will be created by the RenderTargetSwapChain.
                return;
            }

            PlatformConstructTexture2D_rt(contextStrategy, width, height, mipMap, preferredSurfaceFormat, shared);

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
        #endregion IRenderTarget2DStrategy


        WebGLTexture _glColorBuffer;
        WebGLRenderbuffer _glDepthBuffer;
        WebGLRenderbuffer _glStencilBuffer;

        #region IRenderTargetStrategyGL
        WebGLTexture IRenderTargetStrategyGL.GLTexture { get { return _glTexture; } }
        WebGLTextureTarget IRenderTargetStrategyGL.GLTarget { get { return _glTarget; } }
        WebGLTexture IRenderTargetStrategyGL.GLColorBuffer
        {
            get { return _glColorBuffer; }
            set { _glColorBuffer = value; }
        }
        WebGLRenderbuffer IRenderTargetStrategyGL.GLDepthBuffer
        {
            get { return _glDepthBuffer; }
            set { _glDepthBuffer = value; }
        }
        WebGLRenderbuffer IRenderTargetStrategyGL.GLStencilBuffer
        {
            get { return _glStencilBuffer; }
            set { _glStencilBuffer = value; }
        }

        WebGLTextureTarget IRenderTargetStrategyGL.GetFramebufferTarget(int arraySlice)
        {
            if (arraySlice != 0)
                throw new NotImplementedException("arraySlice is not implemented for Texture2D");

            return _glTarget;
        }
        #endregion IRenderTargetStrategyGL


        private void PlatformConstructTexture2D_rt(GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap, SurfaceFormat format, bool shared)
        {
            base.PlatformConstructTexture2D(contextStrategy, width, height, mipMap, format, shared);
        }
    }
}
