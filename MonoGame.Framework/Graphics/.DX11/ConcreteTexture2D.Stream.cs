// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;
using MonoGame.Utilities.Png;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;
using WIC = SharpDX.WIC;


namespace Microsoft.Xna.Platform.Graphics
{
    internal partial class ConcreteTexture2D
    {

        public void SaveAsPng(Stream stream, int width, int height)
        {
        }

        public void SaveAsJpeg(Stream stream, int width, int height)
        {
        }
    }
}
