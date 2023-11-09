// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Content.Pipeline;


namespace Microsoft.Xna.Framework.Content.Pipeline.Builder
{
    public class PipelineBuildEvent
    {
        public const string Extension = ".kniContent";
        public static readonly string XmlExtension = ".mgcontent";

        private static readonly OpaqueDataDictionary EmptyParameters = new OpaqueDataDictionary();

        public PipelineBuildEvent()
        {
            SourceFile = string.Empty;
            DestFile = string.Empty;
            Importer = string.Empty;
            Processor = string.Empty;
            Parameters = new OpaqueDataDictionary();
            XmlParameters = new List<XmlParameter>();
            Dependencies = new List<string>();
            BuildAsset = new List<string>();
            BuildOutput = new List<string>();
        }

        /// <summary>
        /// Absolute path to the source file.
        /// </summary>
        public string SourceFile { get; set; }

        /// <summary>
        /// The date/time stamp of the source file.
        /// </summary>
        public DateTime SourceTime { get; set; }

        /// <summary>
        /// Absolute path to the output file.
        /// </summary>
        public string DestFile { get; set; }

        /// <summary>
        /// The date/time stamp of the destination file.
        /// </summary>
        public DateTime DestTime { get; set; }

        public string Importer { get; set; }

        /// <summary>
        /// The date/time stamp of the DLL containing the importer.
        /// </summary>
        public DateTime ImporterTime { get; set; }

        public string Processor { get; set; }

        /// <summary>
        /// The date/time stamp of the DLL containing the processor.
        /// </summary>
        public DateTime ProcessorTime { get; set; }

        [XmlIgnore]
        public OpaqueDataDictionary Parameters { get; set; }

        public class XmlParameter
        {
            public string Key { get; set; }
            public string Value { get; set; }

            public XmlParameter()
            {
            }

            public XmlParameter(string key, string value)
            {
                this.Key = key;
                this.Value = value;
            }
        }

        [XmlElement("Parameters")]
        public List<XmlParameter> XmlParameters { get; set; }

        /// <summary>
        /// Gets or sets the dependencies.
        /// </summary>
        /// <value>The dependencies.</value>
        /// <remarks>
        /// Dependencies are extra files that are required in addition to the <see cref="SourceFile"/>.
        /// Dependencies are added using <see cref="ContentProcessorContext.AddDependency"/>. Changes
        /// to the dependent file causes a rebuilt of the content.
        /// </remarks>
        public List<string> Dependencies { get; set; }

        /// <summary>
        /// Gets or sets the additional (nested) assets.
        /// </summary>
        /// <value>The additional (nested) assets.</value>
        /// <remarks>
        /// <para>
        /// Additional assets are built by using an <see cref="ExternalReference{T}"/> and calling
        /// <see cref="ContentProcessorContext.BuildAndLoadAsset{TInput,TOutput}(ExternalReference{TInput},string)"/>
        /// or <see cref="ContentProcessorContext.BuildAsset{TInput,TOutput}(ExternalReference{TInput},string)"/>.
        /// </para>
        /// <para>
        /// Examples: The mesh processor may build textures and effects in addition to the mesh.
        /// </para>
        /// </remarks>
        public List<string> BuildAsset { get; set; }

        /// <summary>
        /// Gets or sets the related output files.
        /// </summary>
        /// <value>The related output files.</value>
        /// <remarks>
        /// Related output files are non-XNB files that are included in addition to the XNB files.
        /// Related output files need to be copied to the output folder by a content processor and
        /// registered by calling <see cref="ContentProcessorContext.AddOutputFile"/>.
        /// </remarks>
        public List<string> BuildOutput { get; set; }

        public static PipelineBuildEvent LoadXml(string filePath)
        {
            PipelineBuildEvent buildEvent;
            try
            {
                using (var textReader = new XmlTextReader(filePath))
                {
                    XmlSerializer deserializer = new XmlSerializer(typeof(PipelineBuildEvent));
                    buildEvent = (PipelineBuildEvent)deserializer.Deserialize(textReader);
                }
            }
            catch (Exception)
            {
                return null;
            }

            // Repopulate the parameters from the serialized state.
            foreach (XmlParameter xmlParam in buildEvent.XmlParameters)
                buildEvent.Parameters.Add(xmlParam.Key, xmlParam.Value);
            buildEvent.XmlParameters.Clear();

            return buildEvent;
        }

