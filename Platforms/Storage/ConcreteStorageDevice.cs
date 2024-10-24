// Copyright (C)2024 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;

namespace Microsoft.Xna.Platform.Storage
{
    internal class ConcreteStorageDevice : StorageDeviceStrategy
    {

        public override long FreeSpace
        {
            get
            {
#if (UAP || WINUI)
                return long.MaxValue;
#else
                return new DriveInfo(GetDevicePath).AvailableFreeSpace;
#endif
            }
        }

        public override bool IsConnected
        {
            get
            {
#if (UAP || WINUI)
                return true;
#else
                return new DriveInfo(GetDevicePath).IsReady;
#endif
            }
        }

        public override long TotalSpace
        {
            get
            {
#if (UAP || WINUI)
                return long.MaxValue;
#else
                // Not sure if this should be TotalSize or TotalFreeSize
                return new DriveInfo(GetDevicePath).TotalSize;
#endif
            }
        }

        public override string GetDevicePath
        {
            get
            {
                // We may not need to store the StorageContainer in the future
                // when we get DeviceChanged events working.
                if (_storageContainer == null)
                {
                    return StorageDevice.StorageRoot;
                }
                else
                {
                    return _storageContainer.Strategy._storagePath;
                }
            }
        }


        public ConcreteStorageDevice(PlayerIndex? player, int directoryCount) : base(player, directoryCount)
        {
        }


        public override IAsyncResult BeginOpenContainer(string displayName, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public override StorageContainer Open(string displayName)
        {
            throw new NotImplementedException();
        }

        public override void DeleteContainer(string titleName)
        {
        }

        public override StorageContainer EndOpenContainer(IAsyncResult result)
        {
            throw new NotImplementedException();
        }
    }
}