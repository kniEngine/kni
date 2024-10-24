﻿// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;

using MonoGame.Framework.Utilities;

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
            string saved = "";

            switch (CurrentPlatform.OS)
            {
                case OS.Windows:
                    {
                        saved = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        saved = Path.Combine(saved, "SavedGames");
                    }
                    break;
                case OS.Linux:
                    {
                        saved = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
                        if (String.IsNullOrEmpty(saved))
                        {
                            saved = Environment.GetEnvironmentVariable("HOME");
                            if (!String.IsNullOrEmpty(saved))
                                saved += "/.local/share";
                            else
                                saved = ".";
                        }
                    }
                    break;
                case OS.MacOSX:
                    {
                        saved = Environment.GetEnvironmentVariable("HOME");
                        if (!String.IsNullOrEmpty(saved))
                            saved += "/Library/Application Support";
                        else
                            saved = ".";
                    }
                    break;

                default:
                    throw new Exception("Unexpected platform.");
            }
            _storagePath = Path.Combine(saved, name);

            string playerSave = string.Empty;
            if (playerIndex.HasValue)
                playerSave = Path.Combine(_storagePath, "Player" + (int)playerIndex.Value);
            
            if (!string.IsNullOrEmpty(playerSave))
                _storagePath = Path.Combine(_storagePath, "Player" + (int)playerIndex);

            if (!Directory.Exists(_storagePath))
                Directory.CreateDirectory(_storagePath);
        }


        public override void CreateDirectory(string directory)
        {
            // relative so combine with our path
            string dirPath = Path.Combine(_storagePath, directory);

            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
        }

        public override Stream CreateFile(string file)
        {
            // relative so combine with our path
            string filePath = Path.Combine(_storagePath, file);

            // return A new file with read/write access.
            return File.Create(filePath);
        }

        public override void DeleteDirectory(string directory)
        {
            // relative so combine with our path
            string dirPath = Path.Combine(_storagePath, directory);

            // Now let's try to delete it
            Directory.Delete(dirPath);
        }

        public override void DeleteFile(string file)
        {
            // relative so combine with our path
            string filePath = Path.Combine(_storagePath, file);

            // Now let's try to delete it
            File.Delete(filePath);
        }

        public override bool DirectoryExists(string directory)
        {
            // relative so combine with our path
            string dirPath = Path.Combine(_storagePath, directory);

            return Directory.Exists(dirPath);
        }

        public override bool FileExists(string file)
        {
            // relative so combine with our path
            string filePath = Path.Combine(_storagePath, file);

            // return A new file with read/write access.
            return File.Exists(filePath);
        }

        public override string[] GetDirectoryNames()
        {
            return Directory.GetDirectories(_storagePath);
        }

        public override string[] GetDirectoryNames(string searchPattern)
        {
            throw new NotImplementedException();
        }

        public override string[] GetFileNames()
        {
            return Directory.GetFiles(_storagePath);
        }

        public override string[] GetFileNames(string searchPattern)
        {
            return Directory.GetFiles(_storagePath, searchPattern);
        }

        public override Stream OpenFile(string file, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            // relative so combine with our path
            string filePath = Path.Combine(_storagePath, file);

            return File.Open(filePath, fileMode, fileAccess, fileShare);
        }

    }
}