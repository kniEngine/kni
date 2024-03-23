// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;


namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    /// <summary>
    /// Packed vector type containing four 16-bit unsigned normalized values ranging from 0 to 1.
    /// </summary>
    public struct Rgba64 : IPackedVector<ulong>, IEquatable<Rgba64>, IPackedVector
    {
        private ulong _packedValue;

        /// <summary>
        /// Gets and sets the packed value.
        /// </summary>
        [CLSCompliant(false)]
        public ulong PackedValue
        {
            get { return _packedValue; }
            set { _packedValue = value; }
        }


        /// <summary>
        /// Creates a new instance of Rgba64.
        /// </summary>
        /// <param name="x">The x component</param>
        /// <param name="y">The y component</param>
        /// <param name="z">The z component</param>
        /// <param name="w">The w component</param>
        public Rgba64(float x, float y, float z, float w)
        {
            _packedValue = Pack(x, y, z, w);
        }

        /// <summary>
        /// Creates a new instance of Rgba64.
        /// </summary>
        /// <param name="vector">
        /// Vector containing the components for the packed vector.
        /// </param>
        public Rgba64(Vector4 vector)
        {
            _packedValue = Pack(vector.X, vector.Y, vector.Z, vector.W);
        }

        /// <summary>
        /// Gets the packed vector in Vector4 format.
        /// </summary>
        /// <returns>The packed vector in Vector4 format</returns>
        public Vector4 ToVector4()
        {
            return new Vector4(
                (float) (((_packedValue) & 0xFFFF) / 65535.0f),
                (float) (((_packedValue >> 16) & 0xFFFF) / 65535.0f),
                (float) (((_packedValue >> 32) & 0xFFFF) / 65535.0f),
                (float) (((_packedValue >> 48) & 0xFFFF) / 65535.0f)
            );
        }

        /// <summary>
        /// Sets the packed vector from a Vector4.
        /// </summary>
        /// <param name="vector">Vector containing the components.</param>
        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            _packedValue = Pack(vector.X, vector.Y, vector.Z, vector.W);
        }

        /// <summary>
        /// Compares an object with the packed vector.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>True if the object is equal to the packed vector.</returns>
        public override bool Equals(object obj)
        {
            return (obj is Rgba64) && Equals((Rgba64) obj);
        }

        /// <summary>
        /// Compares another Rgba64 packed vector with the packed vector.
        /// </summary>
        /// <param name="other">The Rgba64 packed vector to compare.</param>
        /// <returns>True if the packed vectors are equal.</returns>
        public bool Equals(Rgba64 other)
        {
            return _packedValue == other._packedValue;
        }

        /// <summary>
        /// Gets a string representation of the packed vector.
        /// </summary>
        /// <returns>A string representation of the packed vector.</returns>
        public override string ToString()
        {
            return ToVector4().ToString();
        }

        /// <summary>
        /// Gets a hash code of the packed vector.
        /// </summary>
        /// <returns>The hash code for the packed vector.</returns>
        public override int GetHashCode()
        {
            return _packedValue.GetHashCode();
        }

        public static bool operator ==(Rgba64 left, Rgba64 right)
        {
            return left._packedValue == right._packedValue;
        }

        public static bool operator !=(Rgba64 left, Rgba64 right)
        {
            return left._packedValue != right._packedValue;
        }

        private static ulong Pack(float x, float y, float z, float w)
        {
            return (ulong) (
                (((ulong)Math.Round(MathHelper.Clamp(x * 0xFFFF, 0, 65535f)) ) ) |
                (((ulong)Math.Round(MathHelper.Clamp(y * 0xFFFF, 0, 65535f)) ) << 16) |
                (((ulong)Math.Round(MathHelper.Clamp(z * 0xFFFF, 0, 65535f)) ) << 32) |
                (((ulong)Math.Round(MathHelper.Clamp(w * 0xFFFF, 0, 65535f)) ) << 48)
            );
        }
    }
}
