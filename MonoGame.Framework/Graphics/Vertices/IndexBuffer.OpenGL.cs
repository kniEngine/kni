// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Platform.Graphics;
using MonoGame.Framework.Utilities;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class IndexBuffer
    {

        private void PlatformConstructIndexBuffer(IndexElementSize indexElementSize, int indexCount)
        {
        }

        private void PlatformGetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct
        {
            Strategy.GetData<T>(offsetInBytes, data, startIndex, elementCount);
        }

        private void PlatformSetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options)
            where T : struct
        {
            Strategy.SetData<T>(offsetInBytes, data, startIndex, elementCount, options);
        }

        private void PlatformGraphicsDeviceResetting()
        {
        }

        protected override void Dispose(bool disposing)
        {

            base.Dispose(disposing);
        }

    }
}
