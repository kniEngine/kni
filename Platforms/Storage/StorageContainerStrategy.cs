// Copyright (C)2024 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Storage
{
    internal class StorageContainerStrategy : IDisposable
    {
        internal /*readonly*/ string _storagePath;
        private readonly string _name;

        public virtual string DisplayName
        {
            get { return _name; }
        }

        public StorageContainerStrategy(string name)
        {
            _name = name;
        }


        #region IDisposable
        ~StorageContainerStrategy()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

        }
        #endregion
    }
}