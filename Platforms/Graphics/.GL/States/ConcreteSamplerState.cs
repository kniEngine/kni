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


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteSamplerState : ResourceSamplerStateStrategy
    {
        private readonly float[] _openGLBorderColor = new float[4];

        internal ConcreteSamplerState(GraphicsContextStrategy contextStrategy, ISamplerStateStrategy source)
            : base(contextStrategy, source)
        {
        }

        internal void PlatformApplyState(ConcreteGraphicsContextGL cgraphicsContext, TextureTarget target, bool useMipmaps = false)
        {
            Debug.Assert(GraphicsDevice == ((IPlatformGraphicsContext)cgraphicsContext.Context).DeviceStrategy.Device, "The state was created for a different device!");

            var GL = cgraphicsContext.GL;

            // texture filtering
            float textureMaxAnisotropy;
            TextureMinFilter textureMinFilter = default(TextureMinFilter);
            TextureMagFilter textureMaxFilter;
            switch (Filter)
            {
                case TextureFilter.Point:
                    textureMaxAnisotropy = 1.0f;
                    textureMinFilter = (useMipmaps ? TextureMinFilter.NearestMipmapNearest : TextureMinFilter.Nearest);
                    textureMaxFilter = TextureMagFilter.Nearest;
                    break;
                case TextureFilter.Linear:
                    textureMaxAnisotropy = 1.0f;
                    textureMinFilter = (useMipmaps ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear);
                    textureMaxFilter = TextureMagFilter.Linear;
                    break;
                case TextureFilter.Anisotropic:
                    textureMaxAnisotropy = MathHelper.Clamp(this.MaxAnisotropy, 1.0f, cgraphicsContext.Capabilities.MaxTextureAnisotropy);
                    textureMinFilter = (useMipmaps ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear);
                    textureMaxFilter = TextureMagFilter.Linear;
                    break;
                case TextureFilter.PointMipLinear:
                    textureMaxAnisotropy = 1.0f;
                    textureMinFilter = (useMipmaps ? TextureMinFilter.NearestMipmapLinear : TextureMinFilter.Nearest);
                    textureMaxFilter = TextureMagFilter.Nearest;
                    break;
                case TextureFilter.LinearMipPoint:
                    textureMaxAnisotropy = 1.0f;
                    textureMinFilter = (useMipmaps ? TextureMinFilter.LinearMipmapNearest : TextureMinFilter.Linear);
                    textureMaxFilter = TextureMagFilter.Linear;
                    break;
                case TextureFilter.MinLinearMagPointMipLinear:
                    textureMaxAnisotropy = 1.0f;
                    textureMinFilter = (useMipmaps ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear);
                    textureMaxFilter = TextureMagFilter.Nearest;
                    break;
                case TextureFilter.MinLinearMagPointMipPoint:
                    textureMaxAnisotropy = 1.0f;
                    textureMinFilter = (useMipmaps ? TextureMinFilter.LinearMipmapNearest : TextureMinFilter.Linear);
                    textureMaxFilter = TextureMagFilter.Nearest;
                    break;
                case TextureFilter.MinPointMagLinearMipLinear:
                    textureMaxAnisotropy = 1.0f;
                    textureMinFilter = (useMipmaps ? TextureMinFilter.NearestMipmapLinear : TextureMinFilter.Nearest);
                    textureMaxFilter = TextureMagFilter.Linear;
                    break;
                case TextureFilter.MinPointMagLinearMipPoint:
                    textureMaxAnisotropy = 1.0f;
                    textureMinFilter = (useMipmaps ? TextureMinFilter.NearestMipmapNearest : TextureMinFilter.Nearest);
                    textureMaxFilter = TextureMagFilter.Linear;
                    break;

                default:
                    throw new NotSupportedException();
            }
            if (cgraphicsContext.Capabilities.SupportsTextureFilterAnisotropic)
            {
                GL.TexParameter(target, TextureParameterName.TextureMaxAnisotropyExt, textureMaxAnisotropy);
                GL.CheckGLError();
            }
            GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)textureMinFilter);
            GL.CheckGLError();
            GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)textureMaxFilter);
            GL.CheckGLError();

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

            // TextureMaxLevel
            if (cgraphicsContext.Capabilities.SupportsTextureMaxLevel)
            {
                int textureMaxLevel = 1000;
                if (this.MaxMipLevel > 0)
                    textureMaxLevel = this.MaxMipLevel;
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, textureMaxLevel);
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
