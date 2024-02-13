// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;


namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    public struct NormalizedByte4 : IPackedVector<uint>, IEquatable<NormalizedByte4>
    {
        private uint _packedValue;

        public NormalizedByte4(Vector4 vector)
        {
            _packedValue = Pack(vector.X, vector.Y, vector.Z, vector.W);
        }

        public NormalizedByte4(float x, float y, float z, float w)
        {
            _packedValue = Pack(x, y, z, w);
        }

        public static bool operator !=(NormalizedByte4 left, NormalizedByte4 right)
        {
            return left._packedValue != right._packedValue;
        }

        public static bool operator ==(NormalizedByte4 left, NormalizedByte4 right)
        {
            return left._packedValue == right._packedValue;
        }

        [CLSCompliant(false)]
        public uint PackedValue
        {
            get { return _packedValue; }
            set { _packedValue = value; }
        }

        public override bool Equals(object obj)
        {
            return  (obj is NormalizedByte4) &&
                    ((NormalizedByte4)obj)._packedValue == _packedValue;
        }

        public bool Equals(NormalizedByte4 other)
        {
            return _packedValue == other._packedValue;
        }

        public override int GetHashCode()
        {
            return _packedValue.GetHashCode();
        }

        public override string ToString()
        {
            return _packedValue.ToString("X");
        }

        private static uint Pack(float x, float y, float z, float w)
        {
            var byte4 = (((uint) Math.Round(MathHelper.Clamp(x, -1.0f, 1.0f) * 127.0f)) & 0xff) << 0;
            var byte3 = (((uint) Math.Round(MathHelper.Clamp(y, -1.0f, 1.0f) * 127.0f)) & 0xff) << 8;
            var byte2 = (((uint) Math.Round(MathHelper.Clamp(z, -1.0f, 1.0f) * 127.0f)) & 0xff) << 16;
            var byte1 = (((uint) Math.Round(MathHelper.Clamp(w, -1.0f, 1.0f) * 127.0f)) & 0xff) << 24;

            return byte4 | byte3 | byte2 | byte1;
        }

        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            _packedValue = Pack(vector.X, vector.Y, vector.Z, vector.W);
        }

        public Vector4 ToVector4()
        {
            return new Vector4(
                ((sbyte) ((_packedValue >> 0) & 0xFF)) / 127.0f,
                ((sbyte) ((_packedValue >> 8) & 0xFF)) / 127.0f,
                ((sbyte) ((_packedValue >> 16) & 0xFF)) / 127.0f,
                ((sbyte) ((_packedValue >> 24) & 0xFF)) / 127.0f);
        }
    }
}
