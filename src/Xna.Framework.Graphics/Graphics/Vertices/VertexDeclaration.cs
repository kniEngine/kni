// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Platform.Graphics;
using Microsoft.Xna.Platform.Graphics.Utilities;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Defines per-vertex data of a vertex buffer.
    /// </summary>
    /// <remarks>
    /// <see cref="VertexDeclaration"/> implements <see cref="IEquatable{T}"/> and can be used as
    /// a key in a dictionary. Two vertex declarations are considered equal if the vertices are
    /// structurally equivalent, i.e. the vertex elements and the vertex stride are identical. (The
    /// properties <see cref="GraphicsResource.Name"/> and <see cref="GraphicsResource.Tag"/> are
    /// ignored in <see cref="GetHashCode"/> and <see cref="Equals(VertexDeclaration)"/>!)
    /// </remarks>
    public class VertexDeclaration : IEquatable<VertexDeclaration>, IDisposable
        , IPlatformVertexDeclaration
    {

        #region ----- Data shared between structurally identical vertex declarations -----

        private sealed class VertexDeclarationData : IEquatable<VertexDeclarationData>
        {
            private readonly int _hashCode;

            public readonly int VertexStride;
            public VertexElement[] Elements;


            public VertexDeclarationData(int vertexStride, VertexElement[] elements)
            {
                this.VertexStride = vertexStride;
                this.Elements = elements;

                // Pre-calculate hash code for fast comparisons and lookup in dictionaries.
                _hashCode = VertexDeclarationData.CalculateHashCode(this.VertexStride, this.Elements);
            }

            private static int CalculateHashCode(int vertexStride, VertexElement[] elements)
            {
                int hashCode;

                unchecked
                {
                    hashCode = elements[0].GetHashCode();
                    for (int i = 1; i < elements.Length; i++)
                        hashCode = (hashCode * 397) ^ elements[i].GetHashCode();

                    hashCode = (hashCode * 397) ^ elements.Length;
                    hashCode = (hashCode * 397) ^ vertexStride;

                    return hashCode;
                }
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as VertexDeclarationData);
            }

            public bool Equals(VertexDeclarationData other)
            {
                if (ReferenceEquals(null, other))
                    return false;

                if (ReferenceEquals(this, other))
                    return true;

                if (_hashCode != other._hashCode
                || VertexStride != other.VertexStride
                || Elements.Length != other.Elements.Length)
                {
                    return false;
                }

                for (int i = 0; i < Elements.Length; i++)
                {
                    if (!Elements[i].Equals(other.Elements[i]))
                        return false;
                }

                return true;
            }

            public override int GetHashCode()
            {
                return _hashCode;
            }
        }
        #endregion


        #region ----- VertexDeclaration Cache -----

        private static readonly Dictionary<VertexDeclarationData, VertexDeclaration> _vertexDeclarationDataCache = new Dictionary<VertexDeclarationData, VertexDeclaration>();

        internal static VertexDeclaration GetOrCreate(int vertexStride, VertexElement[] elements)
        {
            lock (_vertexDeclarationDataCache)
            {
                VertexDeclarationData data = new VertexDeclarationData(vertexStride, elements);
                VertexDeclaration vertexDeclaration;
                if (_vertexDeclarationDataCache.TryGetValue(data, out vertexDeclaration))
                    return vertexDeclaration;

                // Data.Elements have already been set in the Data ctor. However, entries
                // in the vertex declaration cache must be immutable. Therefore, we create a 
                // copy of the array, which the user cannot access.
                data.Elements = (VertexElement[])elements.Clone();

                vertexDeclaration = new VertexDeclaration(data);
                _vertexDeclarationDataCache[data] = vertexDeclaration;

                return vertexDeclaration;
            }
        }

        private VertexDeclaration(VertexDeclarationData data)
        {
            _data = data;
        }
        #endregion


        private readonly VertexDeclarationData _data;

        /// <summary>
        /// Gets the internal vertex elements array.
        /// </summary>
        /// <value>The internal vertex elements array.</value>
        VertexElement[] IPlatformVertexDeclaration.InternalVertexElements
        {
            get { return _data.Elements; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexDeclaration"/> class.
        /// </summary>
        /// <param name="elements">The vertex elements.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="elements"/> is <see langword="null"/> or empty.
        /// </exception>
        public VertexDeclaration(params VertexElement[] elements)
            : this(CalculateVertexStride(elements), elements)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexDeclaration"/> class.
        /// </summary>
        /// <param name="vertexStride">The size of a vertex (including padding) in bytes.</param>
        /// <param name="elements">The vertex elements.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="elements"/> is <see langword="null"/> or empty.
        /// </exception>
        public VertexDeclaration(int vertexStride, params VertexElement[] elements)
        {
            if ((elements == null) || (elements.Length == 0))
                throw new ArgumentNullException("elements", "Elements cannot be empty");

            lock (_vertexDeclarationDataCache)
            {
                VertexDeclarationData data = new VertexDeclarationData(vertexStride, elements);

                VertexDeclaration vertexDeclaration;
                if (_vertexDeclarationDataCache.TryGetValue(data, out vertexDeclaration))
                {
                    // Reuse existing data.
                    _data = vertexDeclaration._data;
                }
                else
                {
                    // Cache new vertex declaration.
                    data.Elements = (VertexElement[])elements.Clone();
                    _data = data;
                    _vertexDeclarationDataCache[data] = this;
                }
            }
        }

        private static int CalculateVertexStride(VertexElement[] elements)
        {
            int max = 0;
            for (int i = 0; i < elements.Length; i++)
            {
                int start = elements[i].Offset + elements[i].VertexElementFormat.GetSize();
                if (max < start)
                    max = start;
            }

            return max;
        }

        /// <summary>
        /// Returns the VertexDeclaration for Type.
        /// </summary>
        /// <param name="vertexType">A value type which implements the IVertexType interface.</param>
        /// <returns>The VertexDeclaration.</returns>
        /// <remarks>
        /// Prefer to use VertexDeclarationCache when the declaration lookup
        /// can be performed with a templated type.
        /// </remarks>
        internal static VertexDeclaration FromType(Type vertexType)
        {
            if (vertexType == null)
                throw new ArgumentNullException("vertexType", "Cannot be null");

            if (!ReflectionHelpers.IsValueType(vertexType))
            {
                throw new ArgumentException("Must be value type", "vertexType");
            }

            IVertexType type = Activator.CreateInstance(vertexType) as IVertexType;
            if (type == null)
            {
                throw new ArgumentException("vertexData does not inherit IVertexType");
            }

            VertexDeclaration vertexDeclaration = type.VertexDeclaration;
            if (vertexDeclaration == null)
            {
                throw new Exception("VertexDeclaration cannot be null");
            }

            return vertexDeclaration;
        }

        /// <summary>
        /// Gets a copy of the vertex elements.
        /// </summary>
        /// <returns>A copy of the vertex elements.</returns>
        public VertexElement[] GetVertexElements()
        {
            return (VertexElement[])_data.Elements.Clone();
        }

        /// <summary>
        /// Gets the size of a vertex (including padding) in bytes.
        /// </summary>
        /// <value>The size of a vertex (including padding) in bytes.</value>
        public int VertexStride
        {
            get { return _data.VertexStride; }
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>
        /// <see langword="true"/> if the specified <see cref="object"/> is equal to this instance;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as VertexDeclaration);
        }

        /// <summary>
        /// Determines whether the specified <see cref="VertexDeclaration"/> is equal to this
        /// instance.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>
        /// <see langword="true"/> if the specified <see cref="VertexDeclaration"/> is equal to this
        /// instance; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(VertexDeclaration other)
        {
            return other != null && ReferenceEquals(_data, other._data);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data
        /// structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return _data.GetHashCode();
        }

        /// <summary>
        /// Compares two <see cref="VertexElement"/> instances to determine whether they are the
        /// same.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> are
        /// the same; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(VertexDeclaration left, VertexDeclaration right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Compares two <see cref="VertexElement"/> instances to determine whether they are
        /// different.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> are
        /// the different; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator !=(VertexDeclaration left, VertexDeclaration right)
        {
            return !Equals(left, right);
        }


        #region IDisposable Implementation

        ~VertexDeclaration()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

        }

        #endregion IDisposable Implementation

    }
}
