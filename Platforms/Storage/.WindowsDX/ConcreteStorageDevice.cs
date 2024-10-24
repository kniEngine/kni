// Copyright (C)2024 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;

#if ANDROID || IOS || TVOS
using System.Threading.Tasks;
#endif


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
#if ANDROID || IOS || TVOS
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
#else
            try
            {
                OpenContainerAsynchronous AsynchronousOpen = new OpenContainerAsynchronous(Open);
                return AsynchronousOpen.BeginInvoke(storageDevice, displayName, callback, state);
            }
            finally
            {
            }
#endif
        }

        public override StorageContainer EndOpenContainer(IAsyncResult result)
        {
#if ANDROID || IOS || TVOS
            try
            {
                return ((Task<StorageContainer>)result).Result;
            }
            catch (AggregateException ex)
            {
                throw;
            }
#else
            StorageContainer returnValue = null;
            try
            {
#if NET4_0_OR_GREATER
                // Retrieve the delegate.
                AsyncResult asyncResult = result as AsyncResult;
                if (asyncResult != null)
                {
                    OpenContainerAsynchronous asyncDelegate = asyncResult.AsyncDelegate as OpenContainerAsynchronous;

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
#endif
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