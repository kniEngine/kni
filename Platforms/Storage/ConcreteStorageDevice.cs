// Copyright (C)2024 Nick Kastellanos


namespace Microsoft.Xna.Framework.Storage
{
    internal class ConcreteStorageDevice : StorageDeviceStrategy
    {
        public ConcreteStorageDevice(PlayerIndex? player, int directoryCount) : base(player, directoryCount)
        {
        }
    }
}