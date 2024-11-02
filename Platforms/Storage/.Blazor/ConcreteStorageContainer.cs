// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;

namespace Microsoft.Xna.Platform.Storage
{

    internal sealed class ConcreteStorageContainer : StorageContainerStrategy
    {
        public override string DisplayName
        {
            get { return base.DisplayName; }
        }

        internal ConcreteStorageContainer(string name, PlayerIndex? playerIndex) : base(name)
        {
            throw new NotImplementedException();
        }


        public override void CreateDirectory(string directory)
        {
            throw new NotImplementedException();
        }

        public override Stream CreateFile(string file)
        {
            throw new NotImplementedException();
        }

        public override void DeleteDirectory(string directory)
        {
            throw new NotImplementedException();
        }

        public override void DeleteFile(string file)
        {
            throw new NotImplementedException();
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