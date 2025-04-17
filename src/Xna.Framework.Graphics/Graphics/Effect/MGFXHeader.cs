// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Diagnostics;
using Microsoft.Xna.Platform.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
        internal struct MGFXHeader
        {
            /// <summary>
            /// The MonoGame Effect file format header identifier ("MGFX"). 
            /// </summary>
            public static readonly int MGFXSignature = (BitConverter.IsLittleEndian) ? 0x5846474D: 0x4D474658;

            /// <summary>
            /// The current MonoGame Effect file format versions
            /// used to detect old packaged content.
            /// </summary>
            /// <remarks>
            /// We should avoid supporting old versions for very long if at all 
            /// as users should be rebuilding content when packaging their game.
            /// </remarks>
            public const int MGFXVersion = 10;

            public readonly int Signature;
            public readonly int Version;
            public readonly ShaderProfileType Profile;
            public readonly int EffectKey;
            public readonly int HeaderSize;

            public MGFXHeader(byte[] effectCode, int index)
            {
                int offset = 0;
                Signature = BitConverter.ToInt32(effectCode, index + offset); offset += 4;
                Version = (int)effectCode[index + offset]; offset += 1;
                Profile = (ShaderProfileType)effectCode[index + offset]; offset += 1;
                EffectKey = BitConverter.ToInt32(effectCode, index + offset); offset += 4;
                HeaderSize = offset;
            }
        }
}
