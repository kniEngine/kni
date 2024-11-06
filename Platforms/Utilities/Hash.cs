// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.IO;

namespace Microsoft.Xna.Platform.Graphics.Utilities
{
    internal static class HashHelper
    {
        /// <summary>
        /// Compute a hash from a byte array.
        /// </summary>
        /// <remarks>
        /// Modified FNV Hash in C#
        /// http://stackoverflow.com/a/468084
        /// </remarks>
        internal unsafe static int ComputeHash(params byte[] data)
        {
            unchecked
            {
                const int p = 16777619;
                int hash = (int)2166136261;

                int count = data.Length;
                fixed (byte* pData = data)
                {
                    for (int i = 0; i < count; i++)
                    {
                        hash = (hash ^ pData[i]) * p;
                    }
                }

                hash += hash << 13;
                hash ^= hash >> 7;
                hash += hash << 3;
                hash ^= hash >> 17;
                hash += hash << 5;
                return hash;
            }
        }
        
        /// <summary>
        /// Compute a hash from the content of a stream and restore the position.
        /// </summary>
        /// <remarks>
        /// Modified FNV Hash in C#
        /// http://stackoverflow.com/a/468084
        /// </remarks>
        internal unsafe static int ComputeHash(Stream stream)
        {
            System.Diagnostics.Debug.Assert(stream.CanSeek);

            unchecked
            {
                const int p = 16777619;
                int hash = (int)2166136261;

                long prevPosition = stream.Position;
                stream.Position = 0;

                int count;
                byte[] data = new byte[1024];
                fixed (byte* pData = data)
                {
                    while ((count = stream.Read(data, 0, data.Length)) != 0)
                    {
                        for (int i = 0; i < count; i++)
                            hash = (hash ^ pData[i]) * p;
                    }
                }

                // Restore stream position.
                stream.Position = prevPosition;

                hash += hash << 13;
                hash ^= hash >> 7;
                hash += hash << 3;
                hash ^= hash >> 17;
                hash += hash << 5;
                return hash;
            }
        }
    }
}
