// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;

namespace Microsoft.Xna.Platform.Storage
{
    public interface IPlatformStorageService
    {
        StorageServiceStrategy Strategy { get; }
    }

    abstract public class StorageServiceStrategy : IDisposable
    {
        public abstract IAsyncResult BeginShowSelector(PlayerIndex player, int sizeInBytes, int directoryCount, AsyncCallback callback, object state);
        public abstract IAsyncResult BeginShowSelector(int sizeInBytes, int directoryCount, AsyncCallback callback, object state);
        public abstract StorageDevice EndShowSelector(IAsyncResult result);

        public T ToConcrete<T>() where T : StorageServiceStrategy
        {
            return (T)this;
        }


        #region IDisposable
        ~StorageServiceStrategy()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);
        #endregion
    }

}
