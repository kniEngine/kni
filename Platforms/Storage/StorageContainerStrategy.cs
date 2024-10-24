// Copyright (C)2024 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Storage
{
    internal class StorageContainerStrategy : IDisposable
    {
        public StorageContainerStrategy()
        {
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