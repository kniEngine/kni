// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Framework.Graphics
{
    // ARB_framebuffer_object implementation
    partial class GraphicsDevice
    {
        internal class FramebufferHelper
        {
            private GraphicsDevice _device;
            public bool SupportsInvalidateFramebuffer { get; private set; }
            public bool SupportsBlitFramebuffer { get; private set; }

            private IWebGLRenderingContext GL { get { return _device._glContext; } }

            internal FramebufferHelper(GraphicsDevice graphicsDevice)
            {
                this._device = graphicsDevice;

                //if (graphicsDevice.GraphicsCapabilities.SupportsFramebufferObjectARB
                //|| graphicsDevice.GraphicsCapabilities.SupportsFramebufferObjectEXT)
                // TODO: check for FramebufferObjectARB
                if (true)
                {
                    this.SupportsBlitFramebuffer = false;
                    this.SupportsInvalidateFramebuffer = false;
                }
                else
                {
                    throw new PlatformNotSupportedException(
                        "MonoGame requires either ARB_framebuffer_object or EXT_framebuffer_object." +
                        "Try updating your graphics drivers.");
                }
            }

            internal void GenRenderbuffer(out WebGLRenderbuffer renderbuffer)
            {
                renderbuffer = GL.CreateRenderbuffer();
                GraphicsExtensions.CheckGLError();
            }

            internal void BindRenderbuffer(WebGLRenderbuffer renderbuffer)
            {
                GL.BindRenderbuffer(WebGLRenderbufferType.RENDERBUFFER, renderbuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal void DeleteRenderbuffer(WebGLRenderbuffer renderbuffer)
            {
                renderbuffer.Dispose();
                GraphicsExtensions.CheckGLError();
            }

            internal void RenderbufferStorageMultisample(int samples, WebGLRenderbufferInternalFormat internalFormat, int width, int height)
            {
                if (samples > 0 /*&& GL.RenderbufferStorageMultisample != null*/)
                    throw new NotImplementedException();
                else
                    GL.RenderbufferStorage(WebGLRenderbufferType.RENDERBUFFER, internalFormat, width, height);
                GraphicsExtensions.CheckGLError();
            }

            internal void GenFramebuffer(out WebGLFramebuffer framebuffer)
            {
                framebuffer = GL.CreateFramebuffer();
                GraphicsExtensions.CheckGLError();
            }

            internal void BindFramebuffer(WebGLFramebuffer framebuffer)
            {
                GL.BindFramebuffer(WebGLFramebufferType.FRAMEBUFFER, framebuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal void BindReadFramebuffer(int readFramebuffer)
            {
                throw new NotImplementedException();
            }

            static readonly WebGLFramebufferAttachmentPoint[] FramebufferAttachements = {
                WebGLFramebufferAttachmentPoint.COLOR_ATTACHMENT0,
                WebGLFramebufferAttachmentPoint.DEPTH_ATTACHMENT,
                WebGLFramebufferAttachmentPoint.STENCIL_ATTACHMENT,
            };

            internal void InvalidateDrawFramebuffer()
            {
                System.Diagnostics.Debug.Assert(this.SupportsInvalidateFramebuffer);
                throw new NotImplementedException();
            }

            internal void InvalidateReadFramebuffer()
            {
                System.Diagnostics.Debug.Assert(this.SupportsInvalidateFramebuffer);
                throw new NotImplementedException();
            }

            internal void DeleteFramebuffer(WebGLFramebuffer framebuffer)
            {
                framebuffer.Dispose();
                GraphicsExtensions.CheckGLError();
            }

            internal void FramebufferTexture2D(WebGLFramebufferAttachmentPoint attachement, WebGLTextureTarget target, WebGLTexture texture, int level = 0, int samples = 0)
            {
                GL.FramebufferTexture2D(WebGLFramebufferType.FRAMEBUFFER, attachement, target, texture);
                GraphicsExtensions.CheckGLError();
            }

            internal void FramebufferRenderbuffer(WebGLFramebufferAttachmentPoint attachement, WebGLRenderbuffer renderbuffer)
            {
                GL.FramebufferRenderbuffer(WebGLFramebufferType.FRAMEBUFFER, attachement, WebGLRenderbufferType.RENDERBUFFER, renderbuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal void GenerateMipmap(WebGLTextureTarget target)
            {
                GL.GenerateMipmap(target);
                GraphicsExtensions.CheckGLError();
            }

            internal void BlitFramebuffer(int iColorAttachment, int width, int height)
            {
                throw new NotImplementedException();
            }

        }
    }
}
