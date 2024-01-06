// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.OpenGL;
using ExtTextureFilterAnisotropic = Microsoft.Xna.Platform.Graphics.OpenGL.TextureParameterName;



namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteSamplerState : ResourceSamplerStateStrategy
    {
        private readonly float[] _openGLBorderColor = new float[4];

        internal const TextureParameterName TextureParameterNameTextureMaxAnisotropy = (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt;
        internal const TextureParameterName TextureParameterNameTextureMaxLevel = TextureParameterName.TextureMaxLevel;


        internal ConcreteSamplerState(GraphicsContextStrategy contextStrategy, ISamplerStateStrategy source)
            : base(contextStrategy, source)
        {
        }

        internal void PlatformApplyState(GraphicsContext context, TextureTarget target, bool useMipmaps = false)
        {
            Debug.Assert(GraphicsDevice == context.DeviceStrategy.Device, "The state was created for a different device!");

            var GL = context.Strategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

            switch (Filter)
            {
                case TextureFilter.Point:
                    if (base.GraphicsDeviceStrategy.Capabilities.SupportsTextureFilterAnisotropic)
                    {
                        GL.TexParameter(target, TextureParameterNameTextureMaxAnisotropy, 1.0f);
                        GL.CheckGLError();
                    }
                    GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)(useMipmaps ? TextureMinFilter.NearestMipmapNearest : TextureMinFilter.Nearest));
                    GL.CheckGLError();
                    GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                    GL.CheckGLError();
                    break;
                case TextureFilter.Linear:
                    if (base.GraphicsDeviceStrategy.Capabilities.SupportsTextureFilterAnisotropic)
                    {
                        GL.TexParameter(target, TextureParameterNameTextureMaxAnisotropy, 1.0f);
                        GL.CheckGLError();
                    }
                    GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)(useMipmaps ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear));
                    GL.CheckGLError();
                    GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    GL.CheckGLError();
                    break;
                case TextureFilter.Anisotropic:
                    if (base.GraphicsDeviceStrategy.Capabilities.SupportsTextureFilterAnisotropic)
                    {
                        GL.TexParameter(target, TextureParameterNameTextureMaxAnisotropy, MathHelper.Clamp(this.MaxAnisotropy, 1.0f, base.GraphicsDeviceStrategy.Capabilities.MaxTextureAnisotropy));
                        GL.CheckGLError();
                    }
                    GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)(useMipmaps ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear));
                    GL.CheckGLError();
                    GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    GL.CheckGLError();
                    break;
                case TextureFilter.PointMipLinear:
                    if (base.GraphicsDeviceStrategy.Capabilities.SupportsTextureFilterAnisotropic)
                    {
                        GL.TexParameter(target, TextureParameterNameTextureMaxAnisotropy, 1.0f);
                        GL.CheckGLError();
                    }
                    GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)(useMipmaps ? TextureMinFilter.NearestMipmapLinear : TextureMinFilter.Nearest));
                    GL.CheckGLError();
                    GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                    GL.CheckGLError();
                    break;
                case TextureFilter.LinearMipPoint:
                    if (base.GraphicsDeviceStrategy.Capabilities.SupportsTextureFilterAnisotropic)
                    {
                        GL.TexParameter(target, TextureParameterNameTextureMaxAnisotropy, 1.0f);
                        GL.CheckGLError();
                    }
                    GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)(useMipmaps ? TextureMinFilter.LinearMipmapNearest : TextureMinFilter.Linear));
                    GL.CheckGLError();
                    GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    GL.CheckGLError();
                    break;
                case TextureFilter.MinLinearMagPointMipLinear:
                    if (base.GraphicsDeviceStrategy.Capabilities.SupportsTextureFilterAnisotropic)
                    {
                        GL.TexParameter(target, TextureParameterNameTextureMaxAnisotropy, 1.0f);
                        GL.CheckGLError();
                    }
                    GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)(useMipmaps ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear));
                    GL.CheckGLError();
                    GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                    GL.CheckGLError();
                    break;
                case TextureFilter.MinLinearMagPointMipPoint:
                    if (base.GraphicsDeviceStrategy.Capabilities.SupportsTextureFilterAnisotropic)
                    {
                        GL.TexParameter(target, TextureParameterNameTextureMaxAnisotropy, 1.0f);
                        GL.CheckGLError();
                    }
                    GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)(useMipmaps ? TextureMinFilter.LinearMipmapNearest : TextureMinFilter.Linear));
                    GL.CheckGLError();
                    GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                    GL.CheckGLError();
                    break;
                case TextureFilter.MinPointMagLinearMipLinear:
                    if (base.GraphicsDeviceStrategy.Capabilities.SupportsTextureFilterAnisotropic)
                    {
                        GL.TexParameter(target, TextureParameterNameTextureMaxAnisotropy, 1.0f);
                        GL.CheckGLError();
                    }
                    GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)(useMipmaps ? TextureMinFilter.NearestMipmapLinear : TextureMinFilter.Nearest));
                    GL.CheckGLError();
                    GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    GL.CheckGLError();
                    break;
                case TextureFilter.MinPointMagLinearMipPoint:
                    if (base.GraphicsDeviceStrategy.Capabilities.SupportsTextureFilterAnisotropic)
                    {
                        GL.TexParameter(target, TextureParameterNameTextureMaxAnisotropy, 1.0f);
                        GL.CheckGLError();
                    }
                    GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)(useMipmaps ? TextureMinFilter.NearestMipmapNearest : TextureMinFilter.Nearest));
                    GL.CheckGLError();
                    GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    GL.CheckGLError();
                    break;
                default:
                    throw new NotSupportedException();
            }

            // Set up texture addressing.
            GL.TexParameter(target, TextureParameterName.TextureWrapS, (int)ToGLTextureAddressMode(AddressU));
            GL.CheckGLError();
            GL.TexParameter(target, TextureParameterName.TextureWrapT, (int)ToGLTextureAddressMode(AddressV));
            GL.CheckGLError();
