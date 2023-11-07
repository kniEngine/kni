// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            XmlSerializer deserializer = new XmlSerializer(typeof(SourceFileCollection));
            try
            {
                using (var textReader = new StreamReader(filePath))
                    return (SourceFileCollection)deserializer.Deserialize(textReader);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void SaveXml(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SourceFileCollection));
            using (var textWriter = new StreamWriter(filePath, false, new UTF8Encoding(false)))
                serializer.Serialize(textWriter, this);            
        }

        internal void AddFile(string sourceFile, string outputFile)
        {
            this.SourceFiles.Add(sourceFile);
            this.DestFiles.Add(outputFile);
        }

        public void Merge(SourceFileCollection other)
        {
            foreach (string sourceFile in other.SourceFiles)
            {
                bool inContent = SourceFiles.Any(e => string.Equals(e, sourceFile, StringComparison.InvariantCultureIgnoreCase));
                if (!inContent)
                    SourceFiles.Add(sourceFile);
            }

            foreach (string destFile in other.DestFiles)
            {
                bool inContent = DestFiles.Any(e => string.Equals(e, destFile, StringComparison.InvariantCultureIgnoreCase));
                if (!inContent)
                    DestFiles.Add(destFile);
            }
        }
    }
}
