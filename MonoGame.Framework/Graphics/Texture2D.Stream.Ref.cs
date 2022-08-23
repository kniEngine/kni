// Copyright (C)2022 Nick Kastellanos

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D
    {
        private unsafe static Texture2D PlatformFromStream(GraphicsDevice graphicsDevice, Stream stream)
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformSaveAsJpeg(Stream stream, int width, int height)
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformSaveAsPng(Stream stream, int width, int height)
        {
            throw new PlatformNotSupportedException();
        }

    }
}
