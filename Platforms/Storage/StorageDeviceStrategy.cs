// Copyright (C)2024 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Storage
{
    internal class StorageDeviceStrategy
    {
        internal PlayerIndex? _player;
        int _directoryCount;
        internal StorageContainer _storageContainer;

        public virtual long FreeSpace
        {
            get;
        }

        public virtual bool IsConnected
        {
            get;
        }

        public virtual long TotalSpace
        {
            get;
        }

        public virtual string GetDevicePath
        {
            get;
        }

        public StorageDeviceStrategy(PlayerIndex? player, int directoryCount)
        {
            this._player = player;
            this._directoryCount = directoryCount;
        }

        public virtual IAsyncResult BeginOpenContainer(string displayName, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public virtual StorageContainer Open(string displayName)
        {
            throw new NotImplementedException();
        }

        public virtual void DeleteContainer(string titleName)
        {
            throw new NotImplementedException();
        }

        public virtual StorageContainer EndOpenContainer(IAsyncResult result)
        {
            throw new NotImplementedException();
        }
    }
}