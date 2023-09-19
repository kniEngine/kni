// Copyright (C)2022 Nick Kastellanos

using System;
using System.Runtime.InteropServices;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class IndexBuffer
    {

        private void PlatformConstructIndexBuffer(IndexElementSize indexElementSize, int indexCount)
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformGetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformSetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options)
            where T : struct
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformGraphicsDeviceResetting()
        {
            throw new PlatformNotSupportedException();
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
