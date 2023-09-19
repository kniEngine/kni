// Copyright (C)2022 Nick Kastellanos

using System;
using System.Runtime.InteropServices;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class VertexBuffer
    {

        private void PlatformConstructVertexBuffer()
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformGetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride)
            where T : struct
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformSetData<T>(
            int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options, int bufferSize, int elementSizeInBytes)
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
