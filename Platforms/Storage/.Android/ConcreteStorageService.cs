// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;

using System.Threading.Tasks;

namespace Microsoft.Xna.Platform.Storage
{
    internal sealed class ConcreteStorageService : StorageServiceStrategy
    {

        internal ConcreteStorageService()
        {
        }

        public override IAsyncResult BeginShowSelector(PlayerIndex player, int sizeInBytes, int directoryCount, AsyncCallback callback, object state)
        {
            TaskCompletionSource<StorageDevice> tcs = new TaskCompletionSource<StorageDevice>(state);
            Task<StorageDevice> task = Task.Run<StorageDevice>(() => Show(player, sizeInBytes, directoryCount));
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

        public override IAsyncResult BeginShowSelector(int sizeInBytes, int directoryCount, AsyncCallback callback, object state)
        {
            TaskCompletionSource<StorageDevice> tcs = new TaskCompletionSource<StorageDevice>(state);
            Task<StorageDevice> task = Task.Run<StorageDevice>(() => Show(sizeInBytes, directoryCount));
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

        public override StorageDevice EndShowSelector(IAsyncResult result)
        {
            try
            {
                return ((Task<StorageDevice>)result).Result;
            }
            catch (AggregateException ex)
            {
                throw;
            }
        }

        // The MonoTouch AOT cannot deal with nullable types in a delegate
        // (or at least not the straightforward implementation), so we
        // define two delegate types.
        public delegate StorageDevice ShowSelectorAsynchronousShow(PlayerIndex player, int sizeInBytes, int directoryCount);
        public delegate StorageDevice ShowSelectorAsynchronousShow2(int sizeInBytes, int directoryCount);

        public delegate StorageContainer OpenContainerAsynchronous(StorageDevice storageDevice, string displayName);


        private static StorageDevice Show(PlayerIndex player, int sizeInBytes, int directoryCount)
        {
            return new StorageDevice(player, sizeInBytes, directoryCount);
        }

        private static StorageDevice Show(int sizeInBytes, int directoryCount)
        {
            return new StorageDevice(null, sizeInBytes, directoryCount);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

        }

    }
}