#if !GLES
            // Border color is not supported by glTexParameter in OpenGL ES 2.0
            _openGLBorderColor[0] = BorderColor.R / 255.0f;
            _openGLBorderColor[1] = BorderColor.G / 255.0f;
            _openGLBorderColor[2] = BorderColor.B / 255.0f;
            _openGLBorderColor[3] = BorderColor.A / 255.0f;
            GL.TexParameter(target, TextureParameterName.TextureBorderColor, _openGLBorderColor);
            GL.CheckGLError();
            // LOD bias is not supported by glTexParameter in OpenGL ES 2.0
            GL.TexParameter(target, TextureParameterName.TextureLodBias, MipMapLevelOfDetailBias);
            GL.CheckGLError();
            // Comparison samplers are not supported in OpenGL ES 2.0 (without an extension, anyway)
            switch (FilterMode)
            {
                case TextureFilterMode.Comparison:
                    GL.TexParameter(target, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.CompareRefToTexture);
                    GL.CheckGLError();
                    GL.TexParameter(target, TextureParameterName.TextureCompareFunc, (int)ComparisonFunction.ToGLComparisonFunction());
                    GL.CheckGLError();
                    break;
                case TextureFilterMode.Default:
                    GL.TexParameter(target, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.None);
                    GL.CheckGLError();
                    break;
                default:
                    throw new InvalidOperationException("Invalid filter mode!");
            }
#endif
            if (base.GraphicsDeviceStrategy.Capabilities.SupportsTextureMaxLevel)
            {
                if (this.MaxMipLevel > 0)
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterNameTextureMaxLevel, this.MaxMipLevel);
                }
                else
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterNameTextureMaxLevel, 1000);
                }
                GL.CheckGLError();
            }
        }

        private int ToGLTextureAddressMode(TextureAddressMode textureAddressMode)
        {
            switch (textureAddressMode)
            {
                case TextureAddressMode.Clamp:
                    return (int)TextureWrapMode.ClampToEdge;
                case TextureAddressMode.Wrap:
                    return (int)TextureWrapMode.Repeat;
                case TextureAddressMode.Mirror:
                    return (int)TextureWrapMode.MirroredRepeat;

#if !GLES
                case TextureAddressMode.Border:
                    return (int)TextureWrapMode.ClampToBorder;
#endif

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
