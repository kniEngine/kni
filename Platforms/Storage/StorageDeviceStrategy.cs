// Copyright (C)2024 Nick Kastellanos

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
    }
}