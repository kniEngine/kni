// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// An efficient mathematical representation for three dimensional rotations.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{DebugDisplayString,nq}")]
    public struct Quaternion : IEquatable<Quaternion>
    {
        #region Private Fields

        private static readonly Quaternion _identity = new Quaternion(0, 0, 0, 1);

        #endregion

        #region Public Fields

        /// <summary>
        /// The x coordinate of this <see cref="Quaternion"/>.
        /// </summary>
        [DataMember]
        public float X;

        /// <summary>
        /// The y coordinate of this <see cref="Quaternion"/>.
        /// </summary>
        [DataMember]
        public float Y;

        /// <summary>
        /// The z coordinate of this <see cref="Quaternion"/>.
        /// </summary>
        [DataMember]
        public float Z;

        /// <summary>
        /// The rotation component of this <see cref="Quaternion"/>.
        /// </summary>
        [DataMember]
        public float W;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a quaternion with X, Y, Z and W from four values.
        /// </summary>
        /// <param name="x">The x coordinate in 3d-space.</param>
        /// <param name="y">The y coordinate in 3d-space.</param>
        /// <param name="z">The z coordinate in 3d-space.</param>
        /// <param name="w">The rotation component.</param>
        public Quaternion(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        /// <summary>
        /// Constructs a quaternion with X, Y, Z from <see cref="Vector3"/> and rotation component from a scalar.
        /// </summary>
        /// <param name="value">The x, y, z coordinates in 3d-space.</param>
        /// <param name="w">The rotation component.</param>
        public Quaternion(Vector3 value, float w)
        {
            this.X = value.X;
            this.Y = value.Y;
            this.Z = value.Z;
            this.W = w;
        }

        /// <summary>
        /// Constructs a quaternion from <see cref="Vector4"/>.
        /// </summary>
        /// <param name="value">The x, y, z coordinates in 3d-space and the rotation component.</param>
        public Quaternion(Vector4 value)
        {
            this.X = value.X;
            this.Y = value.Y;
            this.Z = value.Z;
            this.W = value.W;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Returns a quaternion representing no rotation.
        /// </summary>
        public static Quaternion Identity
        {
            get{ return _identity; }
        }

        #endregion

        #region Internal Properties

        internal string DebugDisplayString
        {
            get
            {
                if (this == Quaternion._identity)
                {
                    return "Identity";
                }

                return string.Concat(
                    this.X.ToString(), " ",
                    this.Y.ToString(), " ",
                    this.Z.ToString(), " ",
                    this.W.ToString()
                );
            }
        }

        #endregion

        #region Public Methods

        #region Add

        /// <summary>
        /// Creates a new <see cref="Quaternion"/> that contains the sum of two quaternions.
        /// </summary>
        /// <param name="left">Source <see cref="Quaternion"/>.</param>
        /// <param name="right">Source <see cref="Quaternion"/>.</param>
        /// <returns>The result of the quaternion addition.</returns>
        public static Quaternion Add(Quaternion left, Quaternion right)
        {
            Quaternion result;
            result.X = left.X + right.X;
            result.Y = left.Y + right.Y;
            result.Z = left.Z + right.Z;
            result.W = left.W + right.W;
            return result;
        }

        /// <summary>
        /// Creates a new <see cref="Quaternion"/> that contains the sum of two quaternions.
        /// </summary>
        /// <param name="left">Source <see cref="Quaternion"/>.</param>
        /// <param name="right">Source <see cref="Quaternion"/>.</param>
        /// <param name="result">The result of the quaternion addition as an output parameter.</param>
        public static void Add(ref Quaternion left, ref Quaternion right, out Quaternion result)
        {
            result.X = left.X + right.X;
            result.Y = left.Y + right.Y;
            result.Z = left.Z + right.Z;
            result.W = left.W + right.W;
        }

        #endregion

        #region Concatenate

        /// <summary>
        /// Creates a new <see cref="Quaternion"/> that contains concatenation between two quaternion.
        /// </summary>
        /// <param name="value1">The first <see cref="Quaternion"/> to concatenate.</param>
        /// <param name="value2">The second <see cref="Quaternion"/> to concatenate.</param>
        /// <returns>The result of rotation of <paramref name="value1"/> followed by <paramref name="value2"/> rotation.</returns>
        public static Quaternion Concatenate(Quaternion value1, Quaternion value2)
        {
            Quaternion result;
            result.X = (value2.X * value1.W) + (value1.X * value2.W) +((value2.Y * value1.Z) - (value2.Z * value1.Y));
            result.Y = (value2.Y * value1.W) + (value1.Y * value2.W) +((value2.Z * value1.X) - (value2.X * value1.Z));
            result.Z = (value2.Z * value1.W) + (value1.Z * value2.W) +((value2.X * value1.Y) - (value2.Y * value1.X));
            result.W = (value2.W * value1.W) -((value2.X * value1.X) + (value2.Y * value1.Y) + (value2.Z * value1.Z));
            return result;
        }

        /// <summary>
        /// Creates a new <see cref="Quaternion"/> that contains concatenation between two quaternion.
        /// </summary>
        /// <param name="value1">The first <see cref="Quaternion"/> to concatenate.</param>
        /// <param name="value2">The second <see cref="Quaternion"/> to concatenate.</param>
        /// <param name="result">The result of rotation of <paramref name="value1"/> followed by <paramref name="value2"/> rotation as an output parameter.</param>
        public static void Concatenate(ref Quaternion value1, ref Quaternion value2, out Quaternion result)
        {
            float x = (value2.X * value1.W) + (value1.X * value2.W) +((value2.Y * value1.Z) - (value2.Z * value1.Y));
            float y = (value2.Y * value1.W) + (value1.Y * value2.W) +((value2.Z * value1.X) - (value2.X * value1.Z));
            float z = (value2.Z * value1.W) + (value1.Z * value2.W) +((value2.X * value1.Y) - (value2.Y * value1.X));
            float w = (value2.W * value1.W) -((value2.X * value1.X) + (value2.Y * value1.Y) + (value2.Z * value1.Z));
            result.X = x;
            result.Y = y;
            result.Z = z;
            result.W = w;
        }

        #endregion

        #region Conjugate

        /// <summary>
        /// Transforms this quaternion into its conjugated version.
        /// </summary>
        public void Conjugate()
        {
            X = -X;
            Y = -Y;
            Z = -Z;
        }

        /// <summary>
        /// Creates a new <see cref="Quaternion"/> that contains conjugated version of the specified quaternion.
        /// </summary>
        /// <param name="value">The quaternion which values will be used to create the conjugated version.</param>
        /// <returns>The conjugate version of the specified quaternion.</returns>
        public static Quaternion Conjugate(Quaternion value)
        {
            return new Quaternion(-value.X,-value.Y,-value.Z,value.W);
        }

        /// <summary>
        /// Creates a new <see cref="Quaternion"/> that contains conjugated version of the specified quaternion.
        /// </summary>
        /// <param name="value">The quaternion which values will be used to create the conjugated version.</param>
        /// <param name="result">The conjugated version of the specified quaternion as an output parameter.</param>
        public static void Conjugate(ref Quaternion value, out Quaternion result)
        {
            result.X = -value.X;
            result.Y = -value.Y;
            result.Z = -value.Z;
            result.W = value.W;
        }

        #endregion

        #region CreateFromAxisAngle

        /// <summary>
        /// Creates a new <see cref="Quaternion"/> from the specified axis and angle.
        /// </summary>
        /// <param name="axis">The axis of rotation.</param>
        /// <param name="angle">The angle in radians.</param>
        /// <returns>The new quaternion builded from axis and angle.</returns>
        public static Quaternion CreateFromAxisAngle(Vector3 axis, float angle)
        {
            float half = angle * 0.5f;
            float sin = (float)Math.Sin(half);
            float cos = (float)Math.Cos(half);
            return new Quaternion(axis.X * sin, axis.Y * sin, axis.Z * sin, cos);
        }

        /// <summary>
        /// Creates a new <see cref="Quaternion"/> from the specified axis and angle.
        /// </summary>
        /// <param name="axis">The axis of rotation.</param>
        /// <param name="angle">The angle in radians.</param>
        /// <param name="result">The new quaternion builded from axis and angle as an output parameter.</param>
        public static void CreateFromAxisAngle(ref Vector3 axis, float angle, out Quaternion result)
        {
            float half = angle * 0.5f;
            float sin = (float)Math.Sin(half);
            float cos = (float)Math.Cos(half);
            result.X = axis.X * sin;
            result.Y = axis.Y * sin;
            result.Z = axis.Z * sin;
            result.W = cos;
        }

        #endregion

        #region CreateFromRotationMatrix

        /// <summary>
        /// Creates a new <see cref="Quaternion"/> from the specified <see cref="Matrix"/>.
        /// </summary>
        /// <param name="matrix">The rotation matrix.</param>
        /// <returns>A quaternion composed from the rotation part of the matrix.</returns>
        public static Quaternion CreateFromRotationMatrix(Matrix matrix)
        {
            Quaternion result;
            float sqrt;
            float half;
            float scale = matrix.M11 + matrix.M22 + matrix.M33;

            if (scale > 0.0f)
            {
                sqrt = (float)Math.Sqrt(scale + 1.0f);
                result.W = sqrt * 0.5f;
                sqrt = 0.5f / sqrt;

                result.X = (matrix.M23 - matrix.M32) * sqrt;
                result.Y = (matrix.M31 - matrix.M13) * sqrt;
                result.Z = (matrix.M12 - matrix.M21) * sqrt;

                return result;
            }
            if ((matrix.M11 >= matrix.M22) && (matrix.M11 >= matrix.M33))
            {
                sqrt = (float) Math.Sqrt(1.0f + matrix.M11 - matrix.M22 - matrix.M33);
                half = 0.5f / sqrt;

                result.X = 0.5f * sqrt;
                result.Y = (matrix.M12 + matrix.M21) * half;
                result.Z = (matrix.M13 + matrix.M31) * half;
                result.W = (matrix.M23 - matrix.M32) * half;

                return result;
            }
            if (matrix.M22 > matrix.M33)
            {
                sqrt = (float) Math.Sqrt(1.0f + matrix.M22 - matrix.M11 - matrix.M33);
                half = 0.5f / sqrt;

                result.X = (matrix.M21 + matrix.M12) * half;
                result.Y = 0.5f * sqrt;
                result.Z = (matrix.M32 + matrix.M23) * half;
                result.W = (matrix.M31 - matrix.M13) * half;

                return result;
            }
            sqrt = (float) Math.Sqrt(1.0f + matrix.M33 - matrix.M11 - matrix.M22);
            half = 0.5f / sqrt;

            result.X = (matrix.M31 + matrix.M13) * half;
            result.Y = (matrix.M32 + matrix.M23) * half;
            result.Z = 0.5f * sqrt;
            result.W = (matrix.M12 - matrix.M21) * half;

            return result;
        }

        /// <summary>
        /// Creates a new <see cref="Quaternion"/> from the specified <see cref="Matrix"/>.
        /// </summary>
        /// <param name="matrix">The rotation matrix.</param>
        /// <param name="result">A quaternion composed from the rotation part of the matrix as an output parameter.</param>
        public static void CreateFromRotationMatrix(ref Matrix matrix, out Quaternion result)
        {
            float sqrt;
            float half;
            float scale = matrix.M11 + matrix.M22 + matrix.M33;

            if (scale > 0.0f)
            {
                sqrt = (float)Math.Sqrt(scale + 1.0f);
                result.W = sqrt * 0.5f;
                sqrt = 0.5f / sqrt;

                result.X = (matrix.M23 - matrix.M32) * sqrt;
                result.Y = (matrix.M31 - matrix.M13) * sqrt;
                result.Z = (matrix.M12 - matrix.M21) * sqrt;
            }
            else
            if ((matrix.M11 >= matrix.M22) && (matrix.M11 >= matrix.M33))
            {
                sqrt = (float)Math.Sqrt(1.0f + matrix.M11 - matrix.M22 - matrix.M33);
                half = 0.5f / sqrt;

                result.X = 0.5f * sqrt;
                result.Y = (matrix.M12 + matrix.M21) * half;
                result.Z = (matrix.M13 + matrix.M31) * half;
                result.W = (matrix.M23 - matrix.M32) * half;
            }
            else if (matrix.M22 > matrix.M33)
            {
                sqrt = (float) Math.Sqrt(1.0f + matrix.M22 - matrix.M11 - matrix.M33);
                half = 0.5f/sqrt;

                result.X = (matrix.M21 + matrix.M12)*half;
                result.Y = 0.5f*sqrt;
                result.Z = (matrix.M32 + matrix.M23)*half;
                result.W = (matrix.M31 - matrix.M13)*half;
            }
            else
            {
                sqrt = (float)Math.Sqrt(1.0f + matrix.M33 - matrix.M11 - matrix.M22);
                half = 0.5f / sqrt;

                result.X = (matrix.M31 + matrix.M13) * half;
                result.Y = (matrix.M32 + matrix.M23) * half;
                result.Z = 0.5f * sqrt;
                result.W = (matrix.M12 - matrix.M21) * half;
            }
        }

        #endregion

        #region CreateFromYawPitchRoll

        /// <summary>
        /// Creates a new <see cref="Quaternion"/> from the specified yaw, pitch and roll angles.
        /// </summary>
        /// <param name="yaw">Yaw around the y axis in radians.</param>
        /// <param name="pitch">Pitch around the x axis in radians.</param>
        /// <param name="roll">Roll around the z axis in radians.</param>
        /// <returns>A new quaternion from the concatenated yaw, pitch, and roll angles.</returns>
        public static Quaternion CreateFromYawPitchRoll(float yaw, float pitch, float roll)
        {
            float halfRoll = roll * 0.5f;
            float halfPitch = pitch * 0.5f;
            float halfYaw = yaw * 0.5f;

            float sinRoll = (float)Math.Sin(halfRoll);
            float cosRoll = (float)Math.Cos(halfRoll);
            float sinPitch = (float)Math.Sin(halfPitch);
            float cosPitch = (float)Math.Cos(halfPitch);
            float sinYaw = (float)Math.Sin(halfYaw);
            float cosYaw = (float)Math.Cos(halfYaw);

            return new Quaternion((cosYaw * sinPitch * cosRoll) + (sinYaw * cosPitch * sinRoll),
                                  (sinYaw * cosPitch * cosRoll) - (cosYaw * sinPitch * sinRoll),
                                  (cosYaw * cosPitch * sinRoll) - (sinYaw * sinPitch * cosRoll),
                                  (cosYaw * cosPitch * cosRoll) + (sinYaw * sinPitch * sinRoll));
        }

        /// <summary>
        /// Creates a new <see cref="Quaternion"/> from the specified yaw, pitch and roll angles.
        /// </summary>
        /// <param name="yaw">Yaw around the y axis in radians.</param>
        /// <param name="pitch">Pitch around the x axis in radians.</param>
        /// <param name="roll">Roll around the z axis in radians.</param>
        /// <param name="result">A new quaternion from the concatenated yaw, pitch, and roll angles as an output parameter.</param>
        public static void CreateFromYawPitchRoll(float yaw, float pitch, float roll, out Quaternion result)
        {
            float halfRoll = roll * 0.5f;
            float halfPitch = pitch * 0.5f;
            float halfYaw = yaw * 0.5f;

            float sinRoll = (float)Math.Sin(halfRoll);
            float cosRoll = (float)Math.Cos(halfRoll);
            float sinPitch = (float)Math.Sin(halfPitch);
            float cosPitch = (float)Math.Cos(halfPitch);
            float sinYaw = (float)Math.Sin(halfYaw);
            float cosYaw = (float)Math.Cos(halfYaw);

            result.X = (cosYaw * sinPitch * cosRoll) + (sinYaw * cosPitch * sinRoll);
            result.Y = (sinYaw * cosPitch * cosRoll) - (cosYaw * sinPitch * sinRoll);
            result.Z = (cosYaw * cosPitch * sinRoll) - (sinYaw * sinPitch * cosRoll);
            result.W = (cosYaw * cosPitch * cosRoll) + (sinYaw * sinPitch * sinRoll);
        }

        #endregion

        #region Divide

        /// <summary>
        /// Divides a <see cref="Quaternion"/> by the other <see cref="Quaternion"/>.
        /// </summary>
        /// <param name="left">Source <see cref="Quaternion"/>.</param>
        /// <param name="right">Divisor <see cref="Quaternion"/>.</param>
        /// <returns>The result of dividing the quaternions.</returns>
        public static Quaternion Divide(Quaternion left, Quaternion right)
        {
            // Opt: right = Quaternion.Inverse(right);
            float dot = right.X * right.X + right.Y * right.Y + right.Z * right.Z + right.W * right.W;
            float factor = 1f / dot;
            right.X = -right.X * factor;
            right.Y = -right.Y * factor;
            right.Z = -right.Z * factor;
            right.W = right.W * factor;

            float x2 = (left.Y * right.Z) - (left.Z * right.Y);
            float y2 = (left.Z * right.X) - (left.X * right.Z);
            float z2 = (left.X * right.Y) - (left.Y * right.X);
            float w2 = (left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z);

            Quaternion result;
            result.X = (left.X * right.W) + (right.X * left.W) + x2;
            result.Y = (left.Y * right.W) + (right.Y * left.W) + y2;
            result.Z = (left.Z * right.W) + (right.Z * left.W) + z2;
            result.W = (left.W * right.W) - w2;
            return result;
        }

        /// <summary>
        /// Divides a <see cref="Quaternion"/> by the other <see cref="Quaternion"/>.
        /// </summary>
        /// <param name="left">Source <see cref="Quaternion"/>.</param>
        /// <param name="right">Divisor <see cref="Quaternion"/>.</param>
        /// <param name="result">The result of dividing the quaternions as an output parameter.</param>
        public static void Divide(ref Quaternion left, ref Quaternion right, out Quaternion result)
        {
            // Opt: Quaternion invright = Quaternion.Inverse(right);
            float dot = right.X * right.X + right.Y * right.Y + right.Z * right.Z + right.W * right.W;
            float factor = 1f / dot;
            float invrightX = -right.X * factor;
            float invrightY = -right.Y * factor;
            float invrightZ = -right.Z * factor;
            float invrightW =  right.W * factor;

            float x2 = (left.Y * invrightZ) - (left.Z * invrightY);
            float y2 = (left.Z * invrightX) - (left.X * invrightZ);
            float z2 = (left.X * invrightY) - (left.Y * invrightX);
            float w2 = (left.X * invrightX) + (left.Y * invrightY) + (left.Z * invrightZ);

            float x = (left.X * invrightW) + (invrightX * left.W) + x2;
            float y = (left.Y * invrightW) + (invrightY * left.W) + y2;
            float z = (left.Z * invrightW) + (invrightZ * left.W) + z2;
            float w = (left.W * invrightW) - w2;
            result.X = x;
            result.Y = y;
            result.Z = z;
            result.W = w;
        }

        #endregion

        #region Dot

        /// <summary>
        /// Returns a dot product of two quaternions.
        /// </summary>
        /// <param name="left">The first quaternion.</param>
        /// <param name="right">The second quaternion.</param>
        /// <returns>The dot product of two quaternions.</returns>
        public static float Dot(Quaternion left, Quaternion right)
        {
            return (left.X * right.X + left.Y * right.Y + left.Z * right.Z + left.W * right.W);
        }

        /// <summary>
        /// Returns a dot product of two quaternions.
        /// </summary>
        /// <param name="left">The first quaternion.</param>
        /// <param name="right">The second quaternion.</param>
        /// <param name="result">The dot product of two quaternions as an output parameter.</param>
        public static void Dot(ref Quaternion left, ref Quaternion right, out float result)
        {
            result = left.X * right.X + left.Y * right.Y + left.Z * right.Z + left.W * right.W;
        }

        #endregion

        #region Equals

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj is Quaternion)
                return Equals((Quaternion)obj);
            return false;
        }

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Quaternion"/>.
        /// </summary>
        /// <param name="other">The <see cref="Quaternion"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public bool Equals(Quaternion other)
        {
            return X == other.X &&
                   Y == other.Y &&
                   Z == other.Z &&
                   W == other.W;
        }

        #endregion

        /// <summary>
        /// Gets the hash code of this <see cref="Quaternion"/>.
        /// </summary>
        /// <returns>Hash code of this <see cref="Quaternion"/>.</returns>
        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode() + W.GetHashCode();
        }

        #region Inverse

        /// <summary>
        /// Returns the inverse quaternion which represents the opposite rotation.
        /// </summary>
        /// <param name="value">Source <see cref="Quaternion"/>.</param>
        /// <returns>The inverse quaternion.</returns>
        public static Quaternion Inverse(Quaternion value)
        {
            float dot = (value.X * value.X) + (value.Y * value.Y) + (value.Z * value.Z) + (value.W * value.W);

            Quaternion result;
            float factor = 1f / dot;
            result.X = -value.X * factor;
            result.Y = -value.Y * factor;
            result.Z = -value.Z * factor;
            result.W =  value.W * factor;
            return result;
        }

        /// <summary>
        /// Returns the inverse quaternion which represents the opposite rotation.
        /// </summary>
        /// <param name="value">Source <see cref="Quaternion"/>.</param>
        /// <param name="result">The inverse quaternion as an output parameter.</param>
        public static void Inverse(ref Quaternion value, out Quaternion result)
        {
            float dot = (value.X * value.X) + (value.Y * value.Y) + (value.Z * value.Z) + (value.W * value.W);

            float factor = 1f / dot;
            result.X = -value.X * factor;
            result.Y = -value.Y * factor;
            result.Z = -value.Z * factor;
            result.W =  value.W * factor;
        }

        #endregion

        /// <summary>
        /// Returns the magnitude of the quaternion components.
        /// </summary>
        /// <returns>The magnitude of the quaternion components.</returns>
        public float Length()
        {
            return (float) Math.Sqrt((X * X) + (Y * Y) + (Z * Z) + (W * W));
        }

        /// <summary>
        /// Returns the squared magnitude of the quaternion components.
        /// </summary>
        /// <returns>The squared magnitude of the quaternion components.</returns>
        public float LengthSquared()
        {
            return (X * X) + (Y * Y) + (Z * Z) + (W * W);
        }

        #region Lerp

        /// <summary>
        /// Performs a linear blend between two quaternions.
        /// </summary>
        /// <param name="start">Source <see cref="Quaternion"/>.</param>
        /// <param name="end">Source <see cref="Quaternion"/>.</param>
        /// <param name="amount">The blend amount where 0 returns <paramref name="start"/> and 1 <paramref name="end"/>.</param>
        /// <returns>The result of linear blending between two quaternions.</returns>
        public static Quaternion Lerp(Quaternion start, Quaternion end, float amount)
        {
            float a = 1f - amount;
            float b = amount;
            float dot = start.X * end.X + start.Y * end.Y + start.Z * end.Z + start.W * end.W;

            Quaternion result;
            if (dot >= 0f)
            {
                result.X = a * start.X + b * end.X;
                result.Y = a * start.Y + b * end.Y;
                result.Z = a * start.Z + b * end.Z;
                result.W = a * start.W + b * end.W;
            }
            else
            {
                result.X = a * start.X - b * end.X;
                result.Y = a * start.Y - b * end.Y;
                result.Z = a * start.Z - b * end.Z;
                result.W = a * start.W - b * end.W;
            }

            float lengthSq = result.X * result.X + result.Y * result.Y + result.Z * result.Z + result.W * result.W;
            float invLength = 1f / ((float)Math.Sqrt((double)lengthSq));
            result.X *= invLength;
            result.Y *= invLength;
            result.Z *= invLength;
            result.W *= invLength;
            return result;
        }

        /// <summary>
        /// Performs a linear blend between two quaternions.
        /// </summary>
        /// <param name="start">Source <see cref="Quaternion"/>.</param>
        /// <param name="end">Source <see cref="Quaternion"/>.</param>
        /// <param name="amount">The blend amount where 0 returns <paramref name="start"/> and 1 <paramref name="end"/>.</param>
        /// <param name="result">The result of linear blending between two quaternions as an output parameter.</param>
        public static void Lerp(ref Quaternion start, ref Quaternion end, float amount, out Quaternion result)
        {
            float a = 1f - amount;
            float b = amount;
            float dot = start.X * end.X + start.Y * end.Y + start.Z * end.Z + start.W * end.W;

            if (dot >= 0f)
            {
                result.X = a * start.X + b * end.X;
                result.Y = a * start.Y + b * end.Y;
                result.Z = a * start.Z + b * end.Z;
                result.W = a * start.W + b * end.W;
            }
            else
            {
                result.X = a * start.X - b * end.X;
                result.Y = a * start.Y - b * end.Y;
                result.Z = a * start.Z - b * end.Z;
                result.W = a * start.W - b * end.W;
            }
            float lengthSq = result.X * result.X + result.Y * result.Y + result.Z * result.Z + result.W * result.W;
            float invLength = 1f / ((float)Math.Sqrt((double)lengthSq));
            result.X *= invLength;
            result.Y *= invLength;
            result.Z *= invLength;
            result.W *= invLength;

        }

        #endregion

        #region Slerp

        /// <summary>
        /// Performs a spherical linear blend between two quaternions.
        /// </summary>
        /// <param name="start">Source <see cref="Quaternion"/>.</param>
        /// <param name="end">Source <see cref="Quaternion"/>.</param>
        /// <param name="amount">The blend amount where 0 returns <paramref name="start"/> and 1 <paramref name="end"/>.</param>
        /// <returns>The result of spherical linear blending between two quaternions.</returns>
        public static Quaternion Slerp(Quaternion start, Quaternion end, float amount)
        {
            float a = 1f - amount;
            float b = amount;
            float dot = start.X * end.X + start.Y * end.Y + start.Z * end.Z + start.W * end.W;

            float dotAbs = Math.Abs(dot);
            if (dotAbs <= 0.999999f)
            {
                float acos   = (float)Math.Acos(dotAbs);
                float invSin = (float)(1.0 / Math.Sin(acos));
                a = (float)Math.Sin(a * acos) * invSin;
                b = (float)Math.Sin(b * acos) * invSin;
            }
            if (dot < 0f)
                b = -b;

            Quaternion result;
            result.X = a * start.X + b * end.X;
            result.Y = a * start.Y + b * end.Y;
            result.Z = a * start.Z + b * end.Z;
            result.W = a * start.W + b * end.W;
            return result;
        }

        /// <summary>
        /// Performs a spherical linear blend between two quaternions.
        /// </summary>
        /// <param name="start">Source <see cref="Quaternion"/>.</param>
        /// <param name="end">Source <see cref="Quaternion"/>.</param>
        /// <param name="amount">The blend amount where 0 returns <paramref name="start"/> and 1 <paramref name="end"/>.</param>
        /// <param name="result">The result of spherical linear blending between two quaternions as an output parameter.</param>
        public static void Slerp(ref Quaternion start, ref Quaternion end, float amount, out Quaternion result)
        {
            float a = 1f - amount;
            float b = amount;
            float dot = start.X * end.X + start.Y * end.Y + start.Z * end.Z + start.W * end.W;

            float dotAbs = Math.Abs(dot);
            if (dotAbs <= 0.999999f)
            {
                float acos   = (float)Math.Acos(dotAbs);
                float invSin = (float)(1.0 / Math.Sin(acos));
                a = (float)Math.Sin(a * acos) * invSin;
                b = (float)Math.Sin(b * acos) * invSin;
            }
            if (dot < 0f)
                b = -b;

            result.X = a * start.X + b * end.X;
            result.Y = a * start.Y + b * end.Y;
            result.Z = a * start.Z + b * end.Z;
            result.W = a * start.W + b * end.W;
        }

        #endregion

        #region Subtract

        /// <summary>
        /// Creates a new <see cref="Quaternion"/> that contains subtraction of one <see cref="Quaternion"/> from another.
        /// </summary>
        /// <param name="left">Source <see cref="Quaternion"/>.</param>
        /// <param name="right">Source <see cref="Quaternion"/>.</param>
        /// <returns>The result of the quaternion subtraction.</returns>
        public static Quaternion Subtract(Quaternion left, Quaternion right)
        {
            Quaternion result;
            result.X = left.X - right.X;
            result.Y = left.Y - right.Y;
            result.Z = left.Z - right.Z;
            result.W = left.W - right.W;
            return result;
        }

        /// <summary>
        /// Creates a new <see cref="Quaternion"/> that contains subtraction of one <see cref="Quaternion"/> from another.
        /// </summary>
        /// <param name="left">Source <see cref="Quaternion"/>.</param>
        /// <param name="right">Source <see cref="Quaternion"/>.</param>
        /// <param name="result">The result of the quaternion subtraction as an output parameter.</param>
        public static void Subtract(ref Quaternion left, ref Quaternion right, out Quaternion result)
        {
            result.X = left.X - right.X;
            result.Y = left.Y - right.Y;
            result.Z = left.Z - right.Z;
            result.W = left.W - right.W;
        }

        #endregion

        #region Multiply

        /// <summary>
        /// Creates a new <see cref="Quaternion"/> that contains a multiplication of two quaternions.
        /// </summary>
        /// <param name="left">Source <see cref="Quaternion"/>.</param>
        /// <param name="right">Source <see cref="Quaternion"/>.</param>
        /// <returns>The result of the quaternion multiplication.</returns>
        public static Quaternion Multiply(Quaternion left, Quaternion right)
        {
            Quaternion result;
            float x2 = (left.Y * right.Z) - (left.Z * right.Y);
            float y2 = (left.Z * right.X) - (left.X * right.Z);
            float z2 = (left.X * right.Y) - (left.Y * right.X);
            float w2 = (left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z);

            result.X = (left.X * right.W) + (right.X * left.W) + x2;
            result.Y = (left.Y * right.W) + (right.Y * left.W) + y2;
            result.Z = (left.Z * right.W) + (right.Z * left.W) + z2;
            result.W = (left.W * right.W) - w2;
            return result;
        }

        /// <summary>
        /// Creates a new <see cref="Quaternion"/> that contains a multiplication of <see cref="Quaternion"/> and a scalar.
        /// </summary>
        /// <param name="left">Source <see cref="Quaternion"/>.</param>
        /// <param name="right">Scalar value.</param>
        /// <returns>The result of the quaternion multiplication with a scalar.</returns>
        public static Quaternion Multiply(Quaternion left, float right)
        {
            Quaternion result;
            result.X = left.X * right;
            result.Y = left.Y * right;
            result.Z = left.Z * right;
            result.W = left.W * right;
            return result;
        }

        /// <summary>
        /// Creates a new <see cref="Quaternion"/> that contains a multiplication of <see cref="Quaternion"/> and a scalar.
        /// </summary>
        /// <param name="left">Source <see cref="Quaternion"/>.</param>
        /// <param name="right">Scalar value.</param>
        /// <param name="result">The result of the quaternion multiplication with a scalar as an output parameter.</param>
        public static void Multiply(ref Quaternion left, float right, out Quaternion result)
        {
            result.X = left.X * right;
            result.Y = left.Y * right;
            result.Z = left.Z * right;
            result.W = left.W * right;
        }

        /// <summary>
        /// Creates a new <see cref="Quaternion"/> that contains a multiplication of two quaternions.
        /// </summary>
        /// <param name="left">Source <see cref="Quaternion"/>.</param>
        /// <param name="right">Source <see cref="Quaternion"/>.</param>
        /// <param name="result">The result of the quaternion multiplication as an output parameter.</param>
        public static void Multiply(ref Quaternion left, ref Quaternion right, out Quaternion result)
        {
            float x2 = (left.Y * right.Z) - (left.Z * right.Y);
            float y2 = (left.Z * right.X) - (left.X * right.Z);
            float z2 = (left.X * right.Y) - (left.Y * right.X);
            float w2  = (left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z);

            float x = (left.X * right.W) + (right.X * left.W) + x2;
            float y = (left.Y * right.W) + (right.Y * left.W) + y2;
            float z = (left.Z * right.W) + (right.Z * left.W) + z2;
            float w = (left.W * right.W) - w2;

            result.X = x;
            result.Y = y;
            result.Z = z;
            result.W = w;
        }

        #endregion

        #region Negate

        /// <summary>
        /// Flips the sign of the all the quaternion components.
        /// </summary>
        /// <param name="value">Source <see cref="Quaternion"/>.</param>
        /// <returns>The result of the quaternion negation.</returns>
        public static Quaternion Negate(Quaternion value)
        {
            return new Quaternion(-value.X, -value.Y, -value.Z, -value.W);
        }

        /// <summary>
        /// Flips the sign of the all the quaternion components.
        /// </summary>
        /// <param name="value">Source <see cref="Quaternion"/>.</param>
        /// <param name="result">The result of the quaternion negation as an output parameter.</param>
        public static void Negate(ref Quaternion value, out Quaternion result)
        {
            result.X = -value.X;
            result.Y = -value.Y;
            result.Z = -value.Z;
            result.W = -value.W;
        }

        #endregion

        #region Normalize

        /// <summary>
        /// Scales the quaternion magnitude to unit length.
        /// </summary>
        public void Normalize()
        {
            float magnitute = (float)Math.Sqrt((X * X) + (Y * Y) + (Z * Z) + (W * W));

            float factor = 1f / magnitute;
            X *= factor;
            Y *= factor;
            Z *= factor;
            W *= factor;
        }

        /// <summary>
        /// Scales the quaternion magnitude to unit length.
        /// </summary>
        /// <param name="value">Source <see cref="Quaternion"/>.</param>
        /// <returns>The unit length quaternion.</returns>
        public static Quaternion Normalize(Quaternion value)
        {
            float magnitute = (float)Math.Sqrt((value.X * value.X) + (value.Y * value.Y) + (value.Z * value.Z) + (value.W * value.W));

            Quaternion result;
            float factor = 1f / magnitute;
            result.X = value.X * factor;
            result.Y = value.Y * factor;
            result.Z = value.Z * factor;
            result.W = value.W * factor;
            return result;
        }

        /// <summary>
        /// Scales the quaternion magnitude to unit length.
        /// </summary>
        /// <param name="value">Source <see cref="Quaternion"/>.</param>
        /// <param name="result">The unit length quaternion an output parameter.</param>
        public static void Normalize(ref Quaternion value, out Quaternion result)
        {
            float magnitute = (float)Math.Sqrt((value.X * value.X) + (value.Y * value.Y) + (value.Z * value.Z) + (value.W * value.W));

            float factor = 1f / magnitute;
            result.X = value.X * factor;
            result.Y = value.Y * factor;
            result.Z = value.Z * factor;
            result.W = value.W * factor;
        }

        #endregion

        /// <summary>
        /// Returns a <see cref="String"/> representation of this <see cref="Quaternion"/> in the format:
        /// {X:[<see cref="X"/>] Y:[<see cref="Y"/>] Z:[<see cref="Z"/>] W:[<see cref="W"/>]}
        /// </summary>
        /// <returns>A <see cref="String"/> representation of this <see cref="Quaternion"/>.</returns>
        public override string ToString()
        {
            return "{X:" + X + " Y:" + Y + " Z:" + Z + " W:" + W + "}";
        }

        /// <summary>
        /// Gets a <see cref="Vector4"/> representation for this object.
        /// </summary>
        /// <returns>A <see cref="Vector4"/> representation for this object.</returns>
        public Vector4 ToVector4()
        {
            return new Vector4(X,Y,Z,W);
        }

        public void Deconstruct(out float x, out float y, out float z, out float w)
        {
            x = X;
            y = Y;
            z = Z;
            w = W;
        }

        #endregion

        #region Operators

        /// <summary>
        /// Adds two quaternions.
        /// </summary>
        /// <param name="left">Source <see cref="Quaternion"/> on the left of the add sign.</param>
        /// <param name="right">Source <see cref="Quaternion"/> on the right of the add sign.</param>
        /// <returns>Sum of the vectors.</returns>
        public static Quaternion operator +(Quaternion left, Quaternion right)
        {
            Quaternion result;
            result.X = left.X + right.X;
            result.Y = left.Y + right.Y;
            result.Z = left.Z + right.Z;
            result.W = left.W + right.W;
            return result;
        }

        /// <summary>
        /// Divides a <see cref="Quaternion"/> by the other <see cref="Quaternion"/>.
        /// </summary>
        /// <param name="left">Source <see cref="Quaternion"/> on the left of the div sign.</param>
        /// <param name="right">Divisor <see cref="Quaternion"/> on the right of the div sign.</param>
        /// <returns>The result of dividing the quaternions.</returns>
        public static Quaternion operator /(Quaternion left, Quaternion right)
        {
            // Opt: right = Quaternion.Inverse(right);
            float dot = right.X * right.X + right.Y * right.Y + right.Z * right.Z + right.W * right.W;
            float factor = 1f / dot;
            right.X = -right.X * factor;
            right.Y = -right.Y * factor;
            right.Z = -right.Z * factor;
            right.W =  right.W * factor;

            float x2 = (left.Y * right.Z) - (left.Z * right.Y);
            float y2 = (left.Z * right.X) - (left.X * right.Z);
            float z2 = (left.X * right.Y) - (left.Y * right.X);
            float w2 = (left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z);

            Quaternion result;
            result.X = (left.X * right.W) + (right.X * left.W) + x2;
            result.Y = (left.Y * right.W) + (right.Y * left.W) + y2;
            result.Z = (left.Z * right.W) + (right.Z * left.W) + z2;
            result.W = (left.W * right.W) - w2;
            return result;
        }

        /// <summary>
        /// Compares whether two <see cref="Quaternion"/> instances are equal.
        /// </summary>
        /// <param name="left"><see cref="Quaternion"/> instance on the left of the equal sign.</param>
        /// <param name="right"><see cref="Quaternion"/> instance on the right of the equal sign.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public static bool operator ==(Quaternion left, Quaternion right)
        {
            return ((left.X == right.X) && (left.Y == right.Y) && (left.Z == right.Z) && (left.W == right.W));
        }

        /// <summary>
        /// Compares whether two <see cref="Quaternion"/> instances are not equal.
        /// </summary>
        /// <param name="left"><see cref="Quaternion"/> instance on the left of the not equal sign.</param>
        /// <param name="right"><see cref="Quaternion"/> instance on the right of the not equal sign.</param>
        /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
        public static bool operator !=(Quaternion left, Quaternion right)
        {
            if ((left.X == right.X) && (left.Y == right.Y) && (left.Z == right.Z))
            {
                return (left.W != right.W);
            }
            return true;
        }

        /// <summary>
        /// Multiplies two quaternions.
        /// </summary>
        /// <param name="left">Source <see cref="Quaternion"/> on the left of the mul sign.</param>
        /// <param name="right">Source <see cref="Quaternion"/> on the right of the mul sign.</param>
        /// <returns>Result of the quaternions multiplication.</returns>
        public static Quaternion operator *(Quaternion left, Quaternion right)
        {
            float x2 = (left.Y * right.Z) - (left.Z * right.Y);
            float y2 = (left.Z * right.X) - (left.X * right.Z);
            float z2 = (left.X * right.Y) - (left.Y * right.X);
            float w2 = (left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z);

            Quaternion result;
            result.X = (left.X * right.W) + (right.X * left.W) + x2;
            result.Y = (left.Y * right.W) + (right.Y * left.W) + y2;
            result.Z = (left.Z * right.W) + (right.Z * left.W) + z2;
            result.W = (left.W * right.W) - w2;
            return result;
        }

        /// <summary>
        /// Multiplies the components of quaternion by a scalar.
        /// </summary>
        /// <param name="left">Source <see cref="Vector3"/> on the left of the mul sign.</param>
        /// <param name="right">Scalar value on the right of the mul sign.</param>
        /// <returns>Result of the quaternion multiplication with a scalar.</returns>
        public static Quaternion operator *(Quaternion left, float right)
        {
            Quaternion result;
            result.X = left.X * right;
            result.Y = left.Y * right;
            result.Z = left.Z * right;
            result.W = left.W * right;
            return result;
        }

        /// <summary>
        /// Subtracts a <see cref="Quaternion"/> from a <see cref="Quaternion"/>.
        /// </summary>
        /// <param name="left">Source <see cref="Vector3"/> on the left of the sub sign.</param>
        /// <param name="right">Source <see cref="Vector3"/> on the right of the sub sign.</param>
        /// <returns>Result of the quaternion subtraction.</returns>
        public static Quaternion operator -(Quaternion left, Quaternion right)
        {
            Quaternion result;
            result.X = left.X - right.X;
            result.Y = left.Y - right.Y;
            result.Z = left.Z - right.Z;
            result.W = left.W - right.W;
            return result;

        }

        /// <summary>
        /// Flips the sign of the all the quaternion components.
        /// </summary>
        /// <param name="value">Source <see cref="Quaternion"/> on the right of the sub sign.</param>
        /// <returns>The result of the quaternion negation.</returns>
        public static Quaternion operator -(Quaternion value)
        {
            Quaternion quaternion2;
            quaternion2.X = -value.X;
            quaternion2.Y = -value.Y;
            quaternion2.Z = -value.Z;
            quaternion2.W = -value.W;
            return quaternion2;
        }

        #endregion
    }
}
