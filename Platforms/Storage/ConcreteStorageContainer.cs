// Copyright (C)2024 Nick Kastellanos

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Storage
{
    internal class ConcreteStorageContainer : StorageContainerStrategy
    {
        public override string DisplayName
        {
            get { return base.DisplayName; }
        }

        public ConcreteStorageContainer(string name) : base(name)
        {
        }


        public override void CreateDirectory(string directory)
        {
        }

        public override Stream CreateFile(string file)
        {
            throw new NotImplementedException();
        }

        public override void DeleteDirectory(string directory)
        {
        }

        public override void DeleteFile(string file)
        {
        }

        public override bool DirectoryExists(string directory)
        {
            throw new NotImplementedException();
        }

        public override bool FileExists(string file)
        {
            throw new NotImplementedException();
        }

        public override string[] GetDirectoryNames()
        {
            throw new NotImplementedException();
        }

        public override string[] GetDirectoryNames(string searchPattern)
        {
            throw new NotImplementedException();
        }

        public override string[] GetFileNames()
        {
            throw new NotImplementedException();
        }

        public override string[] GetFileNames(string searchPattern)
        {
            throw new NotImplementedException();
        }

        public override Stream OpenFile(string file, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            throw new NotImplementedException();
        }

    }
}