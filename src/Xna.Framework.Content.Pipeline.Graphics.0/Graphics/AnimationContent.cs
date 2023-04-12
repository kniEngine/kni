// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides properties for maintaining an animation.
    /// </summary>
    public class AnimationContent : ContentItem
    {
        TimeSpan _duration;
        AnimationChannelDictionary _channels;

        /// <summary>
        /// Gets the collection of animation data channels. Each channel describes the movement of a single bone or rigid object.
        /// </summary>
        public AnimationChannelDictionary Channels
        {
            get { return _channels; }
        }

        /// <summary>
        /// Gets or sets the total length of the animation.
        /// </summary>
        public TimeSpan Duration
        {
            get { return _duration; }
            set { _duration = value; }
        }

        /// <summary>
        /// Initializes a new instance of AnimationContent.
        /// </summary>
        public AnimationContent()
        {
            _channels = new AnimationChannelDictionary();
        }

        public override string ToString()
        {
            return String.Format("{{Duration:{0}, _channels: {1}}}", _duration, _channels.Count);
        }
    }
}
