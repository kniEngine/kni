// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;


namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    /// <summary>
    /// Packed vector type containing four 16-bit floating-point values.
    /// </summary>
    public struct HalfVector4 : IPackedVector<ulong>, IPackedVector, IEquatable<HalfVector4>
    {
        ulong _packedValue;

        /// <summary>
        /// Directly gets or sets the packed representation of the value.
        /// </summary>
        /// <value>The packed representation of the value.</value>
        [CLSCompliant(false)]
        public ulong PackedValue
        {
            get { return _packedValue; }
            set { _packedValue = value; }
        }

        /// <summary>
        /// Initializes a new instance of the HalfVector4 structure.
        /// </summary>
        /// <param name="x">Initial value for the x component.</param>
        /// <param name="y">Initial value for the y component.</param>
        /// <param name="z">Initial value for the z component.</param>
        /// <param name="w">Initial value for the q component.</param>
        public HalfVector4(float x, float y, float z, float w)
        {
            var vector = new Vector4(x, y, z, w);
            _packedValue = PackHelper(ref vector);
        }

        /// <summary>
        /// Initializes a new instance of the HalfVector4 structure.
        /// </summary>
        /// <param name="vector">A vector containing the initial values for the components of the HalfVector4 structure.</param>
        public HalfVector4(Vector4 vector)
        {
            _packedValue = PackHelper(ref vector);
        }

        /// <summary>
        /// Sets the packed representation from a Vector4.
        /// </summary>
        /// <param name="vector">The vector to create the packed representation from.</param>
        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            _packedValue = PackHelper(ref vector);
        }

        /// <summary>
        /// Packs a vector into a ulong.
        /// </summary>
        /// <param name="vector">The vector containing the values to pack.</param>
        /// <returns>The ulong containing the packed values.</returns>
        static ulong PackHelper(ref Vector4 vector)
        {
            ulong word4 =  (ulong)HalfTypeHelper.Convert(vector.X);
            ulong word3 = ((ulong)HalfTypeHelper.Convert(vector.Y) << 0x10);
            ulong word2 = ((ulong)HalfTypeHelper.Convert(vector.Z) << 0x20);
            ulong word1 = ((ulong)HalfTypeHelper.Convert(vector.W) << 0x30);
            return word4 | word3 | word2 | word1;
        }

        /// <summary>
        /// Expands the packed representation into a Vector4.
        /// </summary>
        /// <returns>The expanded vector.</returns>
        public Vector4 ToVector4()
        {
            Vector4 result;
            result.X = HalfTypeHelper.Convert((ushort)_packedValue);
            result.Y = HalfTypeHelper.Convert((ushort)(_packedValue >> 0x10));
            result.Z = HalfTypeHelper.Convert((ushort)(_packedValue >> 0x20));
            result.W = HalfTypeHelper.Convert((ushort)(_packedValue >> 0x30));
            return result;
        }

        /// <summary>
        /// Returns a string representation of the current instance.
        /// </summary>
        /// <returns>String that represents the object.</returns>
        public override string ToString()
        {
            return ToVector4().ToString();
        }

        /// <summary>
        /// Gets the hash code for the current instance.
        /// </summary>
        /// <returns>Hash code for the instance.</returns>
        public override int GetHashCode()
        {
            return _packedValue.GetHashCode();
        }

        /// <summary>
        /// Returns a value that indicates whether the current instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">The object with which to make the comparison.</param>
        /// <returns>true if the current instance is equal to the specified object; false otherwise.</returns>
        public override bool Equals(object obj)
        {
            return ((obj is HalfVector4) && Equals((HalfVector4)obj));
        }

        /// <summary>
        /// Returns a value that indicates whether the current instance is equal to a specified object.
        /// </summary>
        /// <param name="other">The object with which to make the comparison.</param>
        /// <returns>true if the current instance is equal to the specified object; false otherwise.</returns>
        public bool Equals(HalfVector4 other)
        {
            return _packedValue.Equals(other._packedValue);
        }

        /// <summary>
        /// Compares the current instance of a class to another instance to determine whether they are the same.
        /// </summary>
        /// <param name="left">The object to the left of the equality operator.</param>
        /// <param name="right">The object to the right of the equality operator.</param>
        /// <returns>true if the objects are the same; false otherwise.</returns>
        public static bool operator ==(HalfVector4 left, HalfVector4 right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares the current instance of a class to another instance to determine whether they are different.
        /// </summary>
        /// <param name="left">The object to the left of the equality operator.</param>
        /// <param name="right">The object to the right of the equality operator.</param>
        /// <returns>true if the objects are different; false otherwise.</returns>
        public static bool operator !=(HalfVector4 left, HalfVector4 right)
        {
            return !left.Equals(right);
        }
    }
}
