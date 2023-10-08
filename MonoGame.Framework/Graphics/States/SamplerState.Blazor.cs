// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using Microsoft.Xna.Platform.Graphics;
using nkast.Wasm.Canvas.WebGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class SamplerState
    {

        internal void PlatformApplyState(GraphicsContext context, WebGLTextureTarget target, bool useMipmaps = false)
        {
            Debug.Assert(GraphicsDevice == context.DeviceStrategy.Device, "The state was created for a different device!");

            var GL = context.Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            switch (Filter)
            {
                case TextureFilter.Point:
                    if (GraphicsDevice.Strategy.Capabilities.SupportsTextureFilterAnisotropic)
                    {
                        throw new NotImplementedException();
                    }
                    GL.TexParameter(target, WebGLTexParamName.TEXTURE_MIN_FILTER, (useMipmaps ? WebGLTexParam.NEAREST_MIPMAP_NEAREST : WebGLTexParam.NEAREST));
                    GL.CheckGLError();
                    GL.TexParameter(target, WebGLTexParamName.TEXTURE_MAG_FILTER, WebGLTexParam.NEAREST);
                    GL.CheckGLError();
                    break;
                case TextureFilter.Linear:
                    if (GraphicsDevice.Strategy.Capabilities.SupportsTextureFilterAnisotropic)
                    {
                        throw new NotImplementedException();
                    }
                    GL.TexParameter(target, WebGLTexParamName.TEXTURE_MIN_FILTER, (useMipmaps ? WebGLTexParam.LINEAR_MIPMAP_LINEAR : WebGLTexParam.LINEAR));
                    GL.CheckGLError();
                    GL.TexParameter(target, WebGLTexParamName.TEXTURE_MAG_FILTER, WebGLTexParam.LINEAR);
                    GL.CheckGLError();
                    break;
                case TextureFilter.Anisotropic:
                    if (GraphicsDevice.Strategy.Capabilities.SupportsTextureFilterAnisotropic)
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

            if (GraphicsDevice.Strategy.Capabilities.SupportsTextureMaxLevel)
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
    }
}

