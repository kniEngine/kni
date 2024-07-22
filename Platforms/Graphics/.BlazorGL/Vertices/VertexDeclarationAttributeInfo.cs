// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics;
using nkast.Wasm.Canvas.WebGL;

namespace Microsoft.Xna.Platform.Graphics
{

    // Holds information for caching
    internal class BufferBindingInfo
    {
        private WeakReference _vertexBufferStrategyRef = new WeakReference(null);

        public VertexDeclarationAttributeInfo AttributeInfo;

        public VertexBuffer VertexBuffer
        {
            get { return (VertexBuffer)_vertexBufferStrategyRef.Target; }
            set { _vertexBufferStrategyRef.Target = value; }
        }
        public IntPtr VertexOffset;
        public int InstanceFrequency;


        public BufferBindingInfo(VertexDeclarationAttributeInfo attributeInfo, VertexBuffer vertexBuffer, IntPtr vertexOffset, int instanceFrequency)
        {
            AttributeInfo = attributeInfo;

            VertexBuffer = vertexBuffer;
            VertexOffset = vertexOffset;
            InstanceFrequency = instanceFrequency;
        }

        public override string ToString()
        {
            return String.Format("{{VertexBuffer: {0}, VertexOffset: {1}, InstanceFrequency: {2}}}",
                VertexBuffer, VertexOffset, InstanceFrequency);
        }
    }

    internal class RenderTargetBindingArrayComparer : IEqualityComparer<RenderTargetBinding[]>
    {
        public bool Equals(RenderTargetBinding[] first, RenderTargetBinding[] second)
        {
            if (object.ReferenceEquals(first, second))
                return true;

            if (first == null || second == null)
                return false;

            if (first.Length != second.Length)
                return false;

            for (int i = 0; i < first.Length; i++)
            {
                if ((first[i].RenderTarget != second[i].RenderTarget) || (first[i].ArraySlice != second[i].ArraySlice))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(RenderTargetBinding[] array)
        {
            if (array != null)
            {
                unchecked
                {
                    int hash = 17;
                    foreach (RenderTargetBinding item in array)
                    {
                        if (item.RenderTarget != null)
                            hash = hash * 23 + item.RenderTarget.GetHashCode();
                        hash = hash * 23 + item.ArraySlice.GetHashCode();
                    }
                    return hash;
                }
            }
            return 0;
        }
    }

    /// <summary>
    /// Vertex attribute information for a particular shader/vertex declaration combination.
    /// </summary>
    internal class VertexDeclarationAttributeInfo
    {
        internal bool[] EnabledAttributes;
        internal List<VertexDeclarationAttributeInfoElement> Elements;

        internal VertexDeclarationAttributeInfo(int maxVertexAttributes)
        {
            EnabledAttributes = new bool[maxVertexAttributes];
            Elements = new List<VertexDeclarationAttributeInfoElement>();
        }
    }

    internal class VertexDeclarationAttributeInfoElement
    {
        public int AttributeLocation;
        public int NumberOfElements;
        public WebGLDataType VertexAttribPointerType;
        public bool Normalized;
        public int Offset;
    }
}
