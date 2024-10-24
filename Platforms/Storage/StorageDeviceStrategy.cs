// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;

namespace Microsoft.Xna.Platform.Storage
{
    internal abstract class StorageDeviceStrategy
    {
        internal PlayerIndex? _player;
        int _directoryCount;
        internal StorageContainer _storageContainer;

        public abstract long FreeSpace { get; }

        public abstract bool IsConnected { get; }

        public abstract long TotalSpace { get; }

        public abstract string GetDevicePath { get; }

        public StorageDeviceStrategy(PlayerIndex? player, int directoryCount)
        {
            this._player = player;
            this._directoryCount = directoryCount;
        }

        public abstract IAsyncResult BeginOpenContainer(string displayName, AsyncCallback callback, object state);
        public abstract StorageContainer Open(string displayName);
        public abstract void DeleteContainer(string titleName);
        public abstract StorageContainer EndOpenContainer(IAsyncResult result);
    }
}