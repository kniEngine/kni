// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    /// <summary>
    /// Stores the vertex layout (input elements) for the input assembler stage.
    /// </summary>
    public abstract class VertexInputLayout
    {
        public VertexDeclaration[] VertexDeclarations { get; private set; }
        public int[] InstanceFrequencies { get; private set; }

        /// <summary>
        /// Gets or sets the number of used input slots.
        /// </summary>
        /// <value>The number of used input slots.</value>
        public int Count { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexInputLayout"/> class.
        /// </summary>
        /// <param name="maxVertexBufferSlots">The maximum number of vertex buffer slots.</param>
        protected VertexInputLayout(int maxVertexBufferSlots)
            : this(new VertexDeclaration[maxVertexBufferSlots], new int[maxVertexBufferSlots], 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexInputLayout"/> class.
        /// </summary>
        /// <param name="vertexDeclarations">The array for storing vertex declarations.</param>
        /// <param name="instanceFrequencies">The array for storing instance frequencies.</param>
        /// <param name="count">The number of used slots.</param>
        protected VertexInputLayout(VertexDeclaration[] vertexDeclarations, int[] instanceFrequencies, int count)
        {
            Debug.Assert(vertexDeclarations != null);
            Debug.Assert(instanceFrequencies != null);
            Debug.Assert(count >= 0);
            Debug.Assert(vertexDeclarations.Length >= count);
            Debug.Assert(vertexDeclarations.Length == instanceFrequencies.Length);

            Count = count;
            VertexDeclarations = vertexDeclarations;
            InstanceFrequencies = instanceFrequencies;
        }

    }
}
