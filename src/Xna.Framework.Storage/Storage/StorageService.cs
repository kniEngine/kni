// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;

namespace Microsoft.Xna.Platform.Storage
{   
    public sealed class StorageService : IDisposable
        , IPlatformStorageService
    {
        private volatile static StorageService _current;
        private StorageServiceStrategy _strategy;

        public readonly static object SyncHandle = new object();

        StorageServiceStrategy IPlatformStorageService.Strategy { get { return _strategy; } }

        public static StorageService Current
        {
            get
            {
                StorageService current = _current;
                if (current != null)
                    return current;

                // Create instance
                lock(SyncHandle)
                {
                    if (_current == null)
                    {   
                        try
                        {
                            _current = new StorageService();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("StorageService has failed to initialize.", ex);
                        }
                    }
                    return _current;
                }
            }
        }

        private StorageService()
        {
            _strategy = StorageFactory.Current.CreateStorageServiceStrategy();
        }

        public IAsyncResult BeginShowSelector(PlayerIndex player, int sizeInBytes, int directoryCount, AsyncCallback callback, object state)
        {
            return _strategy.BeginShowSelector(player, sizeInBytes, directoryCount, callback, state);
        }

        public IAsyncResult BeginShowSelector(int sizeInBytes, int directoryCount, AsyncCallback callback, object state)
        {
            return _strategy.BeginShowSelector(sizeInBytes, directoryCount, callback, state);
        }

        public StorageDevice EndShowSelector(IAsyncResult result)
        {
            return _strategy.EndShowSelector(result);
        }

        #region IDisposable

        private bool isDisposed = false;
        public event EventHandler Disposing;

        ~StorageService()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (isDisposed)
                    return;

                var handler = Disposing;
                if (handler != null)
                    handler(this, EventArgs.Empty);

                _strategy.Dispose();
                _strategy = null;

                isDisposed = true;
            }
            else
            {
                if (isDisposed)
                    return;
                                
                _strategy = null;

                isDisposed = true;
            }
        }


        #endregion // IDisposable
    }
}

