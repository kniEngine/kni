// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Runtime.Serialization;

#if NET8_0_OR_GREATER
using System.Runtime.CompilerServices;
using SysNumerics = System.Numerics;
#endif

namespace Microsoft.Xna.Framework
{ 
    /// <summary>
    /// Represents the right-handed 4x4 floating point matrix, which can store translation, scale and rotation information.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{DebugDisplayString,nq}")]
    public struct Matrix : IEquatable<Matrix>
    {
        #region Public Constructors

        /// <summary>
        /// Constructs a matrix.
        /// </summary>
        /// <param name="m11">A first row and first column value.</param>
        /// <param name="m12">A first row and second column value.</param>
        /// <param name="m13">A first row and third column value.</param>
        /// <param name="m14">A first row and fourth column value.</param>
        /// <param name="m21">A second row and first column value.</param>
        /// <param name="m22">A second row and second column value.</param>
        /// <param name="m23">A second row and third column value.</param>
        /// <param name="m24">A second row and fourth column value.</param>
        /// <param name="m31">A third row and first column value.</param>
        /// <param name="m32">A third row and second column value.</param>
        /// <param name="m33">A third row and third column value.</param>
        /// <param name="m34">A third row and fourth column value.</param>
        /// <param name="m41">A fourth row and first column value.</param>
        /// <param name="m42">A fourth row and second column value.</param>
        /// <param name="m43">A fourth row and third column value.</param>
        /// <param name="m44">A fourth row and fourth column value.</param>
        public Matrix(float m11, float m12, float m13, float m14, float m21, float m22, float m23, float m24, float m31,
                      float m32, float m33, float m34, float m41, float m42, float m43, float m44)
        {
            this.M11 = m11;
            this.M12 = m12;
            this.M13 = m13;
            this.M14 = m14;
            this.M21 = m21;
            this.M22 = m22;
            this.M23 = m23;
            this.M24 = m24;
            this.M31 = m31;
            this.M32 = m32;
            this.M33 = m33;
            this.M34 = m34;
            this.M41 = m41;
            this.M42 = m42;
            this.M43 = m43;
            this.M44 = m44;
        }

        /// <summary>
        /// Constructs a matrix.
        /// </summary>
        /// <param name="row1">A first row of the created matrix.</param>
        /// <param name="row2">A second row of the created matrix.</param>
        /// <param name="row3">A third row of the created matrix.</param>
        /// <param name="row4">A fourth row of the created matrix.</param>
        public Matrix(Vector4 row1, Vector4 row2, Vector4 row3, Vector4 row4)
        {
            this.M11 = row1.X;
            this.M12 = row1.Y;
            this.M13 = row1.Z;
            this.M14 = row1.W;
            this.M21 = row2.X;
            this.M22 = row2.Y;
            this.M23 = row2.Z;
            this.M24 = row2.W;
            this.M31 = row3.X;
            this.M32 = row3.Y;
            this.M33 = row3.Z;
            this.M34 = row3.W;
            this.M41 = row4.X;
            this.M42 = row4.Y;
            this.M43 = row4.Z;
            this.M44 = row4.W;
        }

        #endregion

        #region Public Fields

        /// <summary>
        /// A first row and first column value.
        /// </summary>
        [DataMember]
        public float M11;

        /// <summary>
        /// A first row and second column value.
        /// </summary>
        [DataMember]
        public float M12;

        /// <summary>
        /// A first row and third column value.
        /// </summary>
        [DataMember]
        public float M13;

        /// <summary>
        /// A first row and fourth column value.
        /// </summary>
        [DataMember]
        public float M14;

        /// <summary>
        /// A second row and first column value.
        /// </summary>
        [DataMember]
        public float M21;

        /// <summary>
        /// A second row and second column value.
        /// </summary>
        [DataMember]
        public float M22;

        /// <summary>
        /// A second row and third column value.
        /// </summary>
        [DataMember]
        public float M23;

        /// <summary>
        /// A second row and fourth column value.
        /// </summary>
        [DataMember]
        public float M24;

        /// <summary>
        /// A third row and first column value.
        /// </summary>
        [DataMember]
        public float M31;

        /// <summary>
        /// A third row and second column value.
        /// </summary>
        [DataMember]
        public float M32;

        /// <summary>
        /// A third row and third column value.
        /// </summary>
        [DataMember]
        public float M33;

        /// <summary>
        /// A third row and fourth column value.
        /// </summary>
        [DataMember]
        public float M34;

        /// <summary>
        /// A fourth row and first column value.
        /// </summary>
        [DataMember]
        public float M41;

        /// <summary>
        /// A fourth row and second column value.
        /// </summary>
        [DataMember]
        public float M42;

        /// <summary>
        /// A fourth row and third column value.
        /// </summary>
        [DataMember]
        public float M43;

        /// <summary>
        /// A fourth row and fourth column value.
        /// </summary>
        [DataMember]
        public float M44;

        #endregion

        #region Indexers

