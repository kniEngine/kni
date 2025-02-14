﻿// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    internal partial class ConcreteTexture2D
    {
        internal ConcreteTexture2D(GraphicsContextStrategy contextStrategy, Stream stream)
            : base(contextStrategy, SurfaceFormat.Color, 1)
        {
            throw new PlatformNotSupportedException();
        }


        public void SaveAsPng(Stream stream, int width, int height)
        {
            throw new PlatformNotSupportedException();
        }

        public void SaveAsJpeg(Stream stream, int width, int height)
        {
            throw new PlatformNotSupportedException();
        }
    }
}
