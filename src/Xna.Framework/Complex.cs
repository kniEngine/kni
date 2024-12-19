// Copyright (C)2022 Nick Kastellanos

using System;
using System.Diagnostics;
using System.Runtime.Serialization;

#if NET8_0_OR_GREATER
using SysNumerics = System.Numerics;
#endif


namespace Microsoft.Xna.Framework
{
    [DataContract]
    [DebuggerDisplay("{DebugDisplayString,nq}")]
    public struct Complex : IEquatable<Complex>
    {
        #region Private Fields

        private static readonly Complex _zero = new Complex(0, 0);
        private static readonly Complex _one = new Complex(1, 0);
        private static readonly Complex _imaginaryOne = new Complex(0, 1);

        #endregion

        #region Public Fields

        /// <summary>
        /// The real part of this <see cref="Complex"/> number.
        /// </summary>
        [DataMember]
        public float R;

        /// <summary>
        /// The imaginary part of this <see cref="Complex"/> number.
        /// </summary>
        [DataMember]
        public float i;

        #endregion

        #region Properties

        /// <summary>
        /// Returns a <see cref="Complex"/> number with components (0, 0i).
        /// </summary>
        public static Complex Zero { get { return _zero; } }

        /// <summary>
        /// Returns a <see cref="Complex"/> number with components (1, 0i).
        /// </summary>
        public static Complex One { get { return _one; } }

        /// <summary>
        /// Returns a <see cref="Complex"/> number with components (0, 1i).
        /// </summary>
        public static Complex ImaginaryOne { get { return _imaginaryOne; } }

        /// <summary>
        /// Returns a <see cref="Complex"/> number with components (1, 0i).
        /// </summary>
        public static Complex Identity { get { return _one; } }

        public float Phase
        {
            get { return (float)Math.Atan2(i, R); }
        }

        public float Magnitude
        {
            get { return (float)Math.Sqrt((R * R) + (i * i)); }
        }

        internal string DebugDisplayString
        {
            get
            {
                if (this == Complex.Identity)
                    return "Identity";

                return String.Format("{{R: {0} i: {1} Magnitude: {2} Phase: {3}}}",
                    R, i, Magnitude, Phase);
            }
        }

        #endregion

        /// <summary>
        /// Constructs a complex number with real and imaginary components.
        /// </summary>
        /// <param name="real">The real part of this <see cref="Complex"/> number.</param>
        /// <param name="imaginary"> The imaginary part of this <see cref="Complex"/> number.</param>
        public Complex(float real, float imaginary)
        {
            R = real;
            i = imaginary;
        }

        /// <summary>
        /// Constructs a complex number from <see cref="Vector2"/>.
        /// </summary>
        /// <param name="value">The R,i components.</param>
        public Complex(Vector2 value)
        {
            R = value.X;
            i = value.Y;
        }

        /// <summary>
        /// Creates a new <see cref="Complex"/> number from the specified angle.
        /// </summary>
        /// /// <param name="phase">The angle in radians.</param>
        /// /// <param name="magnitude">The magnitude.</param>
        public static Complex CreateFromPolarCoordinates(float phase, float magnitude)
        {
            Complex result;
            result.R = (float)Math.Cos(phase) * magnitude;
            result.i = (float)Math.Sin(phase) * magnitude;
            return result;
        }

        /// <summary>
        /// Creates a new <see cref="Complex"/> number from the specified angle.
        /// </summary>
        /// /// <param name="angle">The angle in radians.</param>
        public static Complex CreateFromAngle(float angle)
        {
            Complex result;
            result.R = (float)Math.Cos(angle);
            result.i = (float)Math.Sin(angle);
            return result;
        }

        /// <summary>
        /// Transforms this complex number into its conjugated version.
        /// </summary>
        public void Conjugate()
        {
            i = -i;
        }

        /// <summary>
        /// Flips the sign of the all the complex number components.
        /// </summary>
        public void Negate()
        {
            R = -R;
            i = -i;
        }

        /// <summary>
        /// Returns the squared magnitude of the complex number.
        /// </summary>
        /// <returns>The squared magnitude of the complex number components.</returns>
        public float MagnitudeSquared()
        {
            return (R * R) + (i * i);
        }

        /// <summary>
        /// Scales the complex number magnitude to unit length.
        /// </summary>
        public void Normalize()
        {
            float mag = Magnitude;
            R = R / mag;
            i = i / mag;
        }

        /// <summary>
        /// Gets a <see cref="Vector2"/> representation for this complex number.
        /// </summary>
        /// <returns>A <see cref="Vector2"/> representation for this complex number.</returns>
        public Vector2 ToVector2()
        {
            return new Vector2(R, i);
        }

        internal Complex Rotate90()
        {
            return new Complex(-i, +R);
        }

        internal Complex Rotate180()
        {
            return new Complex(-R, -i);
        }

        internal Complex Rotate270()
        {
            return new Complex(+i, -R);
        }

