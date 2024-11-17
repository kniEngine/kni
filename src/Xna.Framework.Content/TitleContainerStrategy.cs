// Copyright (C)2024 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Platform
{

    public abstract class TitleContainerStrategy
    {
        public abstract string Location { get; }

        public abstract TitlePlatform Platform { get; }

        protected TitleContainerStrategy()
        {
            
        }

        public abstract Stream PlatformOpenStream(string name);

        public virtual Stream DecompressBrotliStream(Stream stream, uint compressedDataSize, uint decompressedDataSize)
        {
#if NET6_0_OR_GREATER
            Stream decompressedStream = new MemoryStream((int)decompressedDataSize);
            using (var brotliStream = new System.IO.Compression.BrotliStream(stream, System.IO.Compression.CompressionMode.Decompress, true))
            {
                brotliStream.CopyTo(decompressedStream, (int)decompressedDataSize);
            }
            decompressedStream.Seek(0, SeekOrigin.Begin);
            return decompressedStream;
#else
            throw new PlatformNotSupportedException("ContentCompression Brotli not Supported.");
#endif
        }
    }
}
