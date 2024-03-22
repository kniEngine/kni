// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline.Utilities;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    /// <summary>
    /// Provides an implementation for many of the ContentCompiler methods including compilation, state tracking for shared resources and creation of the header type manifest.
    /// </summary>
    /// <remarks>A new ContentWriter is constructed for each compilation operation.</remarks>
    public sealed class ContentWriter : BinaryWriter
    {
        ContentCompiler _compiler;
        TargetPlatform _targetPlatform;
        GraphicsProfile _targetProfile;
        string _rootDirectory;
        string _referenceRelocationPath;

        List<ContentTypeWriter> _typeWriters = new List<ContentTypeWriter>();
        Dictionary<Type, int> _typeWriterMap = new Dictionary<Type, int>();
        Dictionary<Type, ContentTypeWriter> _typeMap = new Dictionary<Type, ContentTypeWriter>();

        List<object> _sharedResources = new List<object>();
        Dictionary<object, int> _sharedResourceMap = new Dictionary<object, int>();

        internal IList<object> SharedResources  { get {return _sharedResources; } }

        internal ICollection<ContentTypeWriter> TypeWriters { get {return _typeWriters; } }

        /// <summary>
        /// Gets the content build target platform.
        /// </summary>
        public TargetPlatform TargetPlatform { get { return _targetPlatform; } }

        /// <summary>
        /// Gets or sets the target graphics profile.
        /// </summary>
        public GraphicsProfile TargetProfile { get { return _targetProfile; } }

        /// <summary>
        /// Creates a new instance of ContentWriter.
        /// </summary>
        /// <param name="compiler">The compiler object that created this writer.</param>
        /// <param name="output">The stream to write the XNB file to.</param>
        /// <param name="targetPlatform">The platform the XNB is intended for.</param>
        /// <param name="targetProfile">The graphics profile of the target.</param>
        /// <param name="rootDirectory">The root directory of the content.</param>
        /// <param name="referenceRelocationPath">The path of the XNB file, used to calculate relative paths for external references.</param>
        internal ContentWriter(ContentCompiler compiler, Stream output, TargetPlatform targetPlatform, GraphicsProfile targetProfile, string rootDirectory, string referenceRelocationPath)
            : base(output)
        {
            this._compiler = compiler;
            this._targetPlatform = targetPlatform;
            this._targetProfile = targetProfile;
            this._rootDirectory = rootDirectory;

            // Normalize the directory format so PathHelper.GetRelativePath will compute external references correctly.
            this._referenceRelocationPath = PathHelper.NormalizeDirectory(referenceRelocationPath);
        }

        /// <summary>
        /// Releases the resources used by the IDisposable class.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Write all shared resources at the end of the file.
        /// </summary>
        internal void WriteSharedResources(IList<object> sharedResources)
        {
            for (int i = 0; i < _sharedResources.Count; i++)
            {
                object resource = _sharedResources[i];
                WriteObject<object>(resource);
            }
        }


        /// <summary>
        /// Gets a ContentTypeWriter for the given type.
        /// </summary>
        /// <param name="type">The type of the object to write.</param>
        /// <returns>The ContentTypeWriter for the type.</returns>
        internal ContentTypeWriter GetTypeWriter(Type type)
        {
            ContentTypeWriter typeWriter = null;
            if (_typeMap.TryGetValue(type, out typeWriter))
                return typeWriter;

            int index = _typeWriters.Count;
            typeWriter = _compiler.GetTypeWriter(type);

            _typeWriters.Add(typeWriter);
            if (!_typeWriterMap.ContainsKey(typeWriter.GetType()))
                _typeWriterMap.Add(typeWriter.GetType(), index);

            _typeMap.Add(type, typeWriter);

            typeWriter.OnAddedToContentWriter(this);

            return typeWriter;
        }

        /// <summary>
        /// Writes the name of an external file to the output binary.
        /// </summary>
        /// <typeparam name="T">The type of reference.</typeparam>
        /// <param name="reference">External reference to a data file for the content item.</param>
        public void WriteExternalReference<T>(ExternalReference<T> reference)
        {
            if (reference == null)
            {
                Write(string.Empty);
            }
            else
            {
                string fileName = reference.Filename;
                if (string.IsNullOrEmpty(fileName))
                {
                    Write(string.Empty);
                }
                else
                {
                    // Make sure the filename ends with .xnb
                    if (!fileName.EndsWith(".xnb"))
                        throw new ArgumentException(string.Format("ExternalReference '{0}' must reference a .xnb file", fileName));
                    // Make sure it is in the same root directory
                    if (!fileName.StartsWith(_rootDirectory, StringComparison.OrdinalIgnoreCase))
                        throw new ArgumentException(string.Format("ExternalReference '{0}' must be in the root directory '{1}'", fileName, _rootDirectory));
                    // Strip the .xnb extension
                    fileName = fileName.Substring(0, fileName.Length - 4);
                    // Get the relative directory
                    fileName = PathHelper.GetRelativePath(_referenceRelocationPath, fileName);
                    Write(fileName);
                }
            }
        }

        /// <summary>
        /// Writes a single object preceded by a type identifier to the output binary.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="value">The value to write.</param>
        /// <remarks>This method can be called recursively with a null value.</remarks>
        public void WriteObject<T>(T value)
        {
            if (value == null)
                Write7BitEncodedInt(0);
            else
            {
                ContentTypeWriter typeWriter = GetTypeWriter(value.GetType());

                // Because zero means null object, we add one to 
                // the index before writing it to the file.
                int index = _typeWriterMap[typeWriter.GetType()];
                Write7BitEncodedInt(index + 1);

                typeWriter.InternalWrite(this, value);
            }
        }

        /// <summary>
        /// Writes a single object to the output binary, using the specified type hint and writer worker.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="value">The value to write.</param>
        /// <param name="typeWriter">The content type writer.</param>
        /// <remarks>The type hint should be retrieved from the Initialize method of the ContentTypeWriter
        /// that is calling WriteObject, by calling GetTypeWriter and passing it the type of the field used
        /// to hold the value being serialized.
        /// </remarks>
        public void WriteObject<T>(T value, ContentTypeWriter typeWriter)
        {
            if (typeWriter == null)
                throw new ArgumentNullException("typeWriter");

            if (typeWriter.TargetType.IsValueType)
                typeWriter.InternalWrite(this, value);
            else
                WriteObject(value);
        }

        /// <summary>
        /// Writes a single object to the output binary as an instance of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="value">The value to write.</param>
        /// <remarks>If you specify a base class of the actual object value only data from this base type
        /// will be written. This method does not write any type identifier so it cannot support null or
        /// polymorphic values, and the reader must specify an identical type while loading the compiled data.</remarks>
        public void WriteRawObject<T>(T value)
        {
            WriteRawObject<T>(value, GetTypeWriter(typeof(T)));
        }

        /// <summary>
        /// Writes a single object to the output binary using the specified writer worker.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="value">The value to write.</param>
        /// <param name="typeWriter">The writer worker. This should be looked up from the Initialize method
        /// of the ContentTypeWriter that is calling WriteRawObject, by calling GetTypeWriter.</param>
        /// <remarks>WriteRawObject does not write any type identifier, so it cannot support null or polymorphic
        /// values, and the reader must specify an identical type while loading the compiled data.</remarks>
        public void WriteRawObject<T>(T value, ContentTypeWriter typeWriter)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            if (typeWriter == null)
                throw new ArgumentNullException("typeWriter");

            typeWriter.InternalWrite(this, value);
        }

        /// <summary>
        /// Adds a shared reference to the output binary and records the object to be serialized later.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="value">The object to record.</param>
        public void WriteSharedResource<T>(T value)
        {
            int index = GetSharedResourceIndex(value);
            Write7BitEncodedInt(index);
        }

        private int GetSharedResourceIndex(object value)
        {
            if (value != null)
            {
                int index;
                if (!_sharedResourceMap.TryGetValue(value, out index))
                {
                    // Add it to the list of shared resources
                    _sharedResources.Add(value);
                    index = _sharedResources.Count - 1;

                    _sharedResourceMap.Add(value, index);
                }
                return (index + 1); // Because zero means null value, we add one to the index
            }
            else
            {
                return 0; // Zero means a null value
            }
        }

        /// <summary>
        /// Writes a Complex value.
        /// </summary>
        /// <param name="value">Value of a color using Red, Green, Blue, and Alpha values to write.</param>
        public void Write(Complex value)
        {
            Write(value.R);
            Write(value.i);
        }

        /// <summary>
        /// Writes a Matrix value.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void Write(Matrix value)
        {
            Write(value.M11);
            Write(value.M12);
            Write(value.M13);
            Write(value.M14);
            Write(value.M21);
            Write(value.M22);
            Write(value.M23);
            Write(value.M24);
            Write(value.M31);
            Write(value.M32);
            Write(value.M33);
            Write(value.M34);
            Write(value.M41);
            Write(value.M42);
            Write(value.M43);
            Write(value.M44);
        }

        /// <summary>
        /// Writes a Quaternion value.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void Write(Quaternion value)
        {
            Write(value.X);
            Write(value.Y);
            Write(value.Z);
            Write(value.W);
        }

        /// <summary>
        /// Writes a Vector2 value.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void Write(Vector2 value)
        {
            Write(value.X);
            Write(value.Y);
        }

        /// <summary>
        /// Writes a Vector3 value.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void Write(Vector3 value)
        {
            Write(value.X);
            Write(value.Y);
            Write(value.Z);
        }

        /// <summary>
        /// Writes a Vector4 value.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void Write(Vector4 value)
        {
            Write(value.X);
            Write(value.Y);
            Write(value.Z);
            Write(value.W);
        }


        /// <summary>
        /// Helper for checking if a type can be deserialized into an existing object.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type can be deserialized into an existing object.</returns>
        internal bool CanDeserializeIntoExistingObject(Type type)
        {
            ContentTypeWriter typeWriter = _compiler.GetTypeWriter(type);
            return typeWriter != null && typeWriter.CanDeserializeIntoExistingObject;
        }
    }
}