        /// <summary>
        /// Get or set the matrix element at the given index, indexed in row major order.
        /// </summary>
        /// <param name="index">The linearized, zero-based index of the matrix element.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If the index is less than <code>0</code> or larger than <code>15</code>.
        /// </exception>
        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return M11;
                    case 1: return M12;
                    case 2: return M13;
                    case 3: return M14;
                    case 4: return M21;
                    case 5: return M22;
                    case 6: return M23;
                    case 7: return M24;
                    case 8: return M31;
                    case 9: return M32;
                    case 10: return M33;
                    case 11: return M34;
                    case 12: return M41;
                    case 13: return M42;
                    case 14: return M43;
                    case 15: return M44;
                    default: throw new ArgumentOutOfRangeException();
                }
            }

            set
            {
                switch (index)
                {
                    case 0: M11 = value; break;
                    case 1: M12 = value; break;
                    case 2: M13 = value; break;
                    case 3: M14 = value; break;
                    case 4: M21 = value; break;
                    case 5: M22 = value; break;
                    case 6: M23 = value; break;
                    case 7: M24 = value; break;
                    case 8: M31 = value; break;
                    case 9: M32 = value; break;
                    case 10: M33 = value; break;
                    case 11: M34 = value; break;
                    case 12: M41 = value; break;
                    case 13: M42 = value; break;
                    case 14: M43 = value; break;
                    case 15: M44 = value; break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Get or set the value at the specified row and column (indices are zero-based).
        /// </summary>
        /// <param name="row">The row of the element.</param>
        /// <param name="column">The column of the element.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If the row or column is less than <code>0</code> or larger than <code>3</code>.
        /// </exception>
        public float this[int row, int column]
        {
            get
            {
                return this[(row * 4) + column];
            }

            set
            {
                this[(row * 4) + column] = value;
            }
        }

        #endregion

        #region Private Members
        private static Matrix identity = new Matrix(1f, 0f, 0f, 0f, 
                                                    0f, 1f, 0f, 0f, 
                                                    0f, 0f, 1f, 0f, 
                                                    0f, 0f, 0f, 1f);
        #endregion

        #region Public Properties

        /// <summary>
        /// The backward vector formed from the third row M31, M32, M33 elements.
        /// </summary>
        public Vector3 Backward
        {
            get { return new Vector3(this.M31, this.M32, this.M33); }
            set
            {
                this.M31 = value.X;
                this.M32 = value.Y;
                this.M33 = value.Z;
            }
        }

        /// <summary>
        /// The down vector formed from the second row -M21, -M22, -M23 elements.
        /// </summary>
        public Vector3 Down
        {
            get { return new Vector3(-this.M21, -this.M22, -this.M23); }
            set
            {
                this.M21 = -value.X;
                this.M22 = -value.Y;
                this.M23 = -value.Z;
            }
        }

        /// <summary>
        /// The forward vector formed from the third row -M31, -M32, -M33 elements.
        /// </summary>
        public Vector3 Forward
        {
            get { return new Vector3(-this.M31, -this.M32, -this.M33); }
            set
            {
                this.M31 = -value.X;
                this.M32 = -value.Y;
                this.M33 = -value.Z;
            }
        }

        /// <summary>
        /// Returns the identity matrix.
        /// </summary>
        public static Matrix Identity
        {
            get { return identity; }
        }

        /// <summary>
        /// The left vector formed from the first row -M11, -M12, -M13 elements.
        /// </summary>
        public Vector3 Left
        {
            get { return new Vector3(-this.M11, -this.M12, -this.M13); }
            set
            {
                this.M11 = -value.X;
                this.M12 = -value.Y;
                this.M13 = -value.Z;
            }
        }

        /// <summary>
        /// The right vector formed from the first row M11, M12, M13 elements.
        /// </summary>
        public Vector3 Right
        {
            get { return new Vector3(this.M11, this.M12, this.M13); }
            set
            {
                this.M11 = value.X;
                this.M12 = value.Y;
                this.M13 = value.Z;
            }
        }

        /// <summary>
        /// Position stored in this matrix.
        /// </summary>
        public Vector3 Translation
        {
            get { return new Vector3(this.M41, this.M42, this.M43); }
            set
            {
                this.M41 = value.X;
                this.M42 = value.Y;
                this.M43 = value.Z;
            }
        }

        /// <summary>
        /// The upper vector formed from the second row M21, M22, M23 elements.
        /// </summary>
        public Vector3 Up
        {
            get { return new Vector3(this.M21, this.M22, this.M23); }
            set
            {
                this.M21 = value.X;
                this.M22 = value.Y;
                this.M23 = value.Z;
            }
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a new <see cref="Matrix"/> which contains sum of two matrixes.
        /// </summary>
        /// <param name="left">The first matrix to add.</param>
        /// <param name="right">The second matrix to add.</param>
        /// <returns>The result of the matrix addition.</returns>
        public static Matrix Add(Matrix left, Matrix right)
        {
            left.M11 += right.M11;
            left.M12 += right.M12;
            left.M13 += right.M13;
            left.M14 += right.M14;
            left.M21 += right.M21;
            left.M22 += right.M22;
            left.M23 += right.M23;
            left.M24 += right.M24;
            left.M31 += right.M31;
            left.M32 += right.M32;
            left.M33 += right.M33;
            left.M34 += right.M34;
            left.M41 += right.M41;
            left.M42 += right.M42;
            left.M43 += right.M43;
            left.M44 += right.M44;
            return left;
        }

        /// <summary>
        /// Creates a new <see cref="Matrix"/> which contains sum of two matrixes.
        /// </summary>
        /// <param name="left">The first matrix to add.</param>
        /// <param name="right">The second matrix to add.</param>
        /// <param name="result">The result of the matrix addition as an output parameter.</param>
        public static void Add(ref Matrix left, ref Matrix right, out Matrix result)
        {
            result.M11 = left.M11 + right.M11;
            result.M12 = left.M12 + right.M12;
            result.M13 = left.M13 + right.M13;
            result.M14 = left.M14 + right.M14;
            result.M21 = left.M21 + right.M21;
            result.M22 = left.M22 + right.M22;
            result.M23 = left.M23 + right.M23;
            result.M24 = left.M24 + right.M24;
            result.M31 = left.M31 + right.M31;
            result.M32 = left.M32 + right.M32;
            result.M33 = left.M33 + right.M33;
            result.M34 = left.M34 + right.M34;
            result.M41 = left.M41 + right.M41;
            result.M42 = left.M42 + right.M42;
            result.M43 = left.M43 + right.M43;
            result.M44 = left.M44 + right.M44;

        }

        /// <summary>
        /// Creates a new <see cref="Matrix"/> for spherical billboarding that rotates around specified object position.
        /// </summary>
        /// <param name="objectPosition">Position of billboard object. It will rotate around that vector.</param>
        /// <param name="cameraPosition">The camera position.</param>
        /// <param name="cameraUpVector">The camera up vector.</param>
        /// <param name="cameraForwardVector">Optional camera forward vector.</param>
        /// <returns>The <see cref="Matrix"/> for spherical billboarding.</returns>
        public static Matrix CreateBillboard(Vector3 objectPosition, Vector3 cameraPosition,
            Vector3 cameraUpVector, Nullable<Vector3> cameraForwardVector)
        {
            Matrix result;

            // Delegate to the other overload of the function to do the work
            CreateBillboard(ref objectPosition, ref cameraPosition, ref cameraUpVector, cameraForwardVector, out result);

            return result;
        }

        /// <summary>
        /// Creates a new <see cref="Matrix"/> for spherical billboarding that rotates around specified object position.
        /// </summary>
        /// <param name="objectPosition">Position of billboard object. It will rotate around that vector.</param>
        /// <param name="cameraPosition">The camera position.</param>
        /// <param name="cameraUpVector">The camera up vector.</param>
        /// <param name="cameraForwardVector">Optional camera forward vector.</param>
        /// <param name="result">The <see cref="Matrix"/> for spherical billboarding as an output parameter.</param>
        public static void CreateBillboard(ref Vector3 objectPosition, ref Vector3 cameraPosition,
            ref Vector3 cameraUpVector, Vector3? cameraForwardVector, out Matrix result)
        {
            Vector3 diffPosition;
            diffPosition.X = objectPosition.X - cameraPosition.X;
            diffPosition.Y = objectPosition.Y - cameraPosition.Y;
            diffPosition.Z = objectPosition.Z - cameraPosition.Z;
            float distanceSq = diffPosition.LengthSquared();
            if (distanceSq < 0.0001f)
            {
                diffPosition = (cameraForwardVector.HasValue)
                             ? -cameraForwardVector.Value
                             : Vector3.Forward;
            }
            else
            {
                float distance = (float)Math.Sqrt((double)distanceSq);
                Vector3.Divide(ref diffPosition, distance, out diffPosition);
            }

            Vector3 right;
            Vector3.Cross(ref cameraUpVector, ref diffPosition, out right);
            right.Normalize();
            Vector3 up;
            Vector3.Cross(ref diffPosition, ref right, out up);

            result.M11 = right.X;
            result.M12 = right.Y;
            result.M13 = right.Z;
            result.M14 = 0;
            result.M21 = up.X;
            result.M22 = up.Y;
            result.M23 = up.Z;
            result.M24 = 0;
            result.M31 = diffPosition.X;
            result.M32 = diffPosition.Y;
            result.M33 = diffPosition.Z;
            result.M34 = 0;
            result.M41 = objectPosition.X;
            result.M42 = objectPosition.Y;
            result.M43 = objectPosition.Z;
            result.M44 = 1;
        }

        /// <summary>
        /// Creates a new <see cref="Matrix"/> for cylindrical billboarding that rotates around specified axis.
        /// </summary>
        /// <param name="objectPosition">Object position the billboard will rotate around.</param>
        /// <param name="cameraPosition">Camera position.</param>
        /// <param name="rotateAxis">Axis of billboard for rotation.</param>
        /// <param name="cameraForwardVector">Optional camera forward vector.</param>
        /// <param name="objectForwardVector">Optional object forward vector.</param>
        /// <returns>The <see cref="Matrix"/> for cylindrical billboarding.</returns>
        public static Matrix CreateConstrainedBillboard(Vector3 objectPosition, Vector3 cameraPosition,
            Vector3 rotateAxis, Nullable<Vector3> cameraForwardVector, Nullable<Vector3> objectForwardVector)
        {
            Matrix result;
            CreateConstrainedBillboard(ref objectPosition, ref cameraPosition, ref rotateAxis,
                cameraForwardVector, objectForwardVector, out result);
            return result;
        }

        /// <summary>
        /// Creates a new <see cref="Matrix"/> for cylindrical billboarding that rotates around specified axis.
        /// </summary>
        /// <param name="objectPosition">Object position the billboard will rotate around.</param>
        /// <param name="cameraPosition">Camera position.</param>
        /// <param name="rotateAxis">Axis of billboard for rotation.</param>
        /// <param name="cameraForwardVector">Optional camera forward vector.</param>
        /// <param name="objectForwardVector">Optional object forward vector.</param>
        /// <param name="result">The <see cref="Matrix"/> for cylindrical billboarding as an output parameter.</param>
        public static void CreateConstrainedBillboard(ref Vector3 objectPosition, ref Vector3 cameraPosition,
            ref Vector3 rotateAxis, Vector3? cameraForwardVector, Vector3? objectForwardVector, out Matrix result)
        {
            Vector3 diffPosition;
            diffPosition.X = objectPosition.X - cameraPosition.X;
            diffPosition.Y = objectPosition.Y - cameraPosition.Y;
            diffPosition.Z = objectPosition.Z - cameraPosition.Z;
            float distanceSq = diffPosition.LengthSquared();
            if (distanceSq < 0.0001f)
            {
                diffPosition = (cameraForwardVector.HasValue)
                             ? -cameraForwardVector.Value
                             : Vector3.Forward;
            }
            else
            {
                float distance = (float)Math.Sqrt((double)distanceSq);
                Vector3.Divide(ref diffPosition, distance, out diffPosition);
            }

            Vector3 backward;
            Vector3 right;
            Vector3 up = rotateAxis;
            float dot;
            Vector3.Dot(ref rotateAxis, ref diffPosition, out dot);
            if (Math.Abs(dot) > 0.9982547f)
            {
                if (objectForwardVector.HasValue)
                {
                    backward = objectForwardVector.Value;
                    Vector3.Dot(ref rotateAxis, ref backward, out dot);
                    if (Math.Abs(dot) > 0.9982547f)
                    {
                        dot = ((rotateAxis.X * Vector3.Forward.X) + (rotateAxis.Y * Vector3.Forward.Y)) + (rotateAxis.Z * Vector3.Forward.Z);
                        backward = (Math.Abs(dot) > 0.9982547f)
                                 ? Vector3.Right
                                 : Vector3.Forward;
                    }
                }
                else
                {
                    dot = ((rotateAxis.X * Vector3.Forward.X) + (rotateAxis.Y * Vector3.Forward.Y)) + (rotateAxis.Z * Vector3.Forward.Z);
                    backward = (Math.Abs(dot) > 0.9982547f)
                             ? Vector3.Right
                             : Vector3.Forward;
                }
                Vector3.Cross(ref rotateAxis, ref backward, out right);
                right.Normalize();
                Vector3.Cross(ref right, ref rotateAxis, out backward);
                backward.Normalize();
            }
            else
            {
                Vector3.Cross(ref rotateAxis, ref diffPosition, out right);
                right.Normalize();
                Vector3.Cross(ref right, ref up, out backward);
                backward.Normalize();
            }
            result.M11 = right.X;
            result.M12 = right.Y;
            result.M13 = right.Z;
            result.M14 = 0;
            result.M21 = up.X;
            result.M22 = up.Y;
            result.M23 = up.Z;
            result.M24 = 0;
            result.M31 = backward.X;
            result.M32 = backward.Y;
            result.M33 = backward.Z;
            result.M34 = 0;
            result.M41 = objectPosition.X;
            result.M42 = objectPosition.Y;
            result.M43 = objectPosition.Z;
            result.M44 = 1;

        }

        /// <summary>
        /// Creates a new <see cref="Matrix"/> which contains the rotation moment around specified axis.
        /// </summary>
        /// <param name="axis">The axis of rotation.</param>
        /// <param name="angle">The angle of rotation in radians.</param>
        /// <returns>The rotation <see cref="Matrix"/>.</returns>
        public static Matrix CreateFromAxisAngle(Vector3 axis, float angle)
        {
            Matrix result;
            CreateFromAxisAngle(ref axis, angle, out result);
            return result;
        }

        /// <summary>
        /// Creates a new <see cref="Matrix"/> which contains the rotation moment around specified axis.
        /// </summary>
        /// <param name="axis">The axis of rotation.</param>
        /// <param name="angle">The angle of rotation in radians.</param>
        /// <param name="result">The rotation <see cref="Matrix"/> as an output parameter.</param>
        public static void CreateFromAxisAngle(ref Vector3 axis, float angle, out Matrix result)
        {
            float x = axis.X;
            float y = axis.Y;
            float z = axis.Z;

            float sin = (float) Math.Sin((double)angle);
            float cos = (float) Math.Cos((double)angle);

            float xx = x * x;
            float yy = y * y;
            float zz = z * z;
            float xy = x * y;
            float xz = x * z;
            float yz = y * z;

            result.M11 = xx + (cos * (1f - xx));
            result.M12 = xy - (cos * xy) + (sin * z);
            result.M13 = xz - (cos * xz) - (sin * y);
            result.M14 = 0;
            result.M21 = xy - (cos * xy) - (sin * z);
            result.M22 = yy + (cos * (1f - yy));
            result.M23 = yz - (cos * yz) + (sin * x);
            result.M24 = 0;
            result.M31 = xz - (cos * xz) + (sin * y);
            result.M32 = yz - (cos * yz) - (sin * x);
            result.M33 = zz + (cos * (1f - zz));
            result.M34 = 0;
            result.M41 = 0;
            result.M42 = 0;
            result.M43 = 0;
            result.M44 = 1;
        }

        /// <summary>
        /// Creates a new rotation <see cref="Matrix"/> from a <see cref="Quaternion"/>.
        /// </summary>
        /// <param name="quaternion"><see cref="Quaternion"/> of rotation moment.</param>
        /// <returns>The rotation <see cref="Matrix"/>.</returns>
        public static Matrix CreateFromQuaternion(Quaternion quaternion)
        {
            Matrix result;
            CreateFromQuaternion(ref quaternion, out result);
            return result;
        }

        /// <summary>
        /// Creates a new rotation <see cref="Matrix"/> from a <see cref="Quaternion"/>.
        /// </summary>
        /// <param name="quaternion"><see cref="Quaternion"/> of rotation moment.</param>
        /// <param name="result">The rotation <see cref="Matrix"/> as an output parameter.</param>
        public static void CreateFromQuaternion(ref Quaternion quaternion, out Matrix result)
        {
            float xx = quaternion.X * quaternion.X;
            float yy = quaternion.Y * quaternion.Y;
            float zz = quaternion.Z * quaternion.Z;
            float xy = quaternion.X * quaternion.Y;
            float zw = quaternion.Z * quaternion.W;
            float zx = quaternion.Z * quaternion.X;
            float yw = quaternion.Y * quaternion.W;
            float yz = quaternion.Y * quaternion.Z;
            float xw = quaternion.X * quaternion.W;

            result.M11 = 1f - (2f * (yy + zz));
            result.M12 = 2f * (xy + zw);
            result.M13 = 2f * (zx - yw);
            result.M14 = 0f;
            result.M21 = 2f * (xy - zw);
            result.M22 = 1f - (2f * (zz + xx));
            result.M23 = 2f * (yz + xw);
            result.M24 = 0f;
            result.M31 = 2f * (zx + yw);
            result.M32 = 2f * (yz - xw);
            result.M33 = 1f - (2f * (yy + xx));
            result.M34 = 0f;
            result.M41 = 0f;
            result.M42 = 0f;
            result.M43 = 0f;
            result.M44 = 1f;
        }

        /// <summary>
        /// Creates a new transform <see cref="Matrix"/> from a <see cref="Pose3"/>.
        /// </summary>
        /// <param name="pose">The <see cref="Pose3"/>.</param>
        /// <returns>The transform <see cref="Matrix"/>.</returns>
        public static Matrix CreateFromPose(Pose3 pose)
        {
            Matrix result;
            result = Matrix.CreateFromQuaternion(pose.Orientation);
            result.Translation = pose.Translation;
            return result;
        }

        /// <summary>
        /// Creates a new rotation <see cref="Matrix"/> from the specified yaw, pitch and roll values.
        /// </summary>
        /// <param name="yaw">The yaw rotation value in radians.</param>
        /// <param name="pitch">The pitch rotation value in radians.</param>
        /// <param name="roll">The roll rotation value in radians.</param>
        /// <returns>The rotation <see cref="Matrix"/>.</returns>
        /// <remarks>For more information about yaw, pitch and roll visit http://en.wikipedia.org/wiki/Euler_angles.
        /// </remarks>
        public static Matrix CreateFromYawPitchRoll(float yaw, float pitch, float roll)
        {
            Matrix result;
            CreateFromYawPitchRoll(yaw, pitch, roll, out result);
            return result;
        }

        /// <summary>
        /// Creates a new rotation <see cref="Matrix"/> from the specified yaw, pitch and roll values.
        /// </summary>
        /// <param name="yaw">The yaw rotation value in radians.</param>
        /// <param name="pitch">The pitch rotation value in radians.</param>
        /// <param name="roll">The roll rotation value in radians.</param>
        /// <param name="result">The rotation <see cref="Matrix"/> as an output parameter.</param>
        /// <remarks>For more information about yaw, pitch and roll visit http://en.wikipedia.org/wiki/Euler_angles.
        /// </remarks>
        public static void CreateFromYawPitchRoll(float yaw, float pitch, float roll, out Matrix result)
        {
            Quaternion quaternion;
            Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll, out quaternion);
            CreateFromQuaternion(ref quaternion, out result);
        }

        /// <summary>
        /// Creates a new viewing <see cref="Matrix"/>.
        /// </summary>
        /// <param name="cameraPosition">Position of the camera.</param>
        /// <param name="cameraTarget">Lookup vector of the camera.</param>
        /// <param name="cameraUpVector">The direction of the upper edge of the camera.</param>
        /// <returns>The viewing <see cref="Matrix"/>.</returns>
        public static Matrix CreateLookAt(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector)
        {
            Matrix result;
            CreateLookAt(ref cameraPosition, ref cameraTarget, ref cameraUpVector, out result);
            return result;
        }

        /// <summary>
        /// Creates a new viewing <see cref="Matrix"/>.
        /// </summary>
        /// <param name="cameraPosition">Position of the camera.</param>
        /// <param name="cameraTarget">Lookup vector of the camera.</param>
        /// <param name="cameraUpVector">The direction of the upper edge of the camera.</param>
        /// <param name="result">The viewing <see cref="Matrix"/> as an output parameter.</param>
        public static void CreateLookAt(ref Vector3 cameraPosition, ref Vector3 cameraTarget, ref Vector3 cameraUpVector, out Matrix result)
        {
            Vector3 vec3 = Vector3.Normalize(cameraPosition - cameraTarget);
            Vector3 vec1 = Vector3.Normalize(Vector3.Cross(cameraUpVector, vec3));
            Vector3 vec2 = Vector3.Cross(vec3, vec1);
            result.M11 = vec1.X;
            result.M12 = vec2.X;
            result.M13 = vec3.X;
            result.M14 = 0f;
            result.M21 = vec1.Y;
            result.M22 = vec2.Y;
            result.M23 = vec3.Y;
            result.M24 = 0f;
            result.M31 = vec1.Z;
            result.M32 = vec2.Z;
            result.M33 = vec3.Z;
            result.M34 = 0f;
            result.M41 = -Vector3.Dot(vec1, cameraPosition);
            result.M42 = -Vector3.Dot(vec2, cameraPosition);
            result.M43 = -Vector3.Dot(vec3, cameraPosition);
            result.M44 = 1f;
        }

        /// <summary>
        /// Creates a new projection <see cref="Matrix"/> for orthographic view.
        /// </summary>
        /// <param name="width">Width of the viewing volume.</param>
        /// <param name="height">Height of the viewing volume.</param>
        /// <param name="zNearPlane">Depth of the near plane.</param>
        /// <param name="zFarPlane">Depth of the far plane.</param>
        /// <returns>The new projection <see cref="Matrix"/> for orthographic view.</returns>
        public static Matrix CreateOrthographic(float width, float height, float zNearPlane, float zFarPlane)
        {
            Matrix result;
            CreateOrthographic(width, height, zNearPlane, zFarPlane, out result);
            return result;
        }

        /// <summary>
        /// Creates a new projection <see cref="Matrix"/> for orthographic view.
        /// </summary>
        /// <param name="width">Width of the viewing volume.</param>
        /// <param name="height">Height of the viewing volume.</param>
        /// <param name="zNearPlane">Depth of the near plane.</param>
        /// <param name="zFarPlane">Depth of the far plane.</param>
        /// <param name="result">The new projection <see cref="Matrix"/> for orthographic view as an output parameter.</param>
        public static void CreateOrthographic(float width, float height, float zNearPlane, float zFarPlane, out Matrix result)
        {
            result.M11 = 2f / width;
            result.M12 = result.M13 = result.M14 = 0f;
            result.M22 = 2f / height;
            result.M21 = result.M23 = result.M24 = 0f;
            result.M33 = 1f / (zNearPlane - zFarPlane);
            result.M31 = result.M32 = result.M34 = 0f;
            result.M41 = result.M42 = 0f;
            result.M43 = zNearPlane / (zNearPlane - zFarPlane);
            result.M44 = 1f;
        }

        /// <summary>
        /// Creates a new projection <see cref="Matrix"/> for customized orthographic view.
        /// </summary>
        /// <param name="left">Lower x-value at the near plane.</param>
        /// <param name="right">Upper x-value at the near plane.</param>
        /// <param name="bottom">Lower y-coordinate at the near plane.</param>
        /// <param name="top">Upper y-value at the near plane.</param>
        /// <param name="zNearPlane">Depth of the near plane.</param>
        /// <param name="zFarPlane">Depth of the far plane.</param>
        /// <returns>The new projection <see cref="Matrix"/> for customized orthographic view.</returns>
        public static Matrix CreateOrthographicOffCenter(float left, float right, float bottom, float top, float zNearPlane, float zFarPlane)
        {
            Matrix result;
            CreateOrthographicOffCenter(left, right, bottom, top, zNearPlane, zFarPlane, out result);
            return result;
        }

        /// <summary>
        /// Creates a new projection <see cref="Matrix"/> for customized orthographic view.
        /// </summary>
        /// <param name="viewingVolume">The viewing volume.</param>
        /// <param name="zNearPlane">Depth of the near plane.</param>
        /// <param name="zFarPlane">Depth of the far plane.</param>
        /// <returns>The new projection <see cref="Matrix"/> for customized orthographic view.</returns>
        public static Matrix CreateOrthographicOffCenter(Rectangle viewingVolume, float zNearPlane, float zFarPlane)
        {
            Matrix result;
            CreateOrthographicOffCenter(viewingVolume.Left, viewingVolume.Right, viewingVolume.Bottom, viewingVolume.Top, zNearPlane, zFarPlane, out result);
            return result;
        }

        /// <summary>
        /// Creates a new projection <see cref="Matrix"/> for customized orthographic view.
        /// </summary>
        /// <param name="left">Lower x-value at the near plane.</param>
        /// <param name="right">Upper x-value at the near plane.</param>
        /// <param name="bottom">Lower y-coordinate at the near plane.</param>
        /// <param name="top">Upper y-value at the near plane.</param>
        /// <param name="zNearPlane">Depth of the near plane.</param>
        /// <param name="zFarPlane">Depth of the far plane.</param>
        /// <param name="result">The new projection <see cref="Matrix"/> for customized orthographic view as an output parameter.</param>
        public static void CreateOrthographicOffCenter(float left, float right, float bottom, float top, float zNearPlane, float zFarPlane, out Matrix result)
        {
            result.M11 = (float)(2.0 / ((double)right - (double)left));
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M14 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = (float)(2.0 / ((double)top - (double)bottom));
            result.M23 = 0.0f;
            result.M24 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = (float)(1.0 / ((double)zNearPlane - (double)zFarPlane));
            result.M34 = 0.0f;
            result.M41 = (float)(((double)left + (double)right) / ((double)left - (double)right));
            result.M42 = (float)(((double)top + (double)bottom) / ((double)bottom - (double)top));
            result.M43 = (float)((double)zNearPlane / ((double)zNearPlane - (double)zFarPlane));
            result.M44 = 1.0f;
        }

        /// <summary>
        /// Creates a new projection <see cref="Matrix"/> for perspective view.
        /// </summary>
        /// <param name="width">Width of the viewing volume.</param>
        /// <param name="height">Height of the viewing volume.</param>
        /// <param name="nearPlaneDistance">Distance to the near plane.</param>
        /// <param name="farPlaneDistance">Distance to the far plane.</param>
        /// <returns>The new projection <see cref="Matrix"/> for perspective view.</returns>
        public static Matrix CreatePerspective(float width, float height, float nearPlaneDistance, float farPlaneDistance)
        {
            Matrix result;
            CreatePerspective(width, height, nearPlaneDistance, farPlaneDistance, out result);
            return result;
        }

        /// <summary>
        /// Creates a new projection <see cref="Matrix"/> for perspective view.
        /// </summary>
        /// <param name="width">Width of the viewing volume.</param>
        /// <param name="height">Height of the viewing volume.</param>
        /// <param name="nearPlaneDistance">Distance to the near plane.</param>
        /// <param name="farPlaneDistance">Distance to the far plane.</param>
        /// <param name="result">The new projection <see cref="Matrix"/> for perspective view as an output parameter.</param>
        public static void CreatePerspective(float width, float height, float nearPlaneDistance, float farPlaneDistance, out Matrix result)
        {
            if (nearPlaneDistance <= 0f)
            {
                throw new ArgumentException("nearPlaneDistance <= 0");
            }
            if (farPlaneDistance <= 0f)
            {
                throw new ArgumentException("farPlaneDistance <= 0");
            }
            if (nearPlaneDistance >= farPlaneDistance)
            {
                throw new ArgumentException("nearPlaneDistance >= farPlaneDistance");
            }

            result.M11 = (2.0f * nearPlaneDistance) / width;
            result.M12 = result.M13 = result.M14 = 0.0f;
            result.M22 = (2.0f * nearPlaneDistance) / height;
            result.M21 = result.M23 = result.M24 = 0.0f;            
            result.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
            result.M31 = result.M32 = 0.0f;
            result.M34 = -1.0f;
            result.M41 = result.M42 = result.M44 = 0.0f;
            result.M43 = (nearPlaneDistance * farPlaneDistance) / (nearPlaneDistance - farPlaneDistance);
        }

        /// <summary>
        /// Creates a new projection <see cref="Matrix"/> for perspective view with field of view.
        /// </summary>
        /// <param name="fieldOfView">Field of view in the y direction in radians.</param>
        /// <param name="aspectRatio">Width divided by height of the viewing volume.</param>
        /// <param name="nearPlaneDistance">Distance to the near plane.</param>
        /// <param name="farPlaneDistance">Distance to the far plane.</param>
        /// <returns>The new projection <see cref="Matrix"/> for perspective view with FOV.</returns>
        public static Matrix CreatePerspectiveFieldOfView(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance)
        {
            Matrix result;
            CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearPlaneDistance, farPlaneDistance, out result);
            return result;
        }

        /// <summary>
        /// Creates a new projection <see cref="Matrix"/> for perspective view with field of view.
        /// </summary>
        /// <param name="fieldOfView">Field of view in the y direction in radians.</param>
        /// <param name="aspectRatio">Width divided by height of the viewing volume.</param>
        /// <param name="nearPlaneDistance">Distance of the near plane.</param>
        /// <param name="farPlaneDistance">Distance of the far plane.</param>
        /// <param name="result">The new projection <see cref="Matrix"/> for perspective view with FOV as an output parameter.</param>
        public static void CreatePerspectiveFieldOfView(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance, out Matrix result)
        {
            if ((fieldOfView <= 0f) || (fieldOfView >= 3.141593f))
            {
                throw new ArgumentException("fieldOfView <= 0 or >= PI");
            }
            if (nearPlaneDistance <= 0f)
            {
                throw new ArgumentException("nearPlaneDistance <= 0");
            }
            if (farPlaneDistance <= 0f)
            {
                throw new ArgumentException("farPlaneDistance <= 0");
            }
            if (nearPlaneDistance >= farPlaneDistance)
            {
                throw new ArgumentException("nearPlaneDistance >= farPlaneDistance");
            }

            float yScale = 1.0f / (float)Math.Tan((double)fieldOfView * 0.5f);
            float xScale = yScale / aspectRatio;

            result.M11 = xScale;
            result.M12 = result.M13 = result.M14 = 0.0f;
            result.M22 = yScale;
            result.M21 = result.M23 = result.M24 = 0.0f;
            result.M31 = result.M32 = 0.0f;            
            result.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
            result.M34 = -1.0f;
            result.M41 = result.M42 = result.M44 = 0.0f;
            result.M43 = (nearPlaneDistance * farPlaneDistance) / (nearPlaneDistance - farPlaneDistance);
        }

        /// <summary>
        /// Creates a new projection <see cref="Matrix"/> for customized perspective view.
        /// </summary>
        /// <param name="left">Lower x-value at the near plane.</param>
        /// <param name="right">Upper x-value at the near plane.</param>
        /// <param name="bottom">Lower y-coordinate at the near plane.</param>
        /// <param name="top">Upper y-value at the near plane.</param>
        /// <param name="nearPlaneDistance">Distance to the near plane.</param>
        /// <param name="farPlaneDistance">Distance to the far plane.</param>
        /// <returns>The new <see cref="Matrix"/> for customized perspective view.</returns>
        public static Matrix CreatePerspectiveOffCenter(float left, float right, float bottom, float top, float nearPlaneDistance, float farPlaneDistance)
        {
            Matrix result;
            CreatePerspectiveOffCenter(left, right, bottom, top, nearPlaneDistance, farPlaneDistance, out result);
            return result;
        }

        /// <summary>
        /// Creates a new projection <see cref="Matrix"/> for customized perspective view.
        /// </summary>
        /// <param name="viewingVolume">The viewing volume.</param>
        /// <param name="nearPlaneDistance">Distance to the near plane.</param>
        /// <param name="farPlaneDistance">Distance to the far plane.</param>
        /// <returns>The new <see cref="Matrix"/> for customized perspective view.</returns>
        public static Matrix CreatePerspectiveOffCenter(Rectangle viewingVolume, float nearPlaneDistance, float farPlaneDistance)
        {
            Matrix result;
            CreatePerspectiveOffCenter(viewingVolume.Left, viewingVolume.Right, viewingVolume.Bottom, viewingVolume.Top, nearPlaneDistance, farPlaneDistance, out result);
            return result;
        }

        /// <summary>
        /// Creates a new projection <see cref="Matrix"/> for customized perspective view.
        /// </summary>
        /// <param name="left">Lower x-value at the near plane.</param>
        /// <param name="right">Upper x-value at the near plane.</param>
        /// <param name="bottom">Lower y-coordinate at the near plane.</param>
        /// <param name="top">Upper y-value at the near plane.</param>
        /// <param name="nearPlaneDistance">Distance to the near plane.</param>
        /// <param name="farPlaneDistance">Distance to the far plane.</param>
        /// <param name="result">The new <see cref="Matrix"/> for customized perspective view as an output parameter.</param>
        public static void CreatePerspectiveOffCenter(float left, float right, float bottom, float top, float nearPlaneDistance, float farPlaneDistance, out Matrix result)
        {
            if (nearPlaneDistance <= 0f)
            {
                throw new ArgumentException("nearPlaneDistance <= 0");
            }
            if (farPlaneDistance <= 0f)
            {
                throw new ArgumentException("farPlaneDistance <= 0");
            }
            if (nearPlaneDistance >= farPlaneDistance)
            {
                throw new ArgumentException("nearPlaneDistance >= farPlaneDistance");
            }
            result.M11 = (2f * nearPlaneDistance) / (right - left);
            result.M12 = result.M13 = result.M14 = 0;
            result.M22 = (2f * nearPlaneDistance) / (top - bottom);
            result.M21 = result.M23 = result.M24 = 0;
            result.M31 = (left + right) / (right - left);
            result.M32 = (top + bottom) / (top - bottom);
            result.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
            result.M34 = -1;
            result.M43 = (nearPlaneDistance * farPlaneDistance) / (nearPlaneDistance - farPlaneDistance);
            result.M41 = result.M42 = result.M44 = 0;
        }

        /// <summary>
        /// Creates a new rotation <see cref="Matrix"/> around X axis.
        /// </summary>
        /// <param name="radians">Angle in radians.</param>
        /// <returns>The rotation <see cref="Matrix"/> around X axis.</returns>
        public static Matrix CreateRotationX(float radians)
        {
            Matrix result;
            CreateRotationX(radians, out result);
            return result;
        }

        /// <summary>
        /// Creates a new rotation <see cref="Matrix"/> around X axis.
        /// </summary>
        /// <param name="radians">Angle in radians.</param>
        /// <param name="result">The rotation <see cref="Matrix"/> around X axis as an output parameter.</param>
        public static void CreateRotationX(float radians, out Matrix result)
        {
            result = Matrix.Identity;

            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);
            
            result.M22 = cos;
            result.M23 = sin;
            result.M32 = -sin;
            result.M33 = cos;
        }

        /// <summary>
        /// Creates a new rotation <see cref="Matrix"/> around Y axis.
        /// </summary>
        /// <param name="radians">Angle in radians.</param>
        /// <returns>The rotation <see cref="Matrix"/> around Y axis.</returns>
        public static Matrix CreateRotationY(float radians)
        {
            Matrix result;
            CreateRotationY(radians, out result);
            return result;
        }

        /// <summary>
        /// Creates a new rotation <see cref="Matrix"/> around Y axis.
        /// </summary>
        /// <param name="radians">Angle in radians.</param>
        /// <param name="result">The rotation <see cref="Matrix"/> around Y axis as an output parameter.</param>
        public static void CreateRotationY(float radians, out Matrix result)
        {
            result = Matrix.Identity;

            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);
            
            result.M11 = cos;
            result.M13 = -sin;
            result.M31 = sin;
            result.M33 = cos;
        }

        /// <summary>
        /// Creates a new rotation <see cref="Matrix"/> around Z axis.
        /// </summary>
        /// <param name="radians">Angle in radians.</param>
        /// <returns>The rotation <see cref="Matrix"/> around Z axis.</returns>
        public static Matrix CreateRotationZ(float radians)
        {
            Matrix result;
            CreateRotationZ(radians, out result);
            return result;
        }

        /// <summary>
        /// Creates a new rotation <see cref="Matrix"/> around Z axis.
        /// </summary>
        /// <param name="radians">Angle in radians.</param>
        /// <param name="result">The rotation <see cref="Matrix"/> around Z axis as an output parameter.</param>
        public static void CreateRotationZ(float radians, out Matrix result)
        {
            result = Matrix.Identity;

            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);
            
            result.M11 = cos;
            result.M12 = sin;
            result.M21 = -sin;
            result.M22 = cos;
        }

        /// <summary>
        /// Creates a new rotation <see cref="Matrix"/> around Z axis.
        /// </summary>
        /// <param name="rotation">A normalized Complex number.</param>
        /// <returns>The rotation <see cref="Matrix"/> around Z axis.</returns>
        public static Matrix CreateRotationZ(Complex rotation)
        {
            Matrix result = Matrix.Identity;
            result.M11 = rotation.R;
            result.M12 = rotation.i;
            result.M21 = -rotation.i;
            result.M22 = rotation.R;
            return result;
        }

        /// <summary>
        /// Creates a new scaling <see cref="Matrix"/>.
        /// </summary>
        /// <param name="scale">Scale value for all three axises.</param>
        /// <returns>The scaling <see cref="Matrix"/>.</returns>
        public static Matrix CreateScale(float scale)
        {
            Matrix result;
            CreateScale(scale, scale, scale, out result);
            return result;
        }

        /// <summary>
        /// Creates a new scaling <see cref="Matrix"/>.
        /// </summary>
        /// <param name="scale">Scale value for all three axises.</param>
        /// <param name="result">The scaling <see cref="Matrix"/> as an output parameter.</param>
        public static void CreateScale(float scale, out Matrix result)
        {
            CreateScale(scale, scale, scale, out result);
        }

        /// <summary>
        /// Creates a new scaling <see cref="Matrix"/>.
        /// </summary>
        /// <param name="xScale">Scale value for X axis.</param>
        /// <param name="yScale">Scale value for Y axis.</param>
        /// <param name="zScale">Scale value for Z axis.</param>
        /// <returns>The scaling <see cref="Matrix"/>.</returns>
        public static Matrix CreateScale(float xScale, float yScale, float zScale)
        {
            Matrix result;
            CreateScale(xScale, yScale, zScale, out result);
            return result;
        }

        /// <summary>
        /// Creates a new scaling <see cref="Matrix"/>.
        /// </summary>
        /// <param name="xScale">Scale value for X axis.</param>
        /// <param name="yScale">Scale value for Y axis.</param>
        /// <param name="zScale">Scale value for Z axis.</param>
        /// <param name="result">The scaling <see cref="Matrix"/> as an output parameter.</param>
        public static void CreateScale(float xScale, float yScale, float zScale, out Matrix result)
        {
            result.M11 = xScale;
            result.M12 = 0;
            result.M13 = 0;
            result.M14 = 0;
            result.M21 = 0;
            result.M22 = yScale;
            result.M23 = 0;
            result.M24 = 0;
            result.M31 = 0;
            result.M32 = 0;
            result.M33 = zScale;
            result.M34 = 0;
            result.M41 = 0;
            result.M42 = 0;
            result.M43 = 0;
            result.M44 = 1;
        }

        /// <summary>
        /// Creates a new scaling <see cref="Matrix"/>.
        /// </summary>
        /// <param name="scales"><see cref="Vector3"/> representing x,y and z scale values.</param>
        /// <returns>The scaling <see cref="Matrix"/>.</returns>
        public static Matrix CreateScale(Vector3 scales)
        {
            Matrix result;
            CreateScale(ref scales, out result);
            return result;
        }

        /// <summary>
        /// Creates a new scaling <see cref="Matrix"/>.
        /// </summary>
        /// <param name="scales"><see cref="Vector3"/> representing x,y and z scale values.</param>
        /// <param name="result">The scaling <see cref="Matrix"/> as an output parameter.</param>
        public static void CreateScale(ref Vector3 scales, out Matrix result)
        {
            result.M11 = scales.X;
            result.M12 = 0;
            result.M13 = 0;
            result.M14 = 0;
            result.M21 = 0;
            result.M22 = scales.Y;
            result.M23 = 0;
            result.M24 = 0;
            result.M31 = 0;
            result.M32 = 0;
            result.M33 = scales.Z;
            result.M34 = 0;
            result.M41 = 0;
            result.M42 = 0;
            result.M43 = 0;
            result.M44 = 1;
        }


        /// <summary>
        /// Creates a new <see cref="Matrix"/> that flattens geometry into a specified <see cref="Plane"/> as if casting a shadow from a specified light source. 
        /// </summary>
        /// <param name="lightDirection">A vector specifying the direction from which the light that will cast the shadow is coming.</param>
        /// <param name="plane">The plane onto which the new matrix should flatten geometry so as to cast a shadow.</param>
        /// <returns>A <see cref="Matrix"/> that can be used to flatten geometry onto the specified plane from the specified direction. </returns>
        public static Matrix CreateShadow(Vector3 lightDirection, Plane plane)
        {
            Matrix result;
            CreateShadow(ref lightDirection, ref plane, out result);
            return result;
        }


        /// <summary>
        /// Creates a new <see cref="Matrix"/> that flattens geometry into a specified <see cref="Plane"/> as if casting a shadow from a specified light source. 
        /// </summary>
        /// <param name="lightDirection">A vector specifying the direction from which the light that will cast the shadow is coming.</param>
        /// <param name="plane">The plane onto which the new matrix should flatten geometry so as to cast a shadow.</param>
        /// <param name="result">A <see cref="Matrix"/> that can be used to flatten geometry onto the specified plane from the specified direction as an output parameter.</param>
        public static void CreateShadow(ref Vector3 lightDirection, ref Plane plane, out Matrix result)
        {
            float dot = (plane.Normal.X * lightDirection.X) + (plane.Normal.Y * lightDirection.Y) + (plane.Normal.Z * lightDirection.Z);
            float x = -plane.Normal.X;
            float y = -plane.Normal.Y;
            float z = -plane.Normal.Z;
            float d = -plane.D;

            result.M11 = (x * lightDirection.X) + dot;
            result.M12 = x * lightDirection.Y;
            result.M13 = x * lightDirection.Z;
            result.M14 = 0;
            result.M21 = y * lightDirection.X;
            result.M22 = (y * lightDirection.Y) + dot;
            result.M23 = y * lightDirection.Z;
            result.M24 = 0;            
            result.M31 = z * lightDirection.X;
            result.M32 = z * lightDirection.Y;
            result.M33 = (z * lightDirection.Z) + dot;
            result.M34 = 0;            
            result.M41 = d * lightDirection.X;
            result.M42 = d * lightDirection.Y;
            result.M43 = d * lightDirection.Z;
            result.M44 = dot;
        }
        
        /// <summary>
        /// Creates a new translation <see cref="Matrix"/>.
        /// </summary>
        /// <param name="xPosition">X coordinate of translation.</param>
        /// <param name="yPosition">Y coordinate of translation.</param>
        /// <param name="zPosition">Z coordinate of translation.</param>
        /// <returns>The translation <see cref="Matrix"/>.</returns>
        public static Matrix CreateTranslation(float xPosition, float yPosition, float zPosition)
        {
            Matrix result;
            CreateTranslation(xPosition, yPosition, zPosition, out result);
            return result;
        }

        /// <summary>
        /// Creates a new translation <see cref="Matrix"/>.
        /// </summary>
        /// <param name="position">X,Y and Z coordinates of translation.</param>
        /// <param name="result">The translation <see cref="Matrix"/> as an output parameter.</param>
        public static void CreateTranslation(ref Vector3 position, out Matrix result)
        {
            result.M11 = 1;
            result.M12 = 0;
            result.M13 = 0;
            result.M14 = 0;
            result.M21 = 0;
            result.M22 = 1;
            result.M23 = 0;
            result.M24 = 0;
            result.M31 = 0;
            result.M32 = 0;
            result.M33 = 1;
            result.M34 = 0;
            result.M41 = position.X;
            result.M42 = position.Y;
            result.M43 = position.Z;
            result.M44 = 1;
        }

        /// <summary>
        /// Creates a new translation <see cref="Matrix"/>.
        /// </summary>
        /// <param name="position">X,Y and Z coordinates of translation.</param>
        /// <returns>The translation <see cref="Matrix"/>.</returns>
        public static Matrix CreateTranslation(Vector3 position)
        {
            Matrix result;
            CreateTranslation(ref position, out result);
            return result;
        }

        /// <summary>
        /// Creates a new translation <see cref="Matrix"/>.
        /// </summary>
        /// <param name="xPosition">X coordinate of translation.</param>
        /// <param name="yPosition">Y coordinate of translation.</param>
        /// <param name="zPosition">Z coordinate of translation.</param>
        /// <param name="result">The translation <see cref="Matrix"/> as an output parameter.</param>
        public static void CreateTranslation(float xPosition, float yPosition, float zPosition, out Matrix result)
        {
            result.M11 = 1;
            result.M12 = 0;
            result.M13 = 0;
            result.M14 = 0;
            result.M21 = 0;
            result.M22 = 1;
            result.M23 = 0;
            result.M24 = 0;
            result.M31 = 0;
            result.M32 = 0;
            result.M33 = 1;
            result.M34 = 0;
            result.M41 = xPosition;
            result.M42 = yPosition;
            result.M43 = zPosition;
            result.M44 = 1;
        }
        
        /// <summary>
        /// Creates a new reflection <see cref="Matrix"/>.
        /// </summary>
        /// <param name="value">The plane that used for reflection calculation.</param>
        /// <returns>The reflection <see cref="Matrix"/>.</returns>
        public static Matrix CreateReflection(Plane value)
        {
            Matrix result;
            CreateReflection(ref value, out result);
            return result;
        }

        /// <summary>
        /// Creates a new reflection <see cref="Matrix"/>.
        /// </summary>
        /// <param name="value">The plane that used for reflection calculation.</param>
        /// <param name="result">The reflection <see cref="Matrix"/> as an output parameter.</param>
        public static void CreateReflection(ref Plane value, out Matrix result)
        {
            Plane plane;
            Plane.Normalize(ref value, out plane);

            float nx = plane.Normal.X;
            float ny = plane.Normal.Y;
            float nz = plane.Normal.Z;
            float x2 = -2f * nx;
            float y2 = -2f * ny;
            float z2 = -2f * nz;

            result.M11 = x2 * nx + 1f;
            result.M12 = y2 * nx;
            result.M13 = z2 * nx;
            result.M14 = 0;
            result.M21 = x2 * ny;
            result.M22 = y2 * ny + 1;
            result.M23 = z2 * ny;
            result.M24 = 0;
            result.M31 = x2 * nz;
            result.M32 = y2 * nz;
            result.M33 = z2 * nz + 1;
            result.M34 = 0;
            result.M41 = x2 * plane.D;
            result.M42 = y2 * plane.D;
            result.M43 = z2 * plane.D;
            result.M44 = 0 + 1;
        }

        /// <summary>
        /// Creates a new world <see cref="Matrix"/>.
        /// </summary>
        /// <param name="position">The position vector.</param>
        /// <param name="forward">The forward direction vector.</param>
        /// <param name="up">The upward direction vector. Usually <see cref="Vector3.Up"/>.</param>
        /// <returns>The world <see cref="Matrix"/>.</returns>
        public static Matrix CreateWorld(Vector3 position, Vector3 forward, Vector3 up)
        {
            Matrix result;
            CreateWorld(ref position, ref forward, ref up, out result);
            return result;
        }

        /// <summary>
        /// Creates a new world <see cref="Matrix"/>.
        /// </summary>
        /// <param name="position">The position vector.</param>
        /// <param name="forward">The forward direction vector.</param>
        /// <param name="up">The upward direction vector. Usually <see cref="Vector3.Up"/>.</param>
        /// <param name="result">The world <see cref="Matrix"/> as an output parameter.</param>
        public static void CreateWorld(ref Vector3 position, ref Vector3 forward, ref Vector3 up, out Matrix result)
        {
                        Vector3 x, y, z;
                        Vector3.Normalize(ref forward, out z);
                        Vector3.Cross(ref forward, ref up, out x);
                        Vector3.Cross(ref x, ref forward, out y);
                        x.Normalize();
                        y.Normalize();            
                        
                        result = new Matrix();
                        result.Right = x;
                        result.Up = y;
                        result.Forward = z;
                        result.Translation = position;
                        result.M44 = 1f;
        }

        /// <summary>
        /// Decomposes this matrix to translation, rotation and scale elements. Returns <c>true</c> if matrix can be decomposed; <c>false</c> otherwise.
        /// </summary>
        /// <param name="scale">Scale vector as an output parameter.</param>
        /// <param name="rotation">Rotation quaternion as an output parameter.</param>
        /// <param name="translation">Translation vector as an output parameter.</param>
        /// <returns><c>true</c> if matrix can be decomposed; <c>false</c> otherwise.</returns>
        public bool Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation)
        {
            translation.X = this.M41;
            translation.Y = this.M42;
            translation.Z = this.M43;

            float xs = (Math.Sign(M11 * M12 * M13 * M14) < 0) ? -1 : 1;
            float ys = (Math.Sign(M21 * M22 * M23 * M24) < 0) ? -1 : 1;
            float zs = (Math.Sign(M31 * M32 * M33 * M34) < 0) ? -1 : 1;

            scale.X = xs * (float)Math.Sqrt(this.M11 * this.M11 + this.M12 * this.M12 + this.M13 * this.M13);
            scale.Y = ys * (float)Math.Sqrt(this.M21 * this.M21 + this.M22 * this.M22 + this.M23 * this.M23);
            scale.Z = zs * (float)Math.Sqrt(this.M31 * this.M31 + this.M32 * this.M32 + this.M33 * this.M33);

            if (scale.X == 0.0 || scale.Y == 0.0 || scale.Z == 0.0)
            {
                rotation = Quaternion.Identity;
                return false;
            }

            Matrix m1 = new Matrix(this.M11 / scale.X, M12 / scale.X, M13 / scale.X, 0,
                                   this.M21 / scale.Y, M22 / scale.Y, M23 / scale.Y, 0,
                                   this.M31 / scale.Z, M32 / scale.Z, M33 / scale.Z, 0,
                                   0, 0, 0, 1);

            rotation = Quaternion.CreateFromRotationMatrix(m1);
            return true;
        }	

        /// <summary>
        /// Returns a determinant of this <see cref="Matrix"/>.
        /// </summary>
        /// <returns>Determinant of this <see cref="Matrix"/></returns>
        /// <remarks>See more about determinant here - http://en.wikipedia.org/wiki/Determinant.
        /// </remarks>
        public float Determinant()
        {
            float tmp1 = (M33 * M44) - (M34 * M43);
            float tmp2 = (M32 * M44) - (M34 * M42);
            float tmp3 = (M32 * M43) - (M33 * M42);
            float tmp4 = (M31 * M44) - (M34 * M41);
            float tmp5 = (M31 * M43) - (M33 * M41);
            float tmp6 = (M31 * M42) - (M32 * M41);

            float det = 0f;
            det = det + M11 * ( (M22 * tmp1) - (M23 * tmp2) + (M24 * tmp3) );
            det = det - M12 * ( (M21 * tmp1) - (M23 * tmp4) + (M24 * tmp5) );
            det = det + M13 * ( (M21 * tmp2) - (M22 * tmp4) + (M24 * tmp6) );
            det = det - M14 * ( (M21 * tmp3) - (M22 * tmp5) + (M23 * tmp6) );
            return det;
        }

        /// <summary>
        /// Divides the elements of a <see cref="Matrix"/> by the elements of another matrix.
        /// </summary>
        /// <param name="left">Source <see cref="Matrix"/>.</param>
        /// <param name="right">Divisor <see cref="Matrix"/>.</param>
        /// <returns>The result of dividing the matrix.</returns>
        public static Matrix Divide(Matrix left, Matrix right)
        {
            left.M11 = left.M11 / right.M11;
            left.M12 = left.M12 / right.M12;
            left.M13 = left.M13 / right.M13;
            left.M14 = left.M14 / right.M14;
            left.M21 = left.M21 / right.M21;
            left.M22 = left.M22 / right.M22;
            left.M23 = left.M23 / right.M23;
            left.M24 = left.M24 / right.M24;
            left.M31 = left.M31 / right.M31;
            left.M32 = left.M32 / right.M32;
            left.M33 = left.M33 / right.M33;
            left.M34 = left.M34 / right.M34;
            left.M41 = left.M41 / right.M41;
            left.M42 = left.M42 / right.M42;
            left.M43 = left.M43 / right.M43;
            left.M44 = left.M44 / right.M44;
            return left;
        }

        /// <summary>
        /// Divides the elements of a <see cref="Matrix"/> by the elements of another matrix.
        /// </summary>
        /// <param name="left">Source <see cref="Matrix"/>.</param>
        /// <param name="right">Divisor <see cref="Matrix"/>.</param>
        /// <param name="result">The result of dividing the matrix as an output parameter.</param>
        public static void Divide(ref Matrix left, ref Matrix right, out Matrix result)
        {
            result.M11 = left.M11 / right.M11;
            result.M12 = left.M12 / right.M12;
            result.M13 = left.M13 / right.M13;
            result.M14 = left.M14 / right.M14;
            result.M21 = left.M21 / right.M21;
            result.M22 = left.M22 / right.M22;
            result.M23 = left.M23 / right.M23;
            result.M24 = left.M24 / right.M24;
            result.M31 = left.M31 / right.M31;
            result.M32 = left.M32 / right.M32;
            result.M33 = left.M33 / right.M33;
            result.M34 = left.M34 / right.M34;
            result.M41 = left.M41 / right.M41;
            result.M42 = left.M42 / right.M42;
            result.M43 = left.M43 / right.M43;
            result.M44 = left.M44 / right.M44;
        }

        /// <summary>
        /// Divides the elements of a <see cref="Matrix"/> by a scalar.
        /// </summary>
        /// <param name="left">Source <see cref="Matrix"/>.</param>
        /// <param name="right">Divisor scalar.</param>
        /// <returns>The result of dividing a matrix by a scalar.</returns>
        public static Matrix Divide(Matrix left, float right)
        {
            float factor = 1f / right;
            left.M11 = left.M11 * factor;
            left.M12 = left.M12 * factor;
            left.M13 = left.M13 * factor;
            left.M14 = left.M14 * factor;
            left.M21 = left.M21 * factor;
            left.M22 = left.M22 * factor;
            left.M23 = left.M23 * factor;
            left.M24 = left.M24 * factor;
            left.M31 = left.M31 * factor;
            left.M32 = left.M32 * factor;
            left.M33 = left.M33 * factor;
            left.M34 = left.M34 * factor;
            left.M41 = left.M41 * factor;
            left.M42 = left.M42 * factor;
            left.M43 = left.M43 * factor;
            left.M44 = left.M44 * factor;
            return left;
        }

        /// <summary>
        /// Divides the elements of a <see cref="Matrix"/> by a scalar.
        /// </summary>
        /// <param name="left">Source <see cref="Matrix"/>.</param>
        /// <param name="right">Divisor scalar.</param>
        /// <param name="result">The result of dividing a matrix by a scalar as an output parameter.</param>
        public static void Divide(ref Matrix left, float right, out Matrix result)
        {
            float factor = 1f / right;
            result.M11 = left.M11 * factor;
            result.M12 = left.M12 * factor;
            result.M13 = left.M13 * factor;
            result.M14 = left.M14 * factor;
            result.M21 = left.M21 * factor;
            result.M22 = left.M22 * factor;
            result.M23 = left.M23 * factor;
            result.M24 = left.M24 * factor;
            result.M31 = left.M31 * factor;
            result.M32 = left.M32 * factor;
            result.M33 = left.M33 * factor;
            result.M34 = left.M34 * factor;
            result.M41 = left.M41 * factor;
            result.M42 = left.M42 * factor;
            result.M43 = left.M43 * factor;
            result.M44 = left.M44 * factor;
        }

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Matrix"/> without any tolerance.
        /// </summary>
        /// <param name="other">The <see cref="Matrix"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public bool Equals(Matrix other)
        {
            return ((this.M11 == other.M11) && (this.M22 == other.M22) && (this.M33 == other.M33) && (this.M44 == other.M44) && (this.M12 == other.M12) && (this.M13 == other.M13) && (this.M14 == other.M14) && (this.M21 == other.M21) && (this.M23 == other.M23) && (this.M24 == other.M24) && (this.M31 == other.M31) && (this.M32 == other.M32) && (this.M34 == other.M34) && (this.M41 == other.M41) && (this.M42 == other.M42) && (this.M43 == other.M43));
        }

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Object"/> without any tolerance.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public override bool Equals(object obj)
        {
            bool flag = false;
            if (obj is Matrix)
            {
                flag = this.Equals((Matrix) obj);
            }
            return flag;
        }

        /// <summary>
        /// Gets the hash code of this <see cref="Matrix"/>.
        /// </summary>
        /// <returns>Hash code of this <see cref="Matrix"/>.</returns>
        public override int GetHashCode()
        {
            return (this.M11.GetHashCode() + this.M12.GetHashCode() + this.M13.GetHashCode() + this.M14.GetHashCode() + this.M21.GetHashCode() + this.M22.GetHashCode() + this.M23.GetHashCode() + this.M24.GetHashCode() + this.M31.GetHashCode() + this.M32.GetHashCode() + this.M33.GetHashCode() + this.M34.GetHashCode() + this.M41.GetHashCode() + this.M42.GetHashCode() + this.M43.GetHashCode() + this.M44.GetHashCode());
        }

        /// <summary>
        /// Creates a new <see cref="Matrix"/> which contains inversion of the specified matrix. 
        /// </summary>
        /// <param name="value">Source <see cref="Matrix"/>.</param>
        /// <returns>The inverted matrix.</returns>
        public static Matrix Invert(Matrix value)
        {
            Matrix result;
            Invert(ref value, out result);
            return result;
        }

        /// <summary>
        /// Creates a new <see cref="Matrix"/> which contains inversion of the specified matrix. 
        /// </summary>
        /// <param name="value">Source <see cref="Matrix"/>.</param>
        /// <param name="result">The inverted matrix as output parameter.</param>
        public static void Invert(ref Matrix value, out Matrix result)
        {
            float tmp01 = (float)  ((double)value.M33 * (double)value.M44 - (double)value.M34 * (double)value.M43);
            float tmp02 = (float)  ((double)value.M32 * (double)value.M44 - (double)value.M34 * (double)value.M42);
            float tmp03 = (float)  ((double)value.M32 * (double)value.M43 - (double)value.M33 * (double)value.M42);
            float tmp04 = (float)  ((double)value.M31 * (double)value.M44 - (double)value.M34 * (double)value.M41);
            float tmp05 = (float)  ((double)value.M31 * (double)value.M43 - (double)value.M33 * (double)value.M41);
            float tmp06 = (float)  ((double)value.M31 * (double)value.M42 - (double)value.M32 * (double)value.M41);

            float tmp07 = (float)  ((double)value.M22 * (double)tmp01 - (double)value.M23 * (double)tmp02 + (double) value.M24 * (double)tmp03);
            float tmp08 = (float) -((double)value.M21 * (double)tmp01 - (double)value.M23 * (double)tmp04 + (double) value.M24 * (double)tmp05);
            float tmp09 = (float)  ((double)value.M21 * (double)tmp02 - (double)value.M22 * (double)tmp04 + (double) value.M24 * (double)tmp06);
            float tmp10 = (float) -((double)value.M21 * (double)tmp03 - (double)value.M22 * (double)tmp05 + (double) value.M23 * (double)tmp06);
            float tmp11 = (float)  (1.0 / ((double)value.M11 * (double)tmp07 + (double)value.M12 * (double)tmp08 + (double)value.M13 * (double)tmp09 + (double)value.M14 * (double)tmp10));

            float m11 = tmp07 * tmp11;
            float m21 = tmp08 * tmp11;
            float m31 = tmp09 * tmp11;
            float m41 = tmp10 * tmp11;

            float m12 = (float) -((double) value.M12 * (double)tmp01 - (double)value.M13 * (double)tmp02 + (double)value.M14 * (double)tmp03) * tmp11;
            float m22 = (float)  ((double) value.M11 * (double)tmp01 - (double)value.M13 * (double)tmp04 + (double)value.M14 * (double)tmp05) * tmp11;
            float m32 = (float) -((double) value.M11 * (double)tmp02 - (double)value.M12 * (double)tmp04 + (double)value.M14 * (double)tmp06) * tmp11;
            float m42 = (float)  ((double) value.M11 * (double)tmp03 - (double)value.M12 * (double)tmp05 + (double)value.M13 * (double)tmp06) * tmp11;

            float tmp12 = (float) ((double)value.M23 * (double)value.M44 - (double)value.M24 * (double)value.M43);
            float tmp13 = (float) ((double)value.M22 * (double)value.M44 - (double)value.M24 * (double)value.M42);
            float tmp14 = (float) ((double)value.M22 * (double)value.M43 - (double)value.M23 * (double)value.M42);
            float tmp15 = (float) ((double)value.M21 * (double)value.M44 - (double)value.M24 * (double)value.M41);
            float tmp16 = (float) ((double)value.M21 * (double)value.M43 - (double)value.M23 * (double)value.M41);
            float tmp17 = (float) ((double)value.M21 * (double)value.M42 - (double)value.M22 * (double)value.M41);

            float m13 = (float)  ((double)value.M12 * (double)tmp12 - (double)value.M13 * (double)tmp13 + (double)value.M14 * (double)tmp14) * tmp11;
            float m23 = (float) -((double)value.M11 * (double)tmp12 - (double)value.M13 * (double)tmp15 + (double)value.M14 * (double)tmp16) * tmp11;
            float m33 = (float)  ((double)value.M11 * (double)tmp13 - (double)value.M12 * (double)tmp15 + (double)value.M14 * (double)tmp17) * tmp11;
            float m43 = (float) -((double)value.M11 * (double)tmp14 - (double)value.M12 * (double)tmp16 + (double)value.M13 * (double)tmp17) * tmp11;

            float tmp18 = (float) ((double)value.M23 * (double)value.M34 - (double)value.M24 * (double)value.M33);
            float tmp19 = (float) ((double)value.M22 * (double)value.M34 - (double)value.M24 * (double)value.M32);
            float tmp20 = (float) ((double)value.M22 * (double)value.M33 - (double)value.M23 * (double)value.M32);
            float tmp21 = (float) ((double)value.M21 * (double)value.M34 - (double)value.M24 * (double)value.M31);
            float tmp22 = (float) ((double)value.M21 * (double)value.M33 - (double)value.M23 * (double)value.M31);
            float tmp23 = (float) ((double)value.M21 * (double)value.M32 - (double)value.M22 * (double)value.M31);

            float m14 = (float) -((double)value.M12 * (double)tmp18 - (double)value.M13 * (double)tmp19 + (double)value.M14 * (double)tmp20) * tmp11;
            float m24 = (float)  ((double)value.M11 * (double)tmp18 - (double)value.M13 * (double)tmp21 + (double)value.M14 * (double)tmp22) * tmp11;
            float m34 = (float) -((double)value.M11 * (double)tmp19 - (double)value.M12 * (double)tmp21 + (double)value.M14 * (double)tmp23) * tmp11;
            float m44 = (float)  ((double)value.M11 * (double)tmp20 - (double)value.M12 * (double)tmp22 + (double)value.M13 * (double)tmp23) * tmp11;

            result.M11 = m11;
            result.M12 = m12;
            result.M13 = m13;
            result.M14 = m14;
            result.M21 = m21;
            result.M22 = m22;
            result.M23 = m23;
            result.M24 = m24;
            result.M31 = m31;
            result.M32 = m32;
            result.M33 = m33;
            result.M34 = m34;
            result.M41 = m41;
            result.M42 = m42;
            result.M43 = m43;
            result.M44 = m44;


            /*			
            
            ///
            // Use Laplace expansion theorem to calculate the inverse of a 4x4 matrix
            // 
            // 1. Calculate the 2x2 determinants needed the 4x4 determinant based on the 2x2 determinants 
            // 3. Create the adjugate matrix, which satisfies: A * adj(A) = det(A) * I
            // 4. Divide adjugate matrix with the determinant to find the inverse
            
            float det1, det2, det3, det4, det5, det6, det7, det8, det9, det10, det11, det12;
            float detMatrix;
            FindDeterminants(ref matrix, out detMatrix, out det1, out det2, out det3, out det4, out det5, out det6, 
                             out det7, out det8, out det9, out det10, out det11, out det12);
            
            float invDetMatrix = 1f / detMatrix;
            
            Matrix ret; // Allow for matrix and result to point to the same structure
            
            ret.M11 = (matrix.M22*det12 - matrix.M23*det11 + matrix.M24*det10) * invDetMatrix;
            ret.M12 = (-matrix.M12*det12 + matrix.M13*det11 - matrix.M14*det10) * invDetMatrix;
            ret.M13 = (matrix.M42*det6 - matrix.M43*det5 + matrix.M44*det4) * invDetMatrix;
            ret.M14 = (-matrix.M32*det6 + matrix.M33*det5 - matrix.M34*det4) * invDetMatrix;
            ret.M21 = (-matrix.M21*det12 + matrix.M23*det9 - matrix.M24*det8) * invDetMatrix;
            ret.M22 = (matrix.M11*det12 - matrix.M13*det9 + matrix.M14*det8) * invDetMatrix;
            ret.M23 = (-matrix.M41*det6 + matrix.M43*det3 - matrix.M44*det2) * invDetMatrix;
            ret.M24 = (matrix.M31*det6 - matrix.M33*det3 + matrix.M34*det2) * invDetMatrix;
            ret.M31 = (matrix.M21*det11 - matrix.M22*det9 + matrix.M24*det7) * invDetMatrix;
            ret.M32 = (-matrix.M11*det11 + matrix.M12*det9 - matrix.M14*det7) * invDetMatrix;
            ret.M33 = (matrix.M41*det5 - matrix.M42*det3 + matrix.M44*det1) * invDetMatrix;
            ret.M34 = (-matrix.M31*det5 + matrix.M32*det3 - matrix.M34*det1) * invDetMatrix;
            ret.M41 = (-matrix.M21*det10 + matrix.M22*det8 - matrix.M23*det7) * invDetMatrix;
            ret.M42 = (matrix.M11*det10 - matrix.M12*det8 + matrix.M13*det7) * invDetMatrix;
            ret.M43 = (-matrix.M41*det4 + matrix.M42*det2 - matrix.M43*det1) * invDetMatrix;
            ret.M44 = (matrix.M31*det4 - matrix.M32*det2 + matrix.M33*det1) * invDetMatrix;
            
            result = ret;
            */
        }

        /// <summary>
        /// Creates a new <see cref="Matrix"/> that contains linear interpolation of the values in specified matrixes.
        /// </summary>
        /// <param name="start">The first <see cref="Matrix"/>.</param>
        /// <param name="end">The second <see cref="Vector2"/>.</param>
        /// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
        /// <returns>>The result of linear interpolation of the specified matrixes.</returns>
        public static Matrix Lerp(Matrix start, Matrix end, float amount)
        {
            start.M11 = start.M11 + ((end.M11 - start.M11) * amount);
            start.M12 = start.M12 + ((end.M12 - start.M12) * amount);
            start.M13 = start.M13 + ((end.M13 - start.M13) * amount);
            start.M14 = start.M14 + ((end.M14 - start.M14) * amount);
            start.M21 = start.M21 + ((end.M21 - start.M21) * amount);
            start.M22 = start.M22 + ((end.M22 - start.M22) * amount);
            start.M23 = start.M23 + ((end.M23 - start.M23) * amount);
            start.M24 = start.M24 + ((end.M24 - start.M24) * amount);
            start.M31 = start.M31 + ((end.M31 - start.M31) * amount);
            start.M32 = start.M32 + ((end.M32 - start.M32) * amount);
            start.M33 = start.M33 + ((end.M33 - start.M33) * amount);
            start.M34 = start.M34 + ((end.M34 - start.M34) * amount);
            start.M41 = start.M41 + ((end.M41 - start.M41) * amount);
            start.M42 = start.M42 + ((end.M42 - start.M42) * amount);
            start.M43 = start.M43 + ((end.M43 - start.M43) * amount);
            start.M44 = start.M44 + ((end.M44 - start.M44) * amount);
            return start;
        }

        /// <summary>
        /// Creates a new <see cref="Matrix"/> that contains linear interpolation of the values in specified matrixes.
        /// </summary>
        /// <param name="start">The first <see cref="Matrix"/>.</param>
        /// <param name="end">The second <see cref="Vector2"/>.</param>
        /// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
        /// <param name="result">The result of linear interpolation of the specified matrixes as an output parameter.</param>
        public static void Lerp(ref Matrix start, ref Matrix end, float amount, out Matrix result)
        {
            result.M11 = start.M11 + ((end.M11 - start.M11) * amount);
            result.M12 = start.M12 + ((end.M12 - start.M12) * amount);
            result.M13 = start.M13 + ((end.M13 - start.M13) * amount);
            result.M14 = start.M14 + ((end.M14 - start.M14) * amount);
            result.M21 = start.M21 + ((end.M21 - start.M21) * amount);
            result.M22 = start.M22 + ((end.M22 - start.M22) * amount);
            result.M23 = start.M23 + ((end.M23 - start.M23) * amount);
            result.M24 = start.M24 + ((end.M24 - start.M24) * amount);
            result.M31 = start.M31 + ((end.M31 - start.M31) * amount);
            result.M32 = start.M32 + ((end.M32 - start.M32) * amount);
            result.M33 = start.M33 + ((end.M33 - start.M33) * amount);
            result.M34 = start.M34 + ((end.M34 - start.M34) * amount);
            result.M41 = start.M41 + ((end.M41 - start.M41) * amount);
            result.M42 = start.M42 + ((end.M42 - start.M42) * amount);
            result.M43 = start.M43 + ((end.M43 - start.M43) * amount);
            result.M44 = start.M44 + ((end.M44 - start.M44) * amount);
        }

        /// <summary>
        /// Creates a new <see cref="Matrix"/> that contains a multiplication of two matrix.
        /// </summary>
        /// <param name="left">Source <see cref="Matrix"/>.</param>
        /// <param name="right">Source <see cref="Matrix"/>.</param>
        /// <returns>Result of the matrix multiplication.</returns>
        public static Matrix Multiply(Matrix left, Matrix right)
        {
            float m11 = (left.M11 * right.M11) + (left.M12 * right.M21) + (left.M13 * right.M31) + (left.M14 * right.M41);
            float m12 = (left.M11 * right.M12) + (left.M12 * right.M22) + (left.M13 * right.M32) + (left.M14 * right.M42);
            float m13 = (left.M11 * right.M13) + (left.M12 * right.M23) + (left.M13 * right.M33) + (left.M14 * right.M43);
            float m14 = (left.M11 * right.M14) + (left.M12 * right.M24) + (left.M13 * right.M34) + (left.M14 * right.M44);
            float m21 = (left.M21 * right.M11) + (left.M22 * right.M21) + (left.M23 * right.M31) + (left.M24 * right.M41);
            float m22 = (left.M21 * right.M12) + (left.M22 * right.M22) + (left.M23 * right.M32) + (left.M24 * right.M42);
            float m23 = (left.M21 * right.M13) + (left.M22 * right.M23) + (left.M23 * right.M33) + (left.M24 * right.M43);
            float m24 = (left.M21 * right.M14) + (left.M22 * right.M24) + (left.M23 * right.M34) + (left.M24 * right.M44);
            float m31 = (left.M31 * right.M11) + (left.M32 * right.M21) + (left.M33 * right.M31) + (left.M34 * right.M41);
            float m32 = (left.M31 * right.M12) + (left.M32 * right.M22) + (left.M33 * right.M32) + (left.M34 * right.M42);
            float m33 = (left.M31 * right.M13) + (left.M32 * right.M23) + (left.M33 * right.M33) + (left.M34 * right.M43);
            float m34 = (left.M31 * right.M14) + (left.M32 * right.M24) + (left.M33 * right.M34) + (left.M34 * right.M44);
            float m41 = (left.M41 * right.M11) + (left.M42 * right.M21) + (left.M43 * right.M31) + (left.M44 * right.M41);
            float m42 = (left.M41 * right.M12) + (left.M42 * right.M22) + (left.M43 * right.M32) + (left.M44 * right.M42);
            float m43 = (left.M41 * right.M13) + (left.M42 * right.M23) + (left.M43 * right.M33) + (left.M44 * right.M43);
            float m44 = (left.M41 * right.M14) + (left.M42 * right.M24) + (left.M43 * right.M34) + (left.M44 * right.M44);
            left.M11 = m11;
            left.M12 = m12;
            left.M13 = m13;
            left.M14 = m14;
            left.M21 = m21;
            left.M22 = m22;
            left.M23 = m23;
            left.M24 = m24;
            left.M31 = m31;
            left.M32 = m32;
            left.M33 = m33;
            left.M34 = m34;
            left.M41 = m41;
            left.M42 = m42;
            left.M43 = m43;
            left.M44 = m44;
            return left;
        }

        /// <summary>
        /// Creates a new <see cref="Matrix"/> that contains a multiplication of two matrix.
        /// </summary>
        /// <param name="left">Source <see cref="Matrix"/>.</param>
        /// <param name="right">Source <see cref="Matrix"/>.</param>
        /// <param name="result">Result of the matrix multiplication as an output parameter.</param>
        public static void Multiply(ref Matrix left, ref Matrix right, out Matrix result)
        {
            float m11 = (left.M11 * right.M11) + (left.M12 * right.M21) + (left.M13 * right.M31) + (left.M14 * right.M41);
            float m12 = (left.M11 * right.M12) + (left.M12 * right.M22) + (left.M13 * right.M32) + (left.M14 * right.M42);
            float m13 = (left.M11 * right.M13) + (left.M12 * right.M23) + (left.M13 * right.M33) + (left.M14 * right.M43);
            float m14 = (left.M11 * right.M14) + (left.M12 * right.M24) + (left.M13 * right.M34) + (left.M14 * right.M44);
            float m21 = (left.M21 * right.M11) + (left.M22 * right.M21) + (left.M23 * right.M31) + (left.M24 * right.M41);
            float m22 = (left.M21 * right.M12) + (left.M22 * right.M22) + (left.M23 * right.M32) + (left.M24 * right.M42);
            float m23 = (left.M21 * right.M13) + (left.M22 * right.M23) + (left.M23 * right.M33) + (left.M24 * right.M43);
            float m24 = (left.M21 * right.M14) + (left.M22 * right.M24) + (left.M23 * right.M34) + (left.M24 * right.M44);
            float m31 = (left.M31 * right.M11) + (left.M32 * right.M21) + (left.M33 * right.M31) + (left.M34 * right.M41);
            float m32 = (left.M31 * right.M12) + (left.M32 * right.M22) + (left.M33 * right.M32) + (left.M34 * right.M42);
            float m33 = (left.M31 * right.M13) + (left.M32 * right.M23) + (left.M33 * right.M33) + (left.M34 * right.M43);
            float m34 = (left.M31 * right.M14) + (left.M32 * right.M24) + (left.M33 * right.M34) + (left.M34 * right.M44);
            float m41 = (left.M41 * right.M11) + (left.M42 * right.M21) + (left.M43 * right.M31) + (left.M44 * right.M41);
            float m42 = (left.M41 * right.M12) + (left.M42 * right.M22) + (left.M43 * right.M32) + (left.M44 * right.M42);
            float m43 = (left.M41 * right.M13) + (left.M42 * right.M23) + (left.M43 * right.M33) + (left.M44 * right.M43);
            float m44 = (left.M41 * right.M14) + (left.M42 * right.M24) + (left.M43 * right.M34) + (left.M44 * right.M44);
            result.M11 = m11;
            result.M12 = m12;
            result.M13 = m13;
            result.M14 = m14;
            result.M21 = m21;
            result.M22 = m22;
            result.M23 = m23;
            result.M24 = m24;
            result.M31 = m31;
            result.M32 = m32;
            result.M33 = m33;
            result.M34 = m34;
            result.M41 = m41;
            result.M42 = m42;
            result.M43 = m43;
            result.M44 = m44;
        }

        /// <summary>
        /// Creates a new <see cref="Matrix"/> that contains a multiplication of <see cref="Matrix"/> and a scalar.
        /// </summary>
        /// <param name="left">Source <see cref="Matrix"/>.</param>
        /// <param name="right">Scalar value.</param>
        /// <returns>Result of the matrix multiplication with a scalar.</returns>
        public static Matrix Multiply(Matrix left, float right)
        {
            left.M11 *= right;
            left.M12 *= right;
            left.M13 *= right;
            left.M14 *= right;
            left.M21 *= right;
            left.M22 *= right;
            left.M23 *= right;
            left.M24 *= right;
            left.M31 *= right;
            left.M32 *= right;
            left.M33 *= right;
            left.M34 *= right;
            left.M41 *= right;
            left.M42 *= right;
            left.M43 *= right;
            left.M44 *= right;
            return left;
        }

        /// <summary>
        /// Creates a new <see cref="Matrix"/> that contains a multiplication of <see cref="Matrix"/> and a scalar.
        /// </summary>
        /// <param name="left">Source <see cref="Matrix"/>.</param>
        /// <param name="right">Scalar value.</param>
        /// <param name="result">Result of the matrix multiplication with a scalar as an output parameter.</param>
        public static void Multiply(ref Matrix left, float right, out Matrix result)
        {
            result.M11 = left.M11 * right;
            result.M12 = left.M12 * right;
            result.M13 = left.M13 * right;
            result.M14 = left.M14 * right;
            result.M21 = left.M21 * right;
            result.M22 = left.M22 * right;
            result.M23 = left.M23 * right;
            result.M24 = left.M24 * right;
            result.M31 = left.M31 * right;
            result.M32 = left.M32 * right;
            result.M33 = left.M33 * right;
            result.M34 = left.M34 * right;
            result.M41 = left.M41 * right;
            result.M42 = left.M42 * right;
            result.M43 = left.M43 * right;
            result.M44 = left.M44 * right;

        }

        /// <summary>
        /// Copy the values of specified <see cref="Matrix"/> to the float array.
        /// </summary>
        /// <param name="matrix">The source <see cref="Matrix"/>.</param>
        /// <returns>The array which matrix values will be stored.</returns>
        /// <remarks>
        /// Required for OpenGL 2.0 projection matrix stuff.
        /// </remarks>
        public static float[] ToFloatArray(Matrix matrix)
        {
            float[] matarray = {
                                    matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                                    matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                                    matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                                    matrix.M41, matrix.M42, matrix.M43, matrix.M44
                                };
            return matarray;
        }

        /// <summary>
        /// Returns a matrix with the all values negated.
        /// </summary>
        /// <param name="value">Source <see cref="Matrix"/>.</param>
        /// <returns>Result of the matrix negation.</returns>
        public static Matrix Negate(Matrix value)
        {
            value.M11 = -value.M11;
            value.M12 = -value.M12;
            value.M13 = -value.M13;
            value.M14 = -value.M14;
            value.M21 = -value.M21;
            value.M22 = -value.M22;
            value.M23 = -value.M23;
            value.M24 = -value.M24;
            value.M31 = -value.M31;
            value.M32 = -value.M32;
            value.M33 = -value.M33;
            value.M34 = -value.M34;
            value.M41 = -value.M41;
            value.M42 = -value.M42;
            value.M43 = -value.M43;
            value.M44 = -value.M44;
            return value;
        }

        /// <summary>
        /// Returns a matrix with the all values negated.
        /// </summary>
        /// <param name="value">Source <see cref="Matrix"/>.</param>
        /// <param name="result">Result of the matrix negation as an output parameter.</param>
        public static void Negate(ref Matrix value, out Matrix result)
        {
            result.M11 = -value.M11;
            result.M12 = -value.M12;
            result.M13 = -value.M13;
            result.M14 = -value.M14;
            result.M21 = -value.M21;
            result.M22 = -value.M22;
            result.M23 = -value.M23;
            result.M24 = -value.M24;
            result.M31 = -value.M31;
            result.M32 = -value.M32;
            result.M33 = -value.M33;
            result.M34 = -value.M34;
            result.M41 = -value.M41;
            result.M42 = -value.M42;
            result.M43 = -value.M43;
            result.M44 = -value.M44;
        }

        /// <summary>
        /// Adds two matrixes.
        /// </summary>
        /// <param name="left">Source <see cref="Matrix"/> on the left of the add sign.</param>
        /// <param name="right">Source <see cref="Matrix"/> on the right of the add sign.</param>
        /// <returns>Sum of the matrixes.</returns>
        public static Matrix operator +(Matrix left, Matrix right)
        {
            left.M11 = left.M11 + right.M11;
            left.M12 = left.M12 + right.M12;
            left.M13 = left.M13 + right.M13;
            left.M14 = left.M14 + right.M14;
            left.M21 = left.M21 + right.M21;
            left.M22 = left.M22 + right.M22;
            left.M23 = left.M23 + right.M23;
            left.M24 = left.M24 + right.M24;
            left.M31 = left.M31 + right.M31;
            left.M32 = left.M32 + right.M32;
            left.M33 = left.M33 + right.M33;
            left.M34 = left.M34 + right.M34;
            left.M41 = left.M41 + right.M41;
            left.M42 = left.M42 + right.M42;
            left.M43 = left.M43 + right.M43;
            left.M44 = left.M44 + right.M44;
            return left;
        }

        /// <summary>
        /// Divides the elements of a <see cref="Matrix"/> by the elements of another <see cref="Matrix"/>.
        /// </summary>
        /// <param name="left">Source <see cref="Matrix"/> on the left of the div sign.</param>
        /// <param name="right">Divisor <see cref="Matrix"/> on the right of the div sign.</param>
        /// <returns>The result of dividing the matrixes.</returns>
        public static Matrix operator /(Matrix left, Matrix right)
        {
            left.M11 = left.M11 / right.M11;
            left.M12 = left.M12 / right.M12;
            left.M13 = left.M13 / right.M13;
            left.M14 = left.M14 / right.M14;
            left.M21 = left.M21 / right.M21;
            left.M22 = left.M22 / right.M22;
            left.M23 = left.M23 / right.M23;
            left.M24 = left.M24 / right.M24;
            left.M31 = left.M31 / right.M31;
            left.M32 = left.M32 / right.M32;
            left.M33 = left.M33 / right.M33;
            left.M34 = left.M34 / right.M34;
            left.M41 = left.M41 / right.M41;
            left.M42 = left.M42 / right.M42;
            left.M43 = left.M43 / right.M43;
            left.M44 = left.M44 / right.M44;
            return left;
        }

        /// <summary>
        /// Divides the elements of a <see cref="Matrix"/> by a scalar.
        /// </summary>
        /// <param name="left">Source <see cref="Matrix"/> on the left of the div sign.</param>
        /// <param name="right">Divisor scalar on the right of the div sign.</param>
        /// <returns>The result of dividing a matrix by a scalar.</returns>
        public static Matrix operator /(Matrix left, float right)
        {
            float factor = 1f / right;
            left.M11 = left.M11 * factor;
            left.M12 = left.M12 * factor;
            left.M13 = left.M13 * factor;
            left.M14 = left.M14 * factor;
            left.M21 = left.M21 * factor;
            left.M22 = left.M22 * factor;
            left.M23 = left.M23 * factor;
            left.M24 = left.M24 * factor;
            left.M31 = left.M31 * factor;
            left.M32 = left.M32 * factor;
            left.M33 = left.M33 * factor;
            left.M34 = left.M34 * factor;
            left.M41 = left.M41 * factor;
            left.M42 = left.M42 * factor;
            left.M43 = left.M43 * factor;
            left.M44 = left.M44 * factor;
            return left;
        }

        /// <summary>
        /// Compares whether two <see cref="Matrix"/> instances are equal without any tolerance.
        /// </summary>
        /// <param name="left">Source <see cref="Matrix"/> on the left of the equal sign.</param>
        /// <param name="right">Source <see cref="Matrix"/> on the right of the equal sign.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public static bool operator ==(Matrix left, Matrix right)
        {
            return (
                left.M11 == right.M11 &&
                left.M12 == right.M12 &&
                left.M13 == right.M13 &&
                left.M14 == right.M14 &&
                left.M21 == right.M21 &&
                left.M22 == right.M22 &&
                left.M23 == right.M23 &&
                left.M24 == right.M24 &&
                left.M31 == right.M31 &&
                left.M32 == right.M32 &&
                left.M33 == right.M33 &&
                left.M34 == right.M34 &&
                left.M41 == right.M41 &&
                left.M42 == right.M42 &&
                left.M43 == right.M43 &&
                left.M44 == right.M44
                );
        }

        /// <summary>
        /// Compares whether two <see cref="Matrix"/> instances are not equal without any tolerance.
        /// </summary>
        /// <param name="left">Source <see cref="Matrix"/> on the left of the not equal sign.</param>
        /// <param name="matrix2">Source <see cref="Matrix"/> on the right of the not equal sign.</param>
        /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
        public static bool operator !=(Matrix left, Matrix matrix2)
        {
            return (
                left.M11 != matrix2.M11 ||
                left.M12 != matrix2.M12 ||
                left.M13 != matrix2.M13 ||
                left.M14 != matrix2.M14 ||
                left.M21 != matrix2.M21 ||
                left.M22 != matrix2.M22 ||
                left.M23 != matrix2.M23 ||
                left.M24 != matrix2.M24 ||
                left.M31 != matrix2.M31 ||
                left.M32 != matrix2.M32 ||
                left.M33 != matrix2.M33 ||
                left.M34 != matrix2.M34 ||
                left.M41 != matrix2.M41 ||
                left.M42 != matrix2.M42 ||
                left.M43 != matrix2.M43 ||
                left.M44 != matrix2.M44
                );
        }

        /// <summary>
        /// Multiplies two matrixes.
        /// </summary>
        /// <param name="left">Source <see cref="Matrix"/> on the left of the mul sign.</param>
        /// <param name="right">Source <see cref="Matrix"/> on the right of the mul sign.</param>
        /// <returns>Result of the matrix multiplication.</returns>
        /// <remarks>
        /// Using matrix multiplication algorithm - see http://en.wikipedia.org/wiki/Matrix_multiplication.
        /// </remarks>
        public static Matrix operator *(Matrix left, Matrix right)
        {
            float m11 = (left.M11 * right.M11) + (left.M12 * right.M21) + (left.M13 * right.M31) + (left.M14 * right.M41);
            float m12 = (left.M11 * right.M12) + (left.M12 * right.M22) + (left.M13 * right.M32) + (left.M14 * right.M42);
            float m13 = (left.M11 * right.M13) + (left.M12 * right.M23) + (left.M13 * right.M33) + (left.M14 * right.M43);
            float m14 = (left.M11 * right.M14) + (left.M12 * right.M24) + (left.M13 * right.M34) + (left.M14 * right.M44);
            float m21 = (left.M21 * right.M11) + (left.M22 * right.M21) + (left.M23 * right.M31) + (left.M24 * right.M41);
            float m22 = (left.M21 * right.M12) + (left.M22 * right.M22) + (left.M23 * right.M32) + (left.M24 * right.M42);
            float m23 = (left.M21 * right.M13) + (left.M22 * right.M23) + (left.M23 * right.M33) + (left.M24 * right.M43);
            float m24 = (left.M21 * right.M14) + (left.M22 * right.M24) + (left.M23 * right.M34) + (left.M24 * right.M44);
            float m31 = (left.M31 * right.M11) + (left.M32 * right.M21) + (left.M33 * right.M31) + (left.M34 * right.M41);
            float m32 = (left.M31 * right.M12) + (left.M32 * right.M22) + (left.M33 * right.M32) + (left.M34 * right.M42);
            float m33 = (left.M31 * right.M13) + (left.M32 * right.M23) + (left.M33 * right.M33) + (left.M34 * right.M43);
            float m34 = (left.M31 * right.M14) + (left.M32 * right.M24) + (left.M33 * right.M34) + (left.M34 * right.M44);
            float m41 = (left.M41 * right.M11) + (left.M42 * right.M21) + (left.M43 * right.M31) + (left.M44 * right.M41);
            float m42 = (left.M41 * right.M12) + (left.M42 * right.M22) + (left.M43 * right.M32) + (left.M44 * right.M42);
            float m43 = (left.M41 * right.M13) + (left.M42 * right.M23) + (left.M43 * right.M33) + (left.M44 * right.M43);
            float m44 = (left.M41 * right.M14) + (left.M42 * right.M24) + (left.M43 * right.M34) + (left.M44 * right.M44);
            left.M11 = m11;
            left.M12 = m12;
            left.M13 = m13;
            left.M14 = m14;
            left.M21 = m21;
            left.M22 = m22;
            left.M23 = m23;
            left.M24 = m24;
            left.M31 = m31;
            left.M32 = m32;
            left.M33 = m33;
            left.M34 = m34;
            left.M41 = m41;
            left.M42 = m42;
            left.M43 = m43;
            left.M44 = m44;
            return left;
        }

        /// <summary>
        /// Multiplies the elements of matrix by a scalar.
        /// </summary>
        /// <param name="left">Source <see cref="Matrix"/> on the left of the mul sign.</param>
        /// <param name="right">Scalar value on the right of the mul sign.</param>
        /// <returns>Result of the matrix multiplication with a scalar.</returns>
        public static Matrix operator *(Matrix left, float right)
        {
            left.M11 = left.M11 * right;
            left.M12 = left.M12 * right;
            left.M13 = left.M13 * right;
            left.M14 = left.M14 * right;
            left.M21 = left.M21 * right;
            left.M22 = left.M22 * right;
            left.M23 = left.M23 * right;
            left.M24 = left.M24 * right;
            left.M31 = left.M31 * right;
            left.M32 = left.M32 * right;
            left.M33 = left.M33 * right;
            left.M34 = left.M34 * right;
            left.M41 = left.M41 * right;
            left.M42 = left.M42 * right;
            left.M43 = left.M43 * right;
            left.M44 = left.M44 * right;
            return left;
        }

        /// <summary>
        /// Subtracts the values of one <see cref="Matrix"/> from another <see cref="Matrix"/>.
        /// </summary>
        /// <param name="left">Source <see cref="Matrix"/> on the left of the sub sign.</param>
        /// <param name="right">Source <see cref="Matrix"/> on the right of the sub sign.</param>
        /// <returns>Result of the matrix subtraction.</returns>
        public static Matrix operator -(Matrix left, Matrix right)
        {
            left.M11 = left.M11 - right.M11;
            left.M12 = left.M12 - right.M12;
            left.M13 = left.M13 - right.M13;
            left.M14 = left.M14 - right.M14;
            left.M21 = left.M21 - right.M21;
            left.M22 = left.M22 - right.M22;
            left.M23 = left.M23 - right.M23;
            left.M24 = left.M24 - right.M24;
            left.M31 = left.M31 - right.M31;
            left.M32 = left.M32 - right.M32;
            left.M33 = left.M33 - right.M33;
            left.M34 = left.M34 - right.M34;
            left.M41 = left.M41 - right.M41;
            left.M42 = left.M42 - right.M42;
            left.M43 = left.M43 - right.M43;
            left.M44 = left.M44 - right.M44;
            return left;
        }

        /// <summary>
        /// Inverts values in the specified <see cref="Matrix"/>.
        /// </summary>
        /// <param name="value">Source <see cref="Matrix"/> on the right of the sub sign.</param>
        /// <returns>Result of the inversion.</returns>
        public static Matrix operator -(Matrix value)
        {
            value.M11 = -value.M11;
            value.M12 = -value.M12;
            value.M13 = -value.M13;
            value.M14 = -value.M14;
            value.M21 = -value.M21;
            value.M22 = -value.M22;
            value.M23 = -value.M23;
            value.M24 = -value.M24;
            value.M31 = -value.M31;
            value.M32 = -value.M32;
            value.M33 = -value.M33;
            value.M34 = -value.M34;
            value.M41 = -value.M41;
            value.M42 = -value.M42;
            value.M43 = -value.M43;
            value.M44 = -value.M44;
            return value;
        }

