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

      internal void PlatformApplyState(ConcreteGraphicsContext cgraphicsContext, WebGLTextureTarget target, bool useMipmaps = false)
        {
            Debug.Assert(GraphicsDevice == ((IPlatformGraphicsContext)cgraphicsContext.Context).DeviceStrategy.Device, "The state was created for a different device!");

            var GL = cgraphicsContext.GL;

            // texture filtering
            float textureMaxAnisotropy;
            WebGLTexParam textureMinFilter;
            WebGLTexParam textureMaxFilter; 
            switch (Filter)
            {
                case TextureFilter.Point:
                    textureMinFilter = (useMipmaps ? WebGLTexParam.NEAREST_MIPMAP_NEAREST : WebGLTexParam.NEAREST);
                    textureMaxFilter = WebGLTexParam.NEAREST;
                    break;
                case TextureFilter.Linear:
                    textureMinFilter = (useMipmaps ? WebGLTexParam.LINEAR_MIPMAP_LINEAR : WebGLTexParam.LINEAR);
                    textureMaxFilter = WebGLTexParam.LINEAR;
                    break;
                case TextureFilter.Anisotropic:
                    textureMinFilter = (useMipmaps ? WebGLTexParam.LINEAR_MIPMAP_LINEAR : WebGLTexParam.LINEAR);
                    textureMaxFilter = WebGLTexParam.LINEAR;
                    break;
                case TextureFilter.PointMipLinear:
                    textureMinFilter = (useMipmaps ? WebGLTexParam.NEAREST_MIPMAP_LINEAR : WebGLTexParam.NEAREST);
                    textureMaxFilter = WebGLTexParam.NEAREST;
                    break;
                case TextureFilter.LinearMipPoint:
                    textureMinFilter = (useMipmaps ? WebGLTexParam.LINEAR_MIPMAP_NEAREST : WebGLTexParam.LINEAR);
                    textureMaxFilter = WebGLTexParam.LINEAR;
                    break;
                case TextureFilter.MinLinearMagPointMipLinear:
                    textureMinFilter = (useMipmaps ? WebGLTexParam.LINEAR_MIPMAP_LINEAR : WebGLTexParam.LINEAR);
                    textureMaxFilter = WebGLTexParam.NEAREST;
                    break;
                case TextureFilter.MinLinearMagPointMipPoint:
                    textureMinFilter = (useMipmaps ? WebGLTexParam.LINEAR_MIPMAP_NEAREST : WebGLTexParam.LINEAR);
                    textureMaxFilter = WebGLTexParam.NEAREST;
                    break;
                case TextureFilter.MinPointMagLinearMipLinear:
                    textureMinFilter = (useMipmaps ? WebGLTexParam.NEAREST_MIPMAP_LINEAR : WebGLTexParam.NEAREST);
                    textureMaxFilter = WebGLTexParam.LINEAR;
                    break;
                case TextureFilter.MinPointMagLinearMipPoint:
                    textureMinFilter = (useMipmaps ? WebGLTexParam.NEAREST_MIPMAP_NEAREST : WebGLTexParam.NEAREST);
                    textureMaxFilter = WebGLTexParam.LINEAR;
                    break;

                default:
                    throw new NotSupportedException();
            }
            if (base.GraphicsDeviceStrategy.Capabilities.SupportsTextureFilterAnisotropic)
            {
                throw new NotImplementedException();
            }
            GL.TexParameter(target, WebGLTexParamName.TEXTURE_MIN_FILTER, textureMinFilter);
            GL.CheckGLError();
            GL.TexParameter(target, WebGLTexParamName.TEXTURE_MAG_FILTER, textureMaxFilter);
            GL.CheckGLError();

            // Set up texture addressing.
            GL.TexParameter(target, WebGLTexParamName.TEXTURE_WRAP_S, ToGLTextureAddressMode(AddressU));
            GL.CheckGLError();
            GL.TexParameter(target, WebGLTexParamName.TEXTURE_WRAP_T, ToGLTextureAddressMode(AddressV));
            GL.CheckGLError();

            // TextureMaxLevel
            if (base.GraphicsDeviceStrategy.Capabilities.SupportsTextureMaxLevel)
            {
                int textureMaxLevel = 1000;
                if (this.MaxMipLevel > 0)
                    textureMaxLevel = this.MaxMipLevel;
                throw new NotImplementedException();
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
