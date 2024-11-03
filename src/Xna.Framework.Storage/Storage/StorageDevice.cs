// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

//using System;
//
//namespace Microsoft.Xna.Framework.Storage
//{
//    public class StorageDevice
//    {
//        public bool IsConnected
//        {
//            get
//            {
//                return true;
//            }
//        }
//
//        public StorageContainer OpenContainer(string containerName)
//        {
//            return new StorageContainer(this,containerName);
//        }
//		
//		public static StorageDevice ShowStorageDeviceGuide()
//		{
//			return new StorageDevice();
//		}
//    }
//}

using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Platform.Storage;

#if NET4_0_OR_GREATER
using System.Runtime.Remoting.Messaging;
#endif

namespace Microsoft.Xna.Platform.Storage
{
    public interface IPlatformStorageDevice
    {
        T GetStrategy<T>() where T : StorageDeviceStrategy;
    }
}

namespace Microsoft.Xna.Framework.Storage
{
        
    /// <summary>
    /// Exposes a storage device for storing user data.
    /// </summary>
    /// <remarks>MSDN documentation contains related conceptual article: http://msdn.microsoft.com/en-us/library/bb200105.aspx</remarks>
    public sealed class StorageDevice : IPlatformStorageDevice
    {
        private StorageDeviceStrategy _strategy;

        T IPlatformStorageDevice.GetStrategy<T>()
        {
            return (T)_strategy;
        }

        /// <summary>
        /// Creates a new <see cref="StorageDevice"/> instance.
        /// </summary>
        /// <param name="player">The playerIndex of the player.</param>
        /// <param name="sizeInBytes">Size of the storage device.</param>
        /// <param name="directoryCount"></param>
        internal StorageDevice(PlayerIndex? player, int sizeInBytes, int directoryCount) 
        {
            this._strategy = StorageFactory.Current.CreateStorageDeviceStrategy(player, directoryCount);
        }
        
        /// <summary>
        /// Returns the amount of free space.
        /// </summary>
        public long FreeSpace
        {
            get { return _strategy.FreeSpace; }
        }

        /// <summary>
        /// Returns true if device is connected, false otherwise.
        /// </summary>
        public bool IsConnected
        {
            get { return _strategy.IsConnected; }
        }

        /// <summary>
        /// Returns the total size of device.
        /// </summary>
        public long TotalSpace
        {
            get { return _strategy.TotalSpace; }
        }

        string GetDevicePath
        {
            get { return _strategy.GetDevicePath; }
        }

        /// <summary>
        /// Fired when a device is removed or inserted.
        /// </summary>
        public static event EventHandler<EventArgs> DeviceChanged;

        // Summary:
        //     Begins the process for opening a StorageContainer containing any files for
        //     the specified title.
        //
        // Parameters:
        //   displayName:
        //     A constant human-readable string that names the file.
        //
        //   callback:
        //     An AsyncCallback that represents the method called when the operation is
        //     complete.
        //
        //   state:
        //     A user-created object used to uniquely identify the request, or null.
        public IAsyncResult BeginOpenContainer(string displayName, AsyncCallback callback, object state)
        {
            return _strategy.BeginOpenContainer(this, displayName, callback, state);
        }
    
        // Summary:
        //     Ends the process for opening a StorageContainer.
        //
        // Parameters:
        //   result:
        //     The IAsyncResult returned from BeginOpenContainer.
        public StorageContainer EndOpenContainer(IAsyncResult result)
        {
            return _strategy.EndOpenContainer(result);
        }			

        // Parameters:
        //   titleName:
        //     The name of the storage container to delete.
        public void DeleteContainer(string titleName)
        {
            _strategy.DeleteContainer(titleName);
        }

        // Private method to handle the creation of the StorageDevice
        private StorageContainer Open(string displayName)
        {
            return _strategy.Open(this, displayName);
        }


        //
        // Summary:
        //     Begins the process for displaying the storage device selector user interface,
        //     and for specifying a callback implemented when the player chooses a device.
        //     Reference page contains links to related code samples.
        //
        // Parameters:
        //   callback:
        //     An AsyncCallback that represents the method called when the player chooses
        //     a device.
        //
        //   state:
        //     A user-created object used to uniquely identify the request, or null.
        public static IAsyncResult BeginShowSelector(AsyncCallback callback, object state)
        {
            return BeginShowSelector(0, 0, callback, state);
        }

        //
        // Summary:
        //     Begins the process for displaying the storage device selector user interface;
        //     specifies the callback implemented when the player chooses a device. Reference
        //     page contains links to related code samples.
        //
        // Parameters:
        //   player:
        //     The PlayerIndex that represents the player who requested the save operation.
        //     On Windows, the only valid option is PlayerIndex.One.
        //
        //   callback:
        //     An AsyncCallback that represents the method called when the player chooses
        //     a device.
        //
        //   state:
        //     A user-created object used to uniquely identify the request, or null.
        public static IAsyncResult BeginShowSelector(PlayerIndex player, AsyncCallback callback, object state)
        {
            return BeginShowSelector(player, 0, 0, callback, state);
        }

        //
        // Summary:
        //     Begins the process for displaying the storage device selector user interface,
        //     and for specifying the size of the data to be written to the storage device
        //     and the callback implemented when the player chooses a device. Reference
        //     page contains links to related code samples.
        //
        // Parameters:
        //   sizeInBytes:
        //     The size, in bytes, of data to write to the storage device.
        //
        //   directoryCount:
        //     The number of directories to write to the storage device.
        //
        //   callback:
        //     An AsyncCallback that represents the method called when the player chooses
        //     a device.
        //
        //   state:
        //     A user-created object used to uniquely identify the request, or null.
        public static IAsyncResult BeginShowSelector(int sizeInBytes, int directoryCount, AsyncCallback callback, object state)
        {
            return StorageService.Current.BeginShowSelector(sizeInBytes, directoryCount, callback, state);
        }
        
        //
        // Summary:
        //     Begins the process for displaying the storage device selector user interface,
        //     for specifying the player who requested the save operation, for setting the
        //     size of data to be written to the storage device, and for naming the callback
        //     implemented when the player chooses a device. Reference page contains links
        //     to related code samples.
        //
        // Parameters:
        //   player:
        //     The PlayerIndex that represents the player who requested the save operation.
        //     On Windows, the only valid option is PlayerIndex.One.
        //
        //   sizeInBytes:
        //     The size, in bytes, of the data to write to the storage device.
        //
        //   directoryCount:
        //     The number of directories to write to the storage device.
        //
        //   callback:
        //     An AsyncCallback that represents the method called when the player chooses
        //     a device.
        //
        //   state:
        //     A user-created object used to uniquely identify the request, or null.
        public static IAsyncResult BeginShowSelector(PlayerIndex player, int sizeInBytes, int directoryCount, AsyncCallback callback, object state)
        {
            return StorageService.Current.BeginShowSelector(player, sizeInBytes, directoryCount, callback, state);
        }
    
        //
        // Summary:
        //     Ends the display of the storage selector user interface. Reference page contains
        //     links to related code samples.
        //
        // Parameters:
        //   result:
        //     The IAsyncResult returned from BeginShowSelector.
        public static StorageDevice EndShowSelector(IAsyncResult result) 
        {
            return StorageService.Current.EndShowSelector(result);
        }
    }
}
