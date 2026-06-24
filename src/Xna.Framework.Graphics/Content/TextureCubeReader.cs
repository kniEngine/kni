// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.Utilities;
using System;

namespace Microsoft.Xna.Framework.Content
{
    internal class TextureCubeReader : ContentTypeReader<TextureCube>
    {

        protected override TextureCube Read(ContentReader input, TextureCube existingInstance)
        {
            TextureCube textureCube = null;

            SurfaceFormat surfaceFormat = (SurfaceFormat)input.ReadInt32();

            int size = input.ReadInt32();
            int levels = input.ReadInt32();

            if (existingInstance == null)
                textureCube = new TextureCube(input.GetGraphicsDevice(), size, levels > 1, SurfaceFormat.Color);
            else
                textureCube = existingInstance;

            switch (surfaceFormat)
            {
                case SurfaceFormat.Dxt1:
                    for (int face = 0; face < 6; face++)
                    {
                        for (int level = 0; level < levels; level++)
                        {
                            int mipSize = Math.Max(1, size >> level);

                            int compressedSize = input.ReadInt32();
                            byte[] compressed = input.BufferPool.Get(compressedSize);
                            input.Read(compressed, 0, compressedSize);

                            byte[] rgba = DxtDecoder.DecompressDxt1(compressed, mipSize, mipSize);

                            textureCube.SetData<byte>((CubeMapFace)face, level, null, rgba, 0, rgba.Length);

                            input.BufferPool.Return(compressed);
                        }
                    }
                    break;
                case SurfaceFormat.Dxt3:
                    for (int face = 0; face < 6; face++)
                    {
                        for (int level = 0; level < levels; level++)
                        {
                            int mipSize = Math.Max(1, size >> level);

                            int compressedSize = input.ReadInt32();
                            byte[] compressed = input.BufferPool.Get(compressedSize);
                            input.Read(compressed, 0, compressedSize);

                            byte[] rgba = DxtDecoder.DecompressDxt3(compressed, mipSize, mipSize);

                            textureCube.SetData<byte>((CubeMapFace)face, level, null, rgba, 0, rgba.Length);

                            input.BufferPool.Return(compressed);
                        }
                    }
                    break;
                case SurfaceFormat.Dxt5:
                    for (int face = 0; face < 6; face++)
                    {
                        for (int level = 0; level < levels; level++)
                        {
                            int mipSize = Math.Max(1, size >> level);

                            int compressedSize = input.ReadInt32();
                            byte[] compressed = input.BufferPool.Get(compressedSize);
                            input.Read(compressed, 0, compressedSize);

                            byte[] rgba = DxtDecoder.DecompressDxt5(compressed, mipSize, mipSize);

                            textureCube.SetData<byte>((CubeMapFace)face, level, null, rgba, 0, rgba.Length);

                            input.BufferPool.Return(compressed);
                        }
                    }
                    break;
                default:
                    {
                        for (int face = 0; face < 6; face++)
                        {
                            for (int i = 0; i < levels; i++)
                            {
                                int faceSize = input.ReadInt32();
                                byte[] faceData = input.BufferPool.Get(faceSize);
                                input.Read(faceData, 0, faceSize);
                                textureCube.SetData<byte>((CubeMapFace)face, i, null, faceData, 0, faceSize);
                                input.BufferPool.Return(faceData);
                            }
                        }
                    }
                    break;
            }

            return textureCube;

        }
    }
}
