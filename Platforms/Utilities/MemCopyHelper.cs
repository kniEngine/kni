// Copyright (C)2024 Nick Kastellanos

using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Platform.Graphics.Utilities;

namespace Microsoft.Xna.Platform.Utilities
{
    internal static class MemCopyHelper
    {

#if WINDOWSDX
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
        static extern void RtlMoveMemory(IntPtr destination, IntPtr source, uint count);
#endif

        public unsafe static void MemoryCopy<T>(T[] data, IntPtr dstPtr, int startIndex, int count) where T : struct
        {
            int elementSizeInBytes = sizeof(T);

            GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                IntPtr srcPtr = dataHandle.AddrOfPinnedObject();
                srcPtr = srcPtr + startIndex * elementSizeInBytes;

                int sizeInBytes = count * elementSizeInBytes;

                MemoryCopy(srcPtr, dstPtr, sizeInBytes);
            }
            finally
            {
                dataHandle.Free();
            }
        }

        unsafe public static void MemoryCopy(IntPtr srcPtr, IntPtr dstPtr, int count)
        {
#if NET6_0_OR_GREATER || NETSTANDARD2_0
               Buffer.MemoryCopy((void*)srcPtr, (void*)dstPtr, count, count);
#else

#if WINDOWSDX
            if (count >= 64)
            {
                RtlMoveMemory(dstPtr, srcPtr, (uint)count);
                return;
            }
#endif

            byte* dst = (byte*)dstPtr;
            byte* src = (byte*)srcPtr;
            int len = count;

            while (len >= 8)
            {
                *((long*)dst) = *((long*)src);
                dst += 8; src += 8;
                len -= 8;
            }
            while (len >= 4)
            {
                *((int*)dst) = *((int*)src);
                dst += 4; src += 4;
                len -= 4;
            }
            while (len > 0)
            {
                *((byte*)dst) = *((byte*)src);
                dst += 1; src += 1;
                len -= 1;
            }
#endif
        }
    }
}
