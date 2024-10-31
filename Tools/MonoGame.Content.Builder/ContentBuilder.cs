// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Framework.Content.Pipeline.Builder
{
    class ContentBuilder
    {
        [CommandLineParameter(
            Name = "singleThread",
            Flag = "s",
            Description = "Use single Thread.")]
        public bool SingleThread = false;

        [CommandLineParameter(
            Name = "quiet",
            Flag = "q",
            Description = "Only output content build errors.")]
        public bool Quiet = false;
        
        [CommandLineParameter(
            Name = "help",
            Flag = "h",
            Description = "Displays this help.")]
        public void Help()
        {
            CommandLineParser.Instance.ShowError(null);
        }

        private string _responseFilename = string.Empty;

        [CommandLineParameter(
            Name = "@",
            Flag = "@",
            ValueName = "responseFile",
            Description = "Read a text response file with additional command line options and switches.")]
        // This property only exists for documentation.
        // The actual handling of '/@' is done in the preprocess step.
        public void SetResponseFile(string responseFilename)
        {
            _responseFilename = responseFilename;
        }

        [CommandLineParameter(
            Name = "workingDir",
            Flag = "w",
            ValueName = "directoryPath",
            Description = "The working directory where all source content is located.")]
        public void SetWorkingDir(string path)
        {
            Directory.SetCurrentDirectory(path);
        }

        private string _outputDir = string.Empty;

        [CommandLineParameter(
            Name = "outputDir",
            Flag = "o",
            ValueName = "path",
            Description = "The directory where all content is written.")]
        public void SetOutputDir(string path)
        {
            _outputDir = Path.GetFullPath(path);
        }

        private string _intermediateDir = string.Empty;

        [CommandLineParameter(
            Name = "intermediateDir",
            Flag = "n",
            ValueName = "path",
            Description = "The directory where all intermediate files are written.")]
        public void SetIntermediateDir(string path)
        {
            _intermediateDir = Path.GetFullPath(path);
        }

        [CommandLineParameter(
            Name = "rebuild",
            Flag = "r",
            Description = "Forces a full rebuild of all content.")]
        public bool Rebuild = false;

        [CommandLineParameter(
            Name = "clean",
            Flag = "c",
            Description = "Delete all previously built content and intermediate files.")]
        public bool Clean = false;

        [CommandLineParameter(
            Name = "incremental",
            Flag = "I",
            Description = "Skip cleaning files not included in the current build.")]
        public bool Incremental = false;

        [CommandLineParameter(
            Name = "reference",
            Flag = "f",
            ValueName = "assembly",
            Description = "Adds an assembly reference for resolving content importers, processors, and writers.")]
        public readonly List<string> References = new List<string>();

        [CommandLineParameter(
            Name = "packageReference",
            ValueName = "package",
            Description = "Adds a nuget package reference for resolving content importers, processors, and writers.")]
        public readonly List<string> PackageReferences = new List<string>();

        [CommandLineParameter(
            Name = "platform",
            Flag = "t",
            ValueName = "targetPlatform",
            Description = "Set the target platform for this build.  Defaults to Windows desktop DirectX.")]
        public TargetPlatform Platform = TargetPlatform.Windows;

        [CommandLineParameter(
            Name = "profile",
            Flag = "g",
            ValueName = "graphicsProfile",
            Description = "Set the target graphics profile for this build.  Defaults to HiDef.")]
        public GraphicsProfile Profile = GraphicsProfile.HiDef;

        [CommandLineParameter(
            Name = "config",
            ValueName = "string",
            Description = "The optional build config string from the build system.")]
        public string Config = string.Empty;

        [CommandLineParameter(
            Name = "importer",
            Flag = "i",
            ValueName = "className",
            Description = "Defines the class name of the content importer for reading source content.")]
        public string Importer = null;

        [CommandLineParameter(
            Name = "processor",
            Flag = "p",
            ValueName = "className",
            Description = "Defines the class name of the content processor for processing imported content.")]
        public void SetProcessor(string processor)
        {
            _processor = processor;
            
            // If you are changing the processor then reset all 
            // the processor parameters.
            _processorParams.Clear();
        }

        private string _processor = null;
        private readonly OpaqueDataDictionary _processorParams = new OpaqueDataDictionary();

        [CommandLineParameter(
            Name = "processorParam",
            Flag = "m",
            ValueName = "name=value",
            Description = "Defines a parameter name and value to set on a content processor.")]
        public void AddProcessorParam(string nameAndValue)
        {
            var keyAndValue = nameAndValue.Split('=', ':');
            if (keyAndValue.Length != 2)
            {
                // Do we error out or something?
                return;
            }

            _processorParams.Remove(keyAndValue[0]);
            _processorParams.Add(keyAndValue[0], keyAndValue[1]);
        }

        [CommandLineParameter(
            Name = "build",
            Flag = "b",
            ValueName = "sourceFile",
            Description = "Build the content source file using the previously set switches and options. Optional destination path may be specified with \"sourceFile;destFile\" if you wish to change the output filepath.")]
        public void OnBuild(string sourceFile)
        {
            string link = null;
            if (sourceFile.Contains(";"))
            {
                var split = sourceFile.Split(';');
                sourceFile = split[0];

                if(split.Length > 0)
                    link = split[1];
            }

            string projectDir = Directory.GetCurrentDirectory();

            //if (sourceFile.Contains(".."))
            //    throw new InvalidOperationException("Content file is not rooted in content path \"" + projectDir + "\"");

            // Make sure the source file is absolute.
            if (!Path.IsPathRooted(sourceFile))
                sourceFile = Path.Combine(projectDir, sourceFile);

            // link should remain relative, absolute path will get set later when the build occurs

            sourceFile = PathHelper.Normalize(sourceFile);

            // Remove duplicates... keep this new one.
            int previous = _contentItems.FindIndex(e => string.Equals(e.SourceFile, sourceFile, StringComparison.InvariantCultureIgnoreCase));
            if (previous != -1)
            {
                _contentItemsMap.Remove(_contentItems[previous].SourceFile.ToLowerInvariant());
                _contentItems.RemoveAt(previous);
            }

            // Create the item for processing later.
            ContentItem contentItem = new ContentItem
            {
                SourceFile = sourceFile,
                OutputFile = link,
                Importer = Importer, 
                Processor = _processor,
                ProcessorParams = new OpaqueDataDictionary()
            };
            _contentItems.Add(contentItem);
            _contentItemsMap.Add(contentItem.SourceFile.ToLowerInvariant(), contentItem);

            // Copy the current processor parameters blind as we
            // will validate and remove invalid parameters during
            // the build process later.
            foreach (var pair in _processorParams)
                contentItem.ProcessorParams.Add(pair.Key, pair.Value);
        }

        [CommandLineParameter(
            Name = "copy",
            ValueName = "sourceFile",
            Description = "Copy the content source file verbatim to the output directory.")]
        public void OnCopy(string sourceFile)
        {
            string link = null;
            if (sourceFile.Contains(";"))
            {
                var split = sourceFile.Split(';');
                sourceFile = split[0];

                if (split.Length > 0)
                    link = split[1];
            }

            if (!Path.IsPathRooted(sourceFile))
                sourceFile = Path.Combine(Directory.GetCurrentDirectory(), sourceFile);

            sourceFile = PathHelper.Normalize(sourceFile);

            // Remove duplicates... keep this new one.
            var previous = _copyItems.FindIndex(e => string.Equals(e.SourceFile, sourceFile, StringComparison.InvariantCultureIgnoreCase));
            if (previous != -1)
                _copyItems.RemoveAt(previous);

            _copyItems.Add(new CopyItem { SourceFile = sourceFile, Link = link });
        }

        [CommandLineParameter(
            Name = "compress",
            Description = "Compress the XNB files for smaller file sizes.")]
        public bool CompressContent = false;

        [CommandLineParameter(
            Name = "compression",
            Description = "The compression method.")]
        public CompressionMethod Compression = CompressionMethod.Default;

        public class ContentItem
        {
            public string SourceFile;
            public string OutputFile; // This refers to the "Link" which can override the default output location

            public string Importer;
            public string Processor;
            public OpaqueDataDictionary ProcessorParams;
        }

        public class CopyItem
        {
            public string SourceFile;
            public string Link;
        }

        private PipelineManager _manager;
        private readonly List<ContentItem> _contentItems = new List<ContentItem>();
        private readonly Dictionary<string, ContentItem> _contentItemsMap = new Dictionary<string, ContentItem>();
        private readonly List<CopyItem> _copyItems = new List<CopyItem>();
        public int SuccessCount { get; private set; }
        public int ErrorCount { get; private set; }

        public bool HasWork
        {
            get { return _contentItems.Count > 0 || _copyItems.Count > 0 || Clean; }    
        }

        string ReplaceSymbols(string parameter)
        {
            if (string.IsNullOrWhiteSpace(parameter))
                return parameter;

            parameter = parameter.Replace("$(Platform)", Platform.ToString());
            parameter = parameter.Replace("$(Configuration)", Config);
            parameter = parameter.Replace("$(Config)", Config);
            parameter = parameter.Replace("$(Profile)", this.Profile.ToString());

            return parameter;
        }

        public void Build()
        {
            string projectDirectory = PathHelper.Normalize(Directory.GetCurrentDirectory());

            string outputPath = ReplaceSymbols(_outputDir);
            if (!Path.IsPathRooted(outputPath))
                outputPath = PathHelper.Normalize(Path.GetFullPath(Path.Combine(projectDirectory, outputPath)));

            string intermediatePath = ReplaceSymbols(_intermediateDir);
            if (!Path.IsPathRooted(intermediatePath))
                intermediatePath = PathHelper.Normalize(Path.GetFullPath(Path.Combine(projectDirectory, intermediatePath)));

            ContentCompression compression = ContentCompression.Uncompressed;
            if (this.CompressContent)
            {
                switch (this.Compression)
                {
                    case CompressionMethod.Default:
                        compression = ContentCompression.LegacyLZ4;
                        break;
                    case CompressionMethod.LZ4:
                        compression = ContentCompression.LZ4;
                        break;
                    case CompressionMethod.Brotli:
                        compression = ContentCompression.Brotli;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            _manager = new PipelineManager(projectDirectory, _responseFilename, outputPath, intermediatePath, this.Quiet);
            _manager.Compression = compression;

            // Feed all the assembly references to the pipeline manager
            // so it can resolve importers, processors, writers, and types.
            AddReferences(_manager, projectDirectory, this.References);

            // Load the previously serialized list of built content.
            SourceFileCollection previousFileCollection = LoadFileCollection(intermediatePath);
            if (previousFileCollection == null)
                previousFileCollection = new SourceFileCollection();

            // If the target changed in any way then we need to force
            // a full rebuild even under incremental builds.
            bool targetChanged = previousFileCollection.Config != Config
                              || previousFileCollection.Platform != Platform
                              || previousFileCollection.Profile != Profile
                               ;

            // First clean previously built content.
            CleanItems(previousFileCollection, targetChanged);
            // TODO: Should we be cleaning copy items?  I think maybe we should.

            if (Clean)
                return;

            SourceFileCollection newFileCollection = new SourceFileCollection
            {
                Profile = _manager.Profile = Profile,
                Platform = _manager.Platform = Platform,
                Config = _manager.Config = Config
            };
            SuccessCount = 0;
            ErrorCount = 0;

            // Before building the content, register all files to be built.
            // (Necessary to correctly resolve external references.)
            foreach (ContentItem contentItem in _contentItems)
            {
                try
                {
                    BuildEvent buildEvent = _manager.CreateBuildEvent(contentItem.SourceFile, contentItem.OutputFile, contentItem.Importer, contentItem.Processor, contentItem.ProcessorParams);
                    _manager.TrackBuildEvent(buildEvent);
                }
                catch { /* Ignore exception */ }
            }

            if (SingleThread)
                BuildItemsSingleThread(_contentItems, newFileCollection);
            else
                BuildItemsMultiThread(_contentItems, newFileCollection);

            // If this is an incremental build we merge the list
            // of previous content with the new list.
            if (Incremental && !targetChanged)
                newFileCollection.Merge(previousFileCollection);

            // Delete the old file and write the new content 
            // list if we have any to serialize.
            DeleteFileCollection(intermediatePath);
            if (newFileCollection.SourceFilesCount > 0)
                SaveFileCollection(intermediatePath, newFileCollection);

            // Process copy items (files that bypass the content pipeline)
            CopyItems(_copyItems, projectDirectory, outputPath);
        }

        private void AddReferences(PipelineManager manager, string projectDirectory, List<string> references)
        {
            foreach (string r in references)
            {
                string assemblyFile = ReplaceSymbols(r);
                if (!Path.IsPathRooted(assemblyFile))
                    assemblyFile = Path.GetFullPath(Path.Combine(projectDirectory, assemblyFile));

                manager.AddAssembly(assemblyFile);
            }
        }

        private void CleanItems(SourceFileCollection previousFileCollection, bool targetChanged)
        {
            bool cleanOrRebuild = Clean || Rebuild;

            for (int i = 0; i < previousFileCollection.SourceFilesCount; i++)
            {
                string prevSourceFile = previousFileCollection.SourceFiles[i];

                bool inContent = _contentItemsMap.ContainsKey(prevSourceFile.ToLowerInvariant());
                bool cleanOldContent = !inContent && !Incremental;
                bool cleanRebuiltContent = inContent && cleanOrRebuild;
                if (cleanRebuiltContent || cleanOldContent || targetChanged)
                    _manager.CleanContent(prevSourceFile, previousFileCollection.DestFiles[i]);
            }
        }

        private void BuildItemsSingleThread(List<ContentItem> contentItems, SourceFileCollection fileCollection)
        {
            foreach (ContentItem contentItem in contentItems)
            {
                try
                {
                    BuildEvent buildEvent = _manager.CreateBuildEvent(
                                          contentItem.SourceFile,
                                          contentItem.OutputFile,
                                          contentItem.Importer,
                                          contentItem.Processor,
                                          contentItem.ProcessorParams
                                          );
                    // Load the previous content event if it exists.
                    BuildEvent cachedBuildEvent = _manager.LoadBuildEvent(buildEvent.DestFile);
                    _manager.BuildContent(_manager.Logger, buildEvent, cachedBuildEvent, buildEvent.DestFile);

                    fileCollection.AddFile(contentItem.SourceFile, contentItem.OutputFile);
                    SuccessCount++;
                }
                catch (InvalidContentException ex)
                {
                    WriteError(ex, contentItem.SourceFile);
                }
                catch (PipelineException ex)
                {
                    WriteError(ex, contentItem.SourceFile);
                }
                catch (Exception ex)
                {
                    WriteError(ex, contentItem.SourceFile);
                }
            }
        }

        private void BuildItemsMultiThread(List<ContentItem> contentItems, SourceFileCollection fileCollection)
        {
            var buildTaskQueue = new Queue<Task<BuildEvent>>();
            var activeBuildTasks = new List<Task<BuildEvent>>();
            bool firstTask = true;

            int maxConcurrentTasks = Environment.ProcessorCount;

            int ci = 0;
            while (ci < contentItems.Count || activeBuildTasks.Count > 0 || buildTaskQueue.Count > 0)
            {
                // Create build tasks.
                while (activeBuildTasks.Count < maxConcurrentTasks && ci < contentItems.Count)
                {
                    BuildAsyncState buildState = new BuildAsyncState()
                    {
                        SourceFile = contentItems[ci].SourceFile,
                        OutputFile = contentItems[ci].OutputFile,
                        Importer = contentItems[ci].Importer,
                        Processor = contentItems[ci].Processor,
                        ProcessorParams = contentItems[ci].ProcessorParams,
                        Logger = new ConsoleAsyncLogger(_manager.Logger),
                    };
                    buildState.Logger.Immediate = firstTask;

                    Task<BuildEvent> task = Task.Factory.StartNew<BuildEvent>((stateobj) =>
                    {
                        BuildAsyncState state = stateobj as BuildAsyncState;
                        //Console.WriteLine("Task Started - " + Path.GetFileName(state.SourceFile));
                        BuildEvent buildEvent = _manager.CreateBuildEvent(
                                              state.SourceFile,
                                              state.OutputFile,
                                              state.Importer,
                                              state.Processor,
                                              state.ProcessorParams
                                              );
                        // Load the previous content event if it exists.
                        BuildEvent cachedBuildEvent = _manager.LoadBuildEvent(buildEvent.DestFile);
                        _manager.BuildContent(state.Logger, buildEvent, cachedBuildEvent, buildEvent.DestFile);
                        //Console.WriteLine("Task Ended - " + Path.GetFileName(state.SourceFile));
                        return buildEvent;
                    }, buildState, TaskCreationOptions.PreferFairness);
                    buildTaskQueue.Enqueue(task);
                    activeBuildTasks.Add(task);
                    firstTask = false;
                    ci++;
                }

                if (buildTaskQueue.Count > 0)
                {
                    // Get task at the top of the queue.
                    var topTask = buildTaskQueue.Peek();
                    var topBuildState = topTask.AsyncState as BuildAsyncState;
                    topBuildState.Logger.Immediate = true;

                    // Remove task from queue if completed.
                    if (topTask.IsCompleted || topTask.IsCanceled || topTask.IsFaulted)
                    {
                        buildTaskQueue.Dequeue();
                        //flash log
                        topBuildState.Logger.Flush();

                        if (topTask.IsFaulted)
                        {
                            if (topTask.Exception.InnerException is InvalidContentException)
                            {
                                InvalidContentException ex = topTask.Exception.InnerException as InvalidContentException;
                                WriteError(ex, topBuildState.SourceFile);
                            }
                            else if (topTask.Exception.InnerException is PipelineException)
                            {
                                PipelineException ex = topTask.Exception.InnerException as PipelineException;
                                WriteError(ex, topBuildState.SourceFile);
                            }
                            else
                            {
                                Exception ex = topTask.Exception.InnerException;
                                WriteError(ex, topBuildState.SourceFile);
                            }
                        }
                        else if (topTask.IsCanceled)
                        {
                            //
                        }

                        continue;
                    }
                }

                Task.WaitAny(activeBuildTasks.ToArray());

                // Remove completed tasks.
                for (int i = activeBuildTasks.Count - 1; i >= 0; i--)
                {
                    Task<BuildEvent> task = activeBuildTasks[i];
                    if (task.IsCompleted || task.IsCanceled || task.IsFaulted)
                    {
                        activeBuildTasks.RemoveAt(i);
                        if (task.IsCompleted)
                        {
                            BuildAsyncState buildState = task.AsyncState as BuildAsyncState;

                            fileCollection.AddFile(buildState.SourceFile, buildState.OutputFile);
                            SuccessCount++;
                        }
                    }
                }
            }
        }

        private void CopyItems(List<CopyItem> copyItems, string projectDirectory, string outputPath)
        {
            object syncHandle = new object();

            Parallel.ForEach(copyItems, (item) =>
            {
                try
                {
                    // Figure out an asset name relative to the project directory,
                    // retaining the file extension.
                    // Note that replacing a sub-path like this requires consistent
                    // directory separator characters.
                    string relativeName = item.Link;
                    if (string.IsNullOrWhiteSpace(relativeName))
                        relativeName = item.SourceFile.Replace(projectDirectory, string.Empty)
                                            .TrimStart(Path.DirectorySeparatorChar)
                                            .TrimStart(Path.AltDirectorySeparatorChar);
                    string dest = Path.Combine(outputPath, relativeName);

                    // Only copy if the source file is newer than the destination.
                    // We may want to provide an option for overriding this, but for
                    // nearly all cases this is the desired behavior.
                    if (File.Exists(dest) && !Rebuild)
                    {
                        DateTime srcTime = File.GetLastWriteTimeUtc(item.SourceFile);
                        DateTime dstTime = File.GetLastWriteTimeUtc(dest);
                        if (srcTime <= dstTime)
                        {
                            lock (syncHandle)
                            {
                                if (!this.Quiet)
                                {
                                    string skipSourceFileOutput = String.Format("Skipping {0}", item.SourceFile);
                                    //if (!string.IsNullOrEmpty(item.Link))
                                    //    skipSourceFileOutput = String.Format("Skipping {0} => {1}", item.SourceFile, item.Link);
                                    Console.WriteLine(skipSourceFileOutput);
                                }
                                SuccessCount++;
                                return;
                            }
                        }
                    }

                    DateTime startTime = DateTime.UtcNow;

                    // Create the destination directory if it doesn't already exist.
                    string destPath = Path.GetDirectoryName(dest);
                    if (!Directory.Exists(destPath))
                        Directory.CreateDirectory(destPath);

                    File.Copy(item.SourceFile, dest, true);

                    // Destination file should not be read-only even if original was.
                    FileAttributes fileAttr = File.GetAttributes(dest);
                    fileAttr = fileAttr & (~FileAttributes.ReadOnly);
                    File.SetAttributes(dest, fileAttr);

                    TimeSpan buildTime = DateTime.UtcNow - startTime;

                }
                catch (Exception ex)
                {
                    lock (syncHandle)
                    {
                        string buildSourceFileOutput = String.Format("{0}", item.SourceFile);
                        if (!string.IsNullOrEmpty(item.Link))
                        {
                            //buildSourceFileOutput = String.Format("{0} => {1}", item.SourceFile, item.Link);
                        }
                        Console.WriteLine(buildSourceFileOutput);
                        WriteError(ex, item.SourceFile);
                        ErrorCount++;
                        return;
                    }
                }

                lock (syncHandle)
                {
                    string buildSourceFileOutput = String.Format("{0}", item.SourceFile);
                    if (!string.IsNullOrEmpty(item.Link))
                    {
                        //buildSourceFileOutput = String.Format("{0} => {1}", item.SourceFile, item.Link);
                    }
                    Console.WriteLine(buildSourceFileOutput);
                    SuccessCount++;
                    return;
                }
            });
        }
        
        private void WriteError(InvalidContentException ex, string sourceFile)
        {
            string message = string.Empty;
            if (ex.ContentIdentity != null && !string.IsNullOrEmpty(ex.ContentIdentity.SourceFilename))
            {
                message = ex.ContentIdentity.SourceFilename;
                if (!string.IsNullOrEmpty(ex.ContentIdentity.FragmentIdentifier))
                    message += "(" + ex.ContentIdentity.FragmentIdentifier + ")";
                else
                    message += " ";
            }
            else
            {
                message = sourceFile;
                message += " ";
            }
            message += ": error ";

            // extract errorCode from message
            System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(ex.Message, @"([A-Z]+[0-9]+):(.+)");
            if (match.Success || match.Groups.Count == 2)
                message += match.Groups[1].Value + " : " + match.Groups[2].Value;
            else
                message += ": " + ex.Message;

            Console.Error.WriteLine(message);
            ErrorCount++;
        }

        private void WriteError(PipelineException ex, string sourceFile)
        {
            Console.Error.WriteLine("{0} : error : {1}", sourceFile, ex.Message);
            if (ex.InnerException != null)
                Console.Error.WriteLine(ex.InnerException.ToString());
            ErrorCount++;
        }

        private void WriteError(Exception ex, string sourceFile)
        {
            Console.Error.WriteLine("{0} : error : {1}", sourceFile, ex.Message);
            if (ex.InnerException != null)
                Console.Error.WriteLine(ex.InnerException.ToString());
            ErrorCount++;
        }
        
        private void DeleteFileCollection(string intermediatePath)
        {
            string dbname = _responseFilename;
            if (dbname == String.Empty)
                dbname = PipelineManager.DefaultFileCollectionFilename;

            string intermediateFileCollectionPath = Path.Combine(intermediatePath, Path.ChangeExtension(dbname, SourceFileCollection.Extension));
            FileHelper.DeleteIfExists(intermediateFileCollectionPath);
        }

        private void SaveFileCollection(string intermediatePath, SourceFileCollection fileCollection)
        {
            string dbname = _responseFilename;
            if (dbname == String.Empty)
                dbname = PipelineManager.DefaultFileCollectionFilename;

            string intermediateFileCollectionPath = Path.Combine(intermediatePath, Path.ChangeExtension(dbname, SourceFileCollection.Extension));
            fileCollection.SaveBinary(intermediateFileCollectionPath);
        }

        private SourceFileCollection LoadFileCollection(string intermediatePath)
        {
            string dbname = _responseFilename;
            if (dbname == String.Empty)
                dbname = PipelineManager.DefaultFileCollectionFilename;

            string intermediateFileCollectionPath = Path.Combine(intermediatePath, Path.ChangeExtension(dbname, SourceFileCollection.Extension));
            return SourceFileCollection.LoadBinary(intermediateFileCollectionPath);
        }

    }
}
