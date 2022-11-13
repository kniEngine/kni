// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Framework.Graphics
{
    // ARB_framebuffer_object implementation
    partial class GraphicsDevice
    {
        internal class FramebufferHelper
        {
            private static FramebufferHelper _instance;
            private GraphicsDevice _device;

            private IWebGLRenderingContext GL { get { return _device._glContext; } }

            public static FramebufferHelper Create(GraphicsDevice device)
            {
                //if (devic.GraphicsCapabilities.SupportsFramebufferObjectARB || devic.GraphicsCapabilities.SupportsFramebufferObjectEXT)
                // TODO: check for FramebufferObjectARB
                if (true)
                {
                    _instance = new FramebufferHelper(device);
                }
                else
                {
                    throw new PlatformNotSupportedException(
                        "MonoGame requires either ARB_framebuffer_object or EXT_framebuffer_object." +
                        "Try updating your graphics drivers.");
                }

                return _instance;
            }

            public static FramebufferHelper Get()
            {
                if (_instance == null)
                    throw new InvalidOperationException("The FramebufferHelper has not been created yet!");
                return _instance;
            }

            public bool SupportsInvalidateFramebuffer { get; private set; }

            public bool SupportsBlitFramebuffer { get; private set; }

            internal FramebufferHelper(GraphicsDevice graphicsDevice)
            {
                this._device = graphicsDevice;

                this.SupportsBlitFramebuffer = false;
                this.SupportsInvalidateFramebuffer = false;
            }

            internal virtual void GenRenderbuffer(out WebGLRenderbuffer renderbuffer)
            {
                renderbuffer = GL.CreateRenderbuffer();
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void BindRenderbuffer(WebGLRenderbuffer renderbuffer)
            {
                GL.BindRenderbuffer(WebGLRenderbufferType.RENDERBUFFER, renderbuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void DeleteRenderbuffer(WebGLRenderbuffer renderbuffer)
            {
                renderbuffer.Dispose();
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void RenderbufferStorageMultisample(int samples, WebGLRenderbufferInternalFormat internalFormat, int width, int height)
            {
                if (samples > 0 /*&& GL.RenderbufferStorageMultisample != null*/)
                    throw new NotImplementedException();
                else
                    GL.RenderbufferStorage(WebGLRenderbufferType.RENDERBUFFER, internalFormat, width, height);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void GenFramebuffer(out WebGLFramebuffer framebuffer)
            {
                framebuffer = GL.CreateFramebuffer();
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void BindFramebuffer(WebGLFramebuffer framebuffer)
            {
                GL.BindFramebuffer(WebGLFramebufferType.FRAMEBUFFER, framebuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void BindReadFramebuffer(int readFramebuffer)
            {
                throw new NotImplementedException();
            }

            static readonly WebGLFramebufferAttachmentPoint[] FramebufferAttachements = {
                WebGLFramebufferAttachmentPoint.COLOR_ATTACHMENT0,
                WebGLFramebufferAttachmentPoint.DEPTH_ATTACHMENT,
                WebGLFramebufferAttachmentPoint.STENCIL_ATTACHMENT,
            };

            internal virtual void InvalidateDrawFramebuffer()
            {
                throw new NotImplementedException();
            }

            internal virtual void InvalidateReadFramebuffer()
            {
                throw new NotImplementedException();
            }

            internal virtual void DeleteFramebuffer(WebGLFramebuffer framebuffer)
            {
                framebuffer.Dispose();
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void FramebufferTexture2D(WebGLFramebufferAttachmentPoint attachement, WebGLTextureTarget target, WebGLTexture texture, int level = 0, int samples = 0)
            {
                GL.FramebufferTexture2D(WebGLFramebufferType.FRAMEBUFFER, attachement, target, texture);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void FramebufferRenderbuffer(WebGLFramebufferAttachmentPoint attachement, WebGLRenderbuffer renderbuffer)
            {
                GL.FramebufferRenderbuffer(WebGLFramebufferType.FRAMEBUFFER, attachement, WebGLRenderbufferType.RENDERBUFFER, renderbuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void GenerateMipmap(WebGLTextureTarget target)
            {
                GL.GenerateMipmap(target);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void BlitFramebuffer(int iColorAttachment, int width, int height)
            {
                throw new NotImplementedException();

            }

            internal virtual void CheckFramebufferStatus()
            {
                var status = GL.CheckFramebufferStatus(WebGLFramebufferType.FRAMEBUFFER);
                switch (status)
                {
                    case WebGLFramebufferStatus.FRAMEBUFFER_COMPLETE:
                        return;
                    case WebGLFramebufferStatus.FRAMEBUFFER_INCOMPLETE_ATTACHMENT:
                        throw new InvalidOperationException("Not all framebuffer attachment points are framebuffer attachment complete.");
                    case WebGLFramebufferStatus.FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT:
                        throw new InvalidOperationException("No images are attached to the framebuffer.");
                    case WebGLFramebufferStatus.FRAMEBUFFER_UNSUPPORTED:
                        throw new InvalidOperationException("The combination of internal formats of the attached images violates an implementation-dependent set of restrictions.");
                    case WebGLFramebufferStatus.FRAMEBUFFER_INCOMPLETE_DIMENSIONS:
                        throw new InvalidOperationException("Not all attached images have the same dimensions.");

                    default:
                        throw new InvalidOperationException("Framebuffer Incomplete.");
                }
            }
        }
    }
}
