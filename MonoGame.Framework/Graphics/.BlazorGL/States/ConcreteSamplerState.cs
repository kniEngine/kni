// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteSamplerState : ResourceSamplerStateStrategy
    {

        internal ConcreteSamplerState(GraphicsContextStrategy contextStrategy, ISamplerStateStrategy source)
            : base(contextStrategy, source)
        {
        }

      internal void PlatformApplyState(GraphicsContext context, WebGLTextureTarget target, bool useMipmaps = false)
        {
            Debug.Assert(GraphicsDevice == ((IPlatformGraphicsContext)context).DeviceStrategy.Device, "The state was created for a different device!");

            var GL = ((IPlatformGraphicsContext)context).Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            switch (Filter)
            {
                case TextureFilter.Point:
                    if (base.GraphicsDeviceStrategy.Capabilities.SupportsTextureFilterAnisotropic)
                    {
                        throw new NotImplementedException();
                    }
                    GL.TexParameter(target, WebGLTexParamName.TEXTURE_MIN_FILTER, (useMipmaps ? WebGLTexParam.NEAREST_MIPMAP_NEAREST : WebGLTexParam.NEAREST));
                    GL.CheckGLError();
                    GL.TexParameter(target, WebGLTexParamName.TEXTURE_MAG_FILTER, WebGLTexParam.NEAREST);
                    GL.CheckGLError();
                    break;
                case TextureFilter.Linear:
                    if (base.GraphicsDeviceStrategy.Capabilities.SupportsTextureFilterAnisotropic)
                    {
                        throw new NotImplementedException();
                    }
                    GL.TexParameter(target, WebGLTexParamName.TEXTURE_MIN_FILTER, (useMipmaps ? WebGLTexParam.LINEAR_MIPMAP_LINEAR : WebGLTexParam.LINEAR));
                    GL.CheckGLError();
                    GL.TexParameter(target, WebGLTexParamName.TEXTURE_MAG_FILTER, WebGLTexParam.LINEAR);
                    GL.CheckGLError();
                    break;
                case TextureFilter.Anisotropic:
                    if (base.GraphicsDeviceStrategy.Capabilities.SupportsTextureFilterAnisotropic)
                    {
                        throw new NotImplementedException();
                    }
                    GL.TexParameter(target, WebGLTexParamName.TEXTURE_MIN_FILTER, (useMipmaps ? WebGLTexParam.LINEAR_MIPMAP_LINEAR : WebGLTexParam.LINEAR));
                    GL.CheckGLError();
                    GL.TexParameter(target, WebGLTexParamName.TEXTURE_MAG_FILTER, WebGLTexParam.LINEAR);
                    GL.CheckGLError();
                    break;
                case TextureFilter.PointMipLinear:
                    throw new NotImplementedException();
                case TextureFilter.LinearMipPoint:
                    throw new NotImplementedException();
                case TextureFilter.MinLinearMagPointMipLinear:
                    throw new NotImplementedException();
                case TextureFilter.MinLinearMagPointMipPoint:
                    throw new NotImplementedException();
                case TextureFilter.MinPointMagLinearMipLinear:
                    throw new NotImplementedException();
                case TextureFilter.MinPointMagLinearMipPoint:
                    throw new NotImplementedException();

                default:
                    throw new NotSupportedException();
            }

            // Set up texture addressing.
            GL.TexParameter(target, WebGLTexParamName.TEXTURE_WRAP_S, ToGLTextureAddressMode(AddressU));
            GL.CheckGLError();
            GL.TexParameter(target, WebGLTexParamName.TEXTURE_WRAP_T, ToGLTextureAddressMode(AddressV));
            GL.CheckGLError();

            if (base.GraphicsDeviceStrategy.Capabilities.SupportsTextureMaxLevel)
            {
                if (this.MaxMipLevel > 0)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    throw new NotImplementedException();
                }
                GL.CheckGLError();
            }
        }

        private WebGLTexParam ToGLTextureAddressMode(TextureAddressMode textureAddressMode)
        {
            switch (textureAddressMode)
            {
                case TextureAddressMode.Clamp:
                    return WebGLTexParam.CLAMP_TO_EDGE;
                case TextureAddressMode.Wrap:
                    return WebGLTexParam.REPEAT;
                case TextureAddressMode.Mirror:
                    return WebGLTexParam.MIRRORED_REPEAT;
                case TextureAddressMode.Border:
                    throw new NotImplementedException();

                default:
                    throw new ArgumentException("No support for " + textureAddressMode);
            }
        }
 
        protected override void PlatformGraphicsContextLost()
        {
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            base.Dispose(disposing);
        }
    }

}
