// Copyright (C)2024 Nick Kastellanos

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Storage
{
    internal class StorageContainerStrategy : IDisposable
    {
        internal /*readonly*/ string _storagePath;
        private readonly string _name;

        public virtual string DisplayName
        {
            get { return _name; }
        }

        public StorageContainerStrategy(string name)
        {
            _name = name;
        }

        public virtual void CreateDirectory(string directory)
        {
        }

        public virtual Stream CreateFile(string file)
        {
            throw new NotImplementedException();
        }

        public virtual void DeleteDirectory(string directory)
        {
        }

        public virtual void DeleteFile(string file)
        {
        }

        public virtual bool DirectoryExists(string directory)
        {
            throw new NotImplementedException();
        }

        public virtual bool FileExists(string file)
        {
            throw new NotImplementedException();
        }

        public virtual string[] GetDirectoryNames()
        {
            throw new NotImplementedException();
        }

        public virtual string[] GetDirectoryNames(string searchPattern)
        {
            throw new NotImplementedException();
        }

        public virtual string[] GetFileNames()
        {
            throw new NotImplementedException();
        }

        public virtual string[] GetFileNames(string searchPattern)
        {
            throw new NotImplementedException();
        }

        public virtual Stream OpenFile(string file, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            throw new NotImplementedException();
        }

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