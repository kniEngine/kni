// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;


namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    public struct HalfSingle : IPackedVector<UInt16>, IEquatable<HalfSingle>, IPackedVector
    {
        UInt16 _packedValue;

        [CLSCompliant(false)]
        public ushort PackedValue
        {
            get { return _packedValue; }
            set { _packedValue = value; }
        }

        public HalfSingle(float single)
        {
            _packedValue = HalfTypeHelper.Convert(single);
        }

        public float ToSingle()
        {
            return HalfTypeHelper.Convert(_packedValue);
        }

        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            _packedValue = HalfTypeHelper.Convert(vector.X);
        }

        /// <summary>
        /// Gets the packed vector in Vector4 format.
        /// </summary>
        /// <returns>The packed vector in Vector4 format</returns>
        public Vector4 ToVector4()
        {
            return new Vector4(this.ToSingle(), 0f, 0f, 1f);
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj.GetType() == this.GetType())
            {
                return this == (HalfSingle)obj;
            }

            return false;
        }

        public bool Equals(HalfSingle other)
        {
            return _packedValue == other._packedValue;
        }

        public override string ToString()
        {
            return this.ToSingle().ToString();
        }

        public override int GetHashCode()
        {
            return _packedValue.GetHashCode();
        }

        public static bool operator ==(HalfSingle left, HalfSingle right)
        {
            return left._packedValue == right._packedValue;
        }

        public static bool operator !=(HalfSingle left, HalfSingle right)
        {
            return left._packedValue != right._packedValue;
        }
    }
}
