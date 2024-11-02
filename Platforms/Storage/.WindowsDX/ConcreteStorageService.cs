// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;

namespace Microsoft.Xna.Platform.Storage
{
    internal sealed class ConcreteStorageService : StorageServiceStrategy
    {

        internal ConcreteStorageService()
        {
        }

        public override IAsyncResult BeginShowSelector(PlayerIndex player, int sizeInBytes, int directoryCount, AsyncCallback callback, object state)
        {
            ShowSelectorAsynchronousShow del = new ShowSelectorAsynchronousShow(Show);
            return del.BeginInvoke(player, sizeInBytes, directoryCount, callback, state);
        }

        public override IAsyncResult BeginShowSelector(int sizeInBytes, int directoryCount, AsyncCallback callback, object state)
        {
            ShowSelectorAsynchronousShow2 del = new ShowSelectorAsynchronousShow2(Show);
            return del.BeginInvoke(sizeInBytes, directoryCount, callback, state);
        }

        public override StorageDevice EndShowSelector(IAsyncResult result)
        {
            if (!result.IsCompleted)
            {
                try
                {
                    result.AsyncWaitHandle.WaitOne();
                }
                finally
                {
                }
            }

  #if NET4_0_OR_GREATER
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
        }

        // The MonoTouch AOT cannot deal with nullable types in a delegate
        // (or at least not the straightforward implementation), so we
        // define two delegate types.
        public delegate StorageDevice ShowSelectorAsynchronousShow(PlayerIndex player, int sizeInBytes, int directoryCount);
        public delegate StorageDevice ShowSelectorAsynchronousShow2(int sizeInBytes, int directoryCount);

        public delegate StorageContainer OpenContainerAsynchronous(StorageDevice storageDevice, string displayName);


        private StorageDevice Show(PlayerIndex player, int sizeInBytes, int directoryCount)
        {
            return base.CreateStorageDevice(player, sizeInBytes, directoryCount);
        }

        private StorageDevice Show(int sizeInBytes, int directoryCount)
        {
            return base.CreateStorageDevice(null, sizeInBytes, directoryCount);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

        }

    }
}
