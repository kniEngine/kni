// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;

#if NETFX_CORE
using System.Threading.Tasks;
#endif

namespace Microsoft.Xna.Platform.Storage
{
    internal sealed class ConcreteStorageService : StorageServiceStrategy
    {

#if (UAP || WINUI)
        // Dirty trick to avoid the need to get the delegate from the IAsyncResult (can't be done in WinRT)
        static Delegate _showDelegate;
        static Delegate _containerDelegate;
#endif


        internal ConcreteStorageService()
        {
        }

        public override IAsyncResult BeginShowSelector(PlayerIndex player, int sizeInBytes, int directoryCount, AsyncCallback callback, object state)
        {
#if NETFX_CORE
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
#else
            ShowSelectorAsynchronousShow del = new ShowSelectorAsynchronousShow(Show);
  #if (UAP || WINUI)
            _showDelegate = del;
  #endif
            return del.BeginInvoke(player, sizeInBytes, directoryCount, callback, state);
#endif
        }

        public override IAsyncResult BeginShowSelector(int sizeInBytes, int directoryCount, AsyncCallback callback, object state)
        {
#if NETFX_CORE
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
#else
            ShowSelectorAsynchronousShow2 del = new ShowSelectorAsynchronousShow2(Show);

  #if (UAP || WINUI)
            _showDelegate = del;
  #endif
            return del.BeginInvoke(sizeInBytes, directoryCount, callback, state);
#endif
        }

        public override StorageDevice EndShowSelector(IAsyncResult result)
        {
#if NETFX_CORE
            try
            {
                return ((Task<StorageDevice>)result).Result;
            }
            catch (AggregateException ex)
            {
                throw;
            }
#else
            if (!result.IsCompleted)
            {
                try
                {
                    result.AsyncWaitHandle.WaitOne();
                }
                finally
                {
  #if !(UAP || WINUI)
                    result.AsyncWaitHandle.Close();
  #endif
                }
            }

  #if (UAP || WINUI)
            var del = _showDelegate;
            _showDelegate = null;
  #elif NET4_0_OR_GREATER
            // Retrieve the delegate.
            AsyncResult asyncResult = (AsyncResult)result;

            object del = asyncResult.AsyncDelegate;
  #else // NET6_0_OR_GREATER
            object del;
            throw new NotImplementedException();
  #endif

            if (del is ShowSelectorAsynchronousShow)
                return (del as ShowSelectorAsynchronousShow).EndInvoke(result);
            else if (del is ShowSelectorAsynchronousShow2)
                return (del as ShowSelectorAsynchronousShow2).EndInvoke(result);
            else
                throw new ArgumentException("result");
#endif
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
