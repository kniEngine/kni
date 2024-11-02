// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;

namespace Microsoft.Xna.Platform.Storage
{
    //	Implementation on Windows
    //	
    //	User storage is in the My Documents folder of the user who is currently logged in, in the SavedGames folder. 
    //	A subfolder is created for each game according to the titleName passed to the BeginOpenContainer method. 
    //	When no PlayerIndex is specified, content is saved in the AllPlayers folder. When a PlayerIndex is specified, 
    //	the content is saved in the Player1, Player2, Player3, or Player4 folder, depending on which PlayerIndex 
    //	was passed to BeginShowSelector.

    internal sealed class ConcreteStorageContainer : StorageContainerStrategy
    {
        public override string DisplayName
        {
            get { return base.DisplayName; }
        }

        internal ConcreteStorageContainer(string name, PlayerIndex? playerIndex) : base(name)
        {
            string saved = "";

            string root = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            saved = Path.Combine(root,"SavedGames");

            StoragePath = Path.Combine(saved, name);

            string playerSave = string.Empty;
            if (playerIndex.HasValue)
                playerSave = Path.Combine(StoragePath, "Player" + (int)playerIndex.Value);
            
            if (!string.IsNullOrEmpty(playerSave))
                StoragePath = Path.Combine(StoragePath, "Player" + (int)playerIndex);

            if (!Directory.Exists(StoragePath))
                Directory.CreateDirectory(StoragePath);
        }


        public override void CreateDirectory(string directory)
        {
            // relative so combine with our path
            string dirPath = Path.Combine(StoragePath, directory);

            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
        }

        public override Stream CreateFile(string file)
        {
            // relative so combine with our path
            string filePath = Path.Combine(StoragePath, file);

            // return A new file with read/write access.
            return File.Create(filePath);
        }

        public override void DeleteDirectory(string directory)
        {
            // relative so combine with our path
            string dirPath = Path.Combine(StoragePath, directory);

            // Now let's try to delete it
            Directory.Delete(dirPath);
        }

        public override void DeleteFile(string file)
        {
            // relative so combine with our path
            string filePath = Path.Combine(StoragePath, file);

            // Now let's try to delete it
            File.Delete(filePath);
        }

        public override bool DirectoryExists(string directory)
        {
            // relative so combine with our path
            string dirPath = Path.Combine(StoragePath, directory);

            return Directory.Exists(dirPath);
        }

        public override bool FileExists(string file)
        {
            // relative so combine with our path
            string filePath = Path.Combine(StoragePath, file);

            // return A new file with read/write access.
            return File.Exists(filePath);
        }

        public override string[] GetDirectoryNames()
        {
            return Directory.GetDirectories(StoragePath);
        }

        public override string[] GetDirectoryNames(string searchPattern)
        {
            throw new NotImplementedException();
        }

        public override string[] GetFileNames()
        {
            return Directory.GetFiles(StoragePath);
        }

        public override string[] GetFileNames(string searchPattern)
        {
            return Directory.GetFiles(StoragePath, searchPattern);
        }

        public override Stream OpenFile(string file, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            // relative so combine with our path
            string filePath = Path.Combine(StoragePath, file);

            return File.Open(filePath, fileMode, fileAccess, fileShare);
        }

    }
}