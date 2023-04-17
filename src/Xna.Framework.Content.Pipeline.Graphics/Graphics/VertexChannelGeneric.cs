// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides methods and properties for maintaining a vertex channel.
    /// This is a generic implementation of VertexChannel and, therefore, can handle strongly typed content data.
    /// </summary>
    public sealed class VertexChannel<T> : VertexChannel, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
    {
        List<T> _items;

        /// <summary>
        /// Gets the strongly-typed list for the base class to access.
        /// </summary>
        internal override IList Items
        {
            get { return _items; }
        }

        /// <summary>
        /// Gets the type of data contained in this channel.
        /// </summary>
        public override Type ElementType
        {
            get { return typeof(T); }
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        public new T this[int index]
        {
            get { return _items[index]; }
            set { _items[index] = value; }
        }

        /// <summary>
        /// true if this object is read-only; false otherwise.
        /// </summary>
        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Creates an instance of VertexChannel.
        /// </summary>
        /// <param name="name">Name of the channel.</param>
        internal VertexChannel(string name)
            : base(name)
        {
            _items = new List<T>();
        }

        /// <summary>
        /// Determines whether the specified element is in the channel.
        /// </summary>
        /// <param name="item">Element being searched for.</param>
        /// <returns>true if the element is present; false otherwise.</returns>
        public bool Contains(T item)
        {
            return _items.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the channel to an array, starting at the specified index.
        /// </summary>
        /// <param name="array">Array that will receive the copied channel elements.</param>
        /// <param name="arrayIndex">Starting index for copy operation.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets an enumerator interface for reading channel content.
        /// </summary>
        /// <returns>Enumeration of the channel content.</returns>
        public new IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        /// <summary>
        /// Gets the index of the specified item.
        /// </summary>
        /// <param name="item">Item whose index is to be retrieved.</param>
        /// <returns>Index of specified item.</returns>
        public int IndexOf(T item)
        {
            return _items.IndexOf(item);
        }

        /// <summary>
        /// Inserts the range of values from the enumerable into the channel.
        /// </summary>
        /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
        /// <param name="data">The data to insert into the channel.</param>
        internal override void InsertRange(int index, IEnumerable data)
        {
            if ((index < 0) || (index > _items.Count))
                throw new ArgumentOutOfRangeException("index");
            if (data == null)
                throw new ArgumentNullException("data");
            if (!(data is IEnumerable<T>))
                throw new ArgumentException("data");
            _items.InsertRange(index, (IEnumerable<T>)data);
        }

        /// <summary>
        /// Reads channel content and automatically converts it to the specified vector format.
        /// </summary>
        /// <typeparam name="TargetType">Target vector format for the converted channel data.</typeparam>
        /// <returns>The converted channel data.</returns>
        public override IEnumerable<TargetType> ReadConvertedContent<TargetType>()
        {
            if (typeof(TargetType).IsAssignableFrom(typeof(T)))
                return _items.Cast<TargetType>();

            return Convert<TargetType>(_items);
        }

        private static IEnumerable<TargetType> Convert<TargetType>(List<T> items)
        {
            // The following formats are supported:
            // - Single
            // - Vector2 Structure
            // - Vector3 Structure
            // - Vector4 Structure
            // - Any implementation of IPackedVector Interface.

            Converter<T, Vector4> vconverter4 = VectorConverter.GetConverter<T, Vector4>();
            Converter<Vector4, TargetType> converter = VectorConverter.GetConverter<Vector4, TargetType>();

            foreach (T item in items)
            {
                Vector4 vec4 = vconverter4(item);
                yield return (TargetType)converter(vec4);
            }
        }

        /// <summary>
        /// Adds a new element to the end of the collection.
        /// </summary>
        /// <param name="value">The element to add.</param>
        void ICollection<T>.Add(T value)
        {
            ((ICollection<T>)Items).Add(value);
        }

        /// <summary>
        /// Removes all elements from the collection.
        /// </summary>
        void ICollection<T>.Clear()
        {
            Items.Clear();
        }

        /// <summary>
        /// Removes a specified element from the collection.
        /// </summary>
        /// <param name="value">The element to remove.</param>
        /// <returns>true if the channel was removed; false otherwise.</returns>
        bool ICollection<T>.Remove(T value)
        {
            return ((ICollection<T>)Items).Remove(value);
        }

        /// <summary>
        /// Inserts an element into the collection at the specified position.
        /// </summary>
        /// <param name="index">Index at which to insert the element.</param>
        /// <param name="value">The element to insert.</param>
        void IList<T>.Insert(int index, T value)
        {
            Items.Insert(index, value);
        }

        /// <summary>
        /// Removes the element at the specified index position.
        /// </summary>
        /// <param name="index">Index of the element to remove.</param>
        void IList<T>.RemoveAt(int index)
        {
            Items.RemoveAt(index);
        }

        /// <summary>
        /// Removes a range of values from the channel.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range of elements to remove.</param>
        /// <param name="count"> The number of elements to remove.</param>
        internal override void RemoveRange(int index, int count)
        {
            _items.RemoveRange(index, count);
        }
    }
}
