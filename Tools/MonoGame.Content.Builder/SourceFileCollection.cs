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
            var deserializer = new XmlSerializer(typeof(SourceFileCollection));
            try
            {
                using (var textReader = new StreamReader(filePath))
                    return (SourceFileCollection)deserializer.Deserialize(textReader);
            }
            catch (Exception)
            {
            }

            return new SourceFileCollection();
        }

        public void SaveXml(string filePath)
        {
            var serializer = new XmlSerializer(typeof(SourceFileCollection));
            using (var textWriter = new StreamWriter(filePath, false, new UTF8Encoding(false)))
                serializer.Serialize(textWriter, this);            
        }

        public void Merge(SourceFileCollection other)
        {
            foreach (var sourceFile in other.SourceFiles)
            {
                var inContent = SourceFiles.Any(e => string.Equals(e, sourceFile, StringComparison.InvariantCultureIgnoreCase));
                if (!inContent)
                    SourceFiles.Add(sourceFile);
            }

            foreach (var destFile in other.DestFiles)
            {
                var inContent = DestFiles.Any(e => string.Equals(e, destFile, StringComparison.InvariantCultureIgnoreCase));
                if (!inContent)
                    DestFiles.Add(destFile);
            }
        }
    }
}
