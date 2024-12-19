// Copyright (C)2024 Nick Kastellanos

using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework
{
    [DataContract]
    [DebuggerDisplay("{DebugDisplayString,nq}")]
    public struct Pose2 : IEquatable<Pose2>
    {
        #region Private Fields

        private static readonly Pose2 _identity = new Pose2(Complex.Identity, Vector2.Zero);


        #endregion

        #region Public Fields

        /// <summary>
        /// The orientation of this <see cref="Pose2"/>.
        /// </summary>
        [DataMember]
        public Complex Orientation;

        /// <summary>
        /// The translation part of this <see cref="Pose2"/>.
        /// </summary>
        [DataMember]
        public Vector2 Translation;

        #endregion

        #region Properties

        /// <summary>
        /// Returns the identity <see cref="Pose2"/> .
        /// </summary>
        public static Pose2 Identity { get { return _identity; } }

        internal string DebugDisplayString
        {
            get
            {
                if (this == Pose2.Identity)
                    return "Identity";

                return String.Format("{{Orientation: {0} Translation: {1} }}",
                    Orientation, Translation);
            }
        }

        #endregion

        /// <summary>
        /// Constructs a 2D Pose with orientation and translation.
        /// </summary>
        /// <param name="orientation">The orientation of this <see cref="Pose2"/>.</param>
        /// <param name="translation"> The translation of this <see cref="Pose2"/>.</param>
        public Pose2(Complex orientation, Vector2 translation)
        {
            this.Orientation = orientation;
            this.Translation = translation;
        }

        /// <summary>
        /// Returns the inverse pose, which represents the opposite transformation.
        /// </summary>
        /// <param name="value">Source <see cref="Pose2"/>.</param>
        /// <returns>The inverse pose.</returns>
        public static Pose2 Inverse(Pose2 value)
        {
            Pose2 result;
            result.Orientation = Complex.Conjugate(value.Orientation);
            result.Translation = Vector2.Transform(-value.Translation, result.Orientation);
            return result;
        }

        /// <summary>
        /// Creates a new <see cref="Pose2"/> that contains a multiplication of two poses.
        /// </summary>
        /// <param name="left">Source <see cref="Pose2"/>.</param>
        /// <param name="right">Source <see cref="Pose2"/>.</param>
        /// <returns>The result of the pose multiplication.</returns>
        public static Pose2 Multiply(Pose2 left, Pose2 right)
        {
            Pose2 result;
            result.Orientation = Complex.Multiply(left.Orientation, right.Orientation);
            result.Translation = Vector2.Transform(left.Translation, right);
            return result;
        }

        /// <summary>
        /// Deconstruction method for <see cref="Pose2"/>.
        /// </summary>
        /// <param name="orientation">The orientation of this <see cref="Pose2" />.</param>
        /// <param name="translation">The translation of this <see cref="Pose2"/>.</param>
        public void Deconstruct(out Complex orientation, out Vector2 translation)
        {
            orientation = this.Orientation;
            translation = this.Translation;
        }

        #region Operators

        /// <summary>
        /// Multiplies two poses.
        /// </summary>
        /// <param name="left">Source <see cref="Pose2"/> on the left of the mul sign.</param>
        /// <param name="right">Source <see cref="Pose2"/> on the right of the mul sign.</param>
        /// <returns>Result of the poses multiplication.</returns>
        public static Pose2 operator *(Pose2 left, Pose2 right)
        {
            Pose2 result;
            result = Pose2.Multiply(left, right);
            return result;
        }

        /// <summary>
        /// Compares whether two <see cref="Pose2"/> instances are equal.
        /// </summary>
        /// <param name="left"><see cref="Pose2"/> instance on the left of the equal sign.</param>
        /// <param name="right"><see cref="Pose2"/> instance on the right of the equal sign.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public static bool operator ==(Pose2 left, Pose2 right)
        {
            return left.Orientation == right.Orientation && left.Translation == right.Translation;
        }

        /// <summary>
        /// Compares whether two <see cref="Pose2"/> instances are not equal.
        /// </summary>
        /// <param name="left"><see cref="Pose2"/> instance on the left of the not equal sign.</param>
        /// <param name="right"><see cref="Pose2"/> instance on the right of the not equal sign.</param>
        /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
        public static bool operator !=(Pose2 left, Pose2 right)
        {
            return left.Orientation != right.Orientation || left.Translation != right.Translation;
        }

        #endregion


        #region IEquatable<Pose2>

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj is Pose2)
                return Equals((Pose2)obj);
            return false;
        }

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Pose2"/>.
        /// </summary>
        /// <param name="other">The <see cref="Pose2"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public bool Equals(Pose2 other)
        {
            return Orientation == other.Orientation
                && Translation == other.Translation;
        }

        #endregion


        /// <summary>
        /// Gets the hash code of this <see cref="Pose2"/>.
        /// </summary>
        /// <returns>Hash code of this <see cref="Pose2"/>.</returns>
        public override int GetHashCode()
        {
            return Orientation.GetHashCode() + Translation.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return String.Format("{{Orientation: {0}, Translation: {1} }}", Orientation, Translation);
        }

    }
}
