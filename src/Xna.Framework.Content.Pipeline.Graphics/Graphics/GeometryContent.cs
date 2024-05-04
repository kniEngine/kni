// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides properties that define various aspects of a geometry batch.
    /// </summary>
    public class GeometryContent : ContentItem
    {
        IndexCollection _indices;
        MaterialContent _material;
        MeshContent _parent;
        VertexContent _vertices;

        /// <summary>
        /// Gets the list of triangle indices for this geometry batch. Geometry is stored as an indexed triangle list, where each group of three indices defines a single triangle.
        /// </summary>
        public IndexCollection Indices
        {
            get { return _indices; }
        }

        /// <summary>
        /// Gets or sets the material of the parent mesh.
        /// </summary>
        public MaterialContent Material
        {
            get { return _material; }
            set { _material = value; }
        }

        /// <summary>
        /// Gets or sets the parent MeshContent for this object.
        /// </summary>
        public MeshContent Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        /// <summary>
        /// Gets the set of vertex batches for the geometry batch.
        /// </summary>
        public VertexContent Vertices
        {
            get { return _vertices; }
        }

        /// <summary>
        /// Creates an instance of GeometryContent.
        /// </summary>
        public GeometryContent()
        {
            _indices = new IndexCollection();
            _vertices = new VertexContent(this);
        }

        public override string ToString()
        {
            return String.Format("{{Name:{0}, Vertices: {1}, Indices: {2} }}",
                base.Name, this.Vertices.VertexCount, this.Indices.Count);
        }
    }
}
