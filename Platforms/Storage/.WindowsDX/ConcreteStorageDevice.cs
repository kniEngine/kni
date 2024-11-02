// Copyright (C)2024 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;


namespace Microsoft.Xna.Platform.Storage
{
    internal sealed class ConcreteStorageDevice : StorageDeviceStrategy
    {

        public override long FreeSpace
        {
            get
            {
                return new DriveInfo(GetDevicePath).AvailableFreeSpace;
            }
        }

        public override bool IsConnected
        {
            get
            {
                return new DriveInfo(GetDevicePath).IsReady;
            }
        }

        public override long TotalSpace
        {
            get
            {
                // Not sure if this should be TotalSize or TotalFreeSize
                return new DriveInfo(GetDevicePath).TotalSize;
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
                    return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }
                else
                {
                    return _storageContainer.Strategy.StoragePath;
                }
            }
        }


        internal ConcreteStorageDevice(PlayerIndex? player, int directoryCount) : base(player, directoryCount)
        {
        }


        public override IAsyncResult BeginOpenContainer(StorageDevice storageDevice, string displayName, AsyncCallback callback, object state)
        {
            try
            {
                ConcreteStorageService.OpenContainerAsynchronous AsynchronousOpen = new ConcreteStorageService.OpenContainerAsynchronous(Open);
                return AsynchronousOpen.BeginInvoke(storageDevice, displayName, callback, state);
            }
            finally
            {
            }
        }

        public override StorageContainer EndOpenContainer(IAsyncResult result)
        {
            StorageContainer returnValue = null;
            try
            {
#if NET4_0_OR_GREATER
                // Retrieve the delegate.
                AsyncResult asyncResult = result as AsyncResult;
                if (asyncResult != null)
                {
                    ConcreteStorageService.OpenContainerAsynchronous asyncDelegate = asyncResult.AsyncDelegate as ConcreteStorageService.OpenContainerAsynchronous;

                    // Wait for the WaitHandle to become signaled.
                    result.AsyncWaitHandle.WaitOne();

                    // Call EndInvoke to retrieve the results.
                    if (asyncDelegate != null)
                        returnValue = asyncDelegate.EndInvoke(result);
                }
#else // NET6_0_OR_GREATER
                throw new NotImplementedException();
#endif
            }
            finally
            {
                // Close the wait handle.
                result.AsyncWaitHandle.Dispose();
            }
            
            return returnValue;
        }

        public override void DeleteContainer(string titleName)
        {
            throw new NotImplementedException();
        }

        public override StorageContainer Open(StorageDevice storageDevice, string displayName)
        {
            StorageContainer storageContainer = new StorageContainer(storageDevice, displayName, _player);
            _storageContainer = storageContainer;

            return storageContainer;
        }
    }
}