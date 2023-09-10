// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;

#if WINDOWS_UAP
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using System.Threading.Tasks;
#endif


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D : Texture
    {

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            base.Dispose(disposing);
        }
    }
}

