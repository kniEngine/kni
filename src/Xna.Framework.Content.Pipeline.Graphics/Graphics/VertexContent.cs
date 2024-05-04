﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides methods and properties for maintaining the vertex data of a GeometryContent.
    /// </summary>
    /// <remarks>This class combines a collection of arbitrarily named data channels with a list of position indices that reference the Positions collection of the parent MeshContent.</remarks>
    public sealed class VertexContent
    {
        VertexChannelCollection _channels;
        VertexChannel<int> _positionIndices;
        IndirectPositionCollection _positions;

        /// <summary>
        /// Gets the list of named vertex data channels in the VertexContent.
        /// </summary>
        /// <value>Collection of vertex data channels.</value>
        public VertexChannelCollection Channels { get { return _channels; } }

        /// <summary>
        /// Gets the list of position indices.
        /// </summary>
        /// <value>Position of the position index being retrieved.</value>
        /// <remarks>This list adds a level of indirection between the actual triangle indices and the Positions member of the parent. This indirection preserves the topological vertex identity in cases where a single vertex position is used by triangles that straddle a discontinuity in some other data channel.
        /// For example, the following code gets the position of the first vertex of the first triangle in a GeometryContent object:
        /// parent.Positions[Vertices.PositionIndices[Indices[0]]]</remarks>
        public VertexChannel<int> PositionIndices { get { return _positionIndices; } }

        /// <summary>
        /// Gets position data from the parent mesh object.
        /// </summary>
        /// <value>Collection of vertex positions for the mesh.</value>
        /// <remarks>The collection returned from this call provides a virtualized view of the vertex positions for this batch. The collection uses the contents of the PositionIndices property to index into the parent Positions. This collection is read-only. If you need to modify any contained values, edit the PositionIndices or Positions members directly.</remarks>
        public IndirectPositionCollection Positions { get { return _positions; } }

        /// <summary>
        /// Number of vertices for the content.
        /// </summary>
        /// <value>Number of vertices.</value>
        public int VertexCount { get { return _positionIndices.Count; } }

        /// <summary>
        /// Constructs a VertexContent instance.
        /// </summary>
        internal VertexContent(GeometryContent geometry)
        {
            _positionIndices = new VertexChannel<int>("PositionIndices");
            _positions = new IndirectPositionCollection(geometry, _positionIndices);
            _channels = new VertexChannelCollection(this);
        }

        /// <summary>
        /// Appends a new vertex index to the end of the PositionIndices collection.
        /// Other vertex channels will automatically be extended and the new indices populated with default values.
        /// </summary>
        /// <param name="positionIndex">Index into the MeshContent.Positions member of the parent.</param>
        /// <returns>Index of the new entry. This can be added to the Indices member of the parent.</returns>
        public int Add(int positionIndex)
        {
            return _positionIndices.Items.Add(positionIndex);
        }

        /// <summary>
        /// Appends multiple vertex indices to the end of the PositionIndices collection.
        /// Other vertex channels will automatically be extended and the new indices populated with default values.
        /// </summary>
        /// <param name="positionIndexCollection">Index into the Positions member of the parent.</param>
        public void AddRange(IEnumerable<int> positionIndexCollection)
        {
            _positionIndices.InsertRange(_positionIndices.Items.Count, positionIndexCollection);
        }

        /// <summary>
        /// Converts design-time vertex position and channel data into a vertex buffer format that a graphics device can recognize.
        /// </summary>
        /// <returns>A packed vertex buffer.</returns>
        /// <exception cref="InvalidContentException">One or more of the vertex channel types are invalid or an unrecognized name was passed to VertexElementUsage.</exception>
        public VertexBufferContent CreateVertexBuffer()
        {
            VertexBufferContent vertexBuffer = new VertexBufferContent(_positions.Count);
            SetupVertexDeclaration(vertexBuffer.VertexDeclaration);
            int stride = vertexBuffer.VertexDeclaration.VertexStride.Value;

            // TODO: Verify enough elements in channels to match positions?

            // Write out data in an interleaved fashion each channel at a time, for example:
            //    |------------------------------------------------------------|
            //    |POSITION[0] | NORMAL[0]  |TEX0[0] | POSITION[1]| NORMAL[1]  |
            //    |-----------------------------------------------|------------|
            // #0 |111111111111|____________|________|111111111111|____________|
            // #1 |111111111111|111111111111|________|111111111111|111111111111|
            // #2 |111111111111|111111111111|11111111|111111111111|111111111111|

            // #0: Write position vertices using stride to skip over the other channels:
            vertexBuffer.Write(0, stride, _positions);

            int channelOffset = VertexBufferContent.SizeOf(typeof(Vector3));
            foreach (VertexChannel channel in Channels)
            {
                // #N: Fill in the channel within each vertex
                Type channelType = channel.ElementType;
                vertexBuffer.Write(channelOffset, stride, channelType, channel);
                channelOffset += VertexBufferContent.SizeOf(channelType);
            }

            return vertexBuffer;
        }

        private void SetupVertexDeclaration(VertexDeclarationContent vertexDeclaration)
        {
            int offset = 0;

            // We always have a position channel
            vertexDeclaration.VertexElements.Add(
                    new VertexElement(offset, VertexElementFormat.Vector3, VertexElementUsage.Position, 0));
            offset += VertexElementFormat.Vector3.GetSize();

            // Optional channels
            foreach (VertexChannel channel in Channels)
            {
                VertexElementFormat format;
                VertexElementUsage usage;

                // Try to determine the vertex format
                if (channel.ElementType == typeof(Single))
                    format = VertexElementFormat.Single;
                else if (channel.ElementType == typeof(Vector2))
                    format = VertexElementFormat.Vector2;
                else if (channel.ElementType == typeof(Vector3))
                    format = VertexElementFormat.Vector3;
                else if (channel.ElementType == typeof(Vector4))
                    format = VertexElementFormat.Vector4;
                else if (channel.ElementType == typeof(Color))
                    format = VertexElementFormat.Color;
                else if (channel.ElementType == typeof(Byte4))
                    format = VertexElementFormat.Byte4;
                else if (channel.ElementType == typeof(Short2))
                    format = VertexElementFormat.Short2;
                else if (channel.ElementType == typeof(Short4))
                    format = VertexElementFormat.Short4;
                else if (channel.ElementType == typeof(NormalizedShort2))
                    format = VertexElementFormat.NormalizedShort2;
                else if (channel.ElementType == typeof(NormalizedShort4))
                    format = VertexElementFormat.NormalizedShort4;
                else if (channel.ElementType == typeof(HalfVector2))
                    format = VertexElementFormat.HalfVector2;
                else if (channel.ElementType == typeof(HalfVector4))
                    format = VertexElementFormat.HalfVector4;
                else
                    throw new InvalidContentException(string.Format("Unrecognized vertex content type: '{0}'", channel.ElementType));

                // Try to determine the vertex usage
                if (!VertexChannelNames.TryDecodeUsage(channel.Name, out usage))
                    throw new InvalidContentException(string.Format("Unknown vertex element usage for channel '{0}'", channel.Name));

                // Try getting the usage index
                int usageIndex = VertexChannelNames.DecodeUsageIndex(channel.Name);

                vertexDeclaration.VertexElements.Add(new VertexElement(offset, format, usage, usageIndex));
                offset += format.GetSize();
            }

            vertexDeclaration.VertexStride = offset;
        }

        /// <summary>
        /// Inserts a new vertex index to the PositionIndices collection.
        /// Other vertex channels will automatically be extended and the new indices populated with default values.
        /// </summary>
        /// <param name="index">Vertex index to be inserted.</param>
        /// <param name="positionIndex">Position of new vertex index in the collection.</param>
        public void Insert(int index, int positionIndex)
        {
            _positionIndices.Items.Insert(index, positionIndex);
        }

        /// <summary>
        /// Inserts multiple vertex indices to the PositionIndices collection.
        /// Other vertex channels will automatically be extended and the new indices populated with default values.
        /// </summary>
        /// <param name="index">Vertex index to be inserted.</param>
        /// <param name="positionIndexCollection">Position of the first element of the inserted range in the collection.</param>
        public void InsertRange(int index, IEnumerable<int> positionIndexCollection)
        {
            _positionIndices.InsertRange(index, positionIndexCollection);
        }

        /// <summary>
        /// Removes a vertex index from the specified location in both PositionIndices and VertexChannel&lt;T&gt;.
        /// </summary>
        /// <param name="index">Index of the vertex to be removed.</param>
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= VertexCount)
                throw new ArgumentOutOfRangeException("index");

            _positionIndices.Items.RemoveAt(index);

            foreach (VertexChannel channel in _channels)
                channel.Items.RemoveAt(index);
        }

        /// <summary>
        /// Removes a range of vertex indices from the specified location in both PositionIndices and VertexChannel&lt;T&gt;.
        /// </summary>
        /// <param name="index">Index of the first vertex index to be removed.</param>
        /// <param name="count">Number of indices to remove.</param>
        public void RemoveRange(int index, int count)
        {
            if (index < 0 || index >= VertexCount)
                throw new ArgumentOutOfRangeException("index");
            if (count < 0 || (index+count) > VertexCount)
                throw new ArgumentOutOfRangeException("count");

            _positionIndices.RemoveRange(index, count);

            foreach (VertexChannel channel in _channels)
                channel.RemoveRange(index, count);
        }
    }
}
