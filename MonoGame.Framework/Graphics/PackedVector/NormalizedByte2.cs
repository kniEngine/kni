// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;


namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    public struct NormalizedByte2 : IPackedVector<ushort>, IEquatable<NormalizedByte2>
    {
        private ushort _packedValue;

        public NormalizedByte2(Vector2 vector)
        {
            _packedValue = Pack(vector.X, vector.Y);
        }

        public NormalizedByte2(float x, float y)
        {
            _packedValue = Pack(x, y);
        }

        public static bool operator !=(NormalizedByte2 left, NormalizedByte2 right)
        {
            return left._packedValue != right._packedValue;
        }

        public static bool operator ==(NormalizedByte2 left, NormalizedByte2 right)
        {
            return left._packedValue == right._packedValue;
        }

        [CLSCompliant(false)]
        public ushort PackedValue
        {
            get { return _packedValue; }
            set { _packedValue = value; }
        }

        public override bool Equals(object obj)
        {
            return (obj is NormalizedByte2) &&
                    ((NormalizedByte2)obj)._packedValue == _packedValue;
        }

        public bool Equals(NormalizedByte2 other)
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

        private static ushort Pack(float x, float y)
        {
            var byte2 = (((ushort) Math.Round(MathHelper.Clamp(x, -1.0f, 1.0f) * 127.0f)) & 0xFF) << 0;
            var byte1 = (((ushort) Math.Round(MathHelper.Clamp(y, -1.0f, 1.0f) * 127.0f)) & 0xFF) << 8;

            return (ushort)(byte2 | byte1);
        }

        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            _packedValue = Pack(vector.X, vector.Y);
        }

        /// <summary>
        /// Gets the packed vector in Vector4 format.
        /// </summary>
        /// <returns>The packed vector in Vector4 format</returns>
        public Vector4 ToVector4()
        {
            return new Vector4(ToVector2(), 0.0f, 1.0f);
        }

        public Vector2 ToVector2()
        {
            return new Vector2(
                ((sbyte) ((_packedValue >> 0) & 0xFF)) / 127.0f,
                ((sbyte) ((_packedValue >> 8) & 0xFF)) / 127.0f);
        }
    }
}
