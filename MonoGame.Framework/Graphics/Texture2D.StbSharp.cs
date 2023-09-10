// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using StbImageSharp;
using StbImageWriteSharp;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D
    {
        private unsafe static Texture2D PlatformFromStream(GraphicsDevice graphicsDevice, Stream stream)
        {
            // Rewind stream if it is at end
            if (stream.CanSeek && stream.Length == stream.Position)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            ImageResult result;
            if (stream.CanSeek)
            {
                result = ImageResult.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
            }
            else
            {
                // If stream doesn't provide seek functionality, use MemoryStream instead
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    result = ImageResult.FromStream(ms, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                }
            }

            Texture2D texture = null;
            texture = new Texture2D(graphicsDevice, result.Width, result.Height);
            texture.SetData(result.Data);

            return texture;
        }

    }
}
