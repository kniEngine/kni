// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using MonoGame.Framework.Utilities;

#if (UAP || WINUI)
using System.Linq;
using Windows.Storage;
using Windows.Storage.Search;
#endif

namespace Microsoft.Xna.Framework.Storage
{
    //	Implementation on Windows
    //	
    //	User storage is in the My Documents folder of the user who is currently logged in, in the SavedGames folder. 
    //	A subfolder is created for each game according to the titleName passed to the BeginOpenContainer method. 
    //	When no PlayerIndex is specified, content is saved in the AllPlayers folder. When a PlayerIndex is specified, 
    //	the content is saved in the Player1, Player2, Player3, or Player4 folder, depending on which PlayerIndex 
    //	was passed to BeginShowSelector.

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

            this._strategy = new ConcreteStorageContainer(name);

            _device = device;


            // From the examples the root is based on MyDocuments folder
#if (UAP || WINUI)
            string saved = "";
#elif DESKTOPGL
            string saved = "";
            if(CurrentPlatform.OS == OS.Linux || CurrentPlatform.OS == OS.MacOSX)
                saved = StorageDevice.StorageRoot;
            else if(CurrentPlatform.OS == OS.Windows)
                saved = Path.Combine(StorageDevice.StorageRoot, "SavedGames");
            else
                throw new Exception("Unexpected platform!");
#else
            string root = StorageDevice.StorageRoot;
            string saved = Path.Combine(root,"SavedGames");
#endif
            _strategy._storagePath = Path.Combine(saved, name);

            string playerSave = string.Empty;
            if (playerIndex.HasValue)
                playerSave = Path.Combine(_strategy._storagePath, "Player" + (int)playerIndex.Value);
            
            if (!string.IsNullOrEmpty(playerSave))
                _strategy._storagePath = Path.Combine(_strategy._storagePath, "Player" + (int)playerIndex);

#if (UAP || WINUI)
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            var task = folder.CreateFolderAsync(_strategy._storagePath, CreationCollisionOption.OpenIfExists);
            task.AsTask().Wait();
#else
            if (!Directory.Exists(_strategy._storagePath))
            {
                Directory.CreateDirectory(_strategy._storagePath);
            }
#endif
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

            // relative so combine with our path
            string dirPath = Path.Combine(_strategy._storagePath, directory);

#if (UAP || WINUI)
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            var task = folder.CreateFolderAsync(dirPath, CreationCollisionOption.OpenIfExists);
            task.AsTask().Wait();
#else
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
#endif
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

            // relative so combine with our path
            string filePath = Path.Combine(_strategy._storagePath, file);

#if (UAP || WINUI)
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            var awaiter = folder.OpenStreamForWriteAsync(filePath, CreationCollisionOption.ReplaceExisting).GetAwaiter();
            return awaiter.GetResult();
#else
            // return A new file with read/write access.
            return File.Create(filePath);
#endif
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

            // relative so combine with our path
            string dirPath = Path.Combine(_strategy._storagePath, directory);

            // Now let's try to delete itd
#if (UAP || WINUI)
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            var deleteFolder = folder.GetFolderAsync(dirPath).AsTask().GetAwaiter().GetResult();
            deleteFolder.DeleteAsync().AsTask().Wait();
#else
            Directory.Delete(dirPath);
#endif
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

            // relative so combine with our path
            string filePath = Path.Combine(_strategy._storagePath, file);

#if (UAP || WINUI)
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile deleteFile = folder.GetFileAsync(filePath).AsTask().GetAwaiter().GetResult();
            deleteFile.DeleteAsync().AsTask().Wait();
#else
            // Now let's try to delete it
            File.Delete(filePath);
#endif
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

            // relative so combine with our path
            string dirPath = Path.Combine(_strategy._storagePath, directory);

#if (UAP || WINUI)
            StorageFolder folder = ApplicationData.Current.LocalFolder;

            try
            {
                StorageFolder result = folder.GetFolderAsync(dirPath).GetResults();
                return result != null;
            }
            catch
            {
                return false;
            }
#else
            return Directory.Exists(dirPath);
#endif
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

