// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Represents a ray with an origin and a direction in 3D space.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{DebugDisplayString,nq}")]
    public struct Ray : IEquatable<Ray>
    {
        #region Public Fields

        /// <summary>
        /// The direction of this <see cref="Ray"/>.
        /// </summary>
        [DataMember]
        public Vector3 Direction;
      
        /// <summary>
        /// The origin of this <see cref="Ray"/>.
        /// </summary>
        [DataMember]
        public Vector3 Position;

        #endregion


        #region Public Constructors

        /// <summary>
        /// Create a <see cref="Ray"/>.
        /// </summary>
        /// <param name="position">The origin of the <see cref="Ray"/>.</param>
        /// <param name="direction">The direction of the <see cref="Ray"/>.</param>
        public Ray(Vector3 position, Vector3 direction)
        {
            this.Position = position;
            this.Direction = direction;
        }

        #endregion


        #region Public Methods

        /// <summary>
        /// Check if the specified <see cref="Object"/> is equal to this <see cref="Ray"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to test for equality with this <see cref="Ray"/>.</param>
        /// <returns>
        /// <code>true</code> if the specified <see cref="Object"/> is equal to this <see cref="Ray"/>,
        /// <code>false</code> if it is not.
        /// </returns>
        public override bool Equals(object obj)
        {
            return (obj is Ray) && this.Equals((Ray)obj);
        }

        /// <summary>
        /// Check if the specified <see cref="Ray"/> is equal to this <see cref="Ray"/>.
        /// </summary>
        /// <param name="other">The <see cref="Ray"/> to test for equality with this <see cref="Ray"/>.</param>
        /// <returns>
        /// <code>true</code> if the specified <see cref="Ray"/> is equal to this <see cref="Ray"/>,
        /// <code>false</code> if it is not.
        /// </returns>
        public bool Equals(Ray other)
        {
            return this.Position.Equals(other.Position) && this.Direction.Equals(other.Direction);
        }

        /// <summary>
        /// Get a hash code for this <see cref="Ray"/>.
        /// </summary>
        /// <returns>A hash code for this <see cref="Ray"/>.</returns>
        public override int GetHashCode()
        {
            return Position.GetHashCode() ^ Direction.GetHashCode();
        }

        /// <summary>
        /// Check if this <see cref="Ray"/> intersects the specified <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="box">The <see cref="BoundingBox"/> to test for intersection.</param>
        /// <returns>
        /// The distance along the ray of the intersection or <code>null</code> if this
        /// <see cref="Ray"/> does not intersect the <see cref="BoundingBox"/>.
        /// </returns>
        public float? Intersects(BoundingBox box)
        {
            float? result;
            IntersectsHelper.BoundingBoxIntersectsRay(ref box, ref this, out result);
            return result;
        }

        /// <summary>
        /// Check if this <see cref="Ray"/> intersects the specified <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="box">The <see cref="BoundingBox"/> to test for intersection.</param>
        /// <param name="result">
        /// The distance along the ray of the intersection or <code>null</code> if this
        /// <see cref="Ray"/> does not intersect the <see cref="BoundingBox"/>.
        /// </param>
        public void Intersects(ref BoundingBox box, out float? result)
        {
            IntersectsHelper.BoundingBoxIntersectsRay(ref box, ref this, out result);
        }

        public float? Intersects(BoundingFrustum frustum)
        {
            if (frustum == null)
                throw new ArgumentNullException("frustum");

            float? result;
            IntersectsHelper.BoundingFrustumIntersectsRay(frustum, ref this, out result);
            return result;
        }

        /// <summary>
        /// Check if this <see cref="Ray"/> intersects the specified <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The <see cref="BoundingBox"/> to test for intersection.</param>
        /// <returns>
        /// The distance along the ray of the intersection or <code>null</code> if this
        /// <see cref="Ray"/> does not intersect the <see cref="BoundingSphere"/>.
        /// </returns>
        public float? Intersects(BoundingSphere sphere)
        {
            float? result;
            IntersectsHelper.BoundingSphereIntersectsRay(ref sphere, ref this, out result);
            return result;
        }

        /// <summary>
        /// Check if this <see cref="Ray"/> intersects the specified <see cref="Plane"/>.
        /// </summary>
        /// <param name="plane">The <see cref="Plane"/> to test for intersection.</param>
        /// <returns>
        /// The distance along the ray of the intersection or <code>null</code> if this
        /// <see cref="Ray"/> does not intersect the <see cref="Plane"/>.
        /// </returns>
        public float? Intersects(Plane plane)
        {
            float? result;
            IntersectsHelper.PlaneIntersectsRay(ref plane, ref this, out result);
            return result;
        }

        /// <summary>
        /// Check if this <see cref="Ray"/> intersects the specified <see cref="Plane"/>.
        /// </summary>
        /// <param name="plane">The <see cref="Plane"/> to test for intersection.</param>
        /// <param name="result">
        /// The distance along the ray of the intersection or <code>null</code> if this
        /// <see cref="Ray"/> does not intersect the <see cref="Plane"/>.
        /// </param>
        public void Intersects(ref Plane plane, out float? result)
        {
            IntersectsHelper.PlaneIntersectsRay(ref plane, ref this, out result);
        }

        /// <summary>
        /// Check if this <see cref="Ray"/> intersects the specified <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The <see cref="BoundingBox"/> to test for intersection.</param>
        /// <param name="result">
        /// The distance along the ray of the intersection or <code>null</code> if this
        /// <see cref="Ray"/> does not intersect the <see cref="BoundingSphere"/>.
        /// </param>
        public void Intersects(ref BoundingSphere sphere, out float? result)
        {
            IntersectsHelper.BoundingSphereIntersectsRay(ref sphere, ref this, out result);
        }

        /// <summary>
        /// Check if two rays are not equal.
        /// </summary>
        /// <param name="left">A ray to check for inequality.</param>
        /// <param name="right">A ray to check for inequality.</param>
        /// <returns><code>true</code> if the two rays are not equal, <code>false</code> if they are.</returns>
        public static bool operator !=(Ray left, Ray right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Check if two rays are equal.
        /// </summary>
        /// <param name="left">A ray to check for equality.</param>
        /// <param name="right">A ray to check for equality.</param>
        /// <returns><code>true</code> if the two rays are equals, <code>false</code> if they are not.</returns>
        public static bool operator ==(Ray left, Ray right)
        {
            return left.Equals(right);
        }

        internal string DebugDisplayString
        {
            get
            {
                return string.Concat(
                    "Pos( ", this.Position.DebugDisplayString, " )  \r\n",
                    "Dir( ", this.Direction.DebugDisplayString, " )"
                );
            }
        }

        /// <summary>
        /// Get a <see cref="String"/> representation of this <see cref="Ray"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> representation of this <see cref="Ray"/>.</returns>
        public override string ToString()
        {
            return "{{Position:" + Position.ToString() + " Direction:" + Direction.ToString() + "}}";
        }

        /// <summary>
        /// Deconstruction method for <see cref="Ray"/>.
        /// </summary>
        /// <param name="position">Receives the start position of the ray.</param>
        /// <param name="direction">Receives the direction of the ray.</param>
        public void Deconstruct(out Vector3 position, out Vector3 direction)
        {
            position = Position;
            direction = Direction;
        }

        #endregion
    }
}
