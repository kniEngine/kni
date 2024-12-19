// Copyright (C)2024 Nick Kastellanos

using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework
{
    [DataContract]
    [DebuggerDisplay("{DebugDisplayString,nq}")]
    public struct Pose3 : IEquatable<Pose3>
    {
        #region Private Fields

        private static readonly Pose3 _identity = new Pose3(Quaternion.Identity, Vector3.Zero);


        #endregion

        #region Public Fields

        /// <summary>
        /// The orientation of this <see cref="Pose3"/>.
        /// </summary>
        [DataMember]
        public Quaternion Orientation;

        /// <summary>
        /// The translation part of this <see cref="Pose3"/>.
        /// </summary>
        [DataMember]
        public Vector3 Translation;
        private float _padw;

        #endregion

        #region Properties

        /// <summary>
        /// Returns the identity <see cref="Pose3"/> .
        /// </summary>
        public static Pose3 Identity { get { return _identity; } }

        internal string DebugDisplayString
        {
            get
            {
                if (this == Pose3.Identity)
                    return "Identity";

                return String.Format("{{Orientation: {0} Translation: {1} }}",
                    Orientation, Translation);
            }
        }

        #endregion

        /// <summary>
        /// Constructs a 3D Pose with orientation and translation.
        /// </summary>
        /// <param name="orientation">The orientation of this <see cref="Pose3"/>.</param>
        /// <param name="translation"> The translation of this <see cref="Pose3"/>.</param>
        public Pose3(Quaternion orientation, Vector3 translation)
        {
            this.Orientation = orientation;
            this.Translation = translation;
            this._padw = default;
        }

        /// <summary>
        /// Returns the inverse pose, which represents the opposite transformation.
        /// </summary>
        /// <param name="value">Source <see cref="Pose3"/>.</param>
        /// <returns>The inverse pose.</returns>
        public static Pose3 Inverse(Pose3 value)
        {
            Pose3 result;
            result.Orientation = Quaternion.Inverse(value.Orientation);
            result.Translation = Vector3.Transform(-value.Translation, result.Orientation);
            result._padw = default;
            return result;
        }

        /// <summary>
        /// Creates a new <see cref="Pose3"/> that contains a multiplication of two poses.
        /// </summary>
        /// <param name="left">Source <see cref="Pose3"/>.</param>
        /// <param name="right">Source <see cref="Pose3"/>.</param>
        /// <returns>The result of the pose multiplication.</returns>
        public static Pose3 Multiply(Pose3 left, Pose3 right)
        {
            Pose3 result;
            result.Orientation = Quaternion.Multiply(left.Orientation, right.Orientation);
            result.Translation = Vector3.Transform(left.Translation, right);
            result._padw = default;
            return result;
        }

        /// <summary>
        /// Deconstruction method for <see cref="Pose3"/>.
        /// </summary>
        /// <param name="orientation">The orientation of this <see cref="Pose3" />.</param>
        /// <param name="translation">The translation of this <see cref="Pose3"/>.</param>
        public void Deconstruct(out Quaternion orientation, out Vector3 translation)
        {
            orientation = this.Orientation;
            translation = this.Translation;
        }

        #region Operators

        /// <summary>
        /// Multiplies two poses.
        /// </summary>
        /// <param name="left">Source <see cref="Pose3"/> on the left of the mul sign.</param>
        /// <param name="right">Source <see cref="Pose3"/> on the right of the mul sign.</param>
        /// <returns>Result of the poses multiplication.</returns>
        public static Pose3 operator *(Pose3 left, Pose3 right)
        {
            Pose3 result;
            result = Pose3.Multiply(left, right);
            return result;
        }

        /// <summary>
        /// Compares whether two <see cref="Pose3"/> instances are equal.
        /// </summary>
        /// <param name="left"><see cref="Pose3"/> instance on the left of the equal sign.</param>
        /// <param name="right"><see cref="Pose3"/> instance on the right of the equal sign.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public static bool operator ==(Pose3 left, Pose3 right)
        {
            return left.Orientation == right.Orientation && left.Translation == right.Translation;
        }

        /// <summary>
        /// Compares whether two <see cref="Pose3"/> instances are not equal.
        /// </summary>
        /// <param name="left"><see cref="Pose3"/> instance on the left of the not equal sign.</param>
        /// <param name="right"><see cref="Pose3"/> instance on the right of the not equal sign.</param>
        /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
        public static bool operator !=(Pose3 left, Pose3 right)
        {
            return left.Orientation != right.Orientation || left.Translation != right.Translation;
        }

        #endregion


        #region IEquatable<Pose3>

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj is Pose3)
                return Equals((Pose3)obj);
            return false;
        }

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Pose3"/>.
        /// </summary>
        /// <param name="other">The <see cref="Pose3"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public bool Equals(Pose3 other)
        {
            return Orientation == other.Orientation
                && Translation == other.Translation;
        }

        #endregion


        /// <summary>
        /// Gets the hash code of this <see cref="Pose3"/>.
        /// </summary>
        /// <returns>Hash code of this <see cref="Pose3"/>.</returns>
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