        /// <summary>
        /// Creates a new <see cref="Complex"/> number that contains a multiplication of two complex numbers.
        /// </summary>
        /// <param name="left">Source <see cref="Complex"/> number.</param>
        /// <param name="right">Source <see cref="Complex"/> number.</param>
        /// <returns>The result of the complex number multiplication.</returns>
        public static Complex Multiply(Complex left, Complex right)
        {
            return new Complex(left.R * right.R - left.i * right.i,
                               left.i * right.R + left.R * right.i);
        }

        /// <summary>
        /// Creates a new <see cref="Complex"/> number that contains a multiplication of two complex numbers.
        /// </summary>
        /// <param name="left">Source <see cref="Complex"/> number.</param>
        /// <param name="right">Source <see cref="Complex"/> number.</param>
        /// <param name="result">The result of the complex number multiplication as an output parameter.</param>
        public static void Multiply(ref Complex left, ref Complex right, out Complex result)
        {
            result = new Complex(left.R * right.R - left.i * right.i,
                                 left.i * right.R + left.R * right.i);
        }

        /// <summary>
        /// Creates a new <see cref="Complex"/> that contains a multiplication of <see cref="Complex"/> and a scalar.
        /// </summary>
        /// <param name="left">Source <see cref="Complex"/>.</param>
        /// <param name="right">Scalar value.</param>
        /// <returns>The result of the Complex multiplication with a scalar.</returns>
        public static Complex Multiply(Complex left, float right)
        {
            left.R *= right;
            left.i *= right;
            return left;
        }

        /// <summary>
        /// Creates a new <see cref="Complex"/> that contains a multiplication of <see cref="Complex"/> and a scalar.
        /// </summary>
        /// <param name="left">Source <see cref="Complex"/>.</param>
        /// <param name="right">Scalar value.</param>
        /// <param name="result">The result of the multiplication with a scalar as an output parameter.</param>
        public static void Multiply(ref Complex left, float right, out Complex result)
        {
            result.R = left.R * right;
            result.i = left.i * right;
        }

        public static Complex Divide(Complex left, Complex right)
        {
            return new Complex(right.R * left.R + right.i * left.i,
                               right.R * left.i - right.i * left.R);
        }

        public static void Divide(ref Complex left, ref Complex right, out Complex result)
        {
            result = new Complex(right.R * left.R + right.i * left.i,
                                 right.R * left.i - right.i * left.R);
        }

        /// <summary>
        /// Divides the components of a <see cref="Complex"/> by a scalar.
        /// </summary>
        /// <param name="left">Source <see cref="Complex"/>.</param>
        /// <param name="right">Divisor scalar.</param>
        /// <returns>The result of dividing a Complex by a scalar.</returns>
        public static Complex Divide(Complex left, float right)
        {
            left.R /= right;
            left.i /= right;
            return left;
        }

        /// <summary>
        /// Divides the components of a <see cref="Complex"/> by a scalar.
        /// </summary>
        /// <param name="left">Source <see cref="Complex"/>.</param>
        /// <param name="right">Divisor scalar.</param>
        /// <param name="result">The result of dividing a Complex by a scalar as an output parameter.</param>
        public static void Divide(ref Complex left, float right, out Complex result)
        {
            result.R = left.R / right;
            result.i = left.i / right;
        }

        /// <summary>
        /// Transforms this complex number into its conjugated version.
        /// </summary>
        /// <param name="value">The complex number which values will be used to create the conjugated version.</param>
        /// <returns>The conjugate version of the specified complex number.</returns>
        public static Complex Conjugate(Complex value)
        {
            return new Complex(value.R, -value.i);
        }

        /// <summary>
        /// Transforms this complex number into its conjugated version.
        /// </summary>
        /// <param name="value">The complex number which values will be used to create the conjugated version.</param>
        /// <param name="result">The conjugated version of the specified complex number as an output parameter.</param>
        public static void Conjugate(ref Complex value, out Complex result)
        {
            result = new Complex(value.R, -value.i);
        }

        /// <summary>
        /// Flips the sign of the all the complex number components.
        /// </summary>
        /// <param name="value">Source <see cref="Complex"/>.</param>
        /// <returns>The result of the complex number negation.</returns>
        public static Complex Negate(Complex value)
        {
            return new Complex(-value.R, -value.i);
        }

        /// <summary>
        /// Flips the sign of the all the complex number components.
        /// </summary>
        /// <param name="value">Source <see cref="Complex"/>.</param>
        /// <param name="result">The result of the complex number negation as an output parameter.</param>
        public static void Negate(ref Complex value, out Complex result)
        {
            result = new Complex(-value.R, -value.i);
        }

        /// <summary>
        /// Scales the complex number magnitude to unit length.
        /// </summary>
        /// <param name="value">Source <see cref="Complex"/>.</param>
        /// <returns>The unit length complex number.</returns>
        public static Complex Normalize(Complex value)
        {
            float mag = value.Magnitude;
            return new Complex(value.R / mag, -value.i / mag);
        }

