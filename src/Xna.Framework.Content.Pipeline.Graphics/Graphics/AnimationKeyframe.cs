// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides methods and properties for managing a keyframe. A keyframe describes the position of an animation channel at a single point in time.
    /// </summary>
    public sealed class AnimationKeyframe : IComparable<AnimationKeyframe>
    {
        TimeSpan _time;
        Matrix _transform;

        /// <summary>
        /// Gets the time offset from the start of the animation to the position described by this keyframe.
        /// </summary>
        public TimeSpan Time { get { return _time; } }

        /// <summary>
        /// Gets or sets the position described by this keyframe.
        /// </summary>
        public Matrix Transform
        {
            get { return _transform; }
            set { _transform = value; }
        }

        /// <summary>
        /// Initializes a new instance of AnimationKeyframe with the specified time offsetand transform.
        /// </summary>
        /// <param name="time">Time offset of the keyframe.</param>
        /// <param name="transform">Position of the keyframe.</param>
        public AnimationKeyframe(TimeSpan time, Matrix transform)
        {
            this._time = time;
            this._transform = transform;
        }

        /// <summary>
        /// Compares this instance of a keyframe to another.
        /// </summary>
        /// <param name="other">Keyframe being compared to.</param>
        /// <returns>Indication of their relative values.</returns>
        public int CompareTo(AnimationKeyframe other)
        {
            // No sense in comparing the transform, so compare the time.
            // This would be used for sorting keyframes in time order.
            return _time.CompareTo(other._time);
        }

        public override string ToString()
        {
            return String.Format("{{Time:{0}, Transform: {1}}}", _time, _transform);
        }
    }
}
