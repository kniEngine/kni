﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace Content.Pipeline.Editor
{
    public class ImporterTypeDescription
    {        
        public string TypeName;
        public string DisplayName;
        public string DefaultProcessor;        
        public IEnumerable<string> FileExtensions;
        public Type OutputType;

        public override string ToString()
        {
            return TypeName;
        }

        public override int GetHashCode()
        {
            return TypeName == null ? 0 : TypeName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as ImporterTypeDescription;
            if (other == null)
                return false;
            
            if (string.IsNullOrEmpty(other.TypeName) != string.IsNullOrEmpty(TypeName))
                return false;

            return TypeName.Equals(other.TypeName);
        }
    };

    public class ProcessorTypeDescription
    {
        #region Supporting Types 

        public struct Property
        {
            public string Name;
            public Type Type;
            public object DefaultValue;

            public override string ToString()
            {
                return Name;
            }
        }

        public class ProcessorPropertyCollection : IEnumerable<Property>
        {
            private readonly Property[] _properties;

            public ProcessorPropertyCollection(IEnumerable<Property> properties)
            {
                _properties = properties.ToArray();
            }
 
            public Property this[int index]
            {
                get
                {
                    return _properties[index];
                }
                set
                {
                    _properties[index] = value;
                }
            }

            public Property this[string name]
            {
                get
                {
                    foreach (var p in _properties)
                    {
                        if (p.Name.Equals(name))
                            return p;
                    }

                    throw new IndexOutOfRangeException();
                }    
            
                set
                {
                    for (var i = 0; i < _properties.Length; i++)
                    {
                        var p = _properties[i];
                        if (p.Name.Equals(name))
                        {
                            _properties[i] = value;
                            return;
                        }

                    }

                    throw new IndexOutOfRangeException();
                }
            }

            public bool Contains(string name)
            {
                return _properties.Any(e => e.Name == name);
            }

            public IEnumerator<Property> GetEnumerator()
            {
                return _properties.AsEnumerable().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _properties.GetEnumerator();
            }
        }

        #endregion
        
        public string TypeName;
        public string DisplayName;
        public ProcessorPropertyCollection Properties;
        public Type InputType;

        public override string ToString()
        {
            return TypeName;
        }
    };

    internal class PipelineTypes
    {
        [DebuggerDisplay("ImporterInfo: {Type.Name}")]
        private struct ImporterInfo
        {
            public ContentImporterAttribute Attribute;
            public Type Type;
        }

        [DebuggerDisplay("ProcessorInfo: {Type.Name}")]
        private struct ProcessorInfo
        {
            public ContentProcessorAttribute Attribute;
            public Type Type;
        }

        private static List<ImporterInfo> _importers;
        private static List<ProcessorInfo> _processors;

        public static ImporterTypeDescription[] Importers { get; private set; }
        public static ProcessorTypeDescription[] Processors { get; private set; }

        public static ImporterTypeDescription NullImporter { get; private set; }
        public static ProcessorTypeDescription NullProcessor { get; private set; }

        public static ImporterTypeDescription MissingImporter { get; private set; }
        public static ProcessorTypeDescription MissingProcessor { get; private set; }

        public static TypeConverter.StandardValuesCollection ImportersStandardValuesCollection { get; private set; }
        public static TypeConverter.StandardValuesCollection ProcessorsStandardValuesCollection { get; private set; }

        private static readonly Dictionary<string, string> _oldNameRemap = new Dictionary<string, string>()
            {
                { "MGMaterialProcessor", "MaterialProcessor" },
                { "MGSongProcessor", "SongProcessor" },
                { "MGSoundEffectProcessor", "SoundEffectProcessor" },
                { "MGSpriteFontDescriptionProcessor", "FontDescriptionProcessor" },
                { "MGSpriteFontTextureProcessor", "FontTextureProcessor" },
                { "MGTextureProcessor", "TextureProcessor" },
                { "MGEffectProcessor", "EffectProcessor" },
            };

        private static string RemapOldNames(string name)
        {
            if (_oldNameRemap.ContainsKey(name))
                return _oldNameRemap[name];

            return name;
        }

        static PipelineTypes()
        {
            MissingImporter = new ImporterTypeDescription()
                {
                    DisplayName = "Invalid / Missing Importer",
                };

            MissingProcessor = new ProcessorTypeDescription()
                {
                    DisplayName = "Invalid / Missing Processor",
                    Properties = new ProcessorTypeDescription.ProcessorPropertyCollection(new ProcessorTypeDescription.Property[0]),
                };

            NullImporter = new ImporterTypeDescription()
            {
                DisplayName = "",
            };

            NullProcessor = new ProcessorTypeDescription()
            {
                DisplayName = "",
                Properties = new ProcessorTypeDescription.ProcessorPropertyCollection(new ProcessorTypeDescription.Property[0]),
            };
        }

        public static void Load(PipelineProject project)
        {
            Unload();

            List<string> assemblyPaths = new List<string>();

            string projectRoot = project.Location;

            foreach (string i in project.References)
            {
                string path = Path.Combine(projectRoot, i);

                if (string.IsNullOrEmpty(path))
                    throw new ArgumentException("assemblyFilePath cannot be null!");
                if (!Path.IsPathRooted(path))
                    throw new ArgumentException("assemblyFilePath must be absolute!");

                // Make sure we're not adding the same assembly twice.
                path = PathHelper.Normalize(path);
                if (!assemblyPaths.Contains(path))
                    assemblyPaths.Add(path);
            }

            try
            {
                AddPackageReferences(project, projectRoot, project.PackageReferences, assemblyPaths);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    string.Format("Failed to resolve package references.\n\n {0}.", ex.Message),
                    "KNI Pipeline - " + Path.GetFileName(project.OriginalPath),
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
            }

            ResolveAssemblies(assemblyPaths);

            var importerDescriptions = new ImporterTypeDescription[_importers.Count];
            int cur = 0;
            foreach (var item in _importers)
            {
                // Find the abstract base class ContentImporter<T>.
                Type baseType = item.Type.BaseType;
                while (!baseType.IsAbstract)
                    baseType = baseType.BaseType;

                Type outputType = baseType.GetGenericArguments()[0];
                var desc = new ImporterTypeDescription()
                    {
                        TypeName = item.Type.Name,
                        DisplayName = item.Attribute.DisplayName,
                        DefaultProcessor = item.Attribute.DefaultProcessor,                        
                        FileExtensions = item.Attribute.FileExtensions,   
                        OutputType = outputType,
                    };
                importerDescriptions[cur] = desc;
                cur++;
            }

            Importers = importerDescriptions;
            ImportersStandardValuesCollection = new TypeConverter.StandardValuesCollection(Importers);

            var processorDescriptions = new ProcessorTypeDescription[_processors.Count];

            const BindingFlags bindings = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

            cur = 0;
            foreach (var item in _processors)
            {
                var obj = Activator.CreateInstance(item.Type);
                var typeProperties = item.Type.GetProperties(bindings);
                var properties = new List<ProcessorTypeDescription.Property>();
                foreach (var i in typeProperties)
                {
                    // TODO:
                    //p.GetCustomAttribute(typeof(ContentPipelineIgnore))

                    var p = new ProcessorTypeDescription.Property()
                        {
                            Name = i.Name,
                            Type = i.PropertyType,
                            DefaultValue = i.GetValue(obj, null),
                        };
                    properties.Add(p);
                }

                Type inputType = (obj as IContentProcessor).InputType;
                var desc = new ProcessorTypeDescription()
                {
                    TypeName = item.Type.Name,
                    DisplayName = item.Attribute.DisplayName,
                    Properties = new ProcessorTypeDescription.ProcessorPropertyCollection(properties),
                    InputType = inputType,
                };
                if (string.IsNullOrEmpty(desc.DisplayName))
                    desc.DisplayName = desc.TypeName;

                processorDescriptions[cur] = desc;
                cur++;
            }

            Processors = processorDescriptions;
            ProcessorsStandardValuesCollection = new TypeConverter.StandardValuesCollection(Processors);
        }

        private static void AddPackageReferences(PipelineProject manager, string projectDirectory, List<Package> packageReferences, List<string> assemblyPaths)
        {
            if (packageReferences.Count == 0)
                return;

            string intermediateFolder = "obj/";
            intermediateFolder = Path.Combine(projectDirectory, intermediateFolder);
            intermediateFolder = PathHelper.Normalize(intermediateFolder);
            if (!Directory.Exists(intermediateFolder))
                Directory.CreateDirectory(intermediateFolder);

            const string packageReferencesProjFolder = ".Packages";
            const string libraryName = "PackagesLibrary";

            string fullPackageReferencesFolder = Path.Combine(intermediateFolder, packageReferencesProjFolder);
            if (!Directory.Exists(fullPackageReferencesFolder))
                Directory.CreateDirectory(fullPackageReferencesFolder);

            string projFolder = manager.Name;

            string fullPackageReferencesProjFolder = Path.Combine(fullPackageReferencesFolder, projFolder);
            fullPackageReferencesProjFolder = PathHelper.Normalize(fullPackageReferencesProjFolder);

            string publishDir = "publish";

            bool rebuild = false;

            // load db
            List<Package> packages = new List<Package>(packageReferences);
            packages.Sort();
            string intermediatePackageCollectionPath = Path.Combine(fullPackageReferencesProjFolder, Path.ChangeExtension(libraryName, PackageReferencesCollection.Extension));
            PackageReferencesCollection previousPackageReferencesCollection = PackageReferencesCollection.LoadBinary(intermediatePackageCollectionPath);
            if (previousPackageReferencesCollection != null
            && previousPackageReferencesCollection.PackagesCount == packages.Count)
            {
                for (int i = 0; i < packages.Count; i++)
                {
                    if (packages[i].Name != previousPackageReferencesCollection.Packages[i].Name
                    ||  packages[i].Version != previousPackageReferencesCollection.Packages[i].Version)
                    {
                        rebuild = true;
                        break;
                    }
                }
            }
            else rebuild = true;

            // build PackageReferencesLibrary
            if (rebuild)
            {
                string framework = "netstandard2.0";
#if NET8_0_OR_GREATER
                framework = "net8.0";
#endif
                string newCmd = String.Format("new classlib --framework \"{0}\" -n {1} -o \"{2}\"", framework, libraryName, projFolder);
                newCmd += " --force";
                ExecuteDotnet(fullPackageReferencesFolder, newCmd);


                foreach (Package packageReference in packageReferences)
                {
                    string addCmd = String.Format("add {0}.csproj package {1} ", libraryName, packageReference.Name);
                    addCmd += " --no-restore";
                    if (packageReference.Version != String.Empty)
                        addCmd += " --version " + packageReference.Version;
                    ExecuteDotnet(fullPackageReferencesProjFolder, addCmd);
                }

                string cleanCmd = String.Format("clean {0}.csproj --output {1}", libraryName, publishDir);
                cleanCmd += " --nologo";
                ExecuteDotnet(fullPackageReferencesProjFolder, cleanCmd);
                string publishCmd = String.Format("publish {0}.csproj --output {1}", libraryName, publishDir);
                publishCmd += " --nologo";
                ExecuteDotnet(fullPackageReferencesProjFolder, publishCmd);

                // save db
                PackageReferencesCollection dbfile = new PackageReferencesCollection();
                foreach (Package package in packages)
                    dbfile.AddPackage(package);
                dbfile.SaveBinary(intermediatePackageCollectionPath);
            }

            // load packages
            string fullPublishDir = Path.Combine(fullPackageReferencesProjFolder, publishDir);
            fullPublishDir = PathHelper.Normalize(fullPublishDir);

            string[] references = Directory.GetFiles(fullPublishDir, "*.dll");

            foreach (string assemblyFile in references)
            {
                string assemblyFileName = Path.GetFileNameWithoutExtension(assemblyFile);
                // skip the empty project and known pipeline libraries.
                if (assemblyFileName == libraryName)
                    continue;
                if (assemblyFileName.StartsWith("Xna.Framework"))
                    continue;

                assemblyPaths.Add(assemblyFile);
            }

            return;
        }

        private static void ExecuteDotnet(string workingDirectory, string args)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("dotnet", args);
            startInfo.CreateNoWindow = true;
            startInfo.WorkingDirectory = workingDirectory;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            using (Process process = Process.Start(startInfo))
            {
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    string output = process.StandardOutput.ReadToEnd();
                    Console.Write(output);
                    string error = process.StandardError.ReadToEnd();
                    Console.Write(error);
                    throw new PipelineException(output + error);
                }
            }
        }

        public static void Unload()
        {
            _importers = null;
            Importers = null;
         
            _processors = null;
            Processors = null;

            ImportersStandardValuesCollection = null;
            ProcessorsStandardValuesCollection = null;
        }        

        public static TypeConverter FindConverter(Type type)
        {

            return TypeDescriptor.GetConverter(type);
        }

        public static ImporterTypeDescription FindImporter(string name, string fileExtension)
        {
            if (!string.IsNullOrEmpty(name))
            {
                name = RemapOldNames(name);
                
                foreach (var i in Importers)
                {
                    if (i.TypeName.Equals(name))
                        return i;
                }

                foreach (var i in Importers)
                {
                    if (i.DisplayName.Equals(name))
                        return i;
                }

                //Debug.Fail(string.Format("Importer not found! name={0}, ext={1}", name, fileExtension));
                return null;
            }

            var lowerFileExt = fileExtension.ToLowerInvariant();
            foreach (var i in Importers)
            {
                if (i.FileExtensions.Any(e => e.ToLowerInvariant() == lowerFileExt))
                    return i;
            }

            //Debug.Fail(string.Format("Importer not found! name={0}, ext={1}", name, fileExtension));
            return null;
        }

        public static ProcessorTypeDescription FindProcessor(string name, ImporterTypeDescription importer)
        {
            if (!string.IsNullOrEmpty(name))
            {
                name = RemapOldNames(name);

                foreach (var i in Processors)
                {
                    if (i.TypeName.Equals(name))
                        return i;
                }

                //Debug.Fail(string.Format("Processor not found! name={0}, importer={1}", name, importer));
                return null;
            }

            if (importer != null)
            {
                foreach (var i in Processors)
                {
                    if (i.TypeName.Equals(importer.DefaultProcessor))
                        return i;
                }
            }

            //Debug.Fail(string.Format("Processor not found! name={0}, importer={1}", name, importer));
            return null;
        }

        private static void ResolveAssemblies(IEnumerable<string> assemblyPaths)
        {
            _importers = new List<ImporterInfo>();
            _processors = new List<ProcessorInfo>();
            
            var assemblies = new List<Assembly>();
            assemblies.Add(typeof(Microsoft.Xna.Framework.Content.Pipeline.IContentProcessor).Assembly); // Common
            assemblies.Add(typeof(Microsoft.Xna.Framework.Content.Pipeline.Processors.SoundEffectProcessor).Assembly); // Audio
            assemblies.Add(typeof(Microsoft.Xna.Framework.Content.Pipeline.Processors.VideoProcessor).Assembly); // Media
            assemblies.Add(typeof(Microsoft.Xna.Framework.Content.Pipeline.Processors.TextureProcessor).Assembly); // Graphics
            assemblies.Add(typeof(Microsoft.Xna.Framework.Content.Pipeline.Processors.EffectProcessor).Assembly); // Graphics Effects

            foreach (var asm in assemblies)
            {
                //try
                {
                    var exportedTypes = asm.GetTypes();
                    ProcessTypes(exportedTypes);
                }
                //catch (Exception e) { }
            }

            foreach (var path in assemblyPaths)
            {
                try
                {
                    var a = Assembly.LoadFrom(path);
                    var types = a.GetTypes();
                    ProcessTypes(types);
                }
                catch 
                {
                    //Logger.LogWarning(null, null, "Failed to load assembly '{0}': {1}", assemblyPath, e.Message);
                    // The assembly failed to load... nothing
                    // we can do but ignore it.
                    continue;
                }                
            }
        }

        private static void ProcessTypes(IEnumerable<Type> types)
        {
            foreach (var t in types)
            {
                if (t.IsAbstract)
                    continue;

                if (t.GetInterface(@"IContentImporter") == typeof(IContentImporter))
                {
                    var attributes = t.GetCustomAttributes(typeof(ContentImporterAttribute), false);
                    if (attributes.Length != 0)
                    {
                        var importerAttribute = attributes[0] as ContentImporterAttribute;
                        _importers.Add(new ImporterInfo { Attribute = importerAttribute, Type = t });
                    }
                    else
                    {
                        // If no attribute specify default one
                        var importerAttribute = new ContentImporterAttribute(".*");
                        importerAttribute.DefaultProcessor = "";
                        importerAttribute.DisplayName = t.Name;
                        _importers.Add(new ImporterInfo { Attribute = importerAttribute, Type = t });
                    }
                }
                else if (t.GetInterface(@"IContentProcessor") == typeof(IContentProcessor))
                {
                    var attributes = t.GetCustomAttributes(typeof(ContentProcessorAttribute), false);
                    if (attributes.Length != 0)
                    {
                        var processorAttribute = attributes[0] as ContentProcessorAttribute;
                        _processors.Add(new ProcessorInfo { Attribute = processorAttribute, Type = t });
                    }
                }
            }
        }
    }
}