#if NET8_0_OR_GREATER
        public static explicit operator Matrix(SysNumerics.Matrix4x4 value)
        {
            return Unsafe.BitCast<SysNumerics.Matrix4x4,Matrix>(value);
        }

        public static explicit operator SysNumerics.Matrix4x4(Matrix value)
        {
            return Unsafe.BitCast<Matrix, SysNumerics.Matrix4x4>(value);
        }
#endif

        /// <summary>
        /// Creates a new <see cref="Matrix"/> that contains subtraction of one matrix from another.
        /// </summary>
        /// <param name="left">The first <see cref="Matrix"/>.</param>
        /// <param name="right">The second <see cref="Matrix"/>.</param>
        /// <returns>The result of the matrix subtraction.</returns>
        public static Matrix Subtract(Matrix left, Matrix right)
        {
            left.M11 = left.M11 - right.M11;
            left.M12 = left.M12 - right.M12;
            left.M13 = left.M13 - right.M13;
            left.M14 = left.M14 - right.M14;
            left.M21 = left.M21 - right.M21;
            left.M22 = left.M22 - right.M22;
            left.M23 = left.M23 - right.M23;
            left.M24 = left.M24 - right.M24;
            left.M31 = left.M31 - right.M31;
            left.M32 = left.M32 - right.M32;
            left.M33 = left.M33 - right.M33;
            left.M34 = left.M34 - right.M34;
            left.M41 = left.M41 - right.M41;
            left.M42 = left.M42 - right.M42;
            left.M43 = left.M43 - right.M43;
            left.M44 = left.M44 - right.M44;
            return left;
        }

        /// <summary>
        /// Creates a new <see cref="Matrix"/> that contains subtraction of one matrix from another.
        /// </summary>
        /// <param name="left">The first <see cref="Matrix"/>.</param>
        /// <param name="right">The second <see cref="Matrix"/>.</param>
        /// <param name="result">The result of the matrix subtraction as an output parameter.</param>
        public static void Subtract(ref Matrix left, ref Matrix right, out Matrix result)
        {
            result.M11 = left.M11 - right.M11;
            result.M12 = left.M12 - right.M12;
            result.M13 = left.M13 - right.M13;
            result.M14 = left.M14 - right.M14;
            result.M21 = left.M21 - right.M21;
            result.M22 = left.M22 - right.M22;
            result.M23 = left.M23 - right.M23;
            result.M24 = left.M24 - right.M24;
            result.M31 = left.M31 - right.M31;
            result.M32 = left.M32 - right.M32;
            result.M33 = left.M33 - right.M33;
            result.M34 = left.M34 - right.M34;
            result.M41 = left.M41 - right.M41;
            result.M42 = left.M42 - right.M42;
            result.M43 = left.M43 - right.M43;
            result.M44 = left.M44 - right.M44;
        }

        internal string DebugDisplayString
        {
            get
            {
                if (this == Identity)
                {
                    return "Identity";
                }

                return string.Concat(
                     "( ", this.M11.ToString(), "  ", this.M12.ToString(), "  ", this.M13.ToString(), "  ", this.M14.ToString(), " )  \r\n",
                     "( ", this.M21.ToString(), "  ", this.M22.ToString(), "  ", this.M23.ToString(), "  ", this.M24.ToString(), " )  \r\n",
                     "( ", this.M31.ToString(), "  ", this.M32.ToString(), "  ", this.M33.ToString(), "  ", this.M34.ToString(), " )  \r\n",
                     "( ", this.M41.ToString(), "  ", this.M42.ToString(), "  ", this.M43.ToString(), "  ", this.M44.ToString(), " )");
            }
        }

        /// <summary>
        /// Returns a <see cref="String"/> representation of this <see cref="Matrix"/> in the format:
        /// {M11:[<see cref="M11"/>] M12:[<see cref="M12"/>] M13:[<see cref="M13"/>] M14:[<see cref="M14"/>]}
        /// {M21:[<see cref="M21"/>] M12:[<see cref="M22"/>] M13:[<see cref="M23"/>] M14:[<see cref="M24"/>]}
        /// {M31:[<see cref="M31"/>] M32:[<see cref="M32"/>] M33:[<see cref="M33"/>] M34:[<see cref="M34"/>]}
        /// {M41:[<see cref="M41"/>] M42:[<see cref="M42"/>] M43:[<see cref="M43"/>] M44:[<see cref="M44"/>]}
        /// </summary>
        /// <returns>A <see cref="String"/> representation of this <see cref="Matrix"/>.</returns>
        public override string ToString()
        {
            return "{M11:" + M11 + " M12:" + M12 + " M13:" + M13 + " M14:" + M14 + "}"
                + " {M21:" + M21 + " M22:" + M22 + " M23:" + M23 + " M24:" + M24 + "}"
                + " {M31:" + M31 + " M32:" + M32 + " M33:" + M33 + " M34:" + M34 + "}"
                + " {M41:" + M41 + " M42:" + M42 + " M43:" + M43 + " M44:" + M44 + "}";
        }

        /// <summary>
        /// Swap the matrix rows and columns.
        /// </summary>
        /// <param name="value">The matrix for transposing operation.</param>
        /// <returns>The new <see cref="Matrix"/> which contains the transposing result.</returns>
        public static Matrix Transpose(Matrix value)
        {
            Matrix result;
            Transpose(ref value, out result);
            return result;
        }

        /// <summary>
        /// Swap the matrix rows and columns.
        /// </summary>
        /// <param name="value">The matrix for transposing operation.</param>
        /// <param name="result">The new <see cref="Matrix"/> which contains the transposing result as an output parameter.</param>
        public static void Transpose(ref Matrix value, out Matrix result)
        {
            Matrix ret;
            
            ret.M11 = value.M11;
            ret.M12 = value.M21;
            ret.M13 = value.M31;
            ret.M14 = value.M41;

            ret.M21 = value.M12;
            ret.M22 = value.M22;
            ret.M23 = value.M32;
            ret.M24 = value.M42;

            ret.M31 = value.M13;
            ret.M32 = value.M23;
            ret.M33 = value.M33;
            ret.M34 = value.M43;

            ret.M41 = value.M14;
            ret.M42 = value.M24;
            ret.M43 = value.M34;
            ret.M44 = value.M44;
            
            result = ret;
        }
        #endregion

        #region Private Static Methods

        /// <summary>
        /// Helper method for using the Laplace expansion theorem using two rows expansions to calculate major and 
        /// minor determinants of a 4x4 matrix. This method is used for inverting a matrix.
        /// </summary>
        private static void FindDeterminants(ref Matrix value, out float major, 
                                             out float minor1, out float minor2, out float minor3, out float minor4, out float minor5, out float minor6,
                                             out float minor7, out float minor8, out float minor9, out float minor10, out float minor11, out float minor12)
        {
                double det1 = (double)value.M11 * (double)value.M22 - (double)value.M12 * (double)value.M21;
                double det2 = (double)value.M11 * (double)value.M23 - (double)value.M13 * (double)value.M21;
                double det3 = (double)value.M11 * (double)value.M24 - (double)value.M14 * (double)value.M21;
                double det4 = (double)value.M12 * (double)value.M23 - (double)value.M13 * (double)value.M22;
                double det5 = (double)value.M12 * (double)value.M24 - (double)value.M14 * (double)value.M22;
                double det6 = (double)value.M13 * (double)value.M24 - (double)value.M14 * (double)value.M23;
                double det7 = (double)value.M31 * (double)value.M42 - (double)value.M32 * (double)value.M41;
                double det8 = (double)value.M31 * (double)value.M43 - (double)value.M33 * (double)value.M41;
                double det9 = (double)value.M31 * (double)value.M44 - (double)value.M34 * (double)value.M41;
                double det10 = (double)value.M32 * (double)value.M43 - (double)value.M33 * (double)value.M42;
                double det11 = (double)value.M32 * (double)value.M44 - (double)value.M34 * (double)value.M42;
                double det12 = (double)value.M33 * (double)value.M44 - (double)value.M34 * (double)value.M43;
                
                major = (float)(det1*det12 - det2*det11 + det3*det10 + det4*det9 - det5*det8 + det6*det7);
                minor1 = (float)det1;
                minor2 = (float)det2;
                minor3 = (float)det3;
                minor4 = (float)det4;
                minor5 = (float)det5;
                minor6 = (float)det6;
                minor7 = (float)det7;
                minor8 = (float)det8;
                minor9 = (float)det9;
                minor10 = (float)det10;
                minor11 = (float)det11;
                minor12 = (float)det12;
        }
        
        #endregion
    }
}
