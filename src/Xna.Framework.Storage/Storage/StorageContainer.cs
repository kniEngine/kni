// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Platform.Storage;


namespace Microsoft.Xna.Framework.Storage
{
    /// <summary>
    /// Contains a logical collection of files used for user-data storage.
    /// </summary>			
    /// <remarks>MSDN documentation contains related conceptual article: http://msdn.microsoft.com/en-us/library/bb200105.aspx#ID4EDB</remarks>
    public class StorageContainer : IDisposable
    {
        private StorageContainerStrategy _strategy;

        internal StorageContainerStrategy Strategy
        {
            get { return _strategy; }
        }

        bool _isDisposed;

        private readonly StorageDevice _device;

        /// <summary>
        /// Initializes a new instance of the <see cref="Microsoft.Xna.Framework.Storage.StorageContainer"/> class.
        /// </summary>
        /// <param name='device'>The attached storage-device.</param>
        /// <param name='name'> name.</param>
        /// <param name='playerIndex'>The player index of the player to save the data.</param>
        internal StorageContainer(StorageDevice device, string name, PlayerIndex? playerIndex)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("A title name has to be provided in parameter name.");

            this._strategy = StorageFactory.Current.CreateStorageContainerStrategy(name, playerIndex);

            _device = device;
        }

        /// <summary>
        /// Returns display name of the title.
        /// </summary>
        public string DisplayName
        {
            get { return _strategy.DisplayName; }
        }
        
