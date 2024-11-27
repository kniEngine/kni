// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace Microsoft.Xna.Framework.Content.Pipeline.Builder
{
    public sealed class PackageReferencesCollection
    {
        public const string Extension = ".kniContent";
       
        public List<Package> Packages { get; set; }


        public PackageReferencesCollection()
        {
            Packages = new List<Package>();
        }

        public void SaveBinary(string filePath)
        {
            using (Stream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new PackageReferencesCollectionBinaryWriter(stream))
            {
                writer.Write(this);
            }
        }

        public static PackageReferencesCollection LoadBinary(string filePath)
        {
            try
            {
                using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                using (var writer = new PackageReferencesCollectionBinaryReader(stream))
                {
                    PackageReferencesCollection result = new PackageReferencesCollection();
                    writer.Read(result);
                    return result;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public int PackagesCount { get { return this.Packages.Count; } }

        internal void AddPackage(Package package)
        {
            this.Packages.Add(package);
        }

        internal class PackageReferencesCollectionBinaryWriter : BinaryWriter
        {
            private const string Header = "KNIC"; // content db
            private const short MajorVersion =  3;
            private const short MinorVersion = 15;
            private const int DataType = 3; // PackageReferencesCollection data


            public PackageReferencesCollectionBinaryWriter(Stream output) : base(output)
            {
            }

            internal void Write(PackageReferencesCollection value)
            {
                Write((byte)Header[0]);
                Write((byte)Header[1]);
                Write((byte)Header[2]);
                Write((byte)Header[3]);
                Write((Int16)MajorVersion);
                Write((Int16)MinorVersion);
                Write((Int32)DataType);
                Write((Int32)0); // reserved


                WritePackedInt(value.Packages.Count);
                for (int i = 0; i < value.Packages.Count; i++)
                {
                    Write(value.Packages[i].Name);
                    Write(value.Packages[i].Version);
                }

                return;
            }

            protected void WritePackedInt(int value)
            {
                // write zigzag encoded int
                int zzint = ((value << 1) ^ (value >> 31));
                Write7BitEncodedInt(zzint);
            }

            private void WriteStringOrNull(string value)
            {
                if (value != null)
                {
                    Write(true);
                    Write(value);
                }
                else
                    Write(false);
            }

        }

        internal class PackageReferencesCollectionBinaryReader : BinaryReader
        {
            private const string Header = "KNIC"; // content db
            private const short MajorVersion =  3;
            private const short MinorVersion = 15;
            private const int DataType = 3; // PackageReferencesCollection data


            public PackageReferencesCollectionBinaryReader(Stream output) : base(output)
            {
            }

            internal void Read(PackageReferencesCollection value)
            {
                if (ReadByte() != Header[0]
                ||  ReadByte() != Header[1]
                ||  ReadByte() != Header[2]
                ||  ReadByte() != Header[3])
                    throw new Exception("Invalid file.");

                if (ReadInt16() != MajorVersion
                ||  ReadInt16() != MinorVersion)
                    throw new Exception("Invalid file version.");

                int dataType = ReadInt32(); 
                if (dataType != DataType)
                    throw new Exception("Invalid data type.");

                int reserved0 = ReadInt32();

                int packagesCount = ReadPackedInt();
                value.Packages = new List<Package>(packagesCount);
                for (int i = 0; i < packagesCount; i++)
                {
                    Package package;
                    package.Name = ReadString();
                    package.Version = ReadString();
                    value.Packages.Add(package);
                }

                return;
            }

            private int ReadPackedInt()
            {
                unchecked
                {
                    // read zigzag encoded int
                    int zzint = Read7BitEncodedInt();
                    return ((int)((uint)zzint >> 1) ^ (-(zzint & 1)));
                }
            }

            private string ReadStringOrNull()
            {
                if (ReadBoolean())
                    return ReadString();
                else
                    return null;
            }

        }
    }
}
