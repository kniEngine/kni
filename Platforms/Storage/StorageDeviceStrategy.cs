// Copyright (C)2024 Nick Kastellanos

namespace Microsoft.Xna.Framework.Storage
{
    internal class StorageDeviceStrategy
    {
        internal PlayerIndex? _player;
        int _directoryCount;
        internal StorageContainer _storageContainer;

        public StorageDeviceStrategy(PlayerIndex? player, int directoryCount)
        {
            this._player = player;
            this._directoryCount = directoryCount;
        }
    }
}