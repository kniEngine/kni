// Copyright (C)2025 Nick Kastellanos

using System;
using System.IO;
using System.Text;

namespace Microsoft.Xna.Platform.Media
{
    internal class PipeBinaryReader : BinaryReader
    {
        public long Position { get; private set; }


        public PipeBinaryReader(Stream input) : base(input)
        {
        }

        public PipeBinaryReader(Stream input, Encoding encoding) : base(input, encoding)
        {
        }

        #region overrides

        public override int Read(byte[] buffer, int index, int count)
        {
            int readBytes = base.Read(buffer, index, count);
            Position += readBytes;
            return readBytes;
        }

        public override byte[] ReadBytes(int count)
        {
            byte[] bytes = base.ReadBytes(count);
            Position += bytes.Length;
            return bytes;
        }

        public override byte ReadByte()
        {
            byte value = base.ReadByte();
            Position++;
            return value;
        }

        public override short ReadInt16()
        {
            short value = base.ReadInt16();
            Position+=2;
            return value;
        }

        public override uint ReadUInt32()
        {
            uint value = base.ReadUInt32();
            Position+=4;
            return value;
        }

        public override float ReadSingle()
        {
            float value = base.ReadSingle();
            Position+=4;
            return value;
        }

        public override double ReadDouble()
        {
            double value = base.ReadDouble();
            Position+=8;
            return value;
        }


        public void SkipBytes(ulong count)
        {
            while (count > 0)
            {
                int skip = (int)Math.Min(count, 4096);
                this.ReadBytes(skip);
                count -= (ulong)skip;
            }
        }

        #endregion overrides


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            
            }
            base.Dispose(disposing);
        }
    }
}