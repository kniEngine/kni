// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Runtime.InteropServices;
using MonoGame.OpenGL;
using GLPixelFormat = MonoGame.OpenGL.PixelFormat;
using MonoGame.Framework.Utilities;
using Microsoft.Xna.Platform.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class TextureCube
    {
        private void PlatformConstructTextureCube(GraphicsDevice graphicsDevice, int size, bool mipMap, SurfaceFormat format, bool renderTarget)
        {
            GetTextureStrategy<ConcreteTexture>()._glTarget = TextureTarget.TextureCubeMap;

            Threading.EnsureUIThread();
            {
                GetTextureStrategy<ConcreteTexture>()._glTexture = GL.GenTexture();
                GraphicsExtensions.CheckGLError();

                GL.BindTexture(TextureTarget.TextureCubeMap, GetTextureStrategy<ConcreteTexture>()._glTexture);
                GraphicsExtensions.CheckGLError();

                GL.TexParameter(
                    TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter,
                    mipMap ? (int)TextureMinFilter.LinearMipmapLinear : (int)TextureMinFilter.Linear);
                GraphicsExtensions.CheckGLError();

                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GraphicsExtensions.CheckGLError();

                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GraphicsExtensions.CheckGLError();

                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GraphicsExtensions.CheckGLError();

                ConcreteTexture.ToGLSurfaceFormat(format, GraphicsDevice, out GetTextureStrategy<ConcreteTexture>()._glInternalFormat, out GetTextureStrategy<ConcreteTexture>()._glFormat, out GetTextureStrategy<ConcreteTexture>()._glType);

                for (int i = 0; i < 6; i++)
                {
                    TextureTarget target = ConcreteTextureCube.GetGLCubeFace((CubeMapFace)i);

                    if (GetTextureStrategy<ConcreteTexture>()._glFormat == GLPixelFormat.CompressedTextureFormats)
                    {
                        int imageSize = 0;
                        switch (format)
                        {
                            case SurfaceFormat.RgbPvrtc2Bpp:
                            case SurfaceFormat.RgbaPvrtc2Bpp:
                                imageSize = (Math.Max(size, 16) * Math.Max(size, 8) * 2 + 7) / 8;
                                break;
                            case SurfaceFormat.RgbPvrtc4Bpp:
                            case SurfaceFormat.RgbaPvrtc4Bpp:
                                imageSize = (Math.Max(size, 8) * Math.Max(size, 8) * 4 + 7) / 8;
                                break;
                            case SurfaceFormat.Dxt1:
                            case SurfaceFormat.Dxt1a:
                            case SurfaceFormat.Dxt1SRgb:
                            case SurfaceFormat.Dxt3:
                            case SurfaceFormat.Dxt3SRgb:
                            case SurfaceFormat.Dxt5:
                            case SurfaceFormat.Dxt5SRgb:
                            case SurfaceFormat.RgbEtc1:
                            case SurfaceFormat.Rgb8Etc2:
                            case SurfaceFormat.Srgb8Etc2:
                            case SurfaceFormat.Rgb8A1Etc2:
                            case SurfaceFormat.Srgb8A1Etc2:
                            case SurfaceFormat.Rgba8Etc2:
                            case SurfaceFormat.SRgb8A8Etc2:
                            case SurfaceFormat.RgbaAtcExplicitAlpha:
                            case SurfaceFormat.RgbaAtcInterpolatedAlpha:
                                imageSize = (size + 3) / 4 * ((size + 3) / 4) * format.GetSize();
                                break;
                            default:
                                throw new NotSupportedException();
                        }
                        GL.CompressedTexImage2D(target, 0, GetTextureStrategy<ConcreteTexture>()._glInternalFormat, size, size, 0, imageSize, IntPtr.Zero);
                        GraphicsExtensions.CheckGLError();
                    }
                    else
                    {
                        GL.TexImage2D(target, 0, GetTextureStrategy<ConcreteTexture>()._glInternalFormat, size, size, 0, GetTextureStrategy<ConcreteTexture>()._glFormat, GetTextureStrategy<ConcreteTexture>()._glType, IntPtr.Zero);
                        GraphicsExtensions.CheckGLError();
                    }
                }

                if (mipMap)
                {
                    System.Diagnostics.Debug.Assert(TextureTarget.TextureCubeMap == GetTextureStrategy<ConcreteTexture>()._glTarget);
#if IOS || TVOS || ANDROID
                    GL.GenerateMipmap(TextureTarget.TextureCubeMap);
                    GraphicsExtensions.CheckGLError();
#else
                    GL.GenerateMipmap(GetTextureStrategy<ConcreteTexture>()._glTarget);
                    GraphicsExtensions.CheckGLError();
                    // This updates the mipmaps after a change in the base texture
                    GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.GenerateMipmap, (int)Bool.True);
#endif
                }
            }
        }

    }
}

