﻿// Copyright (C)2024 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;

namespace Microsoft.Xna.Platform.Storage
{
    public abstract class StorageContainerStrategy : IDisposable
    {
        private string _storagePath;
        private readonly string _name;

        public virtual string DisplayName
        {
            get { return _name; }
        }

        public virtual string StoragePath
        {
            get { return _storagePath; }
            set { _storagePath = value; }
        }

        public StorageContainerStrategy(string name)
        {
            _name = name;
        }

        public abstract void CreateDirectory(string directory);
        public abstract Stream CreateFile(string file);
        public abstract void DeleteDirectory(string directory);
        public abstract void DeleteFile(string file);
        public abstract bool DirectoryExists(string directory);
        public abstract bool FileExists(string file);
        public abstract string[] GetDirectoryNames();
        public abstract string[] GetDirectoryNames(string searchPattern);
        public abstract string[] GetFileNames();
        public abstract string[] GetFileNames(string searchPattern);
        public abstract Stream OpenFile(string file, FileMode fileMode, FileAccess fileAccess, FileShare fileShare);


        #region IDisposable
        ~StorageContainerStrategy()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

        }

        #endregion
    }
}