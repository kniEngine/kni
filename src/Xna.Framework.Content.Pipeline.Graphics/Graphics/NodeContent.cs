// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides a base class for graphics types that define local coordinate systems.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Node '{Name}'")]
    public class NodeContent : ContentItem
    {
        Matrix _transform;
        NodeContent _parent;
        NodeContentCollection _children;
        AnimationContentDictionary _animations;

        /// <summary>
        /// Gets the value of the local Transform property, multiplied by the AbsoluteTransform of the parent.
        /// </summary>
        public Matrix AbsoluteTransform
        {
            get
            {
                if (_parent != null)
                    return _transform * _parent.AbsoluteTransform;
                return _transform;
            }
        }

        /// <summary>
        /// Gets the set of animations belonging to this node.
        /// </summary>
        public AnimationContentDictionary Animations
        {
            get { return _animations; }
        }

        /// <summary>
        /// Gets the children of the NodeContent object.
        /// </summary>
        public NodeContentCollection Children
        {
            get { return _children; }
        }

        /// <summary>
        /// Gets the parent of this NodeContent object.
        /// </summary>
        public NodeContent Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        /// <summary>
        /// Gets the transform matrix of the scene.
        /// The transform matrix defines a local coordinate system for the content in addition to any children of this object.
        /// </summary>
        public Matrix Transform
        {
            get { return _transform; }
            set { _transform = value; }
        }

        /// <summary>
        /// Creates an instance of NodeContent.
        /// </summary>
        public NodeContent()
        {
            _children = new NodeContentCollection(this);
            _animations = new AnimationContentDictionary();
            Transform = Matrix.Identity;
        }
    }
}
