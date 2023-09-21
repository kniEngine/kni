// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Platform.Graphics
{

    // Holds information for caching
    internal class BufferBindingInfo
    {
        public VertexDeclarationAttributeInfo AttributeInfo;
        public IntPtr VertexOffset;
        public int InstanceFrequency;
        public int Vbo;

        public BufferBindingInfo(VertexDeclarationAttributeInfo attributeInfo, IntPtr vertexOffset, int instanceFrequency, int vbo)
        {
            AttributeInfo = attributeInfo;
            VertexOffset = vertexOffset;
            InstanceFrequency = instanceFrequency;
            Vbo = vbo;
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
                    foreach (var item in array)
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
        public VertexAttribPointerType VertexAttribPointerType;
        public bool Normalized;
        public int Offset;
    }
}
