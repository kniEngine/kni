// Copyright (C)2025 Nick Kastellanos

using System;
using System.Diagnostics;
using Microsoft.Xna.Platform.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
        internal struct KNIFXHeader
        {
            /// <summary>
            /// The KNI Effect file format header identifier ("KNIF").
            /// </summary>
            public static readonly int KNIFXSignature = (BitConverter.IsLittleEndian) ? 0x46494E4B : 0x4B4E4946;

            /// <summary>
            /// The current KNI Effect file format versions
            /// </summary>
            public const int CurrentKNIFXVersion = 11;

            public readonly int Signature;
            public readonly int Version;
            public readonly int HeaderSize;

            public KNIFXHeader(byte[] effectCode, int index)
            {
                int offset = 0;
                Signature = BitConverter.ToInt32(effectCode, index + offset); offset += 4;
                Version = (int)BitConverter.ToInt16(effectCode, index + offset); offset += 2;
                short reserved0 = BitConverter.ToInt16(effectCode, index + offset); offset += 2;

                HeaderSize = offset;
            }
        }
}