            // relative so combine with our path
            string filePath = Path.Combine(_strategy._storagePath, file);

#if (UAP || WINUI)
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            // GetFile returns an exception if the file doesn't exist, so we catch it here and return the boolean.
            try
            {
                StorageFile existsFile = folder.GetFileAsync(filePath).GetAwaiter().GetResult();
                return existsFile != null;
            }
            catch
            {
                return false;
            }
#else
            // return A new file with read/write access.
            return File.Exists(filePath);
#endif
            return _strategy.FileExists(file);

        }

        /// <summary>
        /// Returns list of directory names in the storage-container.
        /// </summary>
        /// <returns>List of directory names.</returns>
        public string[] GetDirectoryNames()
        {
#if (UAP || WINUI)
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            IReadOnlyList<StorageFolder> results = folder.GetFoldersAsync().AsTask().GetAwaiter().GetResult();
            return results.Select<StorageFolder, string>(e => e.Name).ToArray();
#else
            return Directory.GetDirectories(_strategy._storagePath);
#endif
            return _strategy.GetDirectoryNames();
        }

        /// <summary>
        /// Returns list of directory names with given search pattern.
        /// </summary>
        /// <param name="searchPattern">A search pattern that supports single-character ("?") and multicharacter ("*") wildcards.</param>
        /// <returns>List of matched directory names.</returns>
        public string[] GetDirectoryNames(string searchPattern)
        {
            throw new NotImplementedException();

            return _strategy.GetDirectoryNames(searchPattern);
        }

        /// <summary>
        /// Returns list of file names in the storage-container.
        /// </summary>
        /// <returns>List of file names.</returns>
        public string[] GetFileNames()
        {
#if (UAP || WINUI)
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            var results = folder.GetFilesAsync().AsTask().GetAwaiter().GetResult();
            return results.Select<StorageFile, string>(e => e.Name).ToArray();
#else
            return Directory.GetFiles(_strategy._storagePath);
#endif
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

#if (UAP || WINUI)
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            QueryOptions options = new QueryOptions( CommonFileQuery.DefaultQuery, new [] { searchPattern } );
            StorageFileQueryResult query = folder.CreateFileQueryWithOptions(options);
            IReadOnlyList<StorageFile> files = query.GetFilesAsync().AsTask().GetAwaiter().GetResult();
            return files.Select<StorageFile, string>(e => e.Name).ToArray();
#else
            return Directory.GetFiles(_strategy._storagePath, searchPattern);
#endif
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

            // relative so combine with our path
            string filePath = Path.Combine(_strategy._storagePath, file);

#if (UAP || WINUI)
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            if (fileMode == FileMode.Create || fileMode == FileMode.CreateNew)
            {
                return folder.OpenStreamForWriteAsync(filePath, CreationCollisionOption.ReplaceExisting).GetAwaiter().GetResult();
            }
            else if (fileMode == FileMode.OpenOrCreate)
            {
                if (fileAccess == FileAccess.Read && FileExists(file))
                    return folder.OpenStreamForReadAsync(filePath).GetAwaiter().GetResult();
                else
                {
                    // Not using OpenStreamForReadAsync because the stream position is placed at the end of the file, instead of the beginning
                    StorageFile f = folder.CreateFileAsync(filePath, CreationCollisionOption.OpenIfExists).AsTask().GetAwaiter().GetResult();
                    return f.OpenAsync(FileAccessMode.ReadWrite).AsTask().GetAwaiter().GetResult().AsStream();
                }
            }
            else if (fileMode == FileMode.Truncate)
            {
                return folder.OpenStreamForWriteAsync(filePath, CreationCollisionOption.ReplaceExisting).GetAwaiter().GetResult();
            }
            else
            {
                //if (fileMode == FileMode.Append)
                // Not using OpenStreamForReadAsync because the stream position is placed at the end of the file, instead of the beginning
                folder.CreateFileAsync(filePath, CreationCollisionOption.OpenIfExists).AsTask().GetAwaiter().GetResult().OpenAsync(FileAccessMode.ReadWrite).AsTask().GetAwaiter().GetResult().AsStream();
                StorageFile f = folder.CreateFileAsync(filePath, CreationCollisionOption.OpenIfExists).AsTask().GetAwaiter().GetResult();
                return f.OpenAsync(FileAccessMode.ReadWrite).AsTask().GetAwaiter().GetResult().AsStream();
            }
#else
            return File.Open(filePath, fileMode, fileAccess, fileShare);
#endif

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
