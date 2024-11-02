// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;

namespace Microsoft.Xna.Platform.Storage
{
    public abstract class StorageDeviceStrategy
    {
        private PlayerIndex? _player;
        private int _directoryCount;
        private StorageContainer _storageContainer;


        public virtual StorageContainer StorageContainer
        {
            get { return _storageContainer; }
            set { _storageContainer = value; }
        }

        public virtual PlayerIndex? Player
        {
            get { return _player; }
            set { _player = value; }
        }

        public abstract long FreeSpace { get; }

        public abstract bool IsConnected { get; }

        public abstract long TotalSpace { get; }

        public abstract string GetDevicePath { get; }

        public StorageDeviceStrategy(PlayerIndex? player, int directoryCount)
        {
            this._player = player;
            this._directoryCount = directoryCount;
        }

        public abstract IAsyncResult BeginOpenContainer(StorageDevice storageDevice, string displayName, AsyncCallback callback, object state);
        public abstract StorageContainer EndOpenContainer(IAsyncResult result);
        public abstract void DeleteContainer(string titleName);
        public abstract StorageContainer Open(StorageDevice storageDevice, string displayName);
    }
}