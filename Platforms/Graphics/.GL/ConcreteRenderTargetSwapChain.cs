// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.OpenGL;
using Microsoft.Xna.Platform.Graphics.Utilities;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteRenderTargetSwapChain : ConcreteRenderTarget2D
        //,
    {
        internal IntPtr _glTextureHandle;
        internal PresentInterval _presentInterval;


        internal ConcreteRenderTargetSwapChain(GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap, RenderTargetUsage usage,
            IntPtr glTextureHandle, PresentInterval presentInterval,
            SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
            :base(contextStrategy, width, height, mipMap, 1, false, usage, preferredSurfaceFormat, preferredDepthFormat, preferredMultiSampleCount,
                  TextureSurfaceType.RenderTargetSwapChain)
        {
            _glTextureHandle = glTextureHandle;
            _presentInterval = presentInterval;

            _glTarget = TextureTarget.Texture2D;
            ConcreteTexture.ToGLSurfaceFormat(preferredSurfaceFormat, contextStrategy,
                out _glInternalFormat,
                out _glFormat,
                out _glType,
                out _glIsCompressedTexture);

            int maxMultiSampleCount = ((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().GetMaxMultiSampleCount(preferredSurfaceFormat);
            this._multiSampleCount = TextureHelpers.GetClampedMultiSampleCount(this.Format, preferredMultiSampleCount, maxMultiSampleCount);

            PlatformConstructTexture2D_rt(contextStrategy, width, height, mipMap, preferredSurfaceFormat, false);

            PlatformConstructRenderTarget2D(contextStrategy, width, height, mipMap, preferredDepthFormat, _multiSampleCount, false);
        }

        private void PlatformConstructTexture2D_rt(GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap, SurfaceFormat preferredSurfaceFormat, object shared)
        {
            var GL = contextStrategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

            _glTarget = TextureTarget.Texture2D;
            _glTexture = _glTextureHandle.ToInt32();

            TextureTarget colorTextureTarget = TextureTarget.Texture2D;
            GL.BindTexture(colorTextureTarget, _glTexture);
            GL.CheckGLError();
            GL.TexParameteri(colorTextureTarget, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.CheckGLError();
            GL.TexParameteri(colorTextureTarget, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.CheckGLError();
            GL.TexParameteri(colorTextureTarget, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.CheckGLError();
            GL.TexParameteri(colorTextureTarget, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.CheckGLError();
            GL.BindTexture(colorTextureTarget, 0);
            GL.CheckGLError();

        }

        private void PlatformConstructRenderTarget2D(GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap, DepthFormat preferredDepthFormat, int multiSampleCount, object shared)
        {
            var GL = contextStrategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

            IRenderTargetStrategyGL renderTargetGL = (IRenderTargetStrategyGL)this;

            if (multiSampleCount > 1)
            {
                RenderbufferStorage colorInternalFormat = RenderbufferStorage.RGBA8;

                renderTargetGL.GLColorBuffer = GL.GenRenderbuffer();
                GL.CheckGLError();
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderTargetGL.GLColorBuffer);
                GL.CheckGLError();
                if (multiSampleCount > 0)
                {
                    System.Diagnostics.Debug.Assert(GL.RenderbufferStorageMultisample != null);
                    GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, multiSampleCount, colorInternalFormat, width, height);
                    GL.CheckGLError();
                }
                else
                {
                    GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, colorInternalFormat, width, height);
                    GL.CheckGLError();
                }
            }

            if (preferredDepthFormat != DepthFormat.None)
            {
                RenderbufferStorage depthInternalFormat = RenderbufferStorage.DepthComponent16;
                RenderbufferStorage stencilInternalFormat = (RenderbufferStorage)0;
                switch (preferredDepthFormat)
                {
                    case DepthFormat.None:
                        break;

                    case DepthFormat.Depth16:
                        depthInternalFormat = RenderbufferStorage.DepthComponent16;
                        break;

#if GLES
                    case DepthFormat.Depth24:
                        if (contextStrategy.Capabilities.SupportsDepth24)
                            depthInternalFormat = RenderbufferStorage.DepthComponent24oes;
                        else if (contextStrategy.Capabilities.SupportsDepthNonLinear)
                            depthInternalFormat = RenderbufferStorage.DepthComponent16NonlinearNv;
                        else
                            depthInternalFormat = RenderbufferStorage.DepthComponent16;
                        break;

                    case DepthFormat.Depth24Stencil8:
                        if (contextStrategy.Capabilities.SupportsPackedDepthStencil)
                            depthInternalFormat = RenderbufferStorage.Depth24Stencil8oes;
                        else
                        {
                            if (contextStrategy.Capabilities.SupportsDepth24)
                                depthInternalFormat = RenderbufferStorage.DepthComponent24oes;
                            else if (contextStrategy.Capabilities.SupportsDepthNonLinear)
                                depthInternalFormat = RenderbufferStorage.DepthComponent16NonlinearNv;
                            else
                                depthInternalFormat = RenderbufferStorage.DepthComponent16;
                            stencilInternalFormat = RenderbufferStorage.StencilIndex8;
                        }
                        break;

#else
                    case DepthFormat.Depth24:
                        depthInternalFormat = RenderbufferStorage.DepthComponent24;
                        break;

                    case DepthFormat.Depth24Stencil8:
                        depthInternalFormat = RenderbufferStorage.Depth24Stencil8;
                        break;
#endif

                    default:
                        throw new InvalidOperationException("preferredDepthFormat");
                }

                if (depthInternalFormat != 0)
                {
                    // Create depth buffer.
                    renderTargetGL.GLDepthBuffer = GL.GenRenderbuffer();
                    GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderTargetGL.GLDepthBuffer);
                    GL.CheckGLError();
                    if (multiSampleCount > 1)
                    {
                        GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, multiSampleCount, depthInternalFormat, width, height);
                        GL.CheckGLError();
                    }
                    else
                    {
                        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, depthInternalFormat, width, height);
                        GL.CheckGLError();
                    }
                    GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
                    GL.CheckGLError();

                    if (preferredDepthFormat == DepthFormat.Depth24Stencil8)
                    {
                        renderTargetGL.GLStencilBuffer = renderTargetGL.GLDepthBuffer;
                        if (stencilInternalFormat != 0)
                        {
                            renderTargetGL.GLStencilBuffer = GL.GenRenderbuffer();
                            GL.CheckGLError();
                            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderTargetGL.GLStencilBuffer);
                            GL.CheckGLError();
                            if (multiSampleCount > 1)
                            {
                                System.Diagnostics.Debug.Assert(GL.RenderbufferStorageMultisample != null);
                                GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, multiSampleCount, stencilInternalFormat, width, height);
                                GL.CheckGLError();
                            }
                            else
                            {
                                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, stencilInternalFormat, width, height);
                                GL.CheckGLError();
                            }
                        }
                    }

                    // Create the frame buffer.
                    //uint framebufferId = (uint)GL.GenFramebuffer();
                    //GL.BindFramebuffer(FramebufferTarget.Framebuffer, (int)framebufferId);
                    //GL.CheckGLError();

                    //GL.FramebufferRenderbuffer(
                    //    FramebufferTarget.Framebuffer,
                    //    FramebufferAttachment.DepthAttachment,
                    //    RenderbufferTarget.Renderbuffer,
                    //    renderTargetGL.GLDepthBuffer);
                    //GL.CheckGLError();

                    //GL.FramebufferTexture2D(
                    //    FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _glTexture, 0);
                    //GL.CheckGLError();
                    //FramebufferErrorCode renderFramebufferStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);

                    //GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                    //GL.CheckGLError();
                    //if (renderFramebufferStatus != FramebufferErrorCode.FramebufferComplete)
                    //{
                    //    Console.WriteLine(
                    //        "Incomplete frame buffer object: {0}",
                    //        "" //GL.FrameBufferStatusString(renderFramebufferStatus)
                    //        );
                    //    throw new Exception("Incomplete frame buffer");
                    //}

                    //GL.DeleteFramebuffer((int)framebufferId);
                }

            }

        }

        public void Present()
        {
            lock (((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.SyncHandle)
            {
                throw new NotSupportedException();
            }
        }

        static readonly FramebufferAttachment[] InvalidateFramebufferAttachements =
        {
            FramebufferAttachment.DepthAttachment,
        };

        public override void ResolveSubresource(GraphicsContextStrategy contextStrategy)
        {
            var GL = contextStrategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

            // Discard the depth buffer, so the tiler won't need to write it back out to memory.
            GL.InvalidateFramebuffer(FramebufferTarget.Framebuffer, 1, InvalidateFramebufferAttachements);
            GL.CheckGLError();
            // We now let the resolve happen implicitly.

            // Using a multisampled SwapChainRenderTarget as a source Texture is not supported.
            //base.ResolveSubresource(contextStrategy);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // ...
            }

            var contextStrategy = ((IPlatformGraphicsContext)this.GraphicsDeviceStrategy.MainContext).Strategy;

            contextStrategy.ToConcrete<ConcreteGraphicsContextGL>().BindDisposeContext();
            try
            {
                var GL = contextStrategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

                IRenderTargetStrategyGL renderTargetGL = ((IRenderTargetStrategyGL)this);

                if (renderTargetGL.GLStencilBuffer != 0 && renderTargetGL.GLStencilBuffer != renderTargetGL.GLDepthBuffer)
                {
                    GL.DeleteRenderbuffer(renderTargetGL.GLStencilBuffer);
                    GL.CheckGLError();
                }
                if (renderTargetGL.GLDepthBuffer != 0)
                {
                    GL.DeleteRenderbuffer(renderTargetGL.GLDepthBuffer);
                    GL.CheckGLError();
                }

                contextStrategy.ToConcrete<ConcreteGraphicsContext>().PlatformUnbindRenderTarget(renderTargetGL);
            }
            finally
            {
                contextStrategy.ToConcrete<ConcreteGraphicsContextGL>().UnbindDisposeContext();
            }

            base.Dispose(disposing);
        }

    }
}
