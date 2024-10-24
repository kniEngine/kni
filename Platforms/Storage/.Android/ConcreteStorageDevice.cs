// Copyright (C)2024 Nick Kastellanos

using System;
using System.IO;
using System.Threading.Tasks;
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
                    return _storageContainer.Strategy._storagePath;
                }
            }
        }


        public ConcreteStorageDevice(PlayerIndex? player, int directoryCount) : base(player, directoryCount)
        {
        }


        public override IAsyncResult BeginOpenContainer(StorageDevice storageDevice, string displayName, AsyncCallback callback, object state)
        {
            TaskCompletionSource<StorageContainer> tcs = new TaskCompletionSource<StorageContainer>(state);
            Task<StorageContainer> task = Task.Run<StorageContainer>(() => Open(storageDevice, displayName));
            task.ContinueWith((t) =>
            {
                // Copy the task result into the returned task.
                if (t.IsFaulted)
                    tcs.TrySetException(t.Exception.InnerExceptions);
                else if (t.IsCanceled)
                    tcs.TrySetCanceled();
                else
                    tcs.TrySetResult(t.Result);

                // Invoke the user callback if necessary.
                if (callback != null)
                    callback(tcs.Task);
            });
            return tcs.Task;
        }

        public override StorageContainer EndOpenContainer(IAsyncResult result)
        {
            try
            {
                return ((Task<StorageContainer>)result).Result;
            }
            catch (AggregateException ex)
            {
                throw;
            }
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