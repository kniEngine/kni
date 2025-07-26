﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Framework.Content.Pipeline.Builder
{
    internal class PipelineManager
    {
        [DebuggerDisplay("ImporterInfo: {type.Name}")]
        private struct ImporterInfo
        {
            public ContentImporterAttribute attribute;
            public Type type;
            public DateTime assemblyTimestamp;
        };

        private List<ImporterInfo> _importers;

        [DebuggerDisplay("ProcessorInfo: {type.Name}")]
        private struct ProcessorInfo
        {
            public ContentProcessorAttribute attribute;
            public Type type;
            public DateTime assemblyTimestamp;
        };

        private List<ProcessorInfo> _processors;

        private List<Type> _writers;

        // Keep track of all built assets. (Required to resolve automatic names "AssetName_n".)
        //   Key = absolute, normalized path of source file
        //   Value = list of build events
        // (Note: When using external references, an asset may be built multiple times
        // with different parameters.)
        internal readonly Dictionary<string, List<BuildEvent>> _buildEventsMap;

        // Store default values for content processor parameters. (Necessary to compare processor
        // parameters. See PipelineBuildEvent.AreParametersEqual.)
        //   Key = name of content processor
        //   Value = processor parameters
        private readonly Dictionary<string, OpaqueDataDictionary> _processorDefaultValues;

        private readonly SortedSet<string> _processingBuildEvents;

        public string ProjectDirectory { get; private set; }
        public string ResponseFilename { get; private set; }
        public string OutputDirectory { get; private set; }
        public string IntermediateDirectory { get; private set; }
        public bool Quiet { get; private set; }

        private ContentCompiler _compiler;

        internal readonly ConsoleLogger Logger;

        public List<string> Assemblies { get; private set; }

        /// <summary>
        /// The current target graphics profile for which all content is built.
        /// </summary>
        public GraphicsProfile Profile { get; set; }

        /// <summary>
        /// The current target platform for which all content is built.
        /// </summary>
        public TargetPlatform Platform { get; set; }

        /// <summary>
        /// The build configuration passed thru to content processors.
        /// </summary>
        public string Config { get; set; }

        /// <summary>
        /// Gets or sets the compression method.
        /// </summary>
        public ContentCompression Compression { get; set; }

        internal const String DefaultFileCollectionFilename = "assetsdb.kniContent";

        public PipelineManager(string projectDir, string responseFilename, string outputDir, string intermediateDir, bool quiet)
        {
            _buildEventsMap = new Dictionary<string, List<BuildEvent>>();
            _processorDefaultValues = new Dictionary<string, OpaqueDataDictionary>();
            _processingBuildEvents = new SortedSet<string>();

            Assemblies = new List<string>();
            Logger = new ConsoleLogger();

            ProjectDirectory = PathHelper.NormalizeDirectory(projectDir);
            ResponseFilename = responseFilename;
            OutputDirectory = PathHelper.NormalizeDirectory(outputDir);
            IntermediateDirectory = PathHelper.NormalizeDirectory(intermediateDir);
            Quiet = quiet;
        }

        public void AddAssembly(string assemblyFilePath)
        {
            if (assemblyFilePath == null)
                throw new ArgumentException("assemblyFilePath cannot be null!");
            if (!Path.IsPathRooted(assemblyFilePath))
                throw new ArgumentException("assemblyFilePath must be absolute!");

            // Make sure we're not adding the same assembly twice.
            assemblyFilePath = PathHelper.Normalize(assemblyFilePath);
            if (!this.Assemblies.Contains(assemblyFilePath))
            {
                this.Assemblies.Add(assemblyFilePath);

                //TODO need better way to update caches
                _processors = null;
                _importers = null;
                _writers = null;
            }
        }

        private void ResolveAssemblies()
        {
            _importers = new List<ImporterInfo>();
            _processors = new List<ProcessorInfo>();
            _writers = new List<Type>();

            // import the build-in Processors
            LoadAssembly(typeof(Microsoft.Xna.Framework.Content.Pipeline.IContentProcessor).Assembly); // Common
            LoadAssembly(typeof(Microsoft.Xna.Framework.Content.Pipeline.Processors.SoundEffectProcessor).Assembly); // Audio
            LoadAssembly(typeof(Microsoft.Xna.Framework.Content.Pipeline.Processors.VideoProcessor).Assembly); // Media
            LoadAssembly(typeof(Microsoft.Xna.Framework.Content.Pipeline.Processors.TextureProcessor).Assembly); // Graphics
            LoadAssembly(typeof(Microsoft.Xna.Framework.Content.Pipeline.Processors.EffectProcessor).Assembly); // Graphics Effects

            // import the referenced Processors
            foreach (string assemblyPath in Assemblies)
            {
                try
                {
                    LoadAssembly(Assembly.LoadFrom(assemblyPath));
                }
                catch (BadImageFormatException e)
                {
                    Logger.LogWarning(null, null, "Assembly is either corrupt or built using a different " +
                        "target platform than this process. Reference another target architecture (x86, x64, " +
                        "AnyCPU, etc.) of this assembly. '{0}': {1}", assemblyPath, e.Message);
                    // The assembly failed to load... nothing
                    // we can do but ignore it.
                    continue;
                }
                catch (Exception e)
                {
                    Logger.LogWarning(null, null, "Failed to load assembly '{0}': {1}", assemblyPath, e.Message);
                    continue;
                }
            }
        }

        private void LoadAssembly(Assembly asm)
        {
            Type[] exportedTypes;
            DateTime assemblyTimestamp;

            try
            {
                exportedTypes = asm.GetTypes();
                assemblyTimestamp = File.GetLastWriteTime(asm.Location);
            }
            catch (ReflectionTypeLoadException e)
            {
                string missingTypes = String.Empty;
                foreach (var le in e.LoaderExceptions)
                    missingTypes += le.Message;

                Logger.LogWarning(null, null, missingTypes +
                    " '{0}': {1}", asm.Location, e.Message);
                // The assembly failed to load... nothing
                // we can do but ignore it.
                return;
            }
            catch (Exception e)
            {
                Logger.LogWarning(null, null, "Failed to load assembly '{0}': {1}", asm.Location, e.Message);
                return;
            }

            foreach (Type type in exportedTypes)
            {
                if (type.IsAbstract)
                    continue;

                if (type.GetInterface(@"IContentImporter") != null)
                {
                    var attributes = type.GetCustomAttributes(typeof(ContentImporterAttribute), false);
                    if (attributes.Length != 0)
                    {
                        var importerAttribute = attributes[0] as ContentImporterAttribute;
                        _importers.Add(new ImporterInfo
                        {
                            attribute = importerAttribute,
                            type = type,
                            assemblyTimestamp = assemblyTimestamp
                        });
                    }
                    else
                    {
                        // If no attribute specify default one
                        var importerAttribute = new ContentImporterAttribute(".*");
                        importerAttribute.DefaultProcessor = "";
                        importerAttribute.DisplayName = type.Name;
                        var importerInfo = new ImporterInfo
                        {
                            attribute = importerAttribute,
                            type = type,
                            assemblyTimestamp = assemblyTimestamp
                        };
                        if (_importers.Contains(importerInfo))
                        {
                            Logger.LogWarning(null, null, "Duplicate Type '{0}': {1}", type.Name);
                            continue;
                        }
                        _importers.Add(importerInfo);
                    }
                }
                else if (type.GetInterface(@"IContentProcessor") != null)
                {
                    var attributes = type.GetCustomAttributes(typeof(ContentProcessorAttribute), false);
                    if (attributes.Length != 0)
                    {
                        var processorAttribute = attributes[0] as ContentProcessorAttribute;
                        var processorInfo = new ProcessorInfo
                        {
                            attribute = processorAttribute,
                            type = type,
                            assemblyTimestamp = assemblyTimestamp
                        };
                        if (_processors.Contains(processorInfo))
                        {
                            Logger.LogWarning(null, null, "Duplicate Type '{0}': {1}", type.Name);
                            continue;
                        }
                        _processors.Add(processorInfo);
                    }
                }
                else if (type.GetInterface(@"ContentTypeWriter") != null)
                {
                    // TODO: This doesn't work... how do i find these?
                    _writers.Add(type);
                }
            }
        }

        public Type[] GetImporterTypes()
        {
            if (_importers == null)
                ResolveAssemblies();

            List<Type> types = new List<Type>();

            foreach (var item in _importers) 
            {
                types.Add(item.type);
            }

            return types.ToArray();
        }

        public Type[] GetProcessorTypes()
        {
            if (_processors == null)
                ResolveAssemblies();
            
            List<Type> types = new List<Type>();
            
            foreach (var item in _processors) 
            {
                types.Add(item.type);
            }
            
            return types.ToArray();
        }

        private IContentImporter CreateImporter(string importerName)
        {
            if (_importers == null)
                ResolveAssemblies();

            // Search for the importer.
            foreach (var info in _importers)
            {
                if (info.type.Name.Equals(importerName))
                    return Activator.CreateInstance(info.type) as IContentImporter;
            }

            throw new PipelineException("Failed to create importer '{0}'", importerName);
        }

        public string FindImporterByExtension(string ext)
        {
            if (_importers == null)
                ResolveAssemblies();

            // Search for the importer.
            foreach (var info in _importers)
            {
                if (info.attribute.FileExtensions.Any(e => e.Equals(ext, StringComparison.InvariantCultureIgnoreCase)))
                    return info.type.Name;
            }

            return null;
        }

        public DateTime GetImporterAssemblyTimestamp(string name)
        {
            if (_importers == null)
                ResolveAssemblies();

            // Search for the importer.
            foreach (var info in _importers)
            {
                if (info.type.Name.Equals(name))
                    return info.assemblyTimestamp;
            }

            return DateTime.MaxValue;
        }

        public string FindDefaultProcessor(string importer)
        {
            if (_importers == null)
                ResolveAssemblies();

            // Search for the importer.
            foreach (var info in _importers)
            {
                if (info.type.Name == importer)
                    return info.attribute.DefaultProcessor;
            }

            return null;
        }

        public Type GetProcessorType(string name)
        {
            if (_processors == null)
                ResolveAssemblies();

            // Search for the processor type.
            foreach (var info in _processors)
            {
                if (info.type.Name.Equals(name))
                    return info.type;
            }

            return null;
        }

        public void ResolveImporterAndProcessor(string sourceFilepath, ref string importerName, ref string processorName)
        {
            // Resolve the importer name.
            if (string.IsNullOrEmpty(importerName))
                importerName = FindImporterByExtension(Path.GetExtension(sourceFilepath));
            if (string.IsNullOrEmpty(importerName))
                throw new Exception(string.Format("Couldn't find a default importer for '{0}'!", sourceFilepath));

            // Resolve the processor name.
            if (string.IsNullOrEmpty(processorName))
                processorName = FindDefaultProcessor(importerName);
            if (string.IsNullOrEmpty(processorName))
                throw new Exception(string.Format("Couldn't find a default processor for importer '{0}'!", importerName));
        }

        public IContentProcessor CreateProcessor(string name, OpaqueDataDictionary processorParameters)
        {
            Type processorType = GetProcessorType(name);
            if (processorType == null)
                return null;

            // Create the processor.
            IContentProcessor processor = (IContentProcessor)Activator.CreateInstance(processorType);

            // Convert and set the parameters on the processor.
            foreach (var param in processorParameters)
            {
                PropertyInfo property = processorType.GetProperty(param.Key, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
                if (property == null || property.GetSetMethod(false) == null)
                    continue;

                // If the property value is already of the correct type then set it.
                if (property.PropertyType.IsInstanceOfType(param.Value))
                    property.SetValue(processor, param.Value, null);
                else
                {
                    // Find a type converter for this property.
                    TypeConverter typeConverter = TypeDescriptor.GetConverter(property.PropertyType);
                    if (typeConverter.CanConvertFrom(param.Value.GetType()))
                    {
                        object propValue = typeConverter.ConvertFrom(null, CultureInfo.InvariantCulture, param.Value);
                        property.SetValue(processor, propValue, null);
                    }
                }
            }

            return processor;
        }

        /// <summary>
        /// Gets the default values for the content processor parameters.
        /// </summary>
        /// <param name="processorName">The name of the content processor.</param>
        /// <returns>
        /// A dictionary containing the default value for each parameter. Returns
        /// <see langword="null"/> if the content processor has not been created yet.
        /// </returns>
        public OpaqueDataDictionary GetProcessorDefaultValues(string processorName)
        {
            // null is not allowed as key in dictionary.
            if (processorName == null)
                processorName = string.Empty;

            OpaqueDataDictionary processorDefaultValues;

            lock (_processorDefaultValues)
            {
                if (!_processorDefaultValues.TryGetValue(processorName, out processorDefaultValues))
                {
                    // Create the content processor instance and read the default values.
                    processorDefaultValues = new OpaqueDataDictionary();
                    var processorType = GetProcessorType(processorName);
                    if (processorType != null)
                    {
                        try
                        {
                            var processor = (IContentProcessor)Activator.CreateInstance(processorType);
                            var properties = processorType.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
                            foreach (var property in properties)
                                processorDefaultValues.Add(property.Name, property.GetValue(processor, null));
                        }
                        catch
                        {
                            // Ignore exception. Will be handled in ProcessContent.
                        }
                    }

                    _processorDefaultValues.Add(processorName, processorDefaultValues);
                }
            }

            return processorDefaultValues;
        }

        public DateTime GetProcessorAssemblyTimestamp(string name)
        {
            if (_processors == null)
                ResolveAssemblies();

            // Search for the processor.
            foreach (var info in _processors)
            {
                if (info.type.Name.Equals(name))
                    return info.assemblyTimestamp;
            }

            return DateTime.MaxValue;
        }

        public OpaqueDataDictionary ValidateProcessorParameters(string processorName, OpaqueDataDictionary processorParameters)
        {
            OpaqueDataDictionary result = new OpaqueDataDictionary();

            Type processorType = GetProcessorType(processorName);
            if (processorType == null || processorParameters == null)
            {
                return result;
            }

            foreach (var param in processorParameters)
            {
                PropertyInfo property = processorType.GetProperty(param.Key, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
                if (property == null || property.GetSetMethod(false) == null)
                    continue;

                // Make sure we can assign the value.
                if (!property.PropertyType.IsInstanceOfType(param.Value))
                {
                    // Make sure we can convert the value.
                    TypeConverter typeConverter = TypeDescriptor.GetConverter(property.PropertyType);
                    if (!typeConverter.CanConvertFrom(param.Value.GetType()))
                        continue;
                }

                result.Add(param.Key, param.Value);
            }

            return result;
        }

        internal void ResolveOutputFilepath(string sourceFilepath, ref string outputFilepath)
        {
            // If the output path is null... build it from the source file path.
            if (string.IsNullOrEmpty(outputFilepath))
            {
                string filename = Path.GetFileNameWithoutExtension(sourceFilepath) + ".xnb";
                string directory = PathHelper.GetRelativePath(ProjectDirectory,
                                                           Path.GetDirectoryName(sourceFilepath) +
                                                           Path.DirectorySeparatorChar);
                outputFilepath = Path.Combine(OutputDirectory, directory, filename);
            }
            else
            {
                // If the extension is not XNB or the source file extension then add XNB.
                string sourceExt = Path.GetExtension(sourceFilepath);
                if (outputFilepath.EndsWith(sourceExt, StringComparison.InvariantCultureIgnoreCase))
                    outputFilepath = outputFilepath.Substring(0, outputFilepath.Length - sourceExt.Length);
                if (!outputFilepath.EndsWith(".xnb", StringComparison.InvariantCultureIgnoreCase))
                    outputFilepath += ".xnb";

                // If the path isn't rooted then put it into the output directory.
                if (!Path.IsPathRooted(outputFilepath))
                    outputFilepath = Path.Combine(OutputDirectory, outputFilepath);
            }

            outputFilepath = PathHelper.Normalize(outputFilepath);
        }

        private void DeleteBuildEvent(string destFile)
        {
            string dbname = ResponseFilename;
            if (dbname == String.Empty)
                dbname = DefaultFileCollectionFilename;

            string relativeEventPath = Path.ChangeExtension(PathHelper.GetRelativePath(OutputDirectory, destFile), BuildEvent.Extension);
            string intermediateEventPath = Path.Combine(IntermediateDirectory, Path.GetFileNameWithoutExtension(dbname), relativeEventPath);
            if (File.Exists(intermediateEventPath))
                File.Delete(intermediateEventPath);
        }

        private void SaveBuildEvent(string destFile, BuildEvent buildEvent)
        {
            string dbname = ResponseFilename;
            if (dbname == String.Empty)
                dbname = DefaultFileCollectionFilename;

            string relativeEventPath = Path.ChangeExtension(PathHelper.GetRelativePath(OutputDirectory, destFile), BuildEvent.Extension);
            string intermediateEventPath = Path.Combine(IntermediateDirectory, Path.GetFileNameWithoutExtension(dbname), relativeEventPath);
            intermediateEventPath = Path.GetFullPath(intermediateEventPath);
            buildEvent.SaveBinary(intermediateEventPath);
        }

        internal BuildEvent LoadBuildEvent(string destFile)
        {
            string dbname = ResponseFilename;
            if (dbname == String.Empty)
                dbname = DefaultFileCollectionFilename;

            string relativeEventPath = Path.ChangeExtension(PathHelper.GetRelativePath(OutputDirectory, destFile), BuildEvent.Extension);
            string intermediateEventPath = Path.Combine(IntermediateDirectory, Path.GetFileNameWithoutExtension(dbname), relativeEventPath);
            intermediateEventPath = Path.GetFullPath(intermediateEventPath);
            return BuildEvent.LoadBinary(intermediateEventPath);
        }

        internal BuildEvent CreateBuildEvent(string sourceFilepath, string outputFilepath, string importerName, string processorName, OpaqueDataDictionary processorParameters)
        {
            sourceFilepath = PathHelper.Normalize(sourceFilepath);
            ResolveOutputFilepath(sourceFilepath, ref outputFilepath);
            ResolveImporterAndProcessor(sourceFilepath, ref importerName, ref processorName);
            OpaqueDataDictionary ProcessorParams = ValidateProcessorParameters(processorName, processorParameters);

            BuildEvent buildEvent = new BuildEvent
            {
                SourceFile = sourceFilepath,
                DestFile = outputFilepath,
                Importer = importerName,
                Processor = processorName,
                Parameters = ProcessorParams,
            };
            return buildEvent;
        }

        internal void BuildContent(ConsoleLogger logger, BuildEvent buildEvent, BuildEvent cachedBuildEvent, string destFilePath)
        {
            if (!File.Exists(buildEvent.SourceFile))
            {
                logger.LogMessage("{0}", buildEvent.SourceFile);
                throw new PipelineException("The source file '{0}' does not exist!", buildEvent.SourceFile);
            }

            logger.PushFile(buildEvent.SourceFile);

            OpaqueDataDictionary processorDefaultValues = GetProcessorDefaultValues(buildEvent.Processor);

            // Keep track of all build events. (Required to resolve automatic names "AssetName_n".)
            TrackBuildEvent(_buildEventsMap, buildEvent, processorDefaultValues);

            bool building = RegisterBuildEvent(buildEvent);
            bool rebuild = buildEvent.NeedsRebuild(this, cachedBuildEvent);
            rebuild = rebuild && !building;

            if (rebuild)
                logger.LogMessage("{0}", buildEvent.SourceFile);
            if (!rebuild && !this.Quiet)
                logger.LogMessage("Skipping {0}", buildEvent.SourceFile);

            logger.Indent();
            try
            {
                if (!rebuild && cachedBuildEvent != null)
                {
                    // While this asset doesn't need to be rebuilt the dependent assets might.
                    foreach (string asset in cachedBuildEvent.BuildAsset)
                    {
                        BuildEvent assetCachedBuildEvent = LoadBuildEvent(asset);

                        // If we cannot find the cached event for the dependancy
                        // then we have to trigger a rebuild of the parent content.
                        if (assetCachedBuildEvent == null)
                        {
                            rebuild = true;
                            break;
                        }

                        BuildEvent depBuildEvent = new BuildEvent
                        {
                            SourceFile = assetCachedBuildEvent.SourceFile,
                            DestFile = assetCachedBuildEvent.DestFile,
                            Importer = assetCachedBuildEvent.Importer,
                            Processor = assetCachedBuildEvent.Processor,
                            Parameters = assetCachedBuildEvent.Parameters,
                        };

                        // Give the asset a chance to rebuild.
                        BuildContent(logger, depBuildEvent, assetCachedBuildEvent, asset);
                    }
                }

                // Do we need to rebuild?
                if (rebuild)
                {
                    // Import and process the content.
                    object importedObject = ImportContent(logger, buildEvent);
                    object processedObject = ProcessContent(logger, buildEvent, importedObject);

                    // Write the content to disk.
                    WriteXnb(processedObject, buildEvent);

                    // Store the timestamp of the DLLs containing the importer and processor.
                    buildEvent.ImporterTime = GetImporterAssemblyTimestamp(buildEvent.Importer);
                    buildEvent.ProcessorTime = GetProcessorAssemblyTimestamp(buildEvent.Processor);

                    // Store the new event into the intermediate folder.
                    SaveBuildEvent(destFilePath, buildEvent);
                }

            }
            finally
            {
                logger.Unindent();
                logger.PopFile();
            }
        }

        private bool RegisterBuildEvent(BuildEvent buildEvent)
        {
            lock (_processingBuildEvents)
            {
                if (!_processingBuildEvents.Contains(buildEvent.DestFile))
                {
                    _processingBuildEvents.Add(buildEvent.DestFile);
                    return false;
                }
            }
            return true;
        }

        public object ImportContent(ConsoleLogger logger, BuildEvent buildEvent)
        {
            if (!File.Exists(buildEvent.SourceFile))
                throw new PipelineException("The source file '{0}' does not exist!", buildEvent.SourceFile);

            // Store the last write time of the source file
            // so we can detect if it has been changed.
            buildEvent.SourceTime = File.GetLastWriteTime(buildEvent.SourceFile);

            // Make sure we can find the importer and processor.
            IContentImporter importer = CreateImporter(buildEvent.Importer);

            // Try importing the content.
            object importedObject;

            try
            {
                ImporterContext importContext = new ImporterContext(this, logger, buildEvent);
                importedObject = importer.Import(buildEvent.SourceFile, importContext);
            }
            catch (PipelineException) { throw; }
            catch (InvalidContentException) { throw; }
            catch (Exception inner)
            {
                throw new PipelineException(string.Format("Importer '{0}' had unexpected failure!", buildEvent.Importer), inner);
            }

            return importedObject;
        }

        public object ProcessContent(ConsoleLogger logger, BuildEvent buildEvent, object importedObject)
        {
            // The pipelineEvent.Processor can be null or empty. In this case the
            // asset should be imported but not processed.
            if (string.IsNullOrEmpty(buildEvent.Processor))
                return importedObject;

            IContentProcessor processor = CreateProcessor(buildEvent.Processor, buildEvent.Parameters);
            if (processor == null)
                throw new PipelineException("Failed to create processor '{0}'", buildEvent.Processor);

            // Make sure the input type is valid.
            if (!processor.InputType.IsAssignableFrom(importedObject.GetType()))
            {
                throw new PipelineException(
                    string.Format("The type '{0}' cannot be processed by {1} as a {2}!",
                    importedObject.GetType().FullName,
                    buildEvent.Processor,
                    processor.InputType.FullName));
            }

            // Process the imported object.

            object processedObject;
            try
            {
                ProcessorContext processContext = new ProcessorContext(this, logger, buildEvent);
                processedObject = processor.Process(importedObject, processContext);
            }
            catch (PipelineException) { throw; }
            catch (InvalidContentException) { throw; }
            catch (Exception inner)
            {
                throw new PipelineException(string.Format("Processor '{0}' had unexpected failure!", buildEvent.Processor), inner);
            }

            return processedObject;
        }

        public void CleanContent(string outputFilepath)
        {
            // First try to load the event file.
            BuildEvent cachedBuildEvent = LoadBuildEvent(outputFilepath);
            if (cachedBuildEvent != null)
            {
                // Remove related output files (non-XNB files) that were copied to the output folder.
                foreach (var asset in cachedBuildEvent.BuildOutput)
                {
                    Logger.LogMessage("Cleaning {0}", asset);
                    // Remove file (non .xnb file) from output folder.
                    if (File.Exists(asset))
                        File.Delete(asset);
                }

                // Recursively clean additional (nested) assets.
                foreach (string asset in cachedBuildEvent.BuildAsset)
                {
                    BuildEvent assetCachedBuildEvent = LoadBuildEvent(asset);
                    if (assetCachedBuildEvent != null)
                    {
                        string assetOutputFilepath = asset;
                        ResolveOutputFilepath(string.Empty, ref assetOutputFilepath);
                        CleanContent(assetOutputFilepath);
                        continue;
                    }
                    
                    Logger.LogMessage("Cleaning {0}", asset);
                    // Remove asset (.xnb file) from output folder.
                    if (File.Exists(asset))
                        File.Delete(asset);
                    // Remove event file (.kniContent file) from intermediate folder.
                    DeleteBuildEvent(asset);
                }
            }

            Logger.LogMessage("Cleaning {0}", outputFilepath);
            // Remove asset (.xnb file) from output folder.
            if (File.Exists(outputFilepath))
                File.Delete(outputFilepath);
            // Remove event file (.kniContent file) from intermediate folder.
            DeleteBuildEvent(outputFilepath);
        }

        private void WriteXnb(object content, BuildEvent buildEvent)
        {
            // Make sure the output directory exists.
            string outputFileDir = Path.GetDirectoryName(buildEvent.DestFile);

            Directory.CreateDirectory(outputFileDir);

            if (_compiler == null)
                _compiler = new ContentCompiler();

            // Write the XNB.
            using (Stream stream = new FileStream(buildEvent.DestFile, FileMode.Create, FileAccess.Write, FileShare.None))
                _compiler.Compile(stream, content, Platform, Profile, Compression, OutputDirectory, outputFileDir);

            // Store the last write time of the output XNB here
            // so we can verify it hasn't been tampered with.
            buildEvent.DestTime = File.GetLastWriteTime(buildEvent.DestFile);
        }

        /// <summary>
        /// Stores the pipeline build event (in memory) if no matching event is found.
        /// </summary>
        /// <param name="buildEvent">The pipeline build event.</param>
        internal static void TrackBuildEvent(Dictionary<string, List<BuildEvent>> buildEventsMap, BuildEvent buildEvent, OpaqueDataDictionary processorDefaultValues)
        {
            lock (buildEventsMap)
            {
                if (!buildEventsMap.TryGetValue(buildEvent.SourceFile, out List<BuildEvent> buildEvents))
                {
                    buildEvents = new List<BuildEvent>(1);
                    buildEventsMap.Add(buildEvent.SourceFile, buildEvents);
                }

                BuildEvent matchedBuildEvent = FindMatchingEvent(buildEvents, buildEvent.DestFile, buildEvent.Importer, buildEvent.Processor, buildEvent.Parameters, processorDefaultValues);
                if (matchedBuildEvent == null)
                    buildEvents.Add(buildEvent);
            }
        }

        /// <summary>
        /// Gets an automatic asset name, such as "AssetName_0".
        /// </summary>
        /// <param name="sourceFileName">The source file name.</param>
        /// <param name="importerName">The name of the content importer. Can be <see langword="null"/>.</param>
        /// <param name="processorName">The name of the content processor. Can be <see langword="null"/>.</param>
        /// <param name="processorParameters">The processor parameters. Can be <see langword="null"/>.</param>
        /// <returns>The asset name.</returns>
        public string GetAssetName(string sourceFileName, string importerName, string processorName, OpaqueDataDictionary processorParameters, ContentBuildLogger logger)
        {
            if (!Path.IsPathRooted(sourceFileName))
                sourceFileName = Path.Combine(this.ProjectDirectory, sourceFileName);

            // Get source file name, which is used for lookup in _pipelineBuildEvents.
            sourceFileName = PathHelper.Normalize(sourceFileName);
            string relativeSourceFileName = PathHelper.GetRelativePath(ProjectDirectory, sourceFileName);

            List<BuildEvent> buildEvents;

            lock (_buildEventsMap)
            {
                if (_buildEventsMap.TryGetValue(sourceFileName, out buildEvents))
                {
                    // This source file has already been build.
                    // --> Compare pipeline build events.
                    ResolveImporterAndProcessor(sourceFileName, ref importerName, ref processorName);

                    OpaqueDataDictionary processorDefaultValues = GetProcessorDefaultValues(processorName);

                    BuildEvent matchedBuildEvent = FindMatchingEvent(buildEvents, null, importerName, processorName, processorParameters, processorDefaultValues);
                    if (matchedBuildEvent != null)
                    {
                        // Matching pipeline build event found.
                        string existingName = matchedBuildEvent.DestFile;
                        existingName = PathHelper.GetRelativePath(OutputDirectory, existingName);
                        existingName = existingName.Substring(0, existingName.Length - 4);   // Remove ".xnb".
                        return existingName;
                    }

                    logger.LogMessage(string.Format("Warning: Asset {0} built multiple times with different settings.", relativeSourceFileName));
                }
            }

            // No pipeline build event with matching settings found.
            // Get default asset name by searching the existing .kniContent files.
            string directoryName = Path.GetDirectoryName(relativeSourceFileName);
            string fileName = Path.GetFileNameWithoutExtension(relativeSourceFileName);
            string assetName = Path.Combine(directoryName, fileName);
            assetName = PathHelper.Normalize(assetName);

            for (int index = 0; ; index++)
            {
                string destFile = assetName + '_' + index;
                BuildEvent existingBuildEvent = LoadBuildEvent(destFile);
                if (existingBuildEvent == null)
                    return destFile;

                string existingBuildEventDestFile = existingBuildEvent.DestFile;
                existingBuildEventDestFile = PathHelper.GetRelativePath(ProjectDirectory, existingBuildEventDestFile);
                existingBuildEventDestFile = Path.Combine(Path.GetDirectoryName(existingBuildEventDestFile), Path.GetFileNameWithoutExtension(existingBuildEventDestFile));
                existingBuildEventDestFile = PathHelper.Normalize(existingBuildEventDestFile);

                string fullDestFile = Path.Combine(OutputDirectory, destFile);
                string relativeDestFile = PathHelper.GetRelativePath(ProjectDirectory, fullDestFile);
                relativeDestFile = PathHelper.Normalize(relativeDestFile);

                if (existingBuildEventDestFile.Equals(relativeDestFile)
                &&  existingBuildEvent.Importer  == importerName
                &&  existingBuildEvent.Processor == processorName)
                {
                    OpaqueDataDictionary processorDefaultValues = GetProcessorDefaultValues(processorName);
                    if (BuildEvent.AreParametersEqual(existingBuildEvent.Parameters, processorParameters, processorDefaultValues))
                        return destFile;
                }
            }
        }

        /// <summary>
        /// Determines whether the specified list contains a matching pipeline build event.
        /// </summary>
        /// <param name="pipelineBuildEvents">The list of pipeline build events.</param>
        /// <param name="destFile">Absolute path to the output file. Can be <see langword="null"/>.</param>
        /// <param name="importerName">The name of the content importer. Can be <see langword="null"/>.</param>
        /// <param name="processorName">The name of the content processor. Can be <see langword="null"/>.</param>
        /// <param name="processorParameters">The processor parameters. Can be <see langword="null"/>.</param>
        /// <returns>
        /// The matching pipeline build event, or <see langword="null"/>.
        /// </returns>
        private static BuildEvent FindMatchingEvent(List<BuildEvent> pipelineBuildEvents, string destFile, string importerName, string processorName, OpaqueDataDictionary processorParameters, OpaqueDataDictionary processorDefaultValues)
        {
            foreach (BuildEvent existingBuildEvent in pipelineBuildEvents)
            {
                if ((destFile == null || existingBuildEvent.DestFile.Equals(destFile))
                &&  existingBuildEvent.Importer == importerName
                &&  existingBuildEvent.Processor == processorName)
                {
                    if (BuildEvent.AreParametersEqual(existingBuildEvent.Parameters, processorParameters, processorDefaultValues))
                    {
                        return existingBuildEvent;
                    }
                }
            }

            return null;
        }

    }
}
