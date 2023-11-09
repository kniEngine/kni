// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Builder
{
    [XmlRoot(ElementName = "SourceFileCollection")]
    public sealed class SourceFileCollection
    {
        public static readonly string XmlExtension = ".mgcontent";

        public GraphicsProfile Profile { get; set; }

        public TargetPlatform Platform { get; set; }

        public string Config { get; set; }

        [XmlArrayItem("File")]
        public List<string> SourceFiles { get; set; }

        [XmlArrayItem("File")]
        public List<string> DestFiles { get; set; }


        public SourceFileCollection()
        {
            SourceFiles = new List<string>();
            DestFiles = new List<string>();
            Config = string.Empty;
        }

        static public SourceFileCollection LoadXml(string filePath)
        {
            try
            {
                using (var textReader = new StreamReader(filePath))
                {
                    XmlSerializer deserializer = new XmlSerializer(typeof(SourceFileCollection));
                    SourceFileCollection result = (SourceFileCollection)deserializer.Deserialize(textReader);

                    if (result.DestFiles.Count != result.SourceFiles.Count)
                        return null; // file is invalid

                    return result;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void SaveXml(string filePath)
        {
            using (var textWriter = new StreamWriter(filePath, false, new UTF8Encoding(false)))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SourceFileCollection));
                serializer.Serialize(textWriter, this);
            }
        }

        public int SourceFilesCount { get { return this.SourceFiles.Count; } }

        internal void AddFile(string sourceFile, string outputFile)
        {
            this.SourceFiles.Add(sourceFile);
            this.DestFiles.Add(outputFile);
        }

        public void Merge(SourceFileCollection previousFileCollection)
        {
            for (int i = 0; i < previousFileCollection.SourceFiles.Count; i++)
            {
                string prevSourceFile = previousFileCollection.SourceFiles[i];
                string prevDestFile = previousFileCollection.DestFiles[i];

                bool contains = this.SourceFiles.Exists((sourceFile) => string.Equals(sourceFile, prevSourceFile, StringComparison.InvariantCultureIgnoreCase));
                if (!contains)
                {
                    this.SourceFiles.Add(prevSourceFile);
                    this.DestFiles.Add(prevDestFile);
                }
            }
        }
    }
}
