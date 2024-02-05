// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides properties and methods that define various aspects of a mesh.
    /// </summary>
    public class MeshContent : NodeContent
    {
        GeometryContentCollection _geometry;
        PositionCollection _positions;

        /// <summary>
        /// Gets the list of geometry batches for the mesh.
        /// </summary>
        public GeometryContentCollection Geometry
        {
            get { return _geometry; }
        }

        /// <summary>
        /// Gets the list of vertex position values.
        /// </summary>
        public PositionCollection Positions
        {
            get { return _positions; }
        }

        /// <summary>
        /// Initializes a new instance of MeshContent.
        /// </summary>
        public MeshContent()
        {
            _geometry = new GeometryContentCollection(this);
            _positions = new PositionCollection();
        }

        /// <summary>
        /// Applies a transform directly to position and normal channels. Node transforms are unaffected.
        /// </summary>
        internal void TransformContents(ref Matrix xform)
        {
            // Transform positions
            for (int i = 0; i < _positions.Count; i++)
                _positions[i] = Vector3.Transform(_positions[i], xform);

            // Transform all vectors too:
            // Normals are "tangent covectors", which need to be transformed using the
            // transpose of the inverse matrix!
            Matrix inverseTranspose = Matrix.Transpose(Matrix.Invert(xform));
            foreach (GeometryContent geometry in _geometry)
            {
                foreach (VertexChannel vertexChannel in geometry.Vertices.Channels)
                {
                    VertexChannel<Vector3> vector3Channel = vertexChannel as VertexChannel<Vector3>;
                    if (vector3Channel == null)
                        continue;

                    if (vertexChannel.Name.StartsWith("Normal")
                    ||  vertexChannel.Name.StartsWith("Binormal")
                    ||  vertexChannel.Name.StartsWith("Tangent"))
                    {
                        for (int i = 0; i < vector3Channel.Count; i++)
                        {
                            Vector3 normal = vector3Channel[i];
                            Vector3.TransformNormal(ref normal, ref inverseTranspose, out normal);
                            Vector3.Normalize(ref normal, out normal);
                            vector3Channel[i] = normal;
                        }
                    }
                }
            }

            // Swap winding order when faces are mirrored.
            if (MeshHelper.IsLeftHanded(ref xform))
                MeshHelper.SwapWindingOrder(this);
        }
    }
}
