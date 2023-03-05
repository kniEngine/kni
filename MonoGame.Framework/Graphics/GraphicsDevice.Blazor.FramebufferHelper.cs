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
                    this.SupportsBlitFramebuffer = false; // GL.BlitFramebuffer != null;
                    this.SupportsInvalidateFramebuffer = false; // GL.InvalidateFramebuffer != null;
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

        }
    }
}
