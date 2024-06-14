// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    /// <summary>
    /// Stores the vertex buffers to be bound to the input assembler stage.
    /// </summary>
    public sealed class VertexBufferCollection
    {
        VertexBufferBinding[] _bindings;

        /// <summary>
        /// Gets or sets the number of used input slots.
        /// </summary>
        /// <value>The number of used input slots.</value>
        public int Count { get; private set; }

        // VertexDeclarations and InstanceFrequencies are used by DX VertexInputLayoutKey and InputLayoutCache
        public VertexDeclaration[] VertexDeclarations { get; private set; }
        public int[] InstanceFrequencies { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="VertexBufferCollection" /> class.
        /// </summary>
        /// <param name="maxVertexBufferSlots">The maximum number of vertex buffer slots.</param>
        internal VertexBufferCollection(int maxVertexBufferSlots)
        {
            Count = 0;
            _bindings = new VertexBufferBinding[maxVertexBufferSlots];

            VertexDeclarations = new VertexDeclaration[maxVertexBufferSlots];
            InstanceFrequencies = new int[maxVertexBufferSlots];

        }

        /// <summary>
        /// Clears the vertex buffer slots.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the input layout was changed; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool Clear()
        {
            if (Count == 0)
                return false;

            Array.Clear(_bindings, 0, Count);

            Array.Clear(VertexDeclarations, 0, Count);
            Array.Clear(InstanceFrequencies, 0, Count);
            Count = 0;
            return true;
        }

        /// <summary>
        /// Binds the specified vertex buffer to the first input slot.
        /// </summary>
        /// <param name="vertexBuffer">The vertex buffer.</param>
        /// <param name="vertexOffset">
        /// The offset (in vertices) from the beginning of the vertex buffer to the first vertex to 
        /// use.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the input layout was changed; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool Set(VertexBuffer vertexBuffer, int vertexOffset)
        {
            Debug.Assert(vertexBuffer != null);
            Debug.Assert(0 <= vertexOffset && vertexOffset < vertexBuffer.VertexCount);

            if (Count == 1
            &&  _bindings[0].VertexBuffer == vertexBuffer
            &&  _bindings[0].VertexOffset == vertexOffset
            &&  _bindings[0].InstanceFrequency == 0)
            {
                return false;
            }

            _bindings[0] = new VertexBufferBinding(vertexBuffer, vertexOffset, 0);

            VertexDeclarations[0] = vertexBuffer.VertexDeclaration;
            InstanceFrequencies[0] = 0;

            if (Count > 1)
            {
                Array.Clear(_bindings, 1, Count - 1);

                Array.Clear(VertexDeclarations, 1, Count - 1);
                Array.Clear(InstanceFrequencies, 1, Count - 1);
            }

            Count = 1;
            return true;
        }

        /// <summary>
        /// Binds the the specified vertex buffers to the input slots.
        /// </summary>
        /// <param name="vertexBufferBindings">The vertex buffer bindings.</param>
        /// <returns>
        /// <see langword="true"/> if the input layout was changed; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool Set(params VertexBufferBinding[] vertexBufferBindings)
        {
            Debug.Assert(vertexBufferBindings != null);
            Debug.Assert(vertexBufferBindings.Length > 0);
            Debug.Assert(vertexBufferBindings.Length <= _bindings.Length);

            bool isDirty = false;
            for (int i = 0; i < vertexBufferBindings.Length; i++)
            {
                Debug.Assert(vertexBufferBindings[i].VertexBuffer != null);

                if (_bindings[i].VertexBuffer != vertexBufferBindings[i].VertexBuffer
                ||  _bindings[i].VertexOffset != vertexBufferBindings[i].VertexOffset
                ||  _bindings[i].InstanceFrequency != vertexBufferBindings[i].InstanceFrequency)
                {
                    _bindings[i] = vertexBufferBindings[i];

                    VertexDeclarations[i] = vertexBufferBindings[i].VertexBuffer.VertexDeclaration;
                    InstanceFrequencies[i] = vertexBufferBindings[i].InstanceFrequency;
                    isDirty = true;
                }
            }

            if (Count > vertexBufferBindings.Length)
            {
                int startIndex = vertexBufferBindings.Length;
                int length = Count - startIndex;
                Array.Clear(_bindings, startIndex, length);

                Array.Clear(VertexDeclarations, startIndex, length);
                Array.Clear(InstanceFrequencies, startIndex, length);
                isDirty = true;
            }

            Count = vertexBufferBindings.Length;
            return isDirty;
        }

        /// <summary>
        /// Gets vertex buffer bound to the specified input slots.
        /// </summary>
        /// <returns>The vertex buffer binding.</returns>
        public VertexBufferBinding Get(int slot)
        {
            Debug.Assert(0 <= slot && slot < Count);
            return _bindings[slot];
        }
    }
}