        /// <summary>
        /// Gets a bool value indicating whether the instance has been disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return _isDisposed; }
        }

        /// <summary>
        /// Returns the <see cref="StorageDevice"/> that holds logical files for the container.
        /// </summary>
        public StorageDevice StorageDevice
        {
            get {return _device; }
        }

        // TODO: Implement the Disposing function.  Find sample first

        /// <summary>
        /// Fired when <see cref="Dispose"/> is called or object if finalized or collected by the garbage collector.
        /// </summary>
        public event EventHandler<EventArgs> Disposing;

        /// <summary>
        /// Creates a new directory in the storage-container.
        /// </summary>
        /// <param name="directory">Relative path of the directory to be created.</param>
        public void CreateDirectory(string directory)
        {
            if (string.IsNullOrEmpty(directory))
                throw new ArgumentNullException("Parameter directory must contain a value.");

            _strategy.CreateDirectory(directory);
        }
        
        /// <summary>
        /// Creates a file in the storage-container.
        /// </summary>
        /// <param name="file">Relative path of the file to be created.</param>
        /// <returns>Returns <see cref="Stream"/> for the created file.</returns>
        public Stream CreateFile(string file)
        {
            if (string.IsNullOrEmpty(file))
                throw new ArgumentNullException("Parameter file must contain a value.");

            return _strategy.CreateFile(file);
        }
        
        /// <summary>
        /// Deletes specified directory for the storage-container.
        /// </summary>
        /// <param name="directory">The relative path of the directory to be deleted.</param>
        public void DeleteDirectory(string directory)
        {
            if (string.IsNullOrEmpty(directory))
                throw new ArgumentNullException("Parameter directory must contain a value.");

            _strategy.DeleteDirectory(directory);
        }
        
        /// <summary>
        /// Deletes a file from the storage-container.
        /// </summary>
        /// <param name="file">The relative path of the file to be deleted.</param>
        public void DeleteFile(string file)
        {
            if (string.IsNullOrEmpty(file))
                throw new ArgumentNullException("Parameter file must contain a value.");

            _strategy.DeleteFile(file);
        }

        /// <summary>
        /// Returns true if specified path exists in the storage-container, false otherwise.
        /// </summary>
        /// <param name="directory">The relative path of directory to query for.</param>
        /// <returns>True if queried directory exists, false otherwise.</returns>
        public bool DirectoryExists(string directory)
        {
            if (string.IsNullOrEmpty(directory))
                throw new ArgumentNullException("Parameter directory must contain a value.");

            return _strategy.DirectoryExists(directory);
        }
        
        /// <summary>
        /// Returns true if the specified file exists in the storage-container, false otherwise.
        /// </summary>
        /// <param name="file">The relative path of file to query for.</param>
        /// <returns>True if queried file exists, false otherwise.</returns>
        public bool FileExists(string file)
        {
            if (string.IsNullOrEmpty(file))
                throw new ArgumentNullException("Parameter file must contain a value.");

            return _strategy.FileExists(file);

        }

        /// <summary>
        /// Returns list of directory names in the storage-container.
        /// </summary>
        /// <returns>List of directory names.</returns>
        public string[] GetDirectoryNames()
        {
            return _strategy.GetDirectoryNames();
        }

        /// <summary>
        /// Returns list of directory names with given search pattern.
        /// </summary>
        /// <param name="searchPattern">A search pattern that supports single-character ("?") and multicharacter ("*") wildcards.</param>
        /// <returns>List of matched directory names.</returns>
        public string[] GetDirectoryNames(string searchPattern)
        {
            return _strategy.GetDirectoryNames(searchPattern);
        }

        /// <summary>
        /// Returns list of file names in the storage-container.
        /// </summary>
        /// <returns>List of file names.</returns>
        public string[] GetFileNames()
        {
            return _strategy.GetFileNames();
        }

        /// <summary>
        /// Returns list of file names with given search pattern.
        /// </summary>
        /// <param name="searchPattern">A search pattern that supports single-character ("?") and multicharacter ("*") wildcards.</param>
        /// <returns>List of matched file names.</returns>
        public string[] GetFileNames(string searchPattern)
        {
            if (string.IsNullOrEmpty(searchPattern))
                throw new ArgumentNullException("Parameter searchPattern must contain a value.");

            return _strategy.GetFileNames(searchPattern);
        }

        /// <summary>
        /// Opens a file contained in storage-container.
        /// </summary>
        /// <param name="file">Relative path of the file.</param>
        /// <param name="fileMode"><see cref="FileMode"/> that specifies how the file is opened.</param>
        /// <returns><see cref="Stream"/> object for the opened file.</returns>
        public Stream OpenFile(string file, FileMode fileMode)
        {
            return OpenFile(file, fileMode, FileAccess.ReadWrite, FileShare.ReadWrite);
        }

        /// <summary>
        /// Opens a file contained in storage-container.
        /// </summary>
        /// <param name="file">Relative path of the file.</param>
        /// <param name="fileMode"><see cref="FileMode"/> that specifies how the file is opened.</param>
        /// <param name="fileAccess"><see cref="FileAccess"/> that specifies access mode.</param>
        /// <returns><see cref="Stream"/> object for the opened file.</returns>
        public Stream OpenFile(string file, FileMode fileMode, FileAccess fileAccess)
        {
            return OpenFile(file, fileMode, fileAccess, FileShare.ReadWrite);
        }

        /// <summary>
        /// Opens a file contained in storage-container.
        /// </summary>
        /// <param name="file">Relative path of the file.</param>
        /// <param name="fileMode"><see cref="FileMode"/> that specifies how the file is opened.</param>
        /// <param name="fileAccess"><see cref="FileAccess"/> that specifies access mode.</param>
        /// <param name="fileShare">A bitwise combination of <see cref="FileShare"/> enumeration values that specifies access modes for other stream objects.</param>
        /// <returns><see cref="Stream"/> object for the opened file.</returns>
        public Stream OpenFile(string file, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            if (string.IsNullOrEmpty(file))
                throw new ArgumentNullException("Parameter file must contain a value.");

            return _strategy.OpenFile(file, fileMode, fileAccess, fileShare);
        }


        /// <summary>
        /// Disposes un-managed objects referenced by this object.
        /// </summary>
        public void Dispose()
        {
            _strategy.Dispose();
            _strategy = null;

            // Fill this in when we figure out what we should be disposing
            _isDisposed = true;
        }
    }
}
