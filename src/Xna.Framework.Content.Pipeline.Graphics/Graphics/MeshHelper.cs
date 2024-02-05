// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public static class MeshHelper
    {
        static bool IsFinite(float v)
        {
            return !float.IsInfinity(v) && !float.IsNaN(v);
        }

        static bool IsFinite(this Vector3 v)
        {
            return IsFinite(v.X) && IsFinite(v.Y) && IsFinite(v.Z);
        }

        static bool IsNaN(this Vector3 v)
        {
            return float.IsNaN(v.X) || float.IsNaN(v.Y) || float.IsNaN(v.Z);
        }

        /// <summary>
        /// Generates vertex normals by accumulation of triangle face normals.
        /// </summary>
        /// <param name="mesh">The mesh which will receive the normals.</param>
        /// <param name="overwriteExistingNormals">Overwrite or skip over geometry with existing normals.</param>
        /// <remarks>
        /// This calls <see cref="CalculateNormals(GeometryContent, bool)"/> to do the work.
        /// </remarks>
        public static void CalculateNormals(MeshContent mesh, bool overwriteExistingNormals)
        {
            foreach (GeometryContent geometry in mesh.Geometry)
                CalculateNormals(geometry, overwriteExistingNormals);
        }

        /// <summary>
        /// Generates vertex normals by accumulation of triangle face normals.
        /// </summary>
        /// <param name="geometry">The geometry which will receive the normals.</param>
        /// <param name="overwriteExistingNormals">Overwrite or skip over geometry with existing normals.</param>
        /// <remarks>
        /// We use a "Mean Weighted Equally" method generate vertex normals from triangle 
        /// face normals.  If normal cannot be calculated from the geometry we set it to zero.
        /// </remarks>
        public static void CalculateNormals(GeometryContent geometry, bool overwriteExistingNormals)
        {
            VertexChannel<Vector3> normalsChannel;
            // Look for an existing normals channel.
            if (!geometry.Vertices.Channels.Contains(VertexChannelNames.Normal()))
            {
                // We don't have existing normals, so add a new channel.
                normalsChannel = geometry.Vertices.Channels.Add<Vector3>(VertexChannelNames.Normal(), null);
            }
            else
            {
                // If we're not supposed to overwrite the existing
                // normals then we're done here.
                if (!overwriteExistingNormals)
                    return;

                normalsChannel = geometry.Vertices.Channels.Get<Vector3>(VertexChannelNames.Normal());
            }

            // Accumulate all the triangle face normals for each vertex.
            Vector3[] normals = new Vector3[normalsChannel.Count];
            for (int i = 0; i < geometry.Indices.Count; i += 3)
            {
                int ia = geometry.Indices[i + 0];
                int ib = geometry.Indices[i + 1];
                int ic = geometry.Indices[i + 2];

                Vector3 aa = geometry.Vertices.Positions[ia];
                Vector3 bb = geometry.Vertices.Positions[ib];
                Vector3 cc = geometry.Vertices.Positions[ic];

                Vector3 faceNormal = Vector3.Cross(cc - bb, bb - aa);
                float len = faceNormal.Length();
                if (len > 0.0f)
                {
                    faceNormal.X /= len;
                    faceNormal.Y /= len;
                    faceNormal.Z /= len;

                    // We are using the "Mean Weighted Equally" method where each
                    // face has an equal weight in the final normal calculation.
                    //
                    // We could maybe switch to "Mean Weighted by Angle" which is said
                    // to look best in most cases, but is more expensive to calculate.
                    //
                    // There is also an idea of weighting by triangle area, but IMO the
                    // triangle area doesn't always have a direct relationship to the 
                    // shape of a mesh.
                    //
                    // For more ideas see:
                    //
                    // "A Comparison of Algorithms for Vertex Normal Computation"
                    // by Shuangshuang Jin, Robert R. Lewis, David West.
                    //

                    Vector3.Add(ref normals[ia], ref faceNormal, out normals[ia]);
                    Vector3.Add(ref normals[ib], ref faceNormal, out normals[ib]);
                    Vector3.Add(ref normals[ic], ref faceNormal, out normals[ic]);
                }
            }

            // Normalize the gathered vertex normals.
            for (int i = 0; i < normals.Length; i++)
            {
                float len = normals[i].Length();
                if (len > 0.0f)
                {
                    normals[i].X /= len;
                    normals[i].Y /= len;
                    normals[i].Z /= len;
                }
                else
                {
                    // TODO: It would be nice to be able to log this to
                    // the pipeline so that it can be fixed in the model.

                    // TODO: We could maybe void this by a better algorithm
                    // above for generating the normals.

                    // We have a zero length normal.  You can argue that putting
                    // anything here is better than nothing, but by leaving it to
                    // zero it allows the caller to detect this and react to it.
                    normals[i] = Vector3.Zero;
                }

                Debug.Assert(!normals[i].IsNaN(), "Bad normal!");
                Debug.Assert(normals[i].IsFinite(), "Bad normal!");
                Debug.Assert(normals[i].Length() >= 0.9999f, "Bad normal!");

                // Set the new normals on the vertex channel.
                normalsChannel[i] = normals[i];
            }

            return;
        }

        /// <summary>
        /// Generate the tangents and binormals (tangent frames) for each vertex in the mesh.
        /// </summary>
        /// <param name="mesh">The mesh which will have add tangent and binormal channels added.</param>
        /// <param name="textureCoordinateChannelName">The Vector2 texture coordinate channel used to generate tangent frames.</param>
        /// <param name="tangentChannelName"></param>
        /// <param name="binormalChannelName"></param>
        public static void CalculateTangentFrames(MeshContent mesh, string textureCoordinateChannelName, string tangentChannelName, string binormalChannelName)
        {
            for (int g = 0; g < mesh.Geometry.Count; g++)
            {
                GeometryContent geometry = mesh.Geometry[g];

                CalculateTangentFrames(geometry, textureCoordinateChannelName, tangentChannelName, binormalChannelName);
            }
        }

        public static void CalculateTangentFrames(GeometryContent geometry, string textureCoordinateChannelName, string tangentChannelName, string binormalChannelName)
        {
            VertexContent verts = geometry.Vertices;
            IndexCollection indices = geometry.Indices;
            VertexChannelCollection channels = geometry.Vertices.Channels;

            VertexChannel<Vector3> normals = channels.Get<Vector3>(VertexChannelNames.Normal(0));
            VertexChannel<Vector2> uvs = channels.Get<Vector2>(textureCoordinateChannelName);

            Vector3[] tangents, bitangents;
            CalculateTangentFrames(verts.Positions, indices, normals, uvs, out tangents, out bitangents);

            // All the indices are 1:1 with the others, so we 
            // can just add the new channels in place.

            if (!string.IsNullOrEmpty(tangentChannelName))
                channels.Add(tangentChannelName, tangents);

            if (!string.IsNullOrEmpty(binormalChannelName))
                channels.Add(binormalChannelName, bitangents);
        }

        public static void CalculateTangentFrames(IList<Vector3> positions,
                                                  IList<int> indices,
                                                  IList<Vector3> normals,
                                                  IList<Vector2> textureCoords,
                                                  out Vector3[] tangents,
                                                  out Vector3[] bitangents)
        {
            // Lengyel, Eric. “Computing Tangent Space Basis Vectors for an Arbitrary Mesh”. 
            // Terathon Software 3D Graphics Library, 2001.
            // http://www.terathon.com/code/tangent.html

            // Hegde, Siddharth. "Messing with Tangent Space". Gamasutra, 2007. 
            // http://www.gamasutra.com/view/feature/129939/messing_with_tangent_space.php

            int numVerts = positions.Count;
            int numIndices = indices.Count;

            Vector3[] tan1 = new Vector3[numVerts];
            Vector3[] tan2 = new Vector3[numVerts];

            for (int index = 0; index < numIndices; index += 3)
            {
                int i1 = indices[index + 0];
                int i2 = indices[index + 1];
                int i3 = indices[index + 2];

                Vector2 w1 = textureCoords[i1];
                Vector2 w2 = textureCoords[i2];
                Vector2 w3 = textureCoords[i3];

                float s1 = w2.X - w1.X;
                float s2 = w3.X - w1.X;
                float t1 = w2.Y - w1.Y;
                float t2 = w3.Y - w1.Y;

                float denom = s1 * t2 - s2 * t1;
                if (Math.Abs(denom) < float.Epsilon)
                {
                    // The triangle UVs are zero sized one dimension.
                    //
                    // So we cannot calculate the vertex tangents for this
                    // one trangle, but maybe it can with other trangles.
                    continue;
                }

                float r = 1.0f / denom;
                Debug.Assert(IsFinite(r), "Bad r!");

                Vector3 v1 = positions[i1];
                Vector3 v2 = positions[i2];
                Vector3 v3 = positions[i3];

                float x1 = v2.X - v1.X;
                float x2 = v3.X - v1.X;
                float y1 = v2.Y - v1.Y;
                float y2 = v3.Y - v1.Y;
                float z1 = v2.Z - v1.Z;
                float z2 = v3.Z - v1.Z;

                Vector3 sdir;
                sdir.X = (t2 * x1 - t1 * x2) * r;
                sdir.Y = (t2 * y1 - t1 * y2) * r;
                sdir.Z = (t2 * z1 - t1 * z2) * r;

                Vector3 tdir;
                tdir.X = (s1 * x2 - s2 * x1) * r;
                tdir.Y = (s1 * y2 - s2 * y1) * r;
                tdir.Z = (s1 * z2 - s2 * z1) * r;

                tan1[i1] += sdir;
                Debug.Assert(tan1[i1].IsFinite(), "Bad tan1[i1]!");
                tan1[i2] += sdir;
                Debug.Assert(tan1[i2].IsFinite(), "Bad tan1[i2]!");
                tan1[i3] += sdir;
                Debug.Assert(tan1[i3].IsFinite(), "Bad tan1[i3]!");

                tan2[i1] += tdir;
                Debug.Assert(tan2[i1].IsFinite(), "Bad tan2[i1]!");
                tan2[i2] += tdir;
                Debug.Assert(tan2[i2].IsFinite(), "Bad tan2[i2]!");
                tan2[i3] += tdir;
                Debug.Assert(tan2[i3].IsFinite(), "Bad tan2[i3]!");
            }

            tangents = new Vector3[numVerts];
            bitangents = new Vector3[numVerts];

            // At this point we have all the vectors accumulated, but we need to average
            // them all out. So we loop through all the final verts and do a Gram-Schmidt
            // orthonormalize, then make sure they're all unit length.
            for (int i = 0; i < numVerts; i++)
            {
                Vector3 n = normals[i];
                Debug.Assert(n.IsFinite(), "Bad normal!");
                Debug.Assert(n.Length() >= 0.9999f, "Bad normal!");

                Vector3 t = tan1[i];
                if (t.LengthSquared() < float.Epsilon)
                {
                    // TODO: Ideally we could spit out a warning to the
                    // content logging here!

                    // We couldn't find a good tanget for this vertex.
                    //
                    // Rather than set them to zero which could produce
                    // errors in other parts of the pipeline, we just take        
                    // a guess at something that may look ok.

                    t = Vector3.Cross(n, Vector3.UnitX);
                    if (t.LengthSquared() < float.Epsilon)
                        t = Vector3.Cross(n, Vector3.UnitY);

                    tangents[i] = Vector3.Normalize(t);
                    bitangents[i] = Vector3.Cross(n, tangents[i]);
                    continue;
                }

                // Gram-Schmidt orthogonalize
                // TODO: This can be zero can cause NaNs on 
                // normalize... how do we fix this?
                Vector3 tangent = t - n * Vector3.Dot(n, t);
                tangent = Vector3.Normalize(tangent);
                Debug.Assert(tangent.IsFinite(), "Bad tangent!");
                tangents[i] = tangent;

                // Calculate handedness
                float w = (Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0F) ? -1.0F : 1.0F;
                Debug.Assert(IsFinite(w), "Bad handedness!");

                // Calculate the bitangent
                Vector3 bitangent = Vector3.Cross(n, tangent) * w;
                Debug.Assert(bitangent.IsFinite(), "Bad bitangent!");
                bitangents[i] = bitangent;
            }
        }

        /// <summary>
        /// Search for the root bone of the skeletion.
        /// </summary>
        /// <param name="node">The node from which to begin the search for the skeleton.</param>
        /// <returns>The root bone of the skeletion or null if none is found.</returns>
        public static BoneContent FindSkeleton(NodeContent node)
        {
            // We should always get a node to search!
            if (node == null)
                throw new ArgumentNullException("node");

            // Search up thru the hierarchy.
            for (; node != null; node = node.Parent)
            {
                // First if this node is a bone then search up for the root.
                BoneContent root = node as BoneContent;
                if (root != null)
                {
                    while (root.Parent is BoneContent)
                        root = (BoneContent)root.Parent;
                    return root;
                }

                // Next try searching the children for a root bone.
                foreach (NodeContent nodeContent in node.Children)
                {
                    BoneContent bone = nodeContent as BoneContent;
                    if (bone == null) 
                        continue;

                    // If we found a bone
                    if (root != null)
                        throw new InvalidContentException("DuplicateSkeleton", node.Identity);

                    // This is our new root.
                    root = bone;
                }

                // If we found a root bone then return it, else
                // we continue the search to the node parent.
                if (root != null)
                    return root;
            }

            // We didn't find any bones!
            return null;
        }

        /// <summary>
        /// Traverses a skeleton depth-first and builds a list of its bones.
        /// </summary>
        public static IList<BoneContent> FlattenSkeleton(BoneContent skeleton)
        {
            if (skeleton == null)
                throw new ArgumentNullException("skeleton");

            List<BoneContent> results = new List<BoneContent>();
            Stack<NodeContent> work = new Stack<NodeContent>(new[] { skeleton });
            while (work.Count > 0)
            {
                NodeContent top = work.Pop();
                BoneContent bone = top as BoneContent;
                if (bone != null)
                    results.Add(bone);

                for (int i = top.Children.Count - 1; i >= 0; i--)
                    work.Push(top.Children[i]);
            }

            return results;
        }

        /// <summary>
        /// Merge any positions in the <see cref="PositionCollection"/> of the
        /// specified mesh that are at a distance less than the specified tolerance
        /// from each other.
        /// </summary>
        /// <param name="mesh">Mesh to be processed.</param>
        /// <param name="tolerance">Tolerance value that determines how close 
        /// positions must be to each other to be merged.</param>
        /// <remarks>
        /// This method will also update the <see cref="VertexContent.PositionIndices"/>
        /// in the <see cref="GeometryContent"/> of the specified mesh.
        /// </remarks>
        public static void MergeDuplicatePositions(MeshContent mesh, float tolerance)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            // TODO Improve performance with spatial partitioning scheme
            List<IndexUpdateList> indexLists = new List<IndexUpdateList>();
            foreach (GeometryContent geometry in mesh.Geometry)
            {
                IndexUpdateList list = new IndexUpdateList(geometry.Vertices.PositionIndices);
                indexLists.Add(list);
            }

            for (int i = mesh.Positions.Count - 1; i >= 1; i--)
            {
                Vector3 pi = mesh.Positions[i];
                for (int j = i - 1; j >= 0; j--)
                {
                    Vector3 pj = mesh.Positions[j];
                    if (Vector3.Distance(pi, pj) <= tolerance)
                    {
                        foreach (IndexUpdateList list in indexLists)
                            list.Update(i, j);
                        mesh.Positions.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Merge vertices with the same <see cref="VertexContent.PositionIndices"/> and
        /// <see cref="VertexChannel"/> data within the specified
        /// <see cref="GeometryContent"/>.
        /// </summary>
        /// <param name="geometry">Geometry to be processed.</param>
        public static void MergeDuplicateVertices(GeometryContent geometry)
        {
            if (geometry == null)
                throw new ArgumentNullException("geometry");

            VertexContent verts = geometry.Vertices;
            var hashMap = new Dictionary<int, List<VertexData>>();

            IndexUpdateList indices = new IndexUpdateList(geometry.Indices);
            int vIndex = 0;

            for (int i = 0; i < geometry.Indices.Count; i++)
            {
                int iIndex = geometry.Indices[i];
                VertexData iData = new VertexData
                {
                    Index = iIndex,
                    PositionIndex = verts.PositionIndices[vIndex],
                    ChannelData = new object[verts.Channels.Count]
                };
                
                for (int channel = 0; channel < verts.Channels.Count; channel++)
                    iData.ChannelData[channel] = verts.Channels[channel][vIndex];

                int hash = iData.ComputeHash();

                bool merged = false;
                List<VertexData> candidates;
                if (hashMap.TryGetValue(hash, out candidates))
                {
                    for (int candidateIndex = 0; candidateIndex < candidates.Count; candidateIndex++)
                    {
                        VertexData c = candidates[candidateIndex];
                        if (!iData.ContentEquals(c))
                            continue;

                        // Match! Update the corresponding indices and remove the vertex
                        indices.Update(iIndex, c.Index);
                        verts.RemoveAt(vIndex);
                        merged = true;
                    }
                    if (!merged)
                        candidates.Add(iData);
                }
                else
                {
                    // no vertices with the same hash yet, create a new list for the data
                    hashMap.Add(hash, new List<VertexData> { iData });
                }

                if (!merged)
                    vIndex++;
            }

            // update the indices because of the vertices we removed
            indices.Pack();
        }

        /// <summary>
        /// Merge vertices with the same <see cref="VertexContent.PositionIndices"/> and
        /// <see cref="VertexChannel"/> data within the <see cref="MeshContent.Geometry"/>
        /// of this mesh. If you want to merge positions too, call 
        /// <see cref="MergeDuplicatePositions"/> on your mesh before this function.
        /// </summary>
        /// <param name="mesh">Mesh to be processed</param>
        public static void MergeDuplicateVertices(MeshContent mesh)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            foreach (GeometryContent geometry in mesh.Geometry)
                MergeDuplicateVertices(geometry);
        }

        public static void OptimizeForCache(MeshContent mesh)
        {
            // We don't throw here as non-optimized still works.
        }
        
        /// <summary>
        /// Reverses the triangle winding order of the mesh.
        /// </summary>
        /// <param name="mesh">The mesh which will be modified.</param>
        /// <remarks>
        /// This method is useful when changing the direction of backface culling
        /// like when switching between left/right handed coordinate systems.
        /// </remarks>
        public static void SwapWindingOrder(MeshContent mesh)
        {
            // Gotta have a mesh to run!
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            foreach (GeometryContent geometry in mesh.Geometry)
            {
                for (int i = 0; i < geometry.Indices.Count; i += 3)
                {
                    int first = geometry.Indices[i];
                    int last = geometry.Indices[i+2];
                    geometry.Indices[i] = last;
                    geometry.Indices[i+2] = first;
                }
            }
        }

        /// <summary>
        /// Transforms the contents of a node and its descendants.
        /// </summary>
        /// <remarks>The node transforms themselves are unaffected.</remarks>
        /// <param name="scene">The root node of the scene to transform.</param>
        /// <param name="transform">The transform matrix to apply to the scene.</param>
        public static void TransformScene(NodeContent scene, Matrix transform)
        {
            if (scene == null)
                throw new ArgumentException("scene");

            // If the transformation is an identity matrix, this is a no-op and
            // we can save ourselves a bunch of work in the first place.
            if (transform == Matrix.Identity)
                return;

            Matrix inverseTransform = Matrix.Invert(transform);

            Stack<NodeContent> work = new Stack<NodeContent>();
            work.Push(scene);

            while (work.Count > 0)
            {
                NodeContent node = work.Pop();
                foreach (NodeContent child in node.Children)
                    work.Push(child);

                // Transform the mesh content.
                MeshContent mesh = node as MeshContent;
                if (mesh != null)
                    mesh.TransformContents(ref transform);

                // Transform local coordinate system using "similarity transform".
                node.Transform = inverseTransform * node.Transform * transform;

                // Transform animations.
                foreach (AnimationContent animation in node.Animations.Values)
                    foreach (AnimationChannel animationChannel in animation.Channels.Values)
                        for (int i = 0; i < animationChannel.Count; i++)
                            animationChannel[i].Transform = inverseTransform * animationChannel[i].Transform * transform;
            }
        }

        /// <summary>
        /// Determines whether the specified transform is left-handed.
        /// </summary>
        /// <param name="xform">The transform.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="xform"/> is left-handed; otherwise,
        /// <see langword="false"/> if <paramref name="xform"/> is right-handed.
        /// </returns>
        internal static bool IsLeftHanded(ref Matrix xform)
        {
            // Check sign of determinant of upper-left 3x3 matrix:
            //   positive determinant ... right-handed
            //   negative determinant ... left-handed

            // Since XNA does not have a 3x3 matrix, use the "scalar triple product"
            // (see http://en.wikipedia.org/wiki/Triple_product) to calculate the
            // determinant.
            float d = Vector3.Dot(xform.Right, Vector3.Cross(xform.Forward, xform.Up));
            return d < 0.0f;
        }

        #region Private helpers

        private static void UpdatePositionIndices(MeshContent mesh, int from, int to)
        {
            foreach (GeometryContent geometry in mesh.Geometry)
            {
                for (int i = 0; i < geometry.Vertices.PositionIndices.Count; i++)
                {
                    int index = geometry.Vertices.PositionIndices[i];
                    if (index == from)
                        geometry.Vertices.PositionIndices[i] = to;
                }
            }
        }

        private class VertexData
        {
            public int Index;
            public int PositionIndex;
            public object[] ChannelData;

            // Compute a hash based on PositionIndex and ChannelData
            public int ComputeHash()
            {
                int hash = PositionIndex;
                foreach (object channel in ChannelData)
                    hash ^= channel.GetHashCode();

                return hash;
            }

            // Check equality on PositionIndex and ChannelData
            public bool ContentEquals(VertexData other)
            {
                if (PositionIndex != other.PositionIndex)
                    return false;

                if (ChannelData.Length != other.ChannelData.Length)
                    return false;

                for (int i = 0; i < ChannelData.Length; i++)
                {
                        if (!Equals(ChannelData[i], other.ChannelData[i]))
                        return false;
                }

                return true;
            }
        }

        // takes an IndexCollection and can efficiently update index values
        private class IndexUpdateList
        {
            private readonly IList<int> _collectionToUpdate;
            private readonly Dictionary<int, List<int>> _indexPositions;

            // create the list, presort the values and compute the start positions of each value
            public IndexUpdateList(IList<int> collectionToUpdate)
            {
                _collectionToUpdate = collectionToUpdate;
                _indexPositions = new Dictionary<int, List<int>>();
                Initialize();
            }

            private void Initialize()
            {
                for (int pos = 0; pos < _collectionToUpdate.Count; pos++)
                {
                    int v = _collectionToUpdate[pos];
                    if (_indexPositions.ContainsKey(v))
                        _indexPositions[v].Add(pos);
                    else
                        _indexPositions.Add(v, new List<int> {pos});
                }
            }

            public void Update(int from, int to)
            {
                if (from == to || !_indexPositions.ContainsKey(from))
                    return;

                foreach (int pos in _indexPositions[from])
                    _collectionToUpdate[pos] = to;

                if (_indexPositions.ContainsKey(to))
                    _indexPositions[to].AddRange(_indexPositions[from]);
                else
                    _indexPositions.Add(to, _indexPositions[from]);

                _indexPositions.Remove(from);
            }

            // Pack all indices together starting from zero
            // E.g. [5, 5, 3, 5, 21, 3] -> [1, 1, 0, 1, 2, 0]
            // note that the order must be kept
            public void Pack()
            {
                if (_collectionToUpdate.Count == 0)
                    return;

                SortedSet<int> sorted = new SortedSet<int>(_collectionToUpdate);

                int newIndex = 0;
                foreach (int value in sorted)
                {
                    foreach (int pos in _indexPositions[value])
                        _collectionToUpdate[pos] = newIndex;

                    newIndex++;
                }

            }

        }

        #endregion
    }
}
