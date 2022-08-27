// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using nkast.Wasm.Canvas.WebGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class SamplerState
    {
        private IWebGLRenderingContext GL { get { return GraphicsDevice._glContext; } }


        internal void Activate(GraphicsDevice device, WebGLTextureTarget target, bool useMipmaps = false)
        {
            if (GraphicsDevice == null)
            {
                // We're now bound to a device... no one should
                // be changing the state of this object now!
                GraphicsDevice = device;
            }
            Debug.Assert(GraphicsDevice == device, "The state was created for a different device!");

            switch (Filter)
            {
                case TextureFilter.Point:
                    if (GraphicsDevice.GraphicsCapabilities.SupportsTextureFilterAnisotropic)
                    {
                        throw new NotImplementedException();
                    }
                    GL.TexParameter(target, WebGLTexParamName.TEXTURE_MIN_FILTER, (useMipmaps ? WebGLTexParam.NEAREST_MIPMAP_NEAREST : WebGLTexParam.NEAREST));
                    GraphicsExtensions.CheckGLError();
                    GL.TexParameter(target, WebGLTexParamName.TEXTURE_MAG_FILTER, WebGLTexParam.NEAREST);
                    GraphicsExtensions.CheckGLError();
                    break;
                case TextureFilter.Linear:
                    if (GraphicsDevice.GraphicsCapabilities.SupportsTextureFilterAnisotropic)
                    {
                        throw new NotImplementedException();
                    }
                    GL.TexParameter(target, WebGLTexParamName.TEXTURE_MIN_FILTER, (useMipmaps ? WebGLTexParam.LINEAR_MIPMAP_LINEAR : WebGLTexParam.LINEAR));
                    GraphicsExtensions.CheckGLError();
                    GL.TexParameter(target, WebGLTexParamName.TEXTURE_MAG_FILTER, WebGLTexParam.LINEAR);
                    GraphicsExtensions.CheckGLError();
                    break;
                case TextureFilter.Anisotropic:
                    if (GraphicsDevice.GraphicsCapabilities.SupportsTextureFilterAnisotropic)
                    {
                        throw new NotImplementedException();
                    }
                    GL.TexParameter(target, WebGLTexParamName.TEXTURE_MIN_FILTER, (useMipmaps ? WebGLTexParam.LINEAR_MIPMAP_LINEAR : WebGLTexParam.LINEAR));
                    GraphicsExtensions.CheckGLError();
                    GL.TexParameter(target, WebGLTexParamName.TEXTURE_MAG_FILTER, WebGLTexParam.LINEAR);
                    GraphicsExtensions.CheckGLError();
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
            GL.TexParameter(target, WebGLTexParamName.TEXTURE_WRAP_S, GetWrapMode(AddressU));
            GraphicsExtensions.CheckGLError();
            GL.TexParameter(target, WebGLTexParamName.TEXTURE_WRAP_T, GetWrapMode(AddressV));
            GraphicsExtensions.CheckGLError();

            if (GraphicsDevice.GraphicsCapabilities.SupportsTextureMaxLevel)
            {
                if (this.MaxMipLevel > 0)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    throw new NotImplementedException();
                }
                GraphicsExtensions.CheckGLError();
            }
        }

        private WebGLTexParam GetWrapMode(TextureAddressMode textureAddressMode)
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

