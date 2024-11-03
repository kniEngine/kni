// Copyright (C)2024 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;

using Windows.Storage;

#if NETFX_CORE
using System.Threading.Tasks;
#endif


namespace Microsoft.Xna.Platform.Storage
{
    internal sealed class ConcreteStorageDevice : StorageDeviceStrategy
    {

        public override long FreeSpace
        {
            get
            {
                return long.MaxValue;
            }
        }

        public override bool IsConnected
        {
            get
            {
                return true;
            }
        }

        public override long TotalSpace
        {
            get
            {
                return long.MaxValue;
            }
        }

        public override string GetDevicePath
        {
            get
            {
                // We may not need to store the StorageContainer in the future
                // when we get DeviceChanged events working.
                if (StorageContainer == null)
                {
                    return ApplicationData.Current.LocalFolder.Path;
                }
                else
                {
                    return ((IPlatformStorageContainer)StorageContainer).GetStrategy<StorageContainerStrategy>().StoragePath;
                }
            }
        }


        internal ConcreteStorageDevice(PlayerIndex? player, int directoryCount) : base(player, directoryCount)
        {
        }


        public override IAsyncResult BeginOpenContainer(StorageDevice storageDevice, string displayName, AsyncCallback callback, object state)
        {
#if NETFX_CORE
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
                ConcreteStorageService.OpenContainerAsynchronous AsynchronousOpen = new ConcreteStorageService.OpenContainerAsynchronous(Open);
#if (UAP || WINUI)
                _containerDelegate = AsynchronousOpen;
#endif
                return AsynchronousOpen.BeginInvoke(storageDevice, displayName, callback, state);
            }
            finally
            {
            }
#endif
        }

        public override StorageContainer EndOpenContainer(IAsyncResult result)
        {
#if NETFX_CORE
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
#if (UAP || WINUI)
                // AsyncResult does not exist in WinRT
                ConcreteStorageService.OpenContainerAsynchronous asyncResult = _containerDelegate as ConcreteStorageService.OpenContainerAsynchronous;
                if (asyncResult != null)
                {
                    // Wait for the WaitHandle to become signaled.
                    result.AsyncWaitHandle.WaitOne();

                    // Call EndInvoke to retrieve the results.
                    returnValue = asyncResult.EndInvoke(result);
                }
                _containerDelegate = null;
#elif NET4_0_OR_GREATER
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
#endif
        }

        public override void DeleteContainer(string titleName)
        {
            throw new NotImplementedException();
        }

        public override StorageContainer Open(StorageDevice storageDevice, string displayName)
        {
            StorageContainer storageContainer = base.CreateStorageContainer(storageDevice, displayName, Player);
            StorageContainer = storageContainer;

            return storageContainer;
        }
    }
}