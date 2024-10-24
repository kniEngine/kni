// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;

namespace Microsoft.Xna.Platform.Storage
{
    internal class ConcreteStorageDevice : StorageDeviceStrategy
    {

        public override long FreeSpace
        { 
            get;
        }

        public override bool IsConnected 
        {
            get;
        }

        public override long TotalSpace
        { 
            get;
        }

        public override string GetDevicePath
        {
            get;
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