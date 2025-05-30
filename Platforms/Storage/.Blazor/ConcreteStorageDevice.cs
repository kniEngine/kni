﻿// Copyright (C)2024 Nick Kastellanos

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
                throw new NotImplementedException();
            }
        }

        public override bool IsConnected
        {
            get
            {
                return false;
            }
        }

        public override long TotalSpace
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string GetDevicePath
        {
            get
            {
                throw new NotImplementedException();
            }
        }


        internal ConcreteStorageDevice(PlayerIndex? player, int directoryCount) : base(player, directoryCount)
        {
        }


        public override IAsyncResult BeginOpenContainer(StorageDevice storageDevice, string displayName, AsyncCallback callback, object state)
        {

            throw new NotImplementedException();
        }

        public override StorageContainer EndOpenContainer(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public override void DeleteContainer(string titleName)
        {
            throw new NotImplementedException();
        }

        public override StorageContainer Open(StorageDevice storageDevice, string displayName)
        {
            throw new NotImplementedException();
        }
    }
}