// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteRenderTarget3D : ConcreteTexture3D, IRenderTarget3DStrategy, IRenderTargetStrategy,
        IRenderTargetStrategyGL
    {
        private readonly DepthFormat _depthStencilFormat;
        internal int _multiSampleCount;
        private readonly RenderTargetUsage _renderTargetUsage;

        internal ConcreteRenderTarget3D(GraphicsContextStrategy contextStrategy, int width, int height, int depth, bool mipMap, RenderTargetUsage usage,
            SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
            : base(contextStrategy, width, height, depth, mipMap, preferredSurfaceFormat,
                   isRenderTarget: true)
        {
            this._renderTargetUsage = usage;
            this._depthStencilFormat = preferredDepthFormat;

            int maxMultiSampleCount = contextStrategy.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().GetMaxMultiSampleCount(contextStrategy.Context.DeviceStrategy.PresentationParameters.BackBufferFormat);
            this._multiSampleCount = contextStrategy.Context.DeviceStrategy.GetClampedMultiSampleCount(this.Format, preferredMultiSampleCount, maxMultiSampleCount);

            PlatformConstructTexture3D_rt(contextStrategy, width, height, depth, mipMap, preferredSurfaceFormat);

            // If we don't need a depth buffer then we're done.
            if (preferredDepthFormat != DepthFormat.None)
                PlatformConstructRenderTarget3D(contextStrategy, width, height, depth, mipMap, preferredDepthFormat, _multiSampleCount);
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
            return _glTarget;
        }
        #endregion IRenderTargetStrategyGL


        private void PlatformConstructTexture3D_rt(GraphicsContextStrategy contextStrategy, int width, int height, int depth, bool mipMap, SurfaceFormat format)
        {
            base.PlatformConstructTexture3D(contextStrategy, width, height, depth, mipMap, format);
        }

        private void PlatformConstructRenderTarget3D(GraphicsContextStrategy contextStrategy, int width, int height, int depth, bool mipMap,
            DepthFormat preferredDepthFormat, int multiSampleCount)
        {
            throw new NotImplementedException();
        }
    }
}
