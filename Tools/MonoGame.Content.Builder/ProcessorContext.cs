// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Framework.Content.Pipeline.Builder
{
    internal class ProcessorContext : ContentProcessorContext
    {
        private readonly PipelineManager _manager;
        ConsoleLogger _logger;
        private readonly BuildEvent _buildEvent;

        public ProcessorContext(PipelineManager manager, ConsoleLogger logger, BuildEvent buildEvent)
        {
            _manager = manager;
            _logger = logger;
            _buildEvent = buildEvent;
        }

        public override TargetPlatform TargetPlatform { get { return _manager.Platform; } }
        public override GraphicsProfile TargetProfile { get { return _manager.Profile; } }

        public override string BuildConfiguration { get { return _manager.Config; } }

        public override string IntermediateDirectory { get { return _manager.IntermediateDirectory; } }
        public override string OutputDirectory { get { return _manager.OutputDirectory; } }
        public override string OutputFilename { get { return _buildEvent.DestFile; } }

        public override OpaqueDataDictionary Parameters { get { return _buildEvent.Parameters; } }

        public override ContentBuildLogger Logger { get { return _logger; } }

        public override void AddDependency(string filename)
        {
            if (!_buildEvent.Dependencies.Contains(filename))
                _buildEvent.Dependencies.Add(filename);
        }

        public override void AddOutputFile(string filename)
        {
            if (!_buildEvent.BuildOutput.Contains(filename))
                _buildEvent.BuildOutput.Add(filename);
        }

        public override TOutput Convert<TInput, TOutput>(   TInput input, 
                                                            string processorName,
                                                            OpaqueDataDictionary processorParameters)
        {
            IContentProcessor processor = _manager.CreateProcessor(processorName, processorParameters);
            ProcessorContext processContext = new ProcessorContext(_manager, this._logger, new BuildEvent { Parameters = processorParameters } );
            var processedObject = processor.Process(input, processContext);
           
            // Add its dependencies and built assets to ours.
            foreach (string dependency in processContext._buildEvent.Dependencies)
            {
                if (!_buildEvent.Dependencies.Contains(dependency))
                    _buildEvent.Dependencies.Add(dependency);
            }
            foreach (string buildAsset in processContext._buildEvent.BuildAsset)
            {
                if (!_buildEvent.BuildAsset.Contains(buildAsset))
                    _buildEvent.BuildAsset.Add(buildAsset);
            }

            return (TOutput)processedObject;
        }

        public override TOutput BuildAndLoadAsset<TInput, TOutput>( ExternalReference<TInput> sourceAsset,
                                                                    string processorName,
                                                                    OpaqueDataDictionary processorParameters,
                                                                    string importerName)
        {
            var sourceFilepath = PathHelper.Normalize(sourceAsset.Filename);

            // The processorName can be null or empty. In this case the asset should
            // be imported but not processed. This is, for example, necessary to merge
            // animation files as described here:
            // http://blogs.msdn.com/b/shawnhar/archive/2010/06/18/merging-animation-files.aspx.

            bool processAsset = !string.IsNullOrEmpty(processorName);
            _manager.ResolveImporterAndProcessor(sourceFilepath, ref importerName, ref processorName);

            BuildEvent buildEvent = new BuildEvent
            { 
                SourceFile = sourceFilepath,
                Importer = importerName,
                Processor = processAsset ? processorName : null,
                Parameters = _manager.ValidateProcessorParameters(processorName, processorParameters),
            };

            var processedObject = _manager.ProcessContent(this._logger, buildEvent);

            // Record that we processed this dependent asset.
            if (!_buildEvent.Dependencies.Contains(sourceFilepath))
                _buildEvent.Dependencies.Add(sourceFilepath);

            return (TOutput)processedObject;
        }

        public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>( ExternalReference<TInput> sourceAsset,
                                                                                string processorName,
                                                                                OpaqueDataDictionary processorParameters,
                                                                                string importerName, 
                                                                                string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
                assetName = _manager.GetAssetName(sourceAsset.Filename, importerName, processorName, processorParameters, this._logger);

            // Build the content.
            BuildEvent buildEvent = _manager.CreateBuildEvent(sourceAsset.Filename, assetName, importerName, processorName, processorParameters);
            BuildEvent cachedBuildEvent = _manager.LoadBuildEvent(buildEvent.DestFile);
            _manager.BuildContent(this._logger, buildEvent, cachedBuildEvent, buildEvent.DestFile);

            // Record that we built this dependent asset.
            if (!_buildEvent.BuildAsset.Contains(buildEvent.DestFile))
                _buildEvent.BuildAsset.Add(buildEvent.DestFile);

            return new ExternalReference<TOutput>(buildEvent.DestFile);
        }
    }
}
