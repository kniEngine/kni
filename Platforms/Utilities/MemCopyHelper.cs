// Copyright (C)2024 Nick Kastellanos

using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Platform.Utilities
{
    internal static class MemCopyHelper
    {
        unsafe public static void MemoryCopy<T>(T[] data, IntPtr dstPtr, int startIndex, int count) where T : struct
        {
            SharpDX.Utilities.Write(dstPtr, data, startIndex, count);
        }

        unsafe public static void MemoryCopy(IntPtr srcPtr, IntPtr dstPtr, int sizeInBytes)
        {
            SharpDX.Utilities.CopyMemory(dstPtr, srcPtr, sizeInBytes);
        }
    }
}