        public void SaveXml(string filePath)
        {
            // Make sure the directory exists.
            Directory.CreateDirectory(Path.GetDirectoryName(filePath) + Path.DirectorySeparatorChar);

            // Convert the parameters into something we can serialize.
            XmlParameters.Clear();
            foreach (KeyValuePair<string, object> param in Parameters)
            {
                string key = param.Key;
                string valueStr = ConvertToString(param.Value);
                XmlParameters.Add(new XmlParameter(key, valueStr));
            }

            using (var textWriter = new StreamWriter(filePath, false, new UTF8Encoding(false)))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(PipelineBuildEvent));
                serializer.Serialize(textWriter, this);
            }
        }

        public void SaveBinary(string filePath)
        {
            // Make sure the directory exists.
            Directory.CreateDirectory(Path.GetDirectoryName(filePath) + Path.DirectorySeparatorChar);

            using (Stream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new PipelineBuildEventBinaryWriter(stream))
            {
                writer.Write(this);
            }
        }

        public static PipelineBuildEvent LoadBinary(string filePath)
        {
            try
            {
                using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                using (var writer = new PipelineBuildEventBinaryReader(stream))
                {
                    PipelineBuildEvent result = new PipelineBuildEvent();
                    writer.Read(result);
                    return result;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        internal bool NeedsRebuild(PipelineManager manager, PipelineBuildEvent cachedEvent)
        {
            // If we have no previously cached build event then we cannot
            // be sure that the state hasn't changed... force a rebuild.
            if (cachedEvent == null)
                return true;

            // Verify that the last write time of the source file matches
            // what we recorded when it was built.  If it is different
            // that means someone modified it and we need to rebuild.
            DateTime sourceWriteTime = File.GetLastWriteTime(SourceFile);
            if (cachedEvent.SourceTime != sourceWriteTime)
                return true;

            // Do the same test for the dest file.
            DateTime destWriteTime = File.GetLastWriteTime(DestFile);
            if (cachedEvent.DestTime != destWriteTime)
                return true;

            // If the source file is newer than the dest file
            // then it must have been updated and needs a rebuild.
            if (sourceWriteTime >= destWriteTime)
                return true;

            // Are any of the dependancy files newer than the dest file?
            foreach (string depFile in cachedEvent.Dependencies)
            {
                if (File.GetLastWriteTime(depFile) >= destWriteTime)
                    return true;
            }

            // This shouldn't happen...  but if the source or dest files changed
            // then force a rebuild.
            if (cachedEvent.SourceFile != SourceFile ||
                cachedEvent.DestFile != DestFile)
                return true;

            // Did the importer change?
            if (cachedEvent.Importer != Importer)
                return true;

            // Did the processor change?
            if (cachedEvent.Processor != Processor)
                return true;

            // Did the importer assembly change?
            DateTime importerAssemblyTimestamp = manager.GetImporterAssemblyTimestamp(cachedEvent.Importer);
            if (importerAssemblyTimestamp > cachedEvent.ImporterTime)
                return true;

            // Did the processor assembly change?
            DateTime processorAssemblyTimestamp = manager.GetProcessorAssemblyTimestamp(cachedEvent.Processor);
            if (processorAssemblyTimestamp > cachedEvent.ProcessorTime)
                return true;

            // Did the parameters change?
            OpaqueDataDictionary defaultValues = manager.GetProcessorDefaultValues(Processor);
            if (!AreParametersEqual(cachedEvent.Parameters, Parameters, defaultValues))
                return true;

            return false;
        }

        internal static bool AreParametersEqual(OpaqueDataDictionary parameters0, OpaqueDataDictionary parameters1, OpaqueDataDictionary defaultValues)
        {
            Debug.Assert(defaultValues != null, "defaultValues must not be empty.");
            Debug.Assert(EmptyParameters != null && EmptyParameters.Count == 0);

            // Same reference or both null?
            if (parameters0 == parameters1)
                return true;

            if (parameters0 == null)
                parameters0 = EmptyParameters;
            if (parameters1 == null)
                parameters1 = EmptyParameters;

            // Are both dictionaries empty?
            if (parameters0.Count == 0 && parameters1.Count == 0)
                return true;

            // Compare the values with the second dictionary or
            // the default values.
            if (parameters0.Count < parameters1.Count)
            {
                OpaqueDataDictionary dummy = parameters0;
                parameters0 = parameters1;
                parameters1 = dummy;
            }

            // Compare parameters0 with parameters1 or defaultValues.
            foreach (KeyValuePair<string, object> pair in parameters0)
            {
                object value0 = pair.Value;
                object value1;

                // Search for matching parameter.
                if (!parameters1.TryGetValue(pair.Key, out value1) && !defaultValues.TryGetValue(pair.Key, out value1))
                    return false;

                if (!AreEqual(value0, value1))
                    return false;
            }

            // Compare parameters which are only in parameters1 with defaultValues.
            foreach (KeyValuePair<string, object> pair in parameters1)
            {
                if (parameters0.ContainsKey(pair.Key))
                    continue;

                object defaultValue;
                if (!defaultValues.TryGetValue(pair.Key, out defaultValue))
                    return false;

                if (!AreEqual(pair.Value, defaultValue))
                    return false;
            }

            return true;
        }

        private static bool AreEqual(object value0, object value1)
        {
            // Are values equal or both null?
            if (Equals(value0, value1))
                return true;

            // Is one value null?
            if (value0 == null || value1 == null)
                return false;

            // Values are of different type: Compare string representation.
            if (ConvertToString(value0) != ConvertToString(value1))
                return false;

            return true;
        }

        private static string ConvertToString(object value)
        {
            if (value == null)
                return null;

            TypeConverter typeConverter = TypeDescriptor.GetConverter(value.GetType());
            return typeConverter.ConvertToInvariantString(value);
        }


        internal class PipelineBuildEventBinaryWriter : BinaryWriter
        {
            private const string Header = "KNIC"; // content db
            private const short MajorVersion = 3;
            private const short MinorVersion = 9;
            private const short DataType = 2; // PipelineBuildEvent data


            public PipelineBuildEventBinaryWriter(Stream output) : base(output)
            {
            }

            internal void Write(PipelineBuildEvent value)
            {
                Write(Header.ToCharArray());
                Write((Int16)MajorVersion);
                Write((Int16)MinorVersion);
                Write((Int32)DataType);
                Write((Int32)0); // reserved


                WriteStringOrNull(value.SourceFile);
                WriteDateTime(value.SourceTime);

                WriteStringOrNull(value.DestFile);
                WriteDateTime(value.DestTime);

                WriteStringOrNull(value.Importer);
                WriteDateTime(value.ImporterTime);

                WriteStringOrNull(value.Processor);
                WriteDateTime(value.ProcessorTime);

                WritePackedInt(value.Parameters.Count);
                foreach (var param in value.Parameters)
                {
                    WriteStringOrNull(param.Key);
                    WriteStringOrNull(ConvertToString(param.Value));
                }

                WritePackedInt(value.Dependencies.Count);
                for (int i = 0; i < value.Dependencies.Count; i++)
                    WriteStringOrNull(value.Dependencies[i]);

                WritePackedInt(value.BuildAsset.Count);
                for (int i = 0; i < value.BuildAsset.Count; i++)
                    WriteStringOrNull(value.BuildAsset[i]);

                WritePackedInt(value.BuildOutput.Count);
                for (int i = 0; i < value.BuildOutput.Count; i++)
                    WriteStringOrNull(value.BuildOutput[i]);

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

            private void WriteDateTime(DateTime value)
            {
                Write((Int64)value.ToBinary());
            }
        }


        internal class PipelineBuildEventBinaryReader : BinaryReader
        {
            private const string Header = "KNIC"; // content db
            private const short MajorVersion = 3;
            private const short MinorVersion = 9;
            private const int DataType = 2; // PipelineBuildEvent data


            public PipelineBuildEventBinaryReader(Stream output) : base(output)
            {
            }

            internal void Read(PipelineBuildEvent value)
            {
                if (ReadByte() != Header[0]
                ||  ReadByte() != Header[1]
                ||  ReadByte() != Header[2]
                ||  ReadByte() != Header[3])
                    throw new Exception("Invalid file.");

                if (ReadInt16() != MajorVersion
                || ReadInt16() != MinorVersion)
                    throw new Exception("Invalid file version.");

                int dataType = ReadInt32();
                if (dataType != DataType)
                    throw new Exception("Invalid data type.");

                int reserved0 = ReadInt32();

                value.SourceFile = ReadStringOrNull();
                value.SourceTime = ReadDateTime();

                value.DestFile = ReadStringOrNull();
                value.DestTime = ReadDateTime();

                value.Importer = ReadStringOrNull();
                value.ImporterTime = ReadDateTime();

                value.Processor = ReadStringOrNull();
                value.ProcessorTime = ReadDateTime();

                int parametersCount = ReadPackedInt();
                value.Parameters = new OpaqueDataDictionary();
                for (int i = 0; i < parametersCount; i++)
                {
                    value.Parameters.Add(
                        ReadStringOrNull(),
                        ReadStringOrNull());
                }

                int dependenciesCount = ReadPackedInt();
                value.Dependencies = new List<string>(dependenciesCount);
                for (int i = 0; i < dependenciesCount; i++)
                    value.Dependencies.Add(ReadStringOrNull());

                int buildAssetCount = ReadPackedInt();
                value.BuildAsset = new List<string>(buildAssetCount);
                for (int i = 0; i < buildAssetCount; i++)
                    value.BuildAsset.Add(ReadStringOrNull());

                int buildOutputCount = ReadPackedInt();
                value.BuildOutput = new List<string>(buildOutputCount);
                for (int i = 0; i < buildOutputCount; i++)
                    value.BuildOutput.Add(ReadStringOrNull());

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

            private DateTime ReadDateTime()
            {
                return DateTime.FromBinary(ReadInt64());
            }
        }
    }
}
