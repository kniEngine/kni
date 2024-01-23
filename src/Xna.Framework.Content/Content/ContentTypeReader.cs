// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content
{
    public abstract class ContentTypeReader
    {
        private Type _targetType;

        public virtual bool CanDeserializeIntoExistingObject
        {
            get { return false; }
        }

        public Type TargetType
        {
            get { return _targetType; }
        }

        public virtual int TypeVersion
        {
            get { return 0; }   // The default version (unless overridden) is zero
        }

        protected ContentTypeReader(Type targetType)
        {
            _targetType = targetType;
        }

        protected internal virtual void Initialize(ContentTypeReaderManager manager)
        {
            // Do nothing. Are we supposed to add ourselves to the manager?
        }

        /// <param name="existingInstance">The object receiving the data, or null if a new instance of the object should be created.</param>
        protected internal abstract object Read(ContentReader input, object existingInstance);
    }

    public abstract class ContentTypeReader<T> : ContentTypeReader
    {
        protected ContentTypeReader()
            : base(typeof(T))
        {
            // Nothing
        }

        /// <inheritdoc/>
        protected internal override object Read(ContentReader input, object existingInstance)
        {
            if (existingInstance == null)
            {
                return Read(input, default(T));
            } 
            return Read(input, (T)existingInstance);
        }

        protected internal abstract T Read(ContentReader input, T existingInstance);
    }
}