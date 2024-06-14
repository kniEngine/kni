// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Platform.Graphics
{
    public class VertexInputLayoutKey : IEquatable<VertexInputLayoutKey>
    {
        public int Count { get; protected set; }
        public VertexDeclaration[] VertexDeclarations { get; protected set; }
        public int[] InstanceFrequencies { get; protected set; }

        internal VertexInputLayoutKey()
        {
            Count = 0;
            VertexDeclarations = null;
            InstanceFrequencies = null;
        }


        internal void Set(VertexBufferCollection vertexBuffers)
        {
            this.VertexDeclarations = vertexBuffers.VertexDeclarations;
            this.InstanceFrequencies = vertexBuffers.InstanceFrequencies;
            this.Count = vertexBuffers.Count;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as VertexInputLayoutKey);
        }

        public bool Equals(VertexInputLayoutKey other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            if (Count != other.Count)
                return false;

            for (int i = 0; i < Count; i++)
            {
                Debug.Assert(VertexDeclarations[i] != null);
                if (!VertexDeclarations[i].Equals(other.VertexDeclarations[i]))
                    return false;
            }

            for (int i = 0; i < Count; i++)
            {
                if (!InstanceFrequencies[i].Equals(other.InstanceFrequencies[i]))
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 0;
                if (Count > 0)
                {
                    hashCode = VertexDeclarations[0].GetHashCode();
                    hashCode = (hashCode * 397) ^ InstanceFrequencies[0];
                    for (int i = 1; i < Count; i++)
                    {
                        hashCode = (hashCode * 397) ^ VertexDeclarations[i].GetHashCode();
                        hashCode = (hashCode * 397) ^ InstanceFrequencies[i];
                    }
                }
                return hashCode;
            }
        }

        /// <summary>
        /// Create an 'ImmutableVertexInputLayout' that can be used as a key in the 'InputLayoutCache'.
        /// </summary>
        /// <returns></returns>
        internal ImmutableVertexInputLayoutKey CreateImmutable()
        {
            VertexDeclaration[] vertexDeclarations = new VertexDeclaration[this.Count];
            int[] instanceFrequencies = new int[this.Count];

            Array.Copy(this.VertexDeclarations, vertexDeclarations, vertexDeclarations.Length);
            Array.Copy(this.InstanceFrequencies, instanceFrequencies, instanceFrequencies.Length);

            return new ImmutableVertexInputLayoutKey(vertexDeclarations, instanceFrequencies, this.Count);
        }

        public static bool operator ==(VertexInputLayoutKey left, VertexInputLayoutKey right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(VertexInputLayoutKey left, VertexInputLayoutKey right)
        {
            return !Equals(left, right);
        }
    }


    internal sealed class ImmutableVertexInputLayoutKey : VertexInputLayoutKey
    {
        private readonly int _hashCode;

        internal ImmutableVertexInputLayoutKey(VertexDeclaration[] vertexDeclarations, int[] instanceFrequencies, int count)
        {
            Debug.Assert(vertexDeclarations != null);
            Debug.Assert(instanceFrequencies != null);
            Debug.Assert(count >= 0);
            Debug.Assert(vertexDeclarations.Length == count);
            Debug.Assert(vertexDeclarations.Length == instanceFrequencies.Length);

            Count = count;
            VertexDeclarations = vertexDeclarations;
            InstanceFrequencies = instanceFrequencies;

            // Pre-calculate hash code for fast lookup in dictionary.
            _hashCode = base.GetHashCode();
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }
    }
}
