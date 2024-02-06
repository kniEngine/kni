// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;


namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    public struct HalfVector2 : IPackedVector<uint>, IPackedVector, IEquatable<HalfVector2>
    {
        private uint _packedValue;

        [CLSCompliant(false)]
        public uint PackedValue
        {
            get { return _packedValue; }
            set { _packedValue = value; }
        }

        public HalfVector2(float x, float y)
        {
            this._packedValue = PackHelper(x, y);
        }

        public HalfVector2(Vector2 vector)
        {
            this._packedValue = PackHelper(vector.X, vector.Y);
        }

        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            _packedValue = PackHelper(vector.X, vector.Y);
        }

        private static uint PackHelper(float vectorX, float vectorY)
        {
            uint word2 = HalfTypeHelper.Convert(vectorX);
            uint word1 = (uint)(HalfTypeHelper.Convert(vectorY) << 0x10);
            return (word2 | word1);
        }

        public Vector2 ToVector2()
        {
            Vector2 result;
            result.X = HalfTypeHelper.Convert((ushort)_packedValue);
            result.Y = HalfTypeHelper.Convert((ushort)(_packedValue >> 0x10));
            return result;
        }

        /// <summary>
        /// Gets the packed vector in Vector4 format.
        /// </summary>
        /// <returns>The packed vector in Vector4 format</returns>
        public Vector4 ToVector4()
        {
            Vector2 vector = this.ToVector2();
            return new Vector4(vector.X, vector.Y, 0f, 1f);
        }

        public override string ToString()
        {
            return this.ToVector2().ToString();
        }

        public override int GetHashCode()
        {
            return _packedValue.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return ((obj is HalfVector2) && this.Equals((HalfVector2)obj));
        }

        public bool Equals(HalfVector2 other)
        {
            return _packedValue.Equals(other._packedValue);
        }

        public static bool operator ==(HalfVector2 left, HalfVector2 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(HalfVector2 left, HalfVector2 right)
        {
            return !left.Equals(right);
        }
    }
}
