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
            throw new PlatformNotSupportedException();
        }

        public override IAsyncResult BeginShowSelector(int sizeInBytes, int directoryCount, AsyncCallback callback, object state)
        {
            throw new PlatformNotSupportedException();
        }

        public override StorageDevice EndShowSelector(IAsyncResult result)
        {
            throw new PlatformNotSupportedException();
        }

        private static StorageDevice Show(PlayerIndex player, int sizeInBytes, int directoryCount)
        {
            throw new PlatformNotSupportedException();
        }

        private static StorageDevice Show(int sizeInBytes, int directoryCount)
        {
            throw new PlatformNotSupportedException();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

        }

    }
}
