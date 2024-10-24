// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;

#if DESKTOPGL
using MonoGame.Framework.Utilities;
#endif

#if (UAP || WINUI)
using System.Linq;
using Windows.Storage;
using Windows.Storage.Search;
#endif

namespace Microsoft.Xna.Platform.Storage
{
    //	Implementation on Windows
    //	
    //	User storage is in the My Documents folder of the user who is currently logged in, in the SavedGames folder. 
    //	A subfolder is created for each game according to the titleName passed to the BeginOpenContainer method. 
    //	When no PlayerIndex is specified, content is saved in the AllPlayers folder. When a PlayerIndex is specified, 
    //	the content is saved in the Player1, Player2, Player3, or Player4 folder, depending on which PlayerIndex 
    //	was passed to BeginShowSelector.

    internal class ConcreteStorageContainer : StorageContainerStrategy
    {
        public override string DisplayName
        {
            get { return base.DisplayName; }
        }

        public ConcreteStorageContainer(string name, PlayerIndex? playerIndex) : base(name)
        {
            // From the examples the root is based on MyDocuments folder
#if (UAP || WINUI)
            string saved = "";
            // ?? saved = StorageDevice.GetStorageRootUAP();
#elif DESKTOPGL
            string saved = "";
            if (CurrentPlatform.OS == OS.Linux)
            {
                saved = StorageDevice.GetStorageRootDESKTOPGL();
            }
            else if(CurrentPlatform.OS == OS.MacOSX)
            {
                saved = StorageDevice.GetStorageRootDESKTOPGL();
            }
            else if (CurrentPlatform.OS == OS.Windows)
            {
                saved = StorageDevice.GetStorageRootDESKTOPGL();

                saved = Path.Combine(saved, "SavedGames");
            }
            else
                throw new Exception("Unexpected platform!");
#else
            string root = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string saved = Path.Combine(root,"SavedGames");
#endif
            _storagePath = Path.Combine(saved, name);

            string playerSave = string.Empty;
            if (playerIndex.HasValue)
                playerSave = Path.Combine(_storagePath, "Player" + (int)playerIndex.Value);
            
            if (!string.IsNullOrEmpty(playerSave))
                _storagePath = Path.Combine(_storagePath, "Player" + (int)playerIndex);

#if (UAP || WINUI)
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            var task = folder.CreateFolderAsync(_storagePath, CreationCollisionOption.OpenIfExists);
            task.AsTask().Wait();
#else
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
#endif
        }


        public override void CreateDirectory(string directory)
        {
            // relative so combine with our path
            string dirPath = Path.Combine(_storagePath, directory);

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
        }

        public override Stream CreateFile(string file)
        {
            // relative so combine with our path
            string filePath = Path.Combine(_storagePath, file);

#if (UAP || WINUI)
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            var awaiter = folder.OpenStreamForWriteAsync(filePath, CreationCollisionOption.ReplaceExisting).GetAwaiter();
            return awaiter.GetResult();
#else
            // return A new file with read/write access.
            return File.Create(filePath);
#endif
        }

        public override void DeleteDirectory(string directory)
        {
            // relative so combine with our path
            string dirPath = Path.Combine(_storagePath, directory);

            // Now let's try to delete itd
#if (UAP || WINUI)
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            var deleteFolder = folder.GetFolderAsync(dirPath).AsTask().GetAwaiter().GetResult();
            deleteFolder.DeleteAsync().AsTask().Wait();
#else
            Directory.Delete(dirPath);
#endif
        }

        public override void DeleteFile(string file)
        {
            // relative so combine with our path
            string filePath = Path.Combine(_storagePath, file);

#if (UAP || WINUI)
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile deleteFile = folder.GetFileAsync(filePath).AsTask().GetAwaiter().GetResult();
            deleteFile.DeleteAsync().AsTask().Wait();
#else
            // Now let's try to delete it
            File.Delete(filePath);
#endif
        }

        public override bool DirectoryExists(string directory)
        {
            // relative so combine with our path
            string dirPath = Path.Combine(_storagePath, directory);

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
        }

        public override bool FileExists(string file)
        {
            // relative so combine with our path
            string filePath = Path.Combine(_storagePath, file);

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
        }

        public override string[] GetDirectoryNames()
        {
#if (UAP || WINUI)
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            IReadOnlyList<StorageFolder> results = folder.GetFoldersAsync().AsTask().GetAwaiter().GetResult();
            return results.Select<StorageFolder, string>(e => e.Name).ToArray();
#else
            return Directory.GetDirectories(_storagePath);
#endif
        }

        public override string[] GetDirectoryNames(string searchPattern)
        {
            throw new NotImplementedException();
        }

        public override string[] GetFileNames()
        {
#if (UAP || WINUI)
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            var results = folder.GetFilesAsync().AsTask().GetAwaiter().GetResult();
            return results.Select<StorageFile, string>(e => e.Name).ToArray();
#else
            return Directory.GetFiles(_storagePath);
#endif
        }

        public override string[] GetFileNames(string searchPattern)
        {
#if (UAP || WINUI)
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            QueryOptions options = new QueryOptions( CommonFileQuery.DefaultQuery, new [] { searchPattern } );
            StorageFileQueryResult query = folder.CreateFileQueryWithOptions(options);
            IReadOnlyList<StorageFile> files = query.GetFilesAsync().AsTask().GetAwaiter().GetResult();
            return files.Select<StorageFile, string>(e => e.Name).ToArray();
#else
            return Directory.GetFiles(_storagePath, searchPattern);
#endif
        }

        public override Stream OpenFile(string file, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            // relative so combine with our path
            string filePath = Path.Combine(_storagePath, file);

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
        }

    }
}