// Copyright (C)2022 Nick Kastellanos

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D : Texture
    {

        private void PlatformConstructTexture2D(int width, int height, bool mipMap, SurfaceFormat format, SurfaceType type, bool shared)
        {
            throw new PlatformNotSupportedException();
        }

        private IntPtr PlatformGetSharedHandle()
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformSetData<T>(int level, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformSetData<T>(int level, int arraySlice, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformGetData<T>(int level, int arraySlice, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            throw new PlatformNotSupportedException();
        }

    }
}

