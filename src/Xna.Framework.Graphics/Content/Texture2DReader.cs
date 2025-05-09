// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics;
using Microsoft.Xna.Platform.Graphics.Utilities;

namespace Microsoft.Xna.Framework.Content
{
    internal class Texture2DReader : ContentTypeReader<Texture2D>
    {
        protected override Texture2D Read(ContentReader input, Texture2D existingInstance)
        {
            Texture2D texture = null;

            SurfaceFormat surfaceFormat = (SurfaceFormat)input.ReadInt32();
            int width = input.ReadInt32();
            int height = input.ReadInt32();
            int levelCount = input.ReadInt32();
            int levelCountOutput = levelCount;

            // If the system does not fully support Power of Two textures,
            // skip any mip maps supplied with any non PoT textures.
            if (levelCount > 1 && !((IPlatformGraphicsDevice)input.GetGraphicsDevice()).Strategy.Capabilities.SupportsNonPowerOfTwo &&
                (!MathHelper.IsPowerOfTwo(width) || !MathHelper.IsPowerOfTwo(height)))
            {
                levelCountOutput = 1;
                System.Diagnostics.Debug.WriteLine(
                    "Device does not support non Power of Two textures. Skipping mipmaps.");
            }

            SurfaceFormat convertedFormat = surfaceFormat;
            switch (surfaceFormat)
            {
                case SurfaceFormat.Dxt1:
                case SurfaceFormat.Dxt1a:
                    if (!((IPlatformGraphicsDevice)input.GetGraphicsDevice()).Strategy.Capabilities.SupportsDxt1)
                        convertedFormat = SurfaceFormat.Color;
                    break;
                case SurfaceFormat.Dxt1SRgb:
                    if (!((IPlatformGraphicsDevice)input.GetGraphicsDevice()).Strategy.Capabilities.SupportsDxt1)
                        convertedFormat = SurfaceFormat.ColorSRgb;
                    break;
                case SurfaceFormat.Dxt3:
                case SurfaceFormat.Dxt5:
                    if (!((IPlatformGraphicsDevice)input.GetGraphicsDevice()).Strategy.Capabilities.SupportsS3tc)
                        convertedFormat = SurfaceFormat.Color;
                    break;
                case SurfaceFormat.Dxt3SRgb:
                case SurfaceFormat.Dxt5SRgb:
                    if (!((IPlatformGraphicsDevice)input.GetGraphicsDevice()).Strategy.Capabilities.SupportsS3tc)
                        convertedFormat = SurfaceFormat.ColorSRgb;
                    break;
                case SurfaceFormat.NormalizedByte4:
                    convertedFormat = SurfaceFormat.Color;
                    break;
                case SurfaceFormat.Bgra5551:
                    if (!((IPlatformGraphicsDevice)input.GetGraphicsDevice()).Strategy.Capabilities.SupportsBgra5551)
                    {
                        //if (((IPlatformGraphicsDevice)input.GetGraphicsDevice()).Strategy.Capabilities.SupportsAbgr5551)
                        //    convertedFormat = SurfaceFormat.Abgr5551;
                    }
                    break;
                case SurfaceFormat.Bgra4444:
                    if (!((IPlatformGraphicsDevice)input.GetGraphicsDevice()).Strategy.Capabilities.SupportsBgra4444)
                    {
                        //if (((IPlatformGraphicsDevice)input.GetGraphicsDevice()).Strategy.Capabilities.SupportsAbgr4444)
                        //    convertedFormat = SurfaceFormat.Abgr4444;
                    }
                    break;
            }
            
            texture = existingInstance ?? new Texture2D(input.GetGraphicsDevice(), width, height, levelCountOutput > 1, convertedFormat);

            for (int level = 0; level < levelCount; level++)
            {
                int levelDataSizeInBytes = input.ReadInt32();
                byte[] levelData = input.BufferPool.Get(levelDataSizeInBytes);
                input.Read(levelData, 0, levelDataSizeInBytes);
                int levelWidth = Math.Max(width >> level, 1);
                int levelHeight = Math.Max(height >> level, 1);

                if (level >= levelCountOutput)
                    continue;

                //Convert the image data if required
                switch (surfaceFormat)
                {
                    case SurfaceFormat.Dxt1:
                    case SurfaceFormat.Dxt1SRgb:
                    case SurfaceFormat.Dxt1a:
                        if (!((IPlatformGraphicsDevice)input.GetGraphicsDevice()).Strategy.Capabilities.SupportsDxt1 && convertedFormat == SurfaceFormat.Color)
                        {
                            levelData = DxtDecoder.DecompressDxt1(levelData, levelWidth, levelHeight);
                            levelDataSizeInBytes = levelData.Length;
                        }
                        break;
                    case SurfaceFormat.Dxt3:
                    case SurfaceFormat.Dxt3SRgb:
                        if (!((IPlatformGraphicsDevice)input.GetGraphicsDevice()).Strategy.Capabilities.SupportsS3tc)
                            if (!((IPlatformGraphicsDevice)input.GetGraphicsDevice()).Strategy.Capabilities.SupportsS3tc &&
                                convertedFormat == SurfaceFormat.Color)
                            {
                                levelData = DxtDecoder.DecompressDxt3(levelData, levelWidth, levelHeight);
                                levelDataSizeInBytes = levelData.Length;
                            }
                        break;
                    case SurfaceFormat.Dxt5:
                    case SurfaceFormat.Dxt5SRgb:
                        if (!((IPlatformGraphicsDevice)input.GetGraphicsDevice()).Strategy.Capabilities.SupportsS3tc)
                            if (!((IPlatformGraphicsDevice)input.GetGraphicsDevice()).Strategy.Capabilities.SupportsS3tc &&
                                convertedFormat == SurfaceFormat.Color)
                            {
                                levelData = DxtDecoder.DecompressDxt5(levelData, levelWidth, levelHeight);
                                levelDataSizeInBytes = levelData.Length;
                            }
                        break;
                    case SurfaceFormat.Bgra5551:
                        if (!((IPlatformGraphicsDevice)input.GetGraphicsDevice()).Strategy.Capabilities.SupportsBgra5551)
                        {
                            //if (convertedFormat == SurfaceFormat.Abgr5551)
                            {
                                // Shift the channels to suit OpenGL
                                int offset = 0;
                                for (int y = 0; y < levelHeight; y++)
                                {
                                    for (int x = 0; x < levelWidth; x++)
                                    {
                                        ushort pixel = BitConverter.ToUInt16(levelData, offset);
                                        pixel = (ushort)(((pixel & 0x7FFF) << 1) | ((pixel & 0x8000) >> 15));
                                        levelData[offset] = (byte)(pixel);
                                        levelData[offset + 1] = (byte)(pixel >> 8);
                                        offset += 2;
                                    }
                                }
                            }
                        }
                        break;
                    case SurfaceFormat.Bgra4444:
                        if (!((IPlatformGraphicsDevice)input.GetGraphicsDevice()).Strategy.Capabilities.SupportsBgra4444)
                        {
                            //if (convertedFormat == SurfaceFormat.Abgr4444)
                            {
                                // Shift the channels to suit OpenGL
                                int offset = 0;
                                for (int y = 0; y < levelHeight; y++)
                                {
                                    for (int x = 0; x < levelWidth; x++)
                                    {
                                        ushort pixel = BitConverter.ToUInt16(levelData, offset);
                                        pixel = (ushort)(((pixel & 0x0FFF) << 4) | ((pixel & 0xF000) >> 12));
                                        levelData[offset] = (byte)(pixel);
                                        levelData[offset + 1] = (byte)(pixel >> 8);
                                        offset += 2;
                                    }
                                }
                            }
                        }
                        break;
                    case SurfaceFormat.NormalizedByte4:
                        {
                            int bytesPerPixel = surfaceFormat.GetSize();
                            int pitch = levelWidth * bytesPerPixel;
                            for (int y = 0; y < levelHeight; y++)
                            {
                                for (int x = 0; x < levelWidth; x++)
                                {
                                    int color = BitConverter.ToInt32(levelData, y * pitch + x * bytesPerPixel);
                                    levelData[y * pitch + x * 4] = (byte)(((color >> 16) & 0xff)); //R:=W
                                    levelData[y * pitch + x * 4 + 1] = (byte)(((color >> 8) & 0xff)); //G:=V
                                    levelData[y * pitch + x * 4 + 2] = (byte)(((color) & 0xff)); //B:=U
                                    levelData[y * pitch + x * 4 + 3] = (byte)(((color >> 24) & 0xff)); //A:=Q
                                }
                            }
                        }
                        break;
                }
                
                texture.SetData(level, null, levelData, 0, levelDataSizeInBytes);
                input.BufferPool.Return(levelData);
            }
                    
            texture.Name = input.AssetName;
            return texture;
        }
    }
}
