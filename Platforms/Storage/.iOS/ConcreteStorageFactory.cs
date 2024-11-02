// Copyright (C)2024 Nick Kastellanos

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;

namespace Microsoft.Xna.Platform.Storage
{
    internal sealed class ConcreteStorageFactory : StorageFactory
    {

        public override StorageContainerStrategy CreateStorageContainerStrategy(string name, PlayerIndex? playerIndex)
        {
            return new ConcreteStorageContainer(name, playerIndex);
        }

        public override StorageDeviceStrategy CreateStorageDeviceStrategy(PlayerIndex? player, int directoryCount)
        {
            return new ConcreteStorageDevice(player, directoryCount);
        }

        public override StorageServiceStrategy CreateStorageServiceStrategy()
        {
            return new ConcreteStorageService();
        }

    }
}
