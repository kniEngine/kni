// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using MonoGame.OpenGL;


namespace Microsoft.Xna.Framework.Graphics
{
    // ARB_framebuffer_object implementation
    partial class GraphicsDevice
    {
        internal class FramebufferHelper
        {
            public bool SupportsInvalidateFramebuffer { get; private set; }
            public bool SupportsBlitFramebuffer { get; private set; }

            internal FramebufferHelper(GraphicsDevice graphicsDevice)
            {
                if (graphicsDevice.GraphicsCapabilities.SupportsFramebufferObjectARB
                || graphicsDevice.GraphicsCapabilities.SupportsFramebufferObjectEXT)
                {
                    this.SupportsBlitFramebuffer = GL.BlitFramebuffer != null;
                    this.SupportsInvalidateFramebuffer = GL.InvalidateFramebuffer != null;
                }
                else
                {
                    throw new PlatformNotSupportedException(
                        "MonoGame requires either ARB_framebuffer_object or EXT_framebuffer_object." +
                        "Try updating your graphics drivers.");
                }
            }

            internal void GenRenderbuffer(out int renderbuffer)
            {
                GL.GenRenderbuffers(1, out renderbuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal void BindRenderbuffer(int renderbuffer)
            {
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderbuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal void DeleteRenderbuffer(int renderbuffer)
            {
                GL.DeleteRenderbuffers(1, ref renderbuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal void RenderbufferStorageMultisample(int samples, RenderbufferStorage internalFormat, int width, int height)
            {
                if (samples > 0 && GL.RenderbufferStorageMultisample != null)
                    GL.RenderbufferStorageMultisample(RenderbufferTarget.RenderbufferExt, samples, internalFormat, width, height);
                else
                    GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, internalFormat, width, height);
                GraphicsExtensions.CheckGLError();
            }

            internal void GenFramebuffer(out int framebuffer)
            {
                GL.GenFramebuffers(1, out framebuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal void BindFramebuffer(int framebuffer)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal void BindReadFramebuffer(int readFramebuffer)
            {
                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, readFramebuffer);
                GraphicsExtensions.CheckGLError();
            }

            static readonly FramebufferAttachment[] FramebufferAttachements = {
                FramebufferAttachment.ColorAttachment0,
                FramebufferAttachment.DepthAttachment,
                FramebufferAttachment.StencilAttachment,
            };

            internal void InvalidateDrawFramebuffer()
            {
                System.Diagnostics.Debug.Assert(this.SupportsInvalidateFramebuffer);
                GL.InvalidateFramebuffer(FramebufferTarget.Framebuffer, 3, FramebufferAttachements);
            }

            internal void InvalidateReadFramebuffer()
            {
                System.Diagnostics.Debug.Assert(this.SupportsInvalidateFramebuffer);
                GL.InvalidateFramebuffer(FramebufferTarget.Framebuffer, 3, FramebufferAttachements);
            }

            internal void DeleteFramebuffer(int framebuffer)
            {
                GL.DeleteFramebuffers(1, ref framebuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal void FramebufferTexture2D(FramebufferAttachment attachement, TextureTarget target, int texture, int level = 0, int samples = 0)
            {
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachement, target, texture, level);
                GraphicsExtensions.CheckGLError();
            }

        }
    }
}