        /// <summary>
        /// Scales the complex number magnitude to unit length.
        /// </summary>
        /// <param name="value">Source <see cref="Complex"/>.</param>
        /// <param name="result">The unit length complex number as an output parameter.</param>
        public static void Normalize(ref Complex value, out Complex result)
        {
            float mag = value.Magnitude;
            result = new Complex(value.R / mag, -value.i / mag);
        }

        /// <summary>
        /// Deconstruction method for <see cref="Complex"/>.
        /// </summary>
        /// <param name="real">The real part of this <see cref="Complex" /> number.</param>
        /// <param name="imaginary">The imaginary part of this <see cref="Complex"/> number.</param>
        public void Deconstruct(out float real, out float imaginary)
        {
            real = this.R;
            imaginary = this.i;
        }


        #region Operators

        /// <summary>
        /// Flips the sign of the all the complex number components.
        /// </summary>
        /// <param name="value">Source <see cref="Complex"/> on the right of the sub sign.</param>
        /// <returns>The result of the complex number negation.</returns>
        public static Complex operator -(Complex value)
        {
            return new Complex(-value.R, -value.i);
        }

        /// <summary>
        /// Multiplies two complex numbers.
        /// </summary>
        /// <param name="left">Source <see cref="Complex"/> number on the left of the mul sign.</param>
        /// <param name="right">Source <see cref="Complex"/> number on the right of the mul sign.</param>
        /// <returns>Result of the complex numbers multiplication.</returns>
        public static Complex operator *(Complex left, Complex right)
        {
            return new Complex(left.R * right.R - left.i * right.i,
                               left.i * right.R + left.R * right.i);
        }

        /// <summary>
        /// Multiplies the components of a <see cref="Complex"/> number by a scalar.
        /// </summary>
        /// <param name="left">Source <see cref="Complex"/> on the left of the mul sign.</param>
        /// <param name="right">Scalar value on the right of the mul sign.</param>
        /// <returns>Result of the complex number multiplication with a scalar.</returns>
        public static Complex operator *(Complex left, float right)
        {
            left.R *= right;
            left.i *= right;
            return left;
        }

        /// <summary>
        /// Divides a <see cref="Complex"/> number by the other <see cref="Complex"/> number.
        /// </summary>
        /// <param name="left">Source <see cref="Complex"/> number on the left of the div sign.</param>
        /// <param name="right">Divisor <see cref="Complex"/> number on the right of the div sign.</param>
        /// <returns>The result of dividing the complex numbers.</returns>
        public static Complex operator /(Complex left, Complex right)
        {
            return new Complex(right.R * left.R + right.i * left.i,
                               right.R * left.i - right.i * left.R);
        }

        /// <summary>
        /// Divides the components of a <see cref="Complex"/> number by a scalar.
        /// </summary>
        /// <param name="left">Source <see cref="Complex"/> on the left of the div sign.</param>
        /// <param name="right">Divisor scalar on the right of the div sign.</param>
        /// <returns>The result of dividing a complex number by a scalar.</returns>
        public static Complex operator /(Complex left, float right)
        {
            left.R /= right;
            left.i /= right;
            return left;
        }


        /// <summary>
        /// Compares whether two <see cref="Complex"/> instances are equal.
        /// </summary>
        /// <param name="left"><see cref="Complex"/> instance on the left of the equal sign.</param>
        /// <param name="right"><see cref="Complex"/> instance on the right of the equal sign.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public static bool operator ==(Complex left, Complex right)
        {
            return left.R == right.R && left.i == right.i;
        }

        /// <summary>
        /// Compares whether two <see cref="Complex"/> instances are not equal.
        /// </summary>
        /// <param name="left"><see cref="Complex"/> instance on the left of the not equal sign.</param>
        /// <param name="right"><see cref="Complex"/> instance on the right of the not equal sign.</param>
        /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
        public static bool operator !=(Complex left, Complex right)
        {
            return left.R != right.R || left.i != right.i;
        }

#if NET8_0_OR_GREATER
        public static explicit operator Complex(SysNumerics.Complex value)
        {
            Complex result;
            result.R = (float)value.Real;
            result.i = (float)value.Imaginary;
            return result;
        }

        public static explicit operator SysNumerics.Complex(Complex value)
        {
            return new SysNumerics.Complex(
                (double)value.R,
                (double)value.i);
        }
#endif

        #endregion


        #region IEquatable<Complex>

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj is Complex)
                return Equals((Complex)obj);
            return false;
        }

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Complex"/>.
        /// </summary>
        /// <param name="other">The <see cref="Complex"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public bool Equals(Complex other)
        {
            return R == other.R &&
                   i == other.i;
        }

        #endregion


        /// <summary>
        /// Gets the hash code of this <see cref="Complex"/>.
        /// </summary>
        /// <returns>Hash code of this <see cref="Complex"/>.</returns>
        public override int GetHashCode()
        {
            return R.GetHashCode() + i.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return String.Format("{{R: {0}, i: {1} }}", R, i);
        }

    }
}
