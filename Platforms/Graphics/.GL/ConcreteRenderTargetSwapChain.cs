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
                out _glType);

            int maxMultiSampleCount = ((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().GetMaxMultiSampleCount(((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.PresentationParameters.BackBufferFormat);
            _multiSampleCount = TextureHelpers.GetClampedMultiSampleCount(this.Format, preferredMultiSampleCount, maxMultiSampleCount);

            var gd = ((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.Device;
            var adapter = ((IPlatformGraphicsAdapter)gd.Adapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
            var GL = adapter.Ogl;

            _glTarget = OpenGL.TextureTarget.Texture2D;
            _glTexture = glTextureHandle.ToInt32();

            OpenGL.TextureTarget colorTextureTarget = OpenGL.TextureTarget.Texture2D;
            GL.BindTexture(colorTextureTarget, _glTexture);
            GL.CheckGLError();
            GL.TexParameteri(colorTextureTarget, OpenGL.TextureParameterName.TextureWrapS, (int)OpenGL.TextureWrapMode.ClampToEdge);
            GL.CheckGLError();
            GL.TexParameteri(colorTextureTarget, OpenGL.TextureParameterName.TextureWrapT, (int)OpenGL.TextureWrapMode.ClampToEdge);
            GL.CheckGLError();
            GL.TexParameteri(colorTextureTarget, OpenGL.TextureParameterName.TextureMinFilter, (int)OpenGL.TextureMinFilter.Linear);
            GL.CheckGLError();
            GL.TexParameteri(colorTextureTarget, OpenGL.TextureParameterName.TextureMagFilter, (int)OpenGL.TextureMagFilter.Linear);
            GL.CheckGLError();
            GL.BindTexture(colorTextureTarget, 0);
            GL.CheckGLError();


            //if (multisamples > 1 && glRenderbufferStorageMultisampleEXT != NULL
            //                     && glFramebufferTexture2DMultisampleEXT != NULL)
            //{

            //}
            //else
            {
                // Create depth buffer.
                ((IRenderTargetStrategyGL)this).GLDepthBuffer = GL.GenRenderbuffer();
                GL.BindRenderbuffer(OpenGL.RenderbufferTarget.Renderbuffer, ((IRenderTargetStrategyGL)this).GLDepthBuffer);
                GL.CheckGLError();
                GL.RenderbufferStorage(OpenGL.RenderbufferTarget.Renderbuffer, OpenGL.RenderbufferStorage.DepthComponent24, width, height);
                GL.CheckGLError();
                GL.BindRenderbuffer(OpenGL.RenderbufferTarget.Renderbuffer, 0);
                GL.CheckGLError();


                // Create the frame buffer.
                uint framebufferId = (uint)GL.GenFramebuffer();
                GL.BindFramebuffer(OpenGL.FramebufferTarget.Framebuffer, (int)framebufferId);
                GL.CheckGLError();
                
                GL.FramebufferRenderbuffer(
                    OpenGL.FramebufferTarget.Framebuffer,
                    OpenGL.FramebufferAttachment.DepthAttachment,
                    OpenGL.RenderbufferTarget.Renderbuffer,
                    ((IRenderTargetStrategyGL)this).GLDepthBuffer);
                GL.CheckGLError();

                GL.FramebufferTexture2D(
                    OpenGL.FramebufferTarget.Framebuffer, OpenGL.FramebufferAttachment.ColorAttachment0, OpenGL.TextureTarget.Texture2D, _glTexture, 0);
                GL.CheckGLError();
                OpenGL.FramebufferErrorCode renderFramebufferStatus = GL.CheckFramebufferStatus(OpenGL.FramebufferTarget.Framebuffer);
             
                GL.BindFramebuffer(OpenGL.FramebufferTarget.Framebuffer, 0);
                GL.CheckGLError();
                if (renderFramebufferStatus != OpenGL.FramebufferErrorCode.FramebufferComplete)
                {
                    Console.WriteLine(
                        "Incomplete frame buffer object: {0}",
                        "" //GL.FrameBufferStatusString(renderFramebufferStatus)
                        );
                    throw new Exception("Incomplete frame buffer");
                }

                GL.DeleteFramebuffer((int)framebufferId);
            }
        }

        public void Present()
        {
            lock (((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.SyncHandle)
            {
                throw new NotSupportedException();
            }
        }

        static readonly OpenGL.FramebufferAttachment[] InvalidateFramebufferAttachements =
        {
            OpenGL.FramebufferAttachment.DepthAttachment,
        };

        public override void ResolveSubresource(GraphicsContextStrategy contextStrategy)
        {
            var adapter = ((IPlatformGraphicsAdapter)this.GraphicsDevice.Adapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
            var GL = adapter.Ogl;

            // Discard the depth buffer, so the tiler won't need to write it back out to memory.
            GL.InvalidateFramebuffer(OpenGL.FramebufferTarget.Framebuffer, 1, InvalidateFramebufferAttachements);
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
